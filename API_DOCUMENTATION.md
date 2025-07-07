# Documentação da API - Sistema Lach

## Visão Geral

O Sistema Lach é composto por microsserviços que se comunicam através de um API Gateway. Todas as requisições devem ser feitas através do gateway na porta 5000.

**Base URL:** `http://localhost:5000`

## Autenticação

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

### Registro
```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "João Silva",
  "email": "joao@example.com",
  "phone": "+5511999999999",
  "password": "password123",
  "role": "Customer"
}
```

### Validar Token
```http
POST /api/auth/validate
Content-Type: application/json

"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

## Produtos

### Listar Todos os Produtos
```http
GET /api/products
```

### Listar Produtos Disponíveis
```http
GET /api/products/available
```

### Buscar Produtos por Categoria
```http
GET /api/products/category/{category}
```

### Buscar Produtos Especiais
```http
GET /api/products/special
```

### Buscar Produto por ID
```http
GET /api/products/{id}
```

### Criar Produto
```http
POST /api/products
Content-Type: application/json

{
  "name": "X-Burger",
  "description": "Hambúrguer com queijo e salada",
  "price": 15.90,
  "category": "Lanches",
  "imageUrl": "https://example.com/xburger.jpg",
  "isSpecial": false
}
```

### Atualizar Produto
```http
PUT /api/products/{id}
Content-Type: application/json

{
  "name": "X-Burger Especial",
  "price": 18.90,
  "isSpecial": true
}
```

### Deletar Produto
```http
DELETE /api/products/{id}
```

## Pedidos

### Criar Pedido
```http
POST /api/orders
Content-Type: application/json

{
  "items": [
    {
      "productId": "123e4567-e89b-12d3-a456-426614174000",
      "quantity": 2,
      "specialInstructions": "Sem cebola"
    }
  ],
  "deliveryAddress": "Rua das Flores, 123 - São Paulo, SP",
  "deliveryInstructions": "Entregar no portão"
}
```

### Buscar Pedido por ID
```http
GET /api/orders/{id}
```

### Listar Pedidos do Cliente
```http
GET /api/orders/customer/{customerId}
```

### Listar Todos os Pedidos
```http
GET /api/orders
```

### Listar Pedidos por Status
```http
GET /api/orders/status/{status}
```

### Atualizar Status do Pedido
```http
PUT /api/orders/{id}/status
Content-Type: application/json

{
  "status": "InProduction",
  "notes": "Pedido em preparação"
}
```

### Aceitar Pedido
```http
POST /api/orders/{id}/accept
```

### Rejeitar Pedido
```http
POST /api/orders/{id}/reject
Content-Type: application/json

"Produto indisponível"
```

### Cancelar Pedido
```http
POST /api/orders/{id}/cancel
Content-Type: application/json

"Cliente solicitou cancelamento"
```

## Status dos Pedidos

- `Pending` - Pendente
- `Accepted` - Aceito
- `InProduction` - Em Produção
- `ReadyForDelivery` - Pronto para Entrega
- `OutForDelivery` - Saiu para Entrega
- `Delivered` - Entregue
- `Cancelled` - Cancelado

## Códigos de Resposta

- `200` - Sucesso
- `201` - Criado com sucesso
- `400` - Requisição inválida
- `401` - Não autorizado
- `404` - Não encontrado
- `500` - Erro interno do servidor

## Fila de Produção

### Listar Fila de Produção
```http
GET /api/productionqueue
```

### Listar Fila Ativa
```http
GET /api/productionqueue/active
```

### Buscar Item da Fila por Pedido
```http
GET /api/productionqueue/order/{orderId}
```

### Adicionar Pedido à Fila
```http
POST /api/productionqueue/order/{orderId}/add
Content-Type: application/json

{
  "customerName": "João Silva",
  "items": [
    {
      "productId": "123e4567-e89b-12d3-a456-426614174000",
      "productName": "X-Burger",
      "quantity": 2,
      "price": 15.90
    }
  ]
}
```

### Atualizar Status do Item
```http
PUT /api/productionqueue/order/{orderId}/status
Content-Type: application/json

{
  "status": "InProgress",
  "notes": "Produção iniciada"
}
```

### Mover Item na Fila
```http
PUT /api/productionqueue/order/{orderId}/position/{newPosition}
```

### Iniciar Produção
```http
POST /api/productionqueue/order/{orderId}/start
```

### Concluir Produção
```http
POST /api/productionqueue/order/{orderId}/complete
```

### Remover da Fila
```http
DELETE /api/productionqueue/order/{orderId}
```

## Entrega

### Calcular Taxa de Entrega
```http
POST /api/delivery/calculate
Content-Type: application/json

