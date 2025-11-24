# Team Daily Goal Tracker with Mood Sync

A simple, lightweight daily standup tool for small teams (5-20 people) to track goals and share mood status without authentication overhead.

## Features

- **Identity Selection**: Select your name from a dropdown to identify yourself (session-based, no login required)
- **Team Dashboard**: View all team members with their goals and current mood status
- **Goal Management**: Add, complete, and delete your daily goals with optimistic UI updates
- **Mood Tracking**: Select your current mood (Great, Good, Okay, Struggling, Overwhelmed) with visual indicators
- **Real-time Updates**: Changes appear immediately with automatic rollback on errors

## Tech Stack

### Frontend
- Vue 3 with TypeScript
- Vite (build tool)
- Tailwind CSS v4 + DaisyUI
- Vue Router

### Backend
- .NET 8 Web API
- Dapper (lightweight ORM)
- SQLite database
- RESTful API architecture

## Prerequisites

- Node.js 18+ and npm
- .NET 8 SDK
- Git

## Quick Start

### 1. Clone the Repository

```bash
git clone <repository-url>
cd team-daily-goal-tracker
```

### 2. Start the Backend

```bash
cd backend
dotnet restore
dotnet run
```

The backend will start on http://localhost:5166

### 3. Start the Frontend

```bash
cd frontend
npm install
npm run dev
```

The frontend will start on http://localhost:5173

### 4. Open the Application

Navigate to http://localhost:5173 in your browser.

## Project Structure

```
team-daily-goal-tracker/
├── backend/                    # .NET 8 Web API
│   ├── Controllers/           # API endpoints
│   ├── Data/
│   │   ├── Migrations/        # Database schema
│   │   └── Repositories/      # Data access layer
│   ├── Models/                # Entity models and DTOs
│   ├── Services/              # Business logic
│   ├── Middleware/            # Exception handling
│   └── Exceptions/            # Custom exceptions
├── frontend/                   # Vue 3 + TypeScript
│   ├── src/
│   │   ├── components/        # Reusable Vue components
│   │   ├── views/             # Page components
│   │   ├── composables/       # Vue composables
│   │   ├── services/          # API client
│   │   └── types/             # TypeScript interfaces
│   └── public/
└── specs/                      # Design documents
```

## API Endpoints

### Team Members
- `GET /api/team-members?includeGoals={bool}` - Get all team members
- `PUT /api/team-members/{id}/mood` - Update team member mood

### Goals
- `POST /api/goals` - Create a new goal
- `PUT /api/goals/{id}/toggle` - Toggle goal completion
- `DELETE /api/goals/{id}` - Delete a goal

## Database Schema

### TeamMembers Table
- `Id` (INTEGER, PRIMARY KEY)
- `Name` (TEXT, NOT NULL, 1-100 chars)
- `CurrentMood` (TEXT, ENUM: Great, Good, Okay, Struggling, Overwhelmed)
- `MoodUpdatedAt` (TEXT, ISO 8601 timestamp)

### Goals Table
- `Id` (INTEGER, PRIMARY KEY)
- `TeamMemberId` (INTEGER, FOREIGN KEY)
- `GoalText` (TEXT, NOT NULL, 1-500 chars)
- `CreatedAt` (TEXT, ISO 8601 timestamp)
- `IsCompleted` (INTEGER, BOOLEAN: 0 or 1)

## Development

### Backend Development

```bash
cd backend

# Run in development mode
dotnet run

# Build for production
dotnet build --configuration Release

# Run tests
dotnet test
```

### Frontend Development

```bash
cd frontend

# Install dependencies
npm install

# Run development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Type check
npm run type-check
```

## Configuration

### Backend Configuration

- **Database**: SQLite file at `backend/Data/team-tracker.db`
- **CORS**: Configured to allow `http://localhost:5173`
- **Port**: 5166 (configurable in `launchSettings.json`)

### Frontend Configuration

- **API Base URL**: Set in `frontend/.env` as `VITE_API_BASE_URL`
- **Default**: `http://localhost:5166`

## Deployment

### Backend Deployment

1. Build the release version:
   ```bash
   cd backend
   dotnet publish --configuration Release
   ```

2. Deploy the contents of `bin/Release/net8.0/publish/` to your server

3. Ensure the database directory is writable

### Frontend Deployment

1. Build the production bundle:
   ```bash
   cd frontend
   npm run build
   ```

2. Deploy the contents of `dist/` to your static hosting service (Netlify, Vercel, etc.)

3. Update `VITE_API_BASE_URL` to point to your production backend

## Assumptions & Constraints

- **Small Teams**: Designed for 5-20 team members
- **Daily Reset**: Goals persist but are intended for daily standup use
- **No Authentication**: Uses session storage for identity (clears on page refresh)
- **Single Device**: No synchronization across multiple devices for the same user
- **No Persistence**: Identity selection doesn't persist across sessions
- **No History**: No tracking of past goals or mood changes

## Browser Support

- Chrome (latest 2 versions)
- Firefox (latest 2 versions)
- Safari (latest 2 versions)
- Edge (latest 2 versions)

## Performance Targets

- Page load: < 3 seconds
- API latency: < 200ms
- Lighthouse score: > 90

## License

[Your License Here]

## Contributing

[Your Contributing Guidelines Here]
