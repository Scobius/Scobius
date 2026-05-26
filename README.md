# Scobius

Scobius is a real-time chat application designed for seamless communication across web and mobile platforms.

## Project Overview

Scobius is built with a .NET backend that serves both as an MVC web application and a RESTful API. The ecosystem includes:

- Backend: ASP.NET Core (MVC + API)
- Mobile: Flutter (Client-side)
- Real-time: SignalR for instant messaging and presence tracking.

This repository is for the Backend service

## Tech Stack

- Framework: .NET 10 (ASP.NET Core)
- Database: PostgreSQL with Entity Framework Core (Supabase used for development)
- Real-time: ASP.NET Core SignalR
- Storage: Supabase Storage (for avatars and attachments)
- Email: Resend (for account verification)
- Authentication: ASP.NET Core Identity with JWT and Refresh Tokens

## Features

- Authentication: Secure registration, login, and email verification.
- Profile Management: Customizable user profiles with avatar uploads.
- Social Connectivity: A friendship system to manage connections.
- Real-time Messaging: Instant chat functionality powered by SignalR.
- Cross-Platform: Designed to work with a Flutter mobile application.

## Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL
- Flutter SDK (for mobile development)

### Backend Configuration

The API uses dotnet user-secrets for local development configuration. Navigate to src/Scobius.API and set the following:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=scobius;Username=postgres;Password=your_password"
dotnet user-secrets set "Jwt:Key" "your-secret-key"
dotnet user-secrets set "Supabase:Url" "your-supabase-url"
dotnet user-secrets set "Supabase:Key" "your-supabase-service-role-key"
dotnet user-secrets set "Resend:ApiKey" "your-resend-api-key"
```

### Running the Backend

1. Apply Migrations:

   ```bash
   cd src/Scobius.Infrastructure
   dotnet ef database update --startup-project ../Scobius.API
   ```

2. Launch the Application:

   ```bash
   cd ../Scobius.API
   dotnet run
   ```

## Project Structure

- src/Scobius.API: The main web project containing MVC views and API Controllers.
- src/Scobius.Core: Domain models, entities, interfaces, and DTOs.
- src/Scobius.Infrastructure: Data access layer, EF Core Migrations, and external service implementations.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
