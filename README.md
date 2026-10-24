# Knowledge Hub — ASP.NET Core Backend

REST API for **Knowledge Hub**, a personal bookmark and learning-resource manager. Users organise resources (articles, videos, PDFs, websites) into collections and label them with tags.

This is the flagship backend. The same API is also implemented in [FastAPI](https://github.com/lahiru1115/knowledge-hub-fastapi-backend) and [Spring Boot](https://github.com/lahiru1115/knowledge-hub-spring-boot-backend), and all three serve the [Next.js frontend](https://github.com/lahiru1115/knowledge-hub-nextjs-frontend).

## Tech stack

- .NET 10, ASP.NET Core Web API (controllers)
- Entity Framework Core 10 + Npgsql (PostgreSQL), code-first migrations
- JWT bearer authentication, BCrypt password hashing
- Swagger / OpenAPI

## Features

- Register, log in, current user (`/api/auth`)
- Collections CRUD, scoped to the logged-in user (`/api/collections`)
- Resources CRUD with tags, search, filters and pagination (`/api/resources`)
- Tags CRUD (`/api/tags`)
- Dashboard counts (`/api/dashboard`)

## Getting started

**Prerequisites:** .NET 10 SDK, PostgreSQL, and the EF Core CLI (`dotnet tool install --global dotnet-ef`).

1. Copy `appsettings.Example.json` to `appsettings.Development.json` (gitignored) and fill in the connection string and a JWT key of at least 32 characters.
2. Apply the migrations:
   ```bash
   dotnet ef database update
   ```
3. Run the API:
   ```bash
   dotnet run
   ```
4. Open Swagger at http://localhost:5113/swagger, or use the requests in `KnowledgeHub.Api.http`.

## Migrations

```bash
dotnet ef migrations add <Name>
dotnet ef database update
```

## Project structure

```
Controllers/   HTTP endpoints
Data/          AppDbContext and EF Core model configuration
DTOs/          Request and response models
Models/        Entities (User, Collection, Resource, Tag, ResourceTag)
Services/      TokenService (JWT)
Migrations/    EF Core migrations
```
