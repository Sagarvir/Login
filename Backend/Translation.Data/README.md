# Translation.Data

## Overview
Data access layer for the translation system. It contains EF Core DbContext configuration, entity mappings, and repository implementations for persistence.

<!-- Notes: Keep this file updated when repository interfaces or migrations change. -->

## Responsibilities
- Define `AppDbContext` and entity relationships.
- Provide repository interfaces and implementations.
- Handle database migrations and data access concerns.

## Key Components
- `Data/AppDbContext.cs`
- `Data/AppDbContextFactory.cs` for design-time migrations
- Repository interfaces in `Repositories/Interfaces/`
- Repository implementations in `Repositories/`

## Project References
- Translation.Contracts
- Translation.Models

## File Structure
- Data/
- Repositories/
  - Interfaces/
