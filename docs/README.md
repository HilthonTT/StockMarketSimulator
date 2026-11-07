# Stock Market Simulator documentation

| Document | What it covers |
| --- | --- |
| [Getting started](getting-started.md) | Prerequisites, configuration, running with .NET Aspire, migrations, tests, the front end |
| [CI/CD](ci-cd.md) | GitHub Actions workflows, published container images, Dependabot |
| [Front end](../frontend/README.md) | Pages, code layout, live price feed |
| [Architecture](architecture.md) | Modules and layers, the CQRS pipeline, persistence, caching, cross-cutting concerns |
| [Events and background jobs](events-and-background-jobs.md) | Domain events and the transactional outbox, integration events over RabbitMQ, Quartz jobs |
| [Price simulation](price-simulation.md) | The Geometric Brownian Motion model behind the real-time feed |
| [Build and dependencies](build-and-dependencies.md) | .NET 10, `.slnx`, central package management, tooling, Docker images |
| [Changelog: fixes and upgrade](changelog.md) | Bugs fixed, architectural changes, migrations and API changes in the .NET 10 upgrade |
| [Known issues and follow-ups](known-issues.md) | Remaining warnings, limitations and suggested next steps |
