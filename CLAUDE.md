# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Personal Touzi is a personal quantitative investment portfolio management system with:
- **Backend**: ASP.NET Core 8.0 Web API with Entity Framework Core (SQLite)
- **Frontend**: Vue 3 + TypeScript + Vite + Pinia + ECharts

## Common Commands

### Backend
```bash
cd backend
dotnet build                          # Build the solution
dotnet run --project src/PersonalTouzi.Api   # Run the API server (http://localhost:5000)
dotnet test                           # Run tests
```

### Frontend
```bash
cd frontend
npm install                           # Install dependencies
npm run dev                           # Start dev server (http://localhost:5173)
npm run build                         # Build for production
npm run preview                       # Preview production build
```

## Architecture

### Backend Structure
```
backend/src/
├── PersonalTouzi.Api/           # API layer (Controllers)
├── PersonalTouzi.Core/          # Domain entities
└── PersonalTouzi.Infrastructure/ # Data access (EF Core, Services)
```

**Key Controllers:**
- `PortfolioController` - Dashboard, summary, snapshots, net value trend
- `AccountsController` - Account CRUD
- `PositionsController` - Position CRUD
- `TransactionsController` - Transaction CRUD
- `AIController` - AI chat, analysis, predictions

**API Routes:**
- `/api/portfolio/*` - Portfolio endpoints
- `/api/ai/*` - AI endpoints

### Frontend Structure
```
frontend/src/
├── api/           # API client (axios)
├── components/    # Reusable Vue components
├── views/         # Page components
├── stores/        # Pinia state management
└── router/        # Vue Router configuration
```

**Key Views:**
- `Dashboard.vue` - Main dashboard
- `Positions.vue` - Position management
- `Transactions.vue` - Transaction history
- `Accounts.vue` - Account management
- `AIDashboard.vue` - AI analysis features

## Data Models

### Core Entities
- **Account**: Investment account with name, broker, initial cash
- **Position**: Holding with symbol, quantity, cost price, current price
- **Transaction**: Buy/sell record with symbol, quantity, price, date
- **AssetSnapshot**: Daily snapshot of total assets, net value, gain/loss

## Development Notes

### API Data Format
- Backend returns camelCase property names (configured via JSON options)
- Frontend interfaces should match backend DTO property names

### Entity Framework
- Uses SQLite database (`personal_touzi.db`)
- Migrations should be created for schema changes:
  ```bash
  dotnet ef migrations add MigrationName --project src/PersonalTouzi.Infrastructure --startup-project src/PersonalTouzi.Api
  ```

### Adding New API Endpoints
1. Create/modify controller in `PersonalTouzi.Api/Controllers/`
2. Use route prefix `/api/portfolio/` for portfolio-related endpoints
3. Return anonymous objects or DTOs with camelCase properties
4. Update frontend `api/index.ts` with corresponding interface and API method
