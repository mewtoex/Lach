# Lach System - WhatsApp Management Script
# Script para gerenciar o WhatsApp Web

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("start", "stop", "status", "qr", "send", "contacts", "messages", "help")]
    [string]$Action,
    
    [string]$PhoneNumber = "",
    [string]$Message = "",
    [string]$SessionId = ""
)

Write-Host "📱 Lach System - WhatsApp Management" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan

$API_BASE = "http://localhost:5006/api/whatsapp"
$WEB_SERVICE_BASE = "http://localhost:3003"

function Show-Help {
    Write-Host "`n📖 Comandos Disponíveis:" -ForegroundColor Yellow
    Write-Host "start <phone>     - Iniciar sessão WhatsApp" -ForegroundColor White
    Write-Host "stop <session>    - Parar sessão WhatsApp" -ForegroundColor White
    Write-Host "status <session>  - Verificar status da sessão" -ForegroundColor White
    Write-Host "qr <session>      - Obter QR Code para conexão" -ForegroundColor White
    Write-Host "send <phone> <msg> - Enviar mensagem" -ForegroundColor White
    Write-Host "contacts          - Listar contatos" -ForegroundColor White
    Write-Host "messages <phone>  - Ver mensagens de um número" -ForegroundColor White
    Write-Host "help              - Esta ajuda" -ForegroundColor White
    
    Write-Host "`n📝 Exemplos:" -ForegroundColor Yellow
    Write-Host "./scripts/manage-whatsapp.ps1 start +5511999999999" -ForegroundColor Gray
    Write-Host "./scripts/manage-whatsapp.ps1 send +5511999999999 'Olá!'" -ForegroundColor Gray
    Write-Host "./scripts/manage-whatsapp.ps1 qr session123" -ForegroundColor Gray
}

