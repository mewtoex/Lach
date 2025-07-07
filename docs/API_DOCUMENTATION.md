# 📚 Documentação da API - Lach Snack Bar

## 📋 Índice

1. [Visão Geral](#visão-geral)
2. [Autenticação](#autenticação)
3. [Endpoints](#endpoints)
   - [Auth Service](#auth-service)
   - [Product Service](#product-service)
   - [Order Service](#order-service)
   - [Production Queue Service](#production-queue-service)
   - [Delivery Service](#delivery-service)
   - [WhatsApp Service](#whatsapp-service)
   - [Notification Service](#notification-service)
4. [Modelos de Dados](#modelos-de-dados)
5. [Códigos de Erro](#códigos-de-erro)
6. [Exemplos de Uso](#exemplos-de-uso)

## 🌟 Visão Geral

O sistema Lach Snack Bar é uma plataforma de microserviços para gerenciamento de pedidos de lanchonete. A API é baseada em REST e utiliza JSON para comunicação.

**Base URL:** `http://localhost:8080` (API Gateway)

**Versão da API:** v1

**Formato de Resposta:** JSON

## 🔐 Autenticação

O sistema utiliza JWT (JSON Web Tokens) para autenticação.

### Headers Necessários

```http
Authorization: Bearer <jwt_token>
Content-Type: application/json
```

### Tipos de Usuário

- **Admin:** Acesso completo ao sistema
- **Staff:** Acesso limitado (produção, delivery)
- **Customer:** Acesso apenas aos próprios pedidos

## 🚀 Endpoints

### Auth Service

#### POST /api/auth/register
Registra um novo usuário.

**Request Body:**
```json
{
  "username": "joao.silva",
  "email": "joao@email.com",
  "password": "senha123",
  "firstName": "João",
  "lastName": "Silva",
  "phoneNumber": "+5511999999999",
  "role": "Customer"
}
```

**Response (201):**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "username": "joao.silva",
  "email": "joao@email.com",
  "firstName": "João",
  "lastName": "Silva",
  "role": "Customer",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

#### POST /api/auth/login
Autentica um usuário.

**Request Body:**
```json
{
  "username": "joao.silva",
  "password": "senha123"
}
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresIn": 3600,
  "user": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "username": "joao.silva",
    "email": "joao@email.com",
    "role": "Customer"
  }
}
```

### Product Service

#### GET /api/products
Lista todos os produtos disponíveis.

**Query Parameters:**
- `category` (opcional): Filtrar por categoria
- `available` (opcional): Filtrar por disponibilidade
- `page` (opcional): Número da página (padrão: 1)
- `pageSize` (opcional): Tamanho da página (padrão: 20)

**Response (200):**
```json
{
  "items": [
    {
      "id": "11111111-1111-1111-1111-111111111111",
      "name": "Açaí Tradicional",
      "description": "Açaí puro com granola e banana",
      "price": 12.90,
      "category": "Açaí",
      "isAvailable": true,
      "imageUrl": "https://example.com/acai.jpg",
      "preparationTimeMinutes": 5,
      "hasAddOns": true,
      "addOnsCount": 4,
      "createdAt": "2024-01-15T10:30:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

#### GET /api/products/{id}
Obtém detalhes de um produto específico.

**Response (200):**
```json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "name": "Açaí Tradicional",
  "description": "Açaí puro com granola e banana",
  "price": 12.90,
  "category": "Açaí",
  "isAvailable": true,
  "imageUrl": "https://example.com/acai.jpg",
  "preparationTimeMinutes": 5,
  "hasAddOns": true,
  "addOns": [
    {
      "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "name": "Banana",
      "description": "Banana fatiada",
      "price": 2.00,
      "category": "Frutas",
      "isAvailable": true,
      "imageUrl": "https://example.com/banana.jpg",
      "maxQuantity": 3,
      "createdAt": "2024-01-15T10:30:00Z"
    },
    {
      "id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
      "name": "Granola",
      "description": "Granola crocante",
      "price": 1.50,
      "category": "Granola",
      "isAvailable": true,
      "imageUrl": "https://example.com/granola.jpg",
      "maxQuantity": 2,
      "createdAt": "2024-01-15T10:30:00Z"
    }
  ],
  "createdAt": "2024-01-15T10:30:00Z"
}
```

#### POST /api/products (Admin/Staff)
Cria um novo produto.

**Request Body:**
```json
{
  "name": "Açaí Especial",
  "description": "Açaí com frutas da estação",
  "price": 15.90,
  "category": "Açaí",
  "imageUrl": "https://example.com/acai-especial.jpg",
  "preparationTimeMinutes": 7,
  "hasAddOns": true,
  "addOns": [
    {
      "name": "Manga",
      "description": "Manga madura",
      "price": 2.50,
      "category": "Frutas",
      "imageUrl": "https://example.com/manga.jpg",
      "maxQuantity": 2
    }
  ]
}
```

### Order Service

#### POST /api/orders
Cria um novo pedido.

**Request Body:**
```json
{
  "customerId": "123e4567-e89b-12d3-a456-426614174000",
  "items": [
    {
      "productId": "11111111-1111-1111-1111-111111111111",
      "quantity": 2,
      "specialInstructions": "Sem granola",
      "addOns": [
        {
          "addOnId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
          "quantity": 1
        },
        {
          "addOnId": "cccccccc-cccc-cccc-cccc-cccccccccccc",
          "quantity": 2
        }
      ]
    }
  ],
  "deliveryAddress": {
    "street": "Rua das Flores, 123",
    "city": "São Paulo",
    "state": "SP",
    "zipCode": "01234-567",
    "complement": "Apto 45"
  },
  "paymentMethod": "CreditCard",
  "deliveryInstructions": "Entregar no portão"
}
```

**Response (201):**
```json
{
  "id": "order-12345",
  "customerId": "123e4567-e89b-12d3-a456-426614174000",
  "status": "Pending",
  "totalAmount": 32.80,
  "items": [
    {
      "id": "item-123",
      "productId": "11111111-1111-1111-1111-111111111111",
      "productName": "Açaí Tradicional",
      "quantity": 2,
      "unitPrice": 12.90,
      "totalPrice": 25.80,
      "specialInstructions": "Sem granola",
      "addOns": [
        {
          "id": "addon-123",
          "addOnId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
          "addOnName": "Banana",
          "addOnCategory": "Frutas",
          "quantity": 1,
          "unitPrice": 2.00,
          "totalPrice": 2.00
        }
      ]
    }
  ],
  "deliveryAddress": {
    "street": "Rua das Flores, 123",
    "city": "São Paulo",
    "state": "SP",
    "zipCode": "01234-567",
    "complement": "Apto 45"
  },
  "paymentMethod": "CreditCard",
  "deliveryInstructions": "Entregar no portão",
  "estimatedDeliveryTime": "2024-01-15T11:30:00Z",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

#### GET /api/orders/{id}
Obtém detalhes de um pedido específico.

#### GET /api/orders
Lista pedidos do usuário autenticado.

**Query Parameters:**
- `status` (opcional): Filtrar por status
- `page` (opcional): Número da página
- `pageSize` (opcional): Tamanho da página

#### PUT /api/orders/{id}/status (Admin/Staff)
Atualiza o status de um pedido.

**Request Body:**
```json
{
  "status": "InProduction",
  "notes": "Pedido em preparação"
}
```

### Production Queue Service

#### GET /api/production/queue
Lista a fila de produção (Admin/Staff).

#### POST /api/production/queue/{orderId}/start
Inicia a produção de um pedido (Admin/Staff).

#### POST /api/production/queue/{orderId}/complete
Marca um pedido como concluído (Admin/Staff).

### Delivery Service

#### GET /api/delivery/orders
Lista pedidos para entrega (Admin/Staff).

#### POST /api/delivery/orders/{orderId}/assign
Atribui um pedido a um entregador (Admin/Staff).

#### POST /api/delivery/orders/{orderId}/pickup
Marca pedido como retirado (Admin/Staff).

#### POST /api/delivery/orders/{orderId}/deliver
Marca pedido como entregue (Admin/Staff).

### WhatsApp Service

#### POST /api/whatsapp/sessions
Cria uma nova sessão do WhatsApp.

**Request Body:**
```json
{
  "phoneNumber": "+5511999999999"
}
```

#### GET /api/whatsapp/sessions/{sessionId}/qr
Obtém o QR code para uma sessão.

#### GET /api/whatsapp/sessions/{sessionId}/status
Verifica o status de uma sessão.

#### POST /api/whatsapp/messages
Envia uma mensagem via WhatsApp.

**Request Body:**
```json
{
  "toNumber": "+5511999999999",
  "content": "Seu pedido está pronto!",
  "sessionId": "session-123"
}
```

#### POST /api/whatsapp/webhook
Webhook para receber mensagens do WhatsApp.

### Notification Service

#### POST /api/notifications/send
Envia uma notificação.

**Request Body:**
```json
{
  "type": "OrderStatusUpdate",
  "recipientId": "123e4567-e89b-12d3-a456-426614174000",
  "title": "Pedido Atualizado",
  "message": "Seu pedido está em produção",
  "data": {
    "orderId": "order-12345",
    "status": "InProduction"
  }
}
```

## 📊 Modelos de Dados

### Status do Pedido
- `Pending`: Pendente
- `Confirmed`: Confirmado
- `InProduction`: Em Produção
- `Ready`: Pronto
- `OutForDelivery`: Saiu para Entrega
- `Delivered`: Entregue
- `Cancelled`: Cancelado

### Método de Pagamento
- `Cash`: Dinheiro
- `CreditCard`: Cartão de Crédito
- `DebitCard`: Cartão de Débito
- `Pix`: PIX
- `DigitalWallet`: Carteira Digital

### Categorias de Produtos
- `Açaí`: Produtos de açaí
- `Lanches`: Sanduíches e hambúrgueres
- `Bebidas`: Refrigerantes e sucos
- `Sobremesas`: Doces e sobremesas

### Categorias de Adicionais
- `Frutas`: Frutas frescas
- `Granola`: Granolas e cereais
- `Adoçantes`: Açúcares e mel
- `Carnes`: Carnes e proteínas
- `Queijos`: Queijos diversos
- `Molhos`: Molhos especiais

## ❌ Códigos de Erro

### 4xx - Erros do Cliente
- `400 Bad Request`: Dados inválidos
- `401 Unauthorized`: Não autenticado
- `403 Forbidden`: Sem permissão
- `404 Not Found`: Recurso não encontrado
- `409 Conflict`: Conflito de dados
- `422 Unprocessable Entity`: Dados não processáveis

### 5xx - Erros do Servidor
- `500 Internal Server Error`: Erro interno
- `502 Bad Gateway`: Erro de gateway
- `503 Service Unavailable`: Serviço indisponível

### Exemplo de Resposta de Erro
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Dados inválidos",
    "details": [
      {
        "field": "email",
        "message": "Email inválido"
      }
    ]
  },
  "timestamp": "2024-01-15T10:30:00Z",
  "requestId": "req-12345"
}
```

## 💡 Exemplos de Uso

### Fluxo Completo de Pedido

1. **Registrar usuário:**
```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "joao.silva",
    "email": "joao@email.com",
    "password": "senha123",
    "firstName": "João",
    "lastName": "Silva",
    "phoneNumber": "+5511999999999"
  }'
```

2. **Fazer login:**
```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "joao.silva",
    "password": "senha123"
  }'
```

3. **Listar produtos:**
```bash
curl -X GET http://localhost:8080/api/products \
  -H "Authorization: Bearer <token>"
```

4. **Criar pedido:**
```bash
curl -X POST http://localhost:8080/api/orders \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "items": [
      {
        "productId": "11111111-1111-1111-1111-111111111111",
        "quantity": 1,
        "addOns": [
          {
            "addOnId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
            "quantity": 1
          }
        ]
      }
    ],
    "deliveryAddress": {
      "street": "Rua das Flores, 123",
      "city": "São Paulo",
      "state": "SP",
      "zipCode": "01234-567"
    },
    "paymentMethod": "Pix"
  }'
```

5. **Acompanhar pedido:**
```bash
curl -X GET http://localhost:8080/api/orders/order-12345 \
  -H "Authorization: Bearer <token>"
```

### Integração com WhatsApp

1. **Criar sessão:**
```bash
curl -X POST http://localhost:8080/api/whatsapp/sessions \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+5511999999999"
  }'
```

2. **Obter QR Code:**
```bash
curl -X GET http://localhost:8080/api/whatsapp/sessions/session-123/qr \
  -H "Authorization: Bearer <token>"
```

3. **Enviar mensagem:**
```bash
curl -X POST http://localhost:8080/api/whatsapp/messages \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "toNumber": "+5511999999999",
    "content": "Seu pedido está pronto!",
    "sessionId": "session-123"
  }'
```

## 🔧 Configuração

### Variáveis de Ambiente

```bash
# Database
ConnectionStrings__DefaultConnection=Host=localhost;Database=lach;Username=postgres;Password=postgres

# RabbitMQ
RabbitMQ__Host=localhost
RabbitMQ__Username=admin
RabbitMQ__Password=admin

# JWT
Jwt__SecretKey=your-secret-key-here
Jwt__Issuer=lach-snack-bar
Jwt__Audience=lach-customers

# WhatsApp
WhatsApp__WebhookUrl=http://localhost:8080/api/whatsapp/webhook
WhatsApp__ApiKey=your-api-key

# Monitoring
Serilog__MinimumLevel__Default=Information
Prometheus__MetricsPort=9090
```

### Rate Limiting

- **Auth endpoints:** 5 requests/minute
- **Product endpoints:** 100 requests/minute
- **Order endpoints:** 20 requests/minute
- **WhatsApp endpoints:** 10 requests/minute

### Timeouts

- **Request timeout:** 30 segundos
- **Database timeout:** 10 segundos
- **External API timeout:** 15 segundos

## 📞 Suporte

Para suporte técnico ou dúvidas sobre a API:

- **Email:** suporte@lach-snackbar.com
- **Documentação:** https://docs.lach-snackbar.com
- **Status:** https://status.lach-snackbar.com 