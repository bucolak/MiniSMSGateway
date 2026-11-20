# Mini SMS Gateway

A small SMS gateway API built with **.NET 10**, **PostgreSQL** and **RabbitMQ**. Clients send SMS requests with an API key; messages are stored, queued and delivered asynchronously to an external SMS provider, with retries, a dead-letter queue and a circuit breaker to protect against provider outages.

Built to learn message-queue based, fault-tolerant backend design. The `to` field accepts either a single number or a list.

## Architecture

```
Client ──(Api-Key)──▶ Minimal API ──▶ PostgreSQL (users, messages)
                          │
                          ▼
                   RabbitMQ  sms-queue ──▶ SmsDispatcherService (BackgroundService)
                          ▲                        │
        sms-queue-retry ──┘ (TTL 15 s)             ├─ success ──▶ status = Sent
                                                   ├─ failure ──▶ retry queue (RetryCount++)
                                                   └─ 3 failures ─▶ dead-letter exchange ──▶ sms-queue-failed
                                                   │
                                                   ▼
                                       External SMS provider (HTTP, circuit breaker)
```

- **.NET Aspire AppHost** starts PostgreSQL in a container (with a data volume) and wires the connection string into the API.
- **EF Core migrations** run automatically at startup.
- **Message lifecycle:** `Pending → Processing → Sent / Failed`, with `RetryCount` saved for every message.
- **Retry with delay:** failed messages go to a retry queue with a 15-second TTL, then dead-letter back into the main queue.
- **Dead-letter queue:** after 3 attempts the message is rejected and ends up in `sms-queue-failed` for inspection.
- **Circuit breaker:** `Microsoft.Extensions.Http.Resilience` stops calling the provider for 15 s when at least 50% of requests fail within a 30-second window.

## API

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/user` | Create a user and generate an API key |
| `GET` | `/api/users` | List users |
| `POST` | `/api/sms/send` | Send an SMS to one or more numbers (header: `Api-Key`) |
| `POST` | `/api/sms/send-bulk` | Send different messages to different numbers in one call (header: `Api-Key`) |
| `GET` | `/api/sms/messages` | List the caller's messages (header: `Api-Key`) |
| `GET` | `/api/sms/status/{id}` | Get the delivery status of a message |

Example:

```bash
curl -X POST http://localhost:5000/api/sms/send \
  -H "Api-Key: <your-api-key>" \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello!", "to": ["905xxxxxxxxx"]}'
```

## Tech Stack

C# · .NET 10 (Minimal API) · .NET Aspire · Entity Framework Core · PostgreSQL · RabbitMQ · Polly-based resilience (circuit breaker) · Docker

## Running Locally

Requirements: .NET 10 SDK, Docker, and a RabbitMQ instance on `localhost`.

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
dotnet run --project BeginningGateway.AppHost
```

Add your SMS provider credentials with user secrets instead of `appsettings.json`:

```bash
cd Sms
dotnet user-secrets set "SmsProvider:Username" "<username>"
dotnet user-secrets set "SmsProvider:Password" "<password>"
```

## What I Learned

- Designing asynchronous processing with queues instead of calling slow external services inside the request
- Retry, dead-letter and circuit-breaker patterns for unreliable third-party APIs
- Orchestrating local infrastructure with .NET Aspire

## Author

**Buket Çolak** — [github.com/bucolak](https://github.com/bucolak)
