# 📱 Integração WhatsApp - Lach System

## 🎯 Visão Geral

A integração WhatsApp do Lach System utiliza a biblioteca `whatsapp-web.js` para automatizar o WhatsApp Web. Esta solução permite:

- ✅ **Gratuita**: Sem custos de API
- ✅ **Flexível**: Controle total sobre as funcionalidades
- ✅ **Local**: Dados armazenados localmente
- ⚠️ **Instável**: Pode apresentar problemas de conexão
- ⚠️ **Manual**: Requer escaneamento de QR Code

## 🏗️ Arquitetura

### Componentes

1. **WhatsAppService (C#)**: Serviço principal de gerenciamento
2. **WhatsApp Web Service (Node.js)**: Interface com whatsapp-web.js
3. **SQLite Database**: Armazenamento local de mensagens e contatos

### Fluxo de Dados

```
WhatsApp Web → Node.js Service → C# Service → SQLite Database
     ↑              ↓              ↓
   QR Code    whatsapp-web.js   Business Logic
```

## 🚀 Configuração

### 1. Pré-requisitos

- Docker e Docker Compose
- Node.js 18+ (para desenvolvimento local)
- WhatsApp no celular

### 2. Executar o Sistema

```bash
# Iniciar todos os serviços
docker-compose up -d

# Verificar status
docker-compose ps
```

### 3. Criar Sessão WhatsApp

```bash
# Usar o script de gerenciamento
./scripts/manage-whatsapp.ps1 start +5511999999999

# Ou via API
curl -X POST http://localhost:5006/api/whatsapp/sessions \
  -H "Content-Type: application/json" \
  -d '{"phoneNumber": "+5511999999999"}'
```

### 4. Conectar WhatsApp

1. **Obter QR Code**:
   ```bash
   ./scripts/manage-whatsapp.ps1 qr <session-id>
   ```

2. **Escanear QR Code**:
   - Abra o WhatsApp no celular
   - Vá em Configurações > Aparelhos conectados
   - Toque em "Conectar um aparelho"
   - Escaneie o QR Code gerado

3. **Verificar Status**:
   ```bash
   ./scripts/manage-whatsapp.ps1 status <session-id>
   ```

## 📱 Funcionalidades

### 1. Chatbot Automático

O sistema responde automaticamente a mensagens com:

- **Cardápio**: Lista de produtos disponíveis
- **Pedidos**: Instruções para fazer pedidos
- **Status**: Verificação de status de pedidos
- **Ajuda**: Comandos disponíveis

### 2. Envio de Mensagens

```bash
# Enviar mensagem
./scripts/manage-whatsapp.ps1 send +5511999999999 "Olá! Como posso ajudar?"

# Via API
curl -X POST http://localhost:5006/api/whatsapp/messages/send \
  -H "Content-Type: application/json" \
  -d '{
    "toNumber": "+5511999999999",
    "content": "Olá! Como posso ajudar?"
  }'
```

### 3. Atualizações de Pedido

```bash
# Enviar atualização de status
curl -X POST http://localhost:5006/api/whatsapp/orders/status \
  -H "Content-Type: application/json" \
  -d '{
    "toNumber": "+5511999999999",
    "orderId": "123",
    "status": "InProgress",
    "message": "Seu pedido está sendo preparado!"
  }'
```

### 4. Confirmação de Pedido

```bash
# Enviar confirmação
curl -X POST http://localhost:5006/api/whatsapp/orders/confirmation \
  -H "Content-Type: application/json" \
  -d '{
    "toNumber": "+5511999999999",
    "order": {
      "customerId": "456",
      "customerName": "João Silva",
      "totalAmount": 31.80,
      "deliveryAddress": "Rua das Flores, 123",
      "items": [
        {
          "productName": "X-Burger",
          "quantity": 2,
          "price": 15.90
        }
      ]
    }
  }'
```

## 📊 Gerenciamento

### Scripts Disponíveis

```bash
# Iniciar sessão
./scripts/manage-whatsapp.ps1 start +5511999999999

# Ver QR Code
./scripts/manage-whatsapp.ps1 qr <session-id>

# Verificar status
./scripts/manage-whatsapp.ps1 status <session-id>

# Enviar mensagem
./scripts/manage-whatsapp.ps1 send +5511999999999 "Mensagem"

# Listar contatos
./scripts/manage-whatsapp.ps1 contacts

# Ver mensagens
./scripts/manage-whatsapp.ps1 messages +5511999999999

# Parar sessão
./scripts/manage-whatsapp.ps1 stop <session-id>

# Testar serviços
./scripts/manage-whatsapp.ps1 test
```

### APIs Disponíveis

#### Sessões
- `POST /api/whatsapp/sessions` - Criar sessão
- `GET /api/whatsapp/sessions/{sessionId}` - Obter sessão
- `DELETE /api/whatsapp/sessions/{sessionId}` - Deletar sessão

#### QR Code e Status
- `GET /api/whatsapp/sessions/{sessionId}/qr` - Obter QR Code
- `GET /api/whatsapp/sessions/{sessionId}/status` - Verificar status

#### Mensagens
- `POST /api/whatsapp/messages/send` - Enviar mensagem
- `GET /api/whatsapp/messages/{messageId}` - Obter mensagem
- `GET /api/whatsapp/messages/phone/{phoneNumber}` - Histórico

#### Contatos
- `GET /api/whatsapp/contacts` - Listar contatos
- `GET /api/whatsapp/contacts/{phoneNumber}` - Obter contato
- `POST /api/whatsapp/contacts` - Criar/atualizar contato

#### Chatbot
- `POST /api/whatsapp/chatbot/process` - Processar mensagem

#### Pedidos
- `POST /api/whatsapp/orders/status` - Atualizar status
- `POST /api/whatsapp/orders/confirmation` - Confirmar pedido

#### Webhook
- `POST /api/whatsapp/webhook/message` - Mensagem recebida
- `POST /api/whatsapp/webhook/status` - Atualização de status

## 💾 Armazenamento

### SQLite Database

O sistema usa SQLite para armazenar:

#### Tabelas
- **Sessions**: Sessões WhatsApp ativas
- **Messages**: Histórico de mensagens
- **Contacts**: Contatos dos clientes

#### Localização
```
whatsapp.db (dentro do container)
```

### Backup

```bash
# Fazer backup do banco
docker cp lach-whatsapp-service:/app/whatsapp.db ./backup/whatsapp_$(Get-Date -Format "yyyyMMdd_HHmmss").db

# Restaurar backup
docker cp ./backup/whatsapp.db lach-whatsapp-service:/app/whatsapp.db
```

## 🔧 Configuração Avançada

### Variáveis de Ambiente

```env
# WhatsApp Web Service
PORT=3003
C_SHARP_SERVICE_URL=http://whatsapp-service:5006

# Puppeteer
PUPPETEER_SKIP_CHROMIUM_DOWNLOAD=true
PUPPETEER_EXECUTABLE_PATH=/usr/bin/chromium-browser

# C# Service
ConnectionStrings__SQLite=Data Source=whatsapp.db
WhatsAppWeb__WebServiceUrl=http://whatsapp-web-service:3003
WhatsAppWeb__TimeoutSeconds=30
```

### Volumes Docker

```yaml
volumes:
  - whatsapp_sessions:/usr/src/app/.wwebjs_auth  # Sessões WhatsApp
```

## 🚨 Troubleshooting

### Problemas Comuns

#### 1. QR Code não aparece
```bash
# Verificar status do serviço
./scripts/manage-whatsapp.ps1 test

# Reiniciar sessão
./scripts/manage-whatsapp.ps1 stop <session-id>
./scripts/manage-whatsapp.ps1 start +5511999999999
```

#### 2. Conexão perdida
```bash
# Verificar logs
docker-compose logs whatsapp-web-service

# Reiniciar serviços
docker-compose restart whatsapp-service whatsapp-web-service
```

#### 3. Mensagens não chegam
```bash
# Verificar webhook
curl -X POST http://localhost:5006/api/whatsapp/webhook/message \
  -H "Content-Type: application/json" \
  -d '{
    "messageId": "test",
    "fromNumber": "+5511999999999",
    "toNumber": "+5511888888888",
    "messageType": "text",
    "content": "test"
  }'
```

### Logs

```bash
# Logs do serviço C#
docker-compose logs whatsapp-service

# Logs do serviço Node.js
docker-compose logs whatsapp-web-service

# Logs em tempo real
docker-compose logs -f whatsapp-service whatsapp-web-service
```

## 🔒 Segurança

### Considerações

1. **QR Code**: Mantenha o QR Code seguro
2. **Sessões**: Monitore sessões ativas
3. **Dados**: Faça backup regular do banco
4. **Logs**: Revise logs periodicamente

### Boas Práticas

- Use números dedicados para o sistema
- Monitore mensagens recebidas
- Configure alertas para falhas
- Mantenha backups atualizados

## 📈 Monitoramento

### Métricas Disponíveis

- Sessões ativas
- Mensagens enviadas/recebidas
- Taxa de erro
- Tempo de resposta

### Dashboards

- **Grafana**: http://localhost:3002
- **Prometheus**: http://localhost:9090
- **Seq Logs**: http://localhost:5341

## 🔄 Migração para APIs Oficiais

Para produção, considere migrar para:

1. **Twilio WhatsApp API**
2. **Meta Cloud API**
3. **WhatsApp Business API**

### Vantagens das APIs Oficiais

- ✅ Estabilidade
- ✅ Suporte oficial
- ✅ Recursos avançados
- ✅ Conformidade

### Desvantagens

- ❌ Custos
- ❌ Limitações de uso
- ❌ Aprovação necessária

## 📝 Próximos Passos

1. **Testes**: Implementar testes automatizados
2. **UI**: Interface web para gerenciamento
3. **Analytics**: Análise de conversas
4. **Integração**: Conectar com outros sistemas
5. **Escalabilidade**: Múltiplas sessões
6. **Backup**: Sistema de backup automático

## 🤝 Suporte

Para dúvidas ou problemas:

1. Verifique os logs
2. Teste os serviços
3. Consulte a documentação
4. Abra uma issue no repositório

---

**⚠️ Importante**: Esta solução é para desenvolvimento e prototipagem. Para produção, considere usar APIs oficiais do WhatsApp. 