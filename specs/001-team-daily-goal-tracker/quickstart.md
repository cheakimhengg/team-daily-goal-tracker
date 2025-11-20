# Quickstart Guide: Team Daily Goal Tracker

**Feature**: [spec.md](spec.md)
**Target Audience**: Developers setting up the project for the first time

---

## Prerequisites

### Required Software

| Tool | Version | Purpose | Installation |
|------|---------|---------|--------------|
| **Node.js** | 20.x LTS | Frontend build tool (Vite) | [nodejs.org](https://nodejs.org) |
| **npm** | 10.x | Frontend package manager | Included with Node.js |
| **.NET SDK** | 8.0 | Backend runtime | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| **Git** | Latest | Version control | [git-scm.com](https://git-scm.com) |
| **VS Code** | Latest | Recommended IDE | [code.visualstudio.com](https://code.visualstudio.com) |

### Optional Tools
- **SQLite Browser**: For inspecting database (https://sqlitebrowser.org/)
- **Postman/Insomnia**: For API testing

---

## Project Structure

```
team-daily-goal-tracker/
├── frontend/                   # Vue 3 + TypeScript frontend
│   ├── src/
│   │   ├── components/        # Reusable Vue components
│   │   ├── views/             # Page-level components
│   │   ├── services/          # API client
│   │   ├── types/             # TypeScript interfaces
│   │   ├── composables/       # Composition API composables
│   │   ├── App.vue            # Root component
│   │   └── main.ts            # Entry point
│   ├── package.json
│   └── vite.config.ts
│
├── backend/                    # .NET 8 Web API backend
│   ├── Controllers/           # API endpoints
│   ├── Services/              # Business logic
│   ├── Data/                  # Database access (Dapper + SQLite)
│   │   ├── Repositories/      # Data access layer
│   │   ├── Migrations/        # SQL migration scripts
│   │   └── team-tracker.db    # SQLite database file (created on first run)
│   ├── Models/                # Entity models and DTOs
│   ├── Middleware/            # Exception handling, CORS
│   └── Program.cs             # Entry point
│
├── specs/                      # Feature specifications
│   └── 001-team-daily-goal-tracker/
│       ├── spec.md            # Product specification
│       ├── plan.md            # Implementation plan
│       ├── data-model.md      # Database schema
│       ├── contracts/         # API contracts
│       └── quickstart.md      # This file
│
└── .specify/                   # Specify framework configuration
    └── memory/
        └── constitution.md    # Project principles and technical constraints
```

---

## Setup Instructions

### 1. Clone Repository

```bash
git clone <repository-url>
cd team-daily-goal-tracker
```

### 2. Backend Setup (.NET 8 API)

#### Install Dependencies

```bash
cd backend
dotnet restore
```

#### Configure Database

The SQLite database file (`team-tracker.db`) will be created automatically on first run in the `backend/Data/` directory.

**Migration Script**: `backend/Data/Migrations/001_InitialSchema.sql`

This script runs automatically at startup and creates:
- `TeamMembers` table
- `Goals` table
- Indexes for query optimization
- Seed data (4 sample team members)

#### Run Backend

```bash
dotnet run
```

**Backend will start on**: `http://localhost:5000`

**Expected Console Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

#### Verify Backend

Open browser and navigate to:
```
http://localhost:5000/api/team-members
```

**Expected Response**:
```json
{
  "data": [
    {"id": 1, "name": "Alice Johnson", "currentMood": null, "moodUpdatedAt": null},
    {"id": 2, "name": "Bob Smith", "currentMood": null, "moodUpdatedAt": null},
    ...
  ]
}
```

---

### 3. Frontend Setup (Vue 3 + TypeScript)

Open a **new terminal** (keep backend running in first terminal).

#### Install Dependencies

```bash
cd frontend
npm install
```

**Packages Installed**:
- `vue@^3.x` - Frontend framework
- `typescript@^5.x` - Type checking
- `vite@^5.x` - Build tool
- `daisyui@^4.x` - UI component library
- `tailwindcss@^3.x` - Utility-first CSS

#### Configure API Base URL

Create `frontend/.env` file:

```env
VITE_API_BASE_URL=http://localhost:5000
```

#### Run Frontend

```bash
npm run dev
```

**Frontend will start on**: `http://localhost:5173`

**Expected Console Output**:
```
VITE v5.x.x  ready in 500 ms

➜  Local:   http://localhost:5173/
➜  Network: use --host to expose
```

---

### 4. Open Application

Navigate to: `http://localhost:5173`

**You should see**:
1. Identity selector dropdown with 4 team members
2. Team member cards (no goals yet)
3. Mood selector buttons (all unset)

**Try These Actions**:
1. Select your identity from dropdown ("I am...")
2. Click a mood button → Card background color changes
3. Add a goal → Goal appears in your card
4. Check goal checkbox → Goal marked complete
5. View Stats panel (if implemented) → See today's statistics

---

## Development Workflow

### Running Both Servers Concurrently

**Terminal 1 (Backend)**:
```bash
cd backend
dotnet watch run  # Auto-restarts on code changes
```

**Terminal 2 (Frontend)**:
```bash
cd frontend
npm run dev  # Hot Module Replacement (HMR)
```

### Making Changes

#### Frontend Changes (Vue Components)
1. Edit files in `frontend/src/components/` or `frontend/src/views/`
2. Save file → Vite HMR automatically reloads browser
3. Check browser console for errors

#### Backend Changes (C# Controllers/Services)
1. Edit files in `backend/Controllers/` or `backend/Services/`
2. Save file → `dotnet watch` automatically rebuilds and restarts
3. Check terminal console for errors

#### Database Schema Changes
1. Edit `backend/Data/Migrations/001_InitialSchema.sql`
2. Delete `backend/Data/team-tracker.db` file
3. Restart backend → Database recreated with new schema

---

## Testing the API

### Using cURL

**Get All Team Members**:
```bash
curl http://localhost:5000/api/team-members?includeGoals=true
```

**Create a Goal**:
```bash
curl -X POST http://localhost:5000/api/goals \
  -H "Content-Type: application/json" \
  -d '{"teamMemberId": 1, "goalText": "Test goal from cURL"}'
```

**Update Mood**:
```bash
curl -X PUT http://localhost:5000/api/team-members/1/mood \
  -H "Content-Type: application/json" \
  -d '{"mood": "Great"}'
```

**Get Stats**:
```bash
curl http://localhost:5000/api/stats
```

### Using Browser DevTools

1. Open browser to `http://localhost:5173`
2. Press `F12` to open DevTools
3. Go to **Network** tab
4. Interact with UI (add goal, change mood)
5. Click API requests to see request/response details

---

## Common Issues & Solutions

### Issue 1: Port Already in Use

**Symptom**: `Address already in use` error

**Solution**:
```bash
# Backend (port 5000)
lsof -ti:5000 | xargs kill -9  # macOS/Linux
netstat -ano | findstr :5000   # Windows (then kill process)

# Frontend (port 5173)
lsof -ti:5173 | xargs kill -9  # macOS/Linux
```

### Issue 2: Database File Locked

**Symptom**: `SQLite Error: database is locked`

**Solution**:
1. Stop backend server
2. Close any SQLite Browser windows
3. Restart backend

### Issue 3: CORS Error in Browser Console

**Symptom**: `Access to fetch at 'http://localhost:5000' from origin 'http://localhost:5173' has been blocked by CORS policy`

**Solution**: Ensure backend `Program.cs` has CORS configured:
```csharp
app.UseCors(policy => policy
    .WithOrigins("http://localhost:5173")
    .AllowAnyMethod()
    .AllowAnyHeader());
```

### Issue 4: Module Not Found (Frontend)

**Symptom**: `Cannot find module '@/components/...'`

**Solution**:
```bash
cd frontend
rm -rf node_modules package-lock.json
npm install
```

### Issue 5: NuGet Package Restore Failed

**Symptom**: `.NET restore errors`

**Solution**:
```bash
cd backend
dotnet clean
dotnet restore --force
dotnet build
```

---

## Database Inspection

### Using SQLite Browser (GUI)

1. Download from https://sqlitebrowser.org/
2. Open `backend/Data/team-tracker.db`
3. Browse Data tab → Select `TeamMembers` or `Goals` table
4. Execute SQL tab → Run custom queries

### Using SQLite CLI

```bash
cd backend/Data
sqlite3 team-tracker.db

# List tables
.tables

# View TeamMembers
SELECT * FROM TeamMembers;

# View Goals
SELECT * FROM Goals;

# Exit
.quit
```

---

## VS Code Extensions (Recommended)

### Frontend Development
- **Vue - Official** (Vue.volar): Vue 3 syntax highlighting and IntelliSense
- **TypeScript Vue Plugin (Volar)**: TypeScript support in Vue files
- **Tailwind CSS IntelliSense**: Auto-complete for Tailwind classes
- **ESLint**: Linting for JavaScript/TypeScript

### Backend Development
- **C#** (ms-dotnettools.csharp): C# IntelliSense and debugging
- **C# Dev Kit**: Enhanced .NET development
- **SQLite Viewer**: View SQLite databases in VS Code

### General
- **Prettier**: Code formatting
- **GitLens**: Git integration

---

## Environment Variables

### Frontend (`frontend/.env`)

```env
# API Base URL
VITE_API_BASE_URL=http://localhost:5000

# Optional: Enable debug logging
VITE_DEBUG=true
```

### Backend (`backend/appsettings.json`)

```json
{
  "ConnectionStrings": {
    "SQLite": "Data Source=Data/team-tracker.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  }
}
```

---

## Building for Production (Out of Scope for MVP)

### Frontend Production Build

```bash
cd frontend
npm run build
```

**Output**: `frontend/dist/` directory with optimized static files

### Backend Production Publish

```bash
cd backend
dotnet publish -c Release -o ./publish
```

**Output**: `backend/publish/` directory with self-contained app

---

## Testing (If Tests Are Implemented)

### Frontend Tests (Vitest)

```bash
cd frontend
npm run test          # Run all tests
npm run test:watch    # Watch mode
```

### Backend Tests (xUnit)

```bash
cd backend
dotnet test
```

---

## Next Steps

1. **Read Specification**: See [spec.md](spec.md) for product requirements and user stories
2. **Review Implementation Plan**: See [plan.md](plan.md) for architecture and component breakdown
3. **Check API Contracts**: See [contracts/](contracts/) for detailed API documentation
4. **View Data Model**: See [data-model.md](data-model.md) for database schema

---

## Getting Help

### Internal Resources
- **Specification**: `specs/001-team-daily-goal-tracker/spec.md`
- **Constitution**: `.specify/memory/constitution.md`
- **API Contracts**: `specs/001-team-daily-goal-tracker/contracts/`

### External Documentation
- **Vue 3**: https://vuejs.org/guide/
- **DaisyUI**: https://daisyui.com/components/
- **.NET 8 Web API**: https://learn.microsoft.com/en-us/aspnet/core/web-api/
- **Dapper**: https://github.com/DapperLib/Dapper
- **SQLite**: https://www.sqlite.org/docs.html

### Troubleshooting
1. Check browser console for frontend errors
2. Check terminal console for backend errors
3. Verify database schema: `sqlite3 backend/Data/team-tracker.db ".schema"`
4. Test API directly with cURL or Postman

---

## Cheat Sheet

```bash
# Start backend
cd backend && dotnet watch run

# Start frontend
cd frontend && npm run dev

# Reset database
rm backend/Data/team-tracker.db && cd backend && dotnet run

# View database
sqlite3 backend/Data/team-tracker.db

# Run tests (if implemented)
cd backend && dotnet test
cd frontend && npm run test

# Format code
cd frontend && npm run format
cd backend && dotnet format
```

---

**Happy coding! 🚀**