function Start-WhatsAppSession {
    param([string]$PhoneNumber)
    
    if ([string]::IsNullOrEmpty($PhoneNumber)) {
        Write-Host "❌ Número de telefone é obrigatório!" -ForegroundColor Red
        return
    }
    
    Write-Host "🚀 Iniciando sessão WhatsApp para: $PhoneNumber" -ForegroundColor Blue
    
    try {
        $body = @{
            phoneNumber = $PhoneNumber
        } | ConvertTo-Json
        
        $response = Invoke-RestMethod -Uri "$API_BASE/sessions" -Method POST -Body $body -ContentType "application/json"
        
        Write-Host "✅ Sessão criada com sucesso!" -ForegroundColor Green
        Write-Host "Session ID: $($response.sessionId)" -ForegroundColor White
        Write-Host "Status: $($response.status)" -ForegroundColor White
        
        # Aguardar um pouco e verificar QR Code
        Start-Sleep -Seconds 3
        Get-QrCode -SessionId $response.sessionId
        
    } catch {
        Write-Host "❌ Erro ao criar sessão: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Stop-WhatsAppSession {
    param([string]$SessionId)
    
    if ([string]::IsNullOrEmpty($SessionId)) {
        Write-Host "❌ Session ID é obrigatório!" -ForegroundColor Red
        return
    }
    
    Write-Host "🛑 Parando sessão: $SessionId" -ForegroundColor Blue
    
    try {
        Invoke-RestMethod -Uri "$API_BASE/sessions/$SessionId" -Method DELETE
        Write-Host "✅ Sessão parada com sucesso!" -ForegroundColor Green
    } catch {
        Write-Host "❌ Erro ao parar sessão: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Get-SessionStatus {
    param([string]$SessionId)
    
    if ([string]::IsNullOrEmpty($SessionId)) {
        Write-Host "❌ Session ID é obrigatório!" -ForegroundColor Red
        return
    }
    
    Write-Host "📊 Status da sessão: $SessionId" -ForegroundColor Blue
    
    try {
        $response = Invoke-RestMethod -Uri "$WEB_SERVICE_BASE/status/$SessionId" -Method GET
        
        Write-Host "Session ID: $($response.sessionId)" -ForegroundColor White
        Write-Host "Conectado: $($response.connected)" -ForegroundColor $(if($response.connected) { "Green" } else { "Red" })
        Write-Host "Status: $($response.status)" -ForegroundColor White
        
    } catch {
        Write-Host "❌ Erro ao obter status: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Get-QrCode {
    param([string]$SessionId)
    
    if ([string]::IsNullOrEmpty($SessionId)) {
        Write-Host "❌ Session ID é obrigatório!" -ForegroundColor Red
        return
    }
    
    Write-Host "📱 Obtendo QR Code para sessão: $SessionId" -ForegroundColor Blue
    
    try {
        $response = Invoke-RestMethod -Uri "$WEB_SERVICE_BASE/qr/$SessionId" -Method GET
        
        if ($response.qrCode) {
            Write-Host "✅ QR Code obtido!" -ForegroundColor Green
            Write-Host "`n📱 Escaneie o QR Code no seu WhatsApp:" -ForegroundColor Yellow
            Write-Host "1. Abra o WhatsApp no seu celular" -ForegroundColor White
            Write-Host "2. Vá em Configurações > Aparelhos conectados" -ForegroundColor White
            Write-Host "3. Toque em 'Conectar um aparelho'" -ForegroundColor White
            Write-Host "4. Escaneie o QR Code abaixo:" -ForegroundColor White
            
            # Salvar QR Code em arquivo
            $qrFile = "qr_code_$SessionId.png"
            $qrData = [System.Convert]::FromBase64String($response.qrCode.Split(',')[1])
            [System.IO.File]::WriteAllBytes($qrFile, $qrData)
            
            Write-Host "`n💾 QR Code salvo em: $qrFile" -ForegroundColor Green
            Write-Host "🖼️  Abra o arquivo para escanear o código" -ForegroundColor Yellow
            
        } else {
            Write-Host "⚠️  QR Code não disponível. Verifique se a sessão está inicializada." -ForegroundColor Yellow
        }
        
    } catch {
        Write-Host "❌ Erro ao obter QR Code: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Send-WhatsAppMessage {
    param([string]$PhoneNumber, [string]$Message)
    
    if ([string]::IsNullOrEmpty($PhoneNumber) -or [string]::IsNullOrEmpty($Message)) {
        Write-Host "❌ Número de telefone e mensagem são obrigatórios!" -ForegroundColor Red
        return
    }
    
    Write-Host "📤 Enviando mensagem para: $PhoneNumber" -ForegroundColor Blue
    Write-Host "Mensagem: $Message" -ForegroundColor White
    
    try {
        $body = @{
            toNumber = $PhoneNumber
            content = $Message
        } | ConvertTo-Json
        
        $response = Invoke-RestMethod -Uri "$API_BASE/messages/send" -Method POST -Body $body -ContentType "application/json"
        
        if ($response.success) {
            Write-Host "✅ Mensagem enviada com sucesso!" -ForegroundColor Green
        } else {
            Write-Host "❌ Falha ao enviar mensagem: $($response.message)" -ForegroundColor Red
        }
        
    } catch {
        Write-Host "❌ Erro ao enviar mensagem: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Get-Contacts {
    Write-Host "👥 Listando contatos..." -ForegroundColor Blue
    
    try {
        $contacts = Invoke-RestMethod -Uri "$API_BASE/contacts" -Method GET
        
        if ($contacts.Count -eq 0) {
            Write-Host "📭 Nenhum contato encontrado." -ForegroundColor Yellow
            return
        }
        
        Write-Host "`n📋 Contatos ($($contacts.Count)):" -ForegroundColor Green
        foreach ($contact in $contacts) {
            Write-Host "📞 $($contact.phoneNumber)" -ForegroundColor White
            if ($contact.name) { Write-Host "   Nome: $($contact.name)" -ForegroundColor Gray }
            if ($contact.email) { Write-Host "   Email: $($contact.email)" -ForegroundColor Gray }
            Write-Host "   Última interação: $($contact.lastInteraction)" -ForegroundColor Gray
            Write-Host ""
        }
        
    } catch {
        Write-Host "❌ Erro ao listar contatos: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Get-Messages {
    param([string]$PhoneNumber)
    
    if ([string]::IsNullOrEmpty($PhoneNumber)) {
        Write-Host "❌ Número de telefone é obrigatório!" -ForegroundColor Red
        return
    }
    
    Write-Host "💬 Mensagens de: $PhoneNumber" -ForegroundColor Blue
    
    try {
        $messages = Invoke-RestMethod -Uri "$API_BASE/messages/phone/$PhoneNumber" -Method GET
        
        if ($messages.Count -eq 0) {
            Write-Host "📭 Nenhuma mensagem encontrada." -ForegroundColor Yellow
            return
        }
        
        Write-Host "`n💬 Mensagens ($($messages.Count)):" -ForegroundColor Green
        foreach ($message in $messages | Sort-Object createdAt) {
            $direction = if ($message.direction -eq "in") { "📥" } else { "📤" }
            $time = [DateTime]::Parse($message.createdAt).ToString("dd/MM/yyyy HH:mm")
            
            Write-Host "$direction [$time] $($message.content)" -ForegroundColor White
            Write-Host "   Status: $($message.status)" -ForegroundColor Gray
            Write-Host ""
        }
        
    } catch {
        Write-Host "❌ Erro ao obter mensagens: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Test-Services {
    Write-Host "🔍 Testando serviços..." -ForegroundColor Blue
    
    # Test C# Service
    try {
        $health = Invoke-RestMethod -Uri "$API_BASE/health" -Method GET
        Write-Host "✅ C# Service: $($health.status)" -ForegroundColor Green
    } catch {
        Write-Host "❌ C# Service: Inacessível" -ForegroundColor Red
    }
    
    # Test Node.js Service
    try {
        $health = Invoke-RestMethod -Uri "$WEB_SERVICE_BASE/health" -Method GET
        Write-Host "✅ Node.js Service: $($health.status)" -ForegroundColor Green
        Write-Host "   Sessões ativas: $($health.activeSessions)" -ForegroundColor White
    } catch {
        Write-Host "❌ Node.js Service: Inacessível" -ForegroundColor Red
    }
}

# Main execution
switch ($Action.ToLower()) {
    "start" { Start-WhatsAppSession -PhoneNumber $PhoneNumber }
    "stop" { Stop-WhatsAppSession -SessionId $SessionId }
    "status" { Get-SessionStatus -SessionId $SessionId }
    "qr" { Get-QrCode -SessionId $SessionId }
    "send" { Send-WhatsAppMessage -PhoneNumber $PhoneNumber -Message $Message }
    "contacts" { Get-Contacts }
    "messages" { Get-Messages -PhoneNumber $PhoneNumber }
    "test" { Test-Services }
    "help" { Show-Help }
    default {
        Write-Host "❌ Ação inválida!" -ForegroundColor Red
        Show-Help
    }
} 