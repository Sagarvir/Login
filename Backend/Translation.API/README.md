# Translation.API

## Overview
ASP.NET Core Web API host for the translation system. It exposes HTTP endpoints, configures dependency injection, and bridges requests to the service and data layers.

## Responsibilities
- Hosts the web server and routing.
- Registers services, repositories, and the EF Core DbContext.
- Handles API-specific concerns such as authentication, authorization, and request/response formatting.

## Key Technologies
- ASP.NET Core (.NET 10)
- Entity Framework Core

## Project References
- Translation.Service
- Translation.Data
- Translation.Contracts
- Translation.Models

## Notes
- Configuration is loaded from appsettings.json in this project.

## File Structure
- Controllers/
- Properties/
- appsettings.json
- Program.cs
