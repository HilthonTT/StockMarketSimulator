# StockMarketSimulator 💸 💰

\
![Build](https://github.com/thelanmi/rally-simulator/workflows/Build/badge.svg)

**Stock market simulator implemented using .NET 9 and postgresql.**

## Technologies

- .NET 9
- Postgresql
- Dapper
- FluentValidation
- Swagger
- Serilog
- SignalR
- Docker 

## System Architecture

The solution utilizes a veritcal slices and clean architecture approach. Each layers are separated in different modules

### Domain

The domain layer contains all of the entities, value objects, enums, interfaces and logic that is specific to the domain. It does not depend on any other layer in the system.

### Application

The application layer is responsible for orchestrating all of the application logic and the necessary use-cases. It depends on the domain layer, but has no dependencies on any other layer. This layer defines additional interfaces that are required for supporting the application logic, that are implemented by outside layers. For example, the ***IDbContext*** interface is defined in this layer but implemented by the Persistence layer.

This layer utilizes the **CQRS pattern** for structuring the application logic into commands and queries, using the MediatR library.

### Infrastucture

The infrastructure layer contains classes tasked with accessing external resources. These classes are based on the interfaces defined within the application layer. In the current system, this layer is possibly oversimplified and redundant as it contains only the ***MachineDateTime*** class for getting the current UTC date and time.

### Persistence

The persistence layer is responsible for implementing database related concerns. It contains EF Core Configurations for entities, implements the application database context and wires up some persistence related dependencies.

## Swagger UI

The Api project will open up the Swagger UI once it starts. Here you can browse the documentation for each endpoint, making it easy to test all of the endpoints.


