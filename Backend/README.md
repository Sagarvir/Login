# Translation Solution

## Overview
This solution implements a multi-layer translation management system. It provides APIs for creating translation keys, managing translations by language, and publishing localized resources. The architecture separates API hosting, business logic, data access, shared contracts, and domain models into dedicated projects.

## What the Solution Does
- Exposes HTTP endpoints to manage translations.
- Validates and applies business rules in the service layer.
- Persists translation data and related entities via EF Core.
- Publishes translation files (JSON and XLF) per language.

## Projects
- **Translation.API**: ASP.NET Core Web API host. Handles routing, configuration, and dependency injection.
- **Translation.Service**: Business logic layer. Implements translation workflows and validation.
- **Translation.Data**: Data access layer. EF Core DbContext, repositories, and migrations.
- **Translation.Models**: Domain entities shared across layers.
- **Translation.Contracts**: DTOs and contracts used across API and service boundaries.

## Solution Structure
- Translation.API/
- Translation.Service/
- Translation.Data/
- Translation.Models/
- Translation.Contracts/

## Key Features
- Translation key management
- Language-specific translation storage
- Bulk translation operations
- Publish translations to JSON and XLF files

## Technology Stack
- .NET 10
- ASP.NET Core
- Entity Framework Core
