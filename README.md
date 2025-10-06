# GeoBlocker - Enterprise Microservices System

> A production-ready geo-blocking solution built with .NET 9, demonstrating Clean Architecture, Domain-Driven Design, CQRS, and Event-Driven Microservices patterns.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![MassTransit](https://img.shields.io/badge/MassTransit-8.2-blue)](https://masstransit.io/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.12-FF6600?logo=rabbitmq)](https://www.rabbitmq.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Quick Start](#quick-start)
- [API Documentation](#api-documentation)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Design Patterns](#design-patterns)
- [Configuration](#configuration)
- [Development](#development)
- [Testing](#testing)
- [Deployment](#deployment)
- [Monitoring](#monitoring)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)

---

## Overview

GeoBlocker is an enterprise-grade microservices system that provides IP-based geo-blocking capabilities. The system validates IP addresses against a configurable list of blocked countries and maintains comprehensive audit logs of all access attempts.

### Key Features

- **Real-time IP Validation**: Instant geo-location lookup and blocking decisions
- **Flexible Blocking Rules**: Permanent or temporary country-level blocks
- **Comprehensive Logging**: Full audit trail of all access attempts
- **High Performance**: Caching layer with 60-minute TTL for geo-location data
- **Fault Tolerant**: Automatic retry with fallback geo-IP providers
- **Scalable Design**: Stateless services with event-driven communication
- **API Gateway**: Centralized routing with rate limiting

### Use Cases

- API endpoint protection against traffic from specific countries
- Compliance with geo-restrictions and data sovereignty requirements
- Security enhancement by blocking high-risk regions
- Traffic analysis and threat intelligence gathering

---

## Architecture

### System Overview

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │
       ▼
┌─────────────────────────────────────────────┐
│           API Gateway (Ocelot)              │
│     • Routing • Rate Limiting • CORS        │
└─────┬─────────────┬──────────────┬──────────┘
      │             │              │
      ▼             ▼              ▼
┌─────────┐   ┌──────────┐   ┌──────────┐
│ Country │   │    IP    │   │   Log    │
│ Service │   │ Service  │   │ Service  │
└────┬────┘   └────┬─────┘   └────┬─────┘
     │             │              │
     └─────────────┴──────────────┘
                   │
                   ▼
         ┌─────────────────┐
         │    RabbitMQ     │
         │  Message Broker │
         └─────────────────┘
```

### Service Responsibilities

| Service | Responsibility | Technology |
|---------|---------------|------------|
| **Country Service** | Manage blocked countries | .NET 9 Web API |
| **IP Service** | Validate IPs & geo-lookup | .NET 9 Web API + External APIs |
| **Log Service** | Store access audit logs | .NET 9 Web API + Event Consumer |
| **API Gateway** | Route & secure requests | Ocelot |

### Communication Flow

```
1. Block Country (Permanent/Temporary)
   CountryService → MediatR → EventHandler → MassTransit → RabbitMQ

2. Validate IP
   IPService → GeoIP Lookup → Check Blocked → Publish Event → RabbitMQ

3. Log Access Attempt
   RabbitMQ → LogService Consumer → Repository → In-Memory Storage
```

---

## Quick Start

### Prerequisites

Ensure you have the following installed:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

### Installation

```bash
# Clone repository
git clone https://github.com/yourusername/geoblocker.git
cd geoblocker

# Start all services with Docker Compose
docker-compose up --build

# Or start individual services for development
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.12-management-alpine

# Run services in separate terminals
dotnet run --project src/Services/CountryService/Country.API
dotnet run --project src/Services/IPService/IP.API
dotnet run --project src/Services/LogService/Log.API
dotnet run --project src/Services/APIGateway/Gateway.API
```

### Access Points

| Service | URL | Description |
|---------|-----|-------------|
| API Gateway | http://localhost:5000 | Main entry point |
| Country Service | https://localhost:7298/swagger | Swagger documentation |
| IP Service | https://localhost:54202/swagger | Swagger documentation |
| Log Service | https://localhost:54206/swagger | Swagger documentation |
| RabbitMQ UI | http://localhost:15672 | Management console (guest/guest) |

---

## API Documentation

### Country Service

#### Block Country (Permanent)

```http
POST /api/countries/block
Content-Type: application/json

{
  "countryCode": "RU",
  "countryName": "Russia"
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "countryCode": "RU",
  "countryName": "Russia",
  "blockedAt": "2025-10-06T10:30:00Z",
  "success": true
}
```

#### Block Country (Temporary)

```http
POST /api/countries/block-temporary
Content-Type: application/json

{
  "countryCode": "CN",
  "countryName": "China",
  "durationMinutes": 30
}
```

#### Get Blocked Countries

```http
GET /api/countries?pageNumber=1&pageSize=20&searchTerm=Russia
```

#### Unblock Country

```http
DELETE /api/countries/RU
```

### IP Service

#### Validate IP Address

```http
POST /api/ip/validate
Content-Type: application/json

{
  "ipAddress": "85.140.1.1",
}
```

**Response (Blocked):**
```json
{
  "ipAddress": "85.140.1.1",
  "countryCode": "RU",
  "countryName": "Russia",
  "isBlocked": true,
  "blockReason": "Country RU is blocked",
  "validatedAt": "2025-10-06T10:40:00Z"
}
```

**Response (Allowed):**
```json
{
  "ipAddress": "8.8.8.8",
  "countryCode": "US",
  "countryName": "United States",
  "isBlocked": false,
  "blockReason": null,
  "validatedAt": "2025-10-06T10:41:00Z"
}
```

#### IP Geolocation Lookup

```http
GET /api/ip/lookup/8.8.8.8
```

**Response:**
```json
{
  "ipAddress": "8.8.8.8",
  "countryCode": "US",
  "countryName": "United States",
  "region": "California",
  "city": "Mountain View",
  "latitude": 37.386,
  "longitude": -122.0838,
  "isp": "Google LLC"
}
```

### Log Service

#### Get Access Logs

```http
GET /api/logs?pageNumber=1&pageSize=20&blockedOnly=true
```

```http
GET /api/logs/blocked
GET /api/logs/country/RU
GET /api/logs/ip/85.140.1.1
```

**Response:**
```json
{
  "items": [
    {
      "id": "9c8a7b6d-5e4f-3a2b-1c0d-9e8f7a6b5c4d",
      "ipAddress": "85.140.1.1",
      "countryCode": "RU",
      "countryName": "Russia",
      "isBlocked": true,
      "blockReason": "Country RU is blocked",
      "userAgent": "Mozilla/5.0",
      "timestamp": "2025-10-06T10:40:00Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1
}
```

---

## Technology Stack

### Core Technologies

- **.NET 9.0**: Latest LTS framework
- **C# 13**: Modern language features
- **ASP.NET Core**: Web API framework
- **MassTransit 8.2**: Distributed application framework
- **RabbitMQ 3.12**: Message broker
- **MediatR 12.2**: Mediator pattern implementation

### Libraries & Tools

- **FluentValidation**: Input validation
- **Serilog**: Structured logging
- **Ocelot**: API Gateway
- **Swashbuckle**: OpenAPI/Swagger documentation
- **Docker**: Containerization

### External Services

- **ip-api.com**: Free geo-IP lookup (primary)
- **ipgeolocation.io**: Paid geo-IP service (fallback)

---

## Project Structure

```
GeoBlockerSolution/
├── src/
│   ├── Services/
│   │   ├── CountryService/
│   │   │   ├── Country.API/              # Controllers, Program.cs
│   │   │   ├── Country.Application/      # CQRS, Event Handlers
│   │   │   ├── Country.Domain/           # Entities, Value Objects
│   │   │   └── Country.Infrastructure/   # Repositories
│   │   ├── IPService/
│   │   │   ├── IP.API/
│   │   │   ├── IP.Application/
│   │   │   ├── IP.Domain/
│   │   │   └── IP.Infrastructure/        # Geo-IP Providers
│   │   ├── LogService/
│   │   │   ├── Log.API/
│   │   │   ├── Log.Application/
│   │   │   ├── Log.Domain/
│   │   │   └── Log.Infrastructure/       # Message Consumers
│   │   └── APIGateway/
│   │       └── Gateway.API/              # Ocelot Configuration
├── docker-compose.yml
├── .dockerignore
├── .gitignore
├── GeoBlockerSolution.sln
└── README.md
```

### Layer Responsibilities

| Layer | Responsibility | Dependencies |
|-------|---------------|--------------|
| **API** | HTTP endpoints, DI configuration | Application, Infrastructure |
| **Application** | Use cases, CQRS, Event Handlers | Domain, MediatR, MassTransit |
| **Domain** | Business logic, Entities, Events | None (pure) |
| **Infrastructure** | External services, Data access | Domain, Application |

---

## Design Patterns

### 1. Clean Architecture

Dependency flow: **API → Application → Domain**

- Domain layer has zero dependencies
- Business rules isolated in domain entities
- Infrastructure details abstracted behind interfaces

### 2. CQRS (Command Query Responsibility Segregation)

**Commands** (Write):
- `BlockCountryCommand`
- `UnblockCountryCommand`
- `ValidateIpCommand`

**Queries** (Read):
- `GetBlockedCountriesQuery`
- `GetLogsQuery`
- `CheckIpBlockedQuery`

### 3. Domain-Driven Design

**Entities**: `BlockedCountry`, `IpValidationResult`, `AccessLog`

**Value Objects**: `CountryCode`, `IpAddress`, `UserAgent`

**Aggregates**: `BlockedCountry` (Aggregate Root)

**Domain Events**: `CountryBlockedEvent`, `BlockedIpAttemptEvent`

### 4. Strategy Pattern

Multiple geo-IP providers with automatic fallback:

```csharp
IpApiProvider → (fails) → IpGeolocationProvider
```

### 5. Decorator Pattern

Caching decorator wraps geo-IP providers for performance

### 6. Repository Pattern

Abstract data access with thread-safe in-memory storage

### 7. Mediator Pattern

MediatR coordinates between handlers without tight coupling

### 8. Observer Pattern

Domain events notify subscribers via MassTransit/RabbitMQ

---

## Configuration

### appsettings.json

**All Services:**
```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning"
      }
    }
  }
}
```

**IP Service Only:**
```json
{
  "GeoIpProviders": {
    "IpGeolocation": {
      "ApiKey": "707110b6f02b4df8828d0c18c69bf5a9"
    }
  }
}
```

### Environment Variables

```bash
# Docker Compose
IPGEOLOCATION_API_KEY=your_key_here

# Local Development
export IPGEOLOCATION_API_KEY=your_key_here  # Linux/Mac
set IPGEOLOCATION_API_KEY=your_key_here     # Windows
```

---

## Development

### Building

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests (when added)
dotnet test
```

### Running Services Individually

```bash
# Terminal 1: RabbitMQ
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.12-management-alpine

# Terminal 2: Country Service
cd src/Services/CountryService/Country.API
dotnet run --urls "http://localhost:5001"

# Terminal 3: IP Service
cd src/Services/IPService/IP.API
dotnet run --urls "http://localhost:5002"

# Terminal 4: Log Service
cd src/Services/LogService/Log.API
dotnet run --urls "http://localhost:5003"

# Terminal 5: API Gateway
cd src/Services/APIGateway/Gateway.API
dotnet run --urls "http://localhost:5000"
```

### Adding New Features

1. **Domain First**: Add entities/value objects to Domain layer
2. **Application Logic**: Create commands/queries in Application layer
3. **API Endpoints**: Add controllers in API layer
4. **Infrastructure**: Implement repositories/external services

---

## Testing

### Manual Testing

```bash
# Block a country
curl -X POST http://localhost:5000/api/countries/block \
  -H "Content-Type: application/json" \
  -d '{"countryCode":"RU","countryName":"Russia"}'

# Validate IP from blocked country
curl -X POST http://localhost:5000/api/ip/validate \
  -H "Content-Type: application/json" \
  -d '{"ipAddress":"85.140.1.1","userAgent":"Test"}'

# Check logs (wait 2-3 seconds for async processing)
curl http://localhost:5000/api/logs/blocked
```

### Test IPs by Country

| Country | IP Address |
|---------|-----------|
| Russia | 85.140.1.1 |
| China | 1.2.4.8 |
| USA | 8.8.8.8 |
| UK | 81.2.69.142 |
| Germany | 5.9.0.1 |

### Load Testing

```bash
# Using Apache Bench
ab -n 1000 -c 10 -p payload.json -T application/json \
  http://localhost:5000/api/ip/validate
```

---

## Deployment

### Docker Compose (Production)

```bash
# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Rebuild specific service
docker-compose up -d --build country-service
```

### Kubernetes (Future)

Deploy to Kubernetes cluster with Helm charts (planned enhancement)

---

## Monitoring

### RabbitMQ Management UI

Access: http://localhost:15672 (guest/guest)

Monitor:
- Queue depths
- Message rates
- Consumer activity
- Connection status

### Structured Logging

All services use Serilog with consistent format:

```
[10:30:45 INF] Country RU (Russia) blocked successfully
[10:30:46 WRN] Blocked IP 85.140.1.1 from country RU
[10:30:46 INF] Logged blocked attempt from IP 85.140.1.1 (RU)
```

### Health Checks (Planned)

Future enhancement: `/health` endpoints for monitoring

---

## Troubleshooting

### RabbitMQ Not Running

**Symptom**: `Connection refused` errors

**Solution**:
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.12-management-alpine
```

### No Logs Being Created

**Checks**:
1. RabbitMQ running: `docker ps | grep rabbitmq`
2. Events published: Check RabbitMQ UI → Queues
3. Consumer connected: RabbitMQ UI → Connections

### Geo-IP Lookup Failing

**Primary provider (ip-api.com)**:
- Rate limited to 45 requests/minute
- No API key required

**Fallback provider (ipgeolocation.io)**:
- Requires API key in configuration
- Check `appsettings.json` for correct key

### Service Won't Start

```bash
# Check logs
docker-compose logs service-name

# Verify ports available
netstat -an | findstr "5001"  # Windows
lsof -i :5001                  # Linux/Mac

# Clean restart
docker-compose down
docker-compose up --build
```

---

## Contributing

Contributions welcome! Please follow these guidelines:

1. Fork the repository
2. Create feature branch: `git checkout -b feature/amazing-feature`
3. Follow Clean Code principles
4. Add unit tests for new features
5. Update documentation
6. Submit pull request

### Code Style

- Follow C# coding conventions
- Use meaningful names
- Keep methods small and focused
- Add XML documentation comments
- Use async/await properly

---

## License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.

---

## Acknowledgments

- Clean Architecture concepts by Robert C. Martin
- Domain-Driven Design by Eric Evans
- MassTransit documentation and community
- .NET community for excellent tooling

---

## Support

For issues, questions, or contributions:

- **Issues**: [GitHub Issues](https://github.com/yourusername/geoblocker/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/geoblocker/discussions)
- **Email**: fawazislam70@gmail.com

---

**Built with .NET 9, Clean Architecture, DDD, and CQRS**
