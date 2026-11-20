# Mini SMS Gateway

A small SMS gateway API built with **.NET 10**, **PostgreSQL**, **RabbitMQ** and **MassTransit**. Clients send SMS requests with an API key; messages are stored, queued and delivered asynchronously to an external SMS provider, with retries, dead-letter queues, a replay API for failed messages and a circuit breaker to protect against provider outages.

Built to learn message-queue based, fault-tolerant backend design. The project contains two queue pipelines side by side — one written directly with `RabbitMQ.Client` and one with **MassTransit** — to compare the low-level and framework approaches.

## Architecture

```
Client ──(Api-Key)──▶ Minimal API ──▶ PostgreSQL (users, messages)
                          │
          ┌───────────────┴────────────────┐
          ▼                                ▼
  RabbitMQ.Client pipeline           MassTransit pipeline
  sms-queue                          sms-mt-queue
    │                                  │
    ▼                                  ▼
  SmsDispatcherService               SmsConsumer (IConsumer)
  (BackgroundService)                  ├─ retry: 3 × 15 s (UseMessageRetry)
    ├─ retry queue (TTL 15 s)          └─ failures ─▶ sms-mt-queue_error
    └─ 3 failures ─▶ sms-queue-failed (DLX)
          │                                │
          └──────────────┬─────────────────┘
                         ▼
          SMS provider (Maradit) over HTTP, Polly circuit breaker
```

- **.NET Aspire AppHost** starts PostgreSQL in a container (with a data volume) and wires the connection string into the API.
- **EF Core migrations** run automatically at startup.
- **Message lifecycle:** `Pending → Processing → Sent / Failed`, with `RetryCount` saved for every message.
- **Raw RabbitMQ pipeline:** failed messages go to a retry queue with a 15-second TTL, then dead-letter back into the main queue; after 3 attempts they end up in `sms-queue-failed`.
- **MassTransit pipeline:** the consumer retries 3 times at 15-second intervals; messages that still fail are moved to MassTransit's `_error` queue.
- **Replay:** failed messages in either dead-letter queue can be reset and sent back to the main queue with one API call.
- **Circuit breaker:** `Microsoft.Extensions.Http.Resilience` (Polly) stops calling the provider for 15 s when at least 50% of requests fail within a 30-second window.
- **Provider selection:** each message stores a `ProviderType` (`Maradit` implemented, `Turkcell` reserved) so new SMS operators can be added.

## API

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/user` | Create a user and generate an API key |
| `GET` | `/api/users` | List users |
| `POST` | `/api/sms/raw/send` | Send one message to one or more numbers via the raw RabbitMQ pipeline |
| `POST` | `/api/sms/raw/send-bulk` | Send different messages to different numbers via the raw RabbitMQ pipeline |
| `POST` | `/api/sms/masstransit/send-bulk` | Send different messages to different numbers via MassTransit |
| `POST` | `/api/sms/raw/failed/replay` | Replay messages from the raw dead-letter queue |
| `POST` | `/api/sms/masstransit/failed/replay` | Replay messages from the MassTransit `_error` queue |
| `GET` | `/api/sms/messages` | List the caller's messages |
| `GET` | `/api/sms/status/{id}` | Get the delivery status of a message |

All `/api/sms/*` endpoints except `status` require the `Api-Key` header.

Example (`providerType`: `0` = Maradit):

```bash
curl -X POST http://localhost:5000/api/sms/raw/send \
  -H "Api-Key: <your-api-key>" \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello!", "to": ["905xxxxxxxxx"], "providerType": 0}'
```

## Tech Stack

C# · .NET 10 (Minimal API) · .NET Aspire · Entity Framework Core · PostgreSQL · RabbitMQ · MassTransit · Polly-based resilience (circuit breaker) · Docker

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
- Retry, dead-letter, replay and circuit-breaker patterns for unreliable third-party APIs
- The difference between using RabbitMQ directly and through a framework like MassTransit
- Orchestrating local infrastructure with .NET Aspire

## Author

**Buket Çolak** — [github.com/bucolak](https://github.com/bucolak)
