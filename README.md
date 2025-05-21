# 📈 Stock Market Simulator 💸

A modular monolithic application simulating stock market trading, budgeting, and user portfolio management using real-world stock data. It simulates market behavior using the Geometric Brownian Motion model and updates user dashboards in real-time via SignalR.

![Build](https://github.com/thelanmi/rally-simulator/workflows/Build/badge.svg)

| Tech                                                                               | Description                                    |
| ---------------------------------------------------------------------------------- | ---------------------------------------------- |
| 🟪 [.NET 9](https://dotnet.microsoft.com/)                                         | Core framework used to build the application   |
| 🐘 [PostgreSQL](https://www.postgresql.org/)                                       | Relational database system                     |
| 💾 [EF Core](https://learn.microsoft.com/en-us/ef/core/)                           | ORM for database access                        |
| 🧱 [Dapper](https://github.com/DapperLib/Dapper)                                   | Lightweight micro ORM                          |
| 🔗 [SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)   | Real-time communication                        |
| 🐇 [RabbitMQ](https://www.rabbitmq.com/)                                           | Messaging system for event-driven architecture |
| 🔴 [Redis](https://redis.io/)                                                      | Distributed caching                            |
| 🔍 [FluentValidation](https://docs.fluentvalidation.net/)                          | Input validation library                       |
| 🧼 [Swagger](https://swagger.io/tools/swagger-ui/)                                 | API documentation and testing                  |
| 📜 [Serilog](https://serilog.net/)                                                 | Structured logging                             |
| 🐳 [Docker](https://www.docker.com/)                                               | Containerization                               |
| 🚀 [.NET Aspire](https://devblogs.microsoft.com/dotnet/introducing-dotnet-aspire/) | Cloud-native orchestration for .NET apps       |


## 🧱 Architecture
This project follows the Modular Monolith architecture with Clean Architecture principles.
- ✅ CQRS pattern used to separate read and write operations
- 🧩 Event-driven communication using RabbitMQ for module decoupling
- ⚙️ Each module has its own Domain, Application, and Infrastructure layers

## Modules

### 👤 Users Module
Manages user profiles, authentication, and settings

Handles email verification, user preferences, etc.

### 📊 Stocks Module
Handles stock listings, market simulation, portfolio tracking

Supports buying, selling, and real-time updates via SignalR

### 💰 Budgeting Module
Manages income, expenses, and budgeting categories
