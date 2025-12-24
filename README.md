# Kibana Logging

A .NET 8 microservice for structured logging to Elasticsearch, designed for integration with Kibana dashboards. This project provides a reusable logging service for capturing application logs at various levels (Information, Debug, Warning, Error, Critical) and storing them in Elasticsearch for centralized monitoring and analysis.

## Features

- Structured logging with support for additional contextual data.
- Logs are indexed in Elasticsearch and can be visualized in Kibana.
- Supports log levels: Information, Debug, Warning, Error, and Critical.
- Automatic index creation if it does not exist.
- Easily extensible for use in other .NET microservices.

## Project Structure

- **Kibana-Logging**: Contains the core logging service (`LoggerService`) and interfaces.
- **Kibana-LoggingService**: Example ASP.NET Core Web API demonstrating logging integration.

## Getting Started

### Prerequisites

- .NET 8 SDK
- Elasticsearch instance (local or remote)
- Kibana (optional, for log visualization)

### Configuration

Set the following configuration values (e.g., in `appsettings.json`):

### Running the Service

1. Restore dependencies and build the solution:
2. Run the API project:
3. Access Swagger UI at `https://localhost:<port>/swagger` to test the API.

### Usage Example

The `WeatherForecastController` demonstrates logging at various levels:

## Visualizing Logs

1. Open Kibana and configure an index pattern matching your `IndexName` (e.g., `application-logs`).
2. Use Kibana Discover or Dashboard to view and analyze logs.

## Extending

To use the logging service in other projects:

- Reference the `Kibana-Logging` project.
- Register `ILoggerService` and `LoggerService` in your DI container.
- Inject and use `ILoggerService` in your classes.

## License

This project is licensed under the MIT License.