{
  "deliveryAddress": "Rua das Flores, 123 - São Paulo, SP",
  "orderSubtotal": 45.90
}
```

### Verificar Disponibilidade de Entrega
```http
GET /api/delivery/available?address=Rua das Flores, 123
```

### Calcular Distância
```http
GET /api/delivery/distance?address=Rua das Flores, 123
```

### Obter Localização do Restaurante
```http
GET /api/delivery/location
```

### Atualizar Localização do Restaurante
```http
PUT /api/delivery/location
Content-Type: application/json

{
  "name": "Lanchonete Lach",
  "address": "Rua das Flores, 123 - São Paulo, SP",
  "latitude": -23.5505,
  "longitude": -46.6333
}
```

### Obter Configuração de Entrega
```http
GET /api/delivery/config
```

### Atualizar Configuração de Entrega
```http
PUT /api/delivery/config
Content-Type: application/json

{
  "baseFee": 5.00,
  "feePerKm": 2.00,
  "maxDistanceKm": 15.0,
  "freeDeliveryThreshold": 50.00
}
```

## WhatsApp

### Enviar Mensagem
```http
POST /api/whatsapp/send
Content-Type: application/json

{
  "phoneNumber": "+5511999999999",
  "message": "Olá! Seu pedido foi aceito."
}
```

### Enviar Atualização de Status
```http
POST /api/whatsapp/order-status
Content-Type: application/json

{
  "phoneNumber": "+5511999999999",
  "orderId": "123e4567-e89b-12d3-a456-426614174000",
  "status": "InProduction"
}
```

### Obter Sessão
```http
GET /api/whatsapp/session/{phoneNumber}
```

### Atualizar Estado da Sessão
```http
PUT /api/whatsapp/session/{phoneNumber}/state
Content-Type: application/json

{
  "state": "Ordering"
}
```

### Obter Histórico de Mensagens
```http
GET /api/whatsapp/messages/{phoneNumber}
```

### Processar Mensagem Recebida
```http
POST /api/whatsapp/process-message
Content-Type: application/json

{
  "phoneNumber": "+5511999999999",
  "message": "Quero fazer um pedido"
}
```

### Webhook do WhatsApp
```http
POST /api/whatsapp/webhook
Content-Type: application/json

{
  "object": "whatsapp_business_account",
  "entry": [...]
}
```

## Notificações

### Enviar Notificação
```http
POST /api/notification/send
Content-Type: application/json

{
  "type": "OrderStatusChanged",
  "title": "Pedido Atualizado",
  "message": "Seu pedido foi aceito!",
  "recipient": "user@example.com",
  "channel": "Email",
  "parameters": {
    "orderId": "123e4567-e89b-12d3-a456-426614174000",
    "status": "Accepted"
  }
}
```

### Enviar Notificação de Status do Pedido
```http
POST /api/notification/order-status
Content-Type: application/json

{
  "orderId": "123e4567-e89b-12d3-a456-426614174000",
  "status": "InProduction",
  "recipient": "user@example.com"
}
```

### Enviar Notificação de Pedido Criado
```http
POST /api/notification/order-created
Content-Type: application/json

{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "customerEmail": "user@example.com",
  "totalAmount": 45.90,
  "items": [...]
}
```

### Enviar Notificação de Pagamento
```http
POST /api/notification/payment
Content-Type: application/json

{
  "orderId": "123e4567-e89b-12d3-a456-426614174000",
  "isSuccess": true,
  "recipient": "user@example.com"
}
```

### Obter Notificações por Destinatário
```http
GET /api/notification/recipient/{recipient}
```

### Marcar como Lida
```http
PUT /api/notification/{notificationId}/read
```

### Marcar como Entregue
```http
PUT /api/notification/{notificationId}/delivered
```

### Obter Notificações Pendentes
```http
GET /api/notification/pending
```

### Deletar Notificação
```http
DELETE /api/notification/{notificationId}
```

## Exemplos de Uso

### Fluxo Completo de Pedido

1. **Login do Cliente**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"cliente@example.com","password":"123456"}'
```

2. **Listar Produtos Disponíveis**
```bash
curl http://localhost:5000/api/products/available
```

3. **Criar Pedido**
```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "items": [
      {
        "productId": "123e4567-e89b-12d3-a456-426614174000",
        "quantity": 1
      }
    ],
    "deliveryAddress": "Rua das Flores, 123 - São Paulo, SP"
  }'
```

4. **Acompanhar Status do Pedido**
```bash
curl http://localhost:5000/api/orders/{orderId}
```

## Mensageria

O sistema utiliza RabbitMQ para comunicação entre microsserviços. Os eventos principais são:

- `order.created` - Pedido criado
- `order.status.updated` - Status do pedido atualizado
- `order.accepted` - Pedido aceito
- `order.cancelled` - Pedido cancelado
- `order.rejected` - Pedido rejeitado

## Monitoramento

- **RabbitMQ Management:** http://localhost:15672
- **Logs dos Containers:** `docker-compose logs -f`
- **Health Check:** `docker-compose ps` 