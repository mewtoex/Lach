const express = require('express');
const { Client, LocalAuth } = require('whatsapp-web.js');
const qrcode = require('qrcode');
const cors = require('cors');
const helmet = require('helmet');
const morgan = require('morgan');
require('dotenv').config();

const app = express();
const PORT = process.env.PORT || 3003;

// Middleware
app.use(helmet());
app.use(cors());
app.use(morgan('combined'));
app.use(express.json());

// Store active sessions
const sessions = new Map();

// Initialize WhatsApp client
function createClient(sessionId) {
    const client = new Client({
        authStrategy: new LocalAuth({ clientId: sessionId }),
        puppeteer: {
            headless: true,
            args: [
                '--no-sandbox',
                '--disable-setuid-sandbox',
                '--disable-dev-shm-usage',
                '--disable-accelerated-2d-canvas',
                '--no-first-run',
                '--no-zygote',
                '--single-process',
                '--disable-gpu'
            ]
        }
    });

    // Store client in sessions
    sessions.set(sessionId, {
        client,
        status: 'initializing',
        qrCode: null,
        connected: false
    });

    // QR Code event
    client.on('qr', async (qr) => {
        console.log(`QR Code received for session ${sessionId}`);
        try {
            const qrCodeDataUrl = await qrcode.toDataURL(qr);
            sessions.get(sessionId).qrCode = qrCodeDataUrl;
            sessions.get(sessionId).status = 'qr_ready';
        } catch (error) {
            console.error('Error generating QR code:', error);
        }
    });

    // Ready event
    client.on('ready', () => {
        console.log(`Client ready for session ${sessionId}`);
        const session = sessions.get(sessionId);
        session.status = 'connected';
        session.connected = true;
        session.qrCode = null;
    });

    // Authenticated event
    client.on('authenticated', () => {
        console.log(`Client authenticated for session ${sessionId}`);
        sessions.get(sessionId).status = 'authenticated';
    });

    // Auth failure event
    client.on('auth_failure', (msg) => {
        console.log(`Auth failure for session ${sessionId}:`, msg);
        sessions.get(sessionId).status = 'auth_failure';
    });

    // Disconnected event
    client.on('disconnected', (reason) => {
        console.log(`Client disconnected for session ${sessionId}:`, reason);
        const session = sessions.get(sessionId);
        session.status = 'disconnected';
        session.connected = false;
        sessions.delete(sessionId);
    });

    // Message event
    client.on('message', async (message) => {
        console.log(`Message received in session ${sessionId}:`, message.body);
        
        // Forward message to C# service webhook
        try {
            const webhookData = {
                messageId: message.id._serialized,
                fromNumber: message.from,
                toNumber: message.to,
                messageType: message.type,
                content: message.body,
                timestamp: message.timestamp
            };

            // Send to C# service webhook
            const response = await fetch('http://whatsapp-service:5006/api/whatsapp/webhook/message', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(webhookData)
            });

            if (!response.ok) {
                console.error('Failed to forward message to webhook');
            }
        } catch (error) {
            console.error('Error forwarding message to webhook:', error);
        }
    });

    return client;
}

// Routes

// Initialize session
app.post('/initialize', async (req, res) => {
    try {
        const { sessionId, headless = true, puppeteerArgs = [] } = req.body;

        if (!sessionId) {
            return res.status(400).json({ error: 'Session ID is required' });
        }

        if (sessions.has(sessionId)) {
            return res.status(400).json({ error: 'Session already exists' });
        }

        const client = createClient(sessionId);
        await client.initialize();

        res.json({ 
            success: true, 
            sessionId,
            message: 'Session initialized successfully' 
        });
    } catch (error) {
        console.error('Error initializing session:', error);
        res.status(500).json({ error: 'Failed to initialize session' });
    }
});

// Get QR Code
app.get('/qr/:sessionId', (req, res) => {
    const { sessionId } = req.params;
    const session = sessions.get(sessionId);

    if (!session) {
        return res.status(404).json({ error: 'Session not found' });
    }

    if (session.qrCode) {
        res.json({ qrCode: session.qrCode });
    } else {
        res.status(404).json({ error: 'QR code not available' });
    }
});

// Get connection status
app.get('/status/:sessionId', (req, res) => {
    const { sessionId } = req.params;
    const session = sessions.get(sessionId);

    if (!session) {
        return res.status(404).json({ error: 'Session not found' });
    }

    res.json({
        sessionId,
        connected: session.connected,
        status: session.status,
        hasQrCode: !!session.qrCode
    });
});

// Send message
app.post('/send', async (req, res) => {
    try {
        const { to, content, sessionId = 'default' } = req.body;

        if (!to || !content) {
            return res.status(400).json({ error: 'To and content are required' });
        }

        const session = sessions.get(sessionId);
        if (!session || !session.connected) {
            return res.status(400).json({ error: 'Session not connected' });
        }

        // Format phone number
        const formattedNumber = to.includes('@c.us') ? to : `${to}@c.us`;

        const message = await session.client.sendMessage(formattedNumber, content);
        
        res.json({ 
            success: true, 
            messageId: message.id._serialized,
            message: 'Message sent successfully' 
        });
    } catch (error) {
        console.error('Error sending message:', error);
        res.status(500).json({ error: 'Failed to send message' });
    }
});

// Get session info
app.get('/info/:sessionId', (req, res) => {
    const { sessionId } = req.params;
    const session = sessions.get(sessionId);

    if (!session) {
        return res.status(404).json({ error: 'Session not found' });
    }

    res.json({
        sessionId,
        connected: session.connected,
        status: session.status,
        hasQrCode: !!session.qrCode,
        info: session.client.info || null
    });
});

// Delete session
app.delete('/session/:sessionId', async (req, res) => {
    try {
        const { sessionId } = req.params;
        const session = sessions.get(sessionId);

        if (!session) {
            return res.status(404).json({ error: 'Session not found' });
        }

        await session.client.destroy();
        sessions.delete(sessionId);

        res.json({ success: true, message: 'Session deleted successfully' });
    } catch (error) {
        console.error('Error deleting session:', error);
        res.status(500).json({ error: 'Failed to delete session' });
    }
});

// Health check
app.get('/health', (req, res) => {
    res.json({
        status: 'Healthy',
        service: 'WhatsApp Web Service',
        timestamp: new Date().toISOString(),
        activeSessions: sessions.size
    });
});

// List all sessions
app.get('/sessions', (req, res) => {
    const sessionList = Array.from(sessions.keys()).map(sessionId => {
        const session = sessions.get(sessionId);
        return {
            sessionId,
            connected: session.connected,
            status: session.status,
            hasQrCode: !!session.qrCode
        };
    });

    res.json(sessionList);
});

// Error handling middleware
app.use((error, req, res, next) => {
    console.error('Unhandled error:', error);
    res.status(500).json({ error: 'Internal server error' });
});

// 404 handler
app.use((req, res) => {
    res.status(404).json({ error: 'Endpoint not found' });
});

// Start server
app.listen(PORT, () => {
    console.log(`WhatsApp Web Service running on port ${PORT}`);
    console.log(`Health check: http://localhost:${PORT}/health`);
});

// Graceful shutdown
process.on('SIGTERM', async () => {
    console.log('SIGTERM received, shutting down gracefully');
    
    for (const [sessionId, session] of sessions) {
        try {
            await session.client.destroy();
        } catch (error) {
            console.error(`Error destroying session ${sessionId}:`, error);
        }
    }
    
    process.exit(0);
});

process.on('SIGINT', async () => {
    console.log('SIGINT received, shutting down gracefully');
    
    for (const [sessionId, session] of sessions) {
        try {
            await session.client.destroy();
        } catch (error) {
            console.error(`Error destroying session ${sessionId}:`, error);
        }
    }
    
    process.exit(0);
}); 