# Hytale ModLister

A web application to list, search, and manage your Hytale mods. It automatically scans a mods folder and retrieves metadata from CurseForge.

## Tech Stack

- **Frontend**: SvelteKit 2 + Svelte 5 + TypeScript + Vite
- **Backend**: .NET 10 ASP.NET Core API
- **Containerization**: Docker + Nginx

## Using Docker (Recommended)

### Docker Images

Docker images are automatically published to GitHub Container Registry on each release.

**Frontend:**
```bash
docker pull ghcr.io/jordanolivet/hytale-mod-lister-frontend:latest
# or a specific version
docker pull ghcr.io/jordanolivet/hytale-mod-lister-frontend:0.0.3
```

**Backend:**
```bash
docker pull ghcr.io/jordanolivet/hytale-mod-lister-backend:latest
# or a specific version
docker pull ghcr.io/jordanolivet/hytale-mod-lister-backend:0.0.3
```

### Docker Compose (Production)

Create a `docker-compose.yml` file:

```yaml
services:
  backend:
    image: ghcr.io/jordanolivet/hytale-mod-lister-backend:latest
    ports:
      - "5000:8080"
    volumes:
      - ./mods:/app/mods:ro          # Folder containing your mods
      - backend-data:/app/data        # Persistent data (cache)
    environment:
      - CURSEFORGE_API_KEY=${CURSEFORGE_API_KEY:-}
      - TZ=UTC
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 30s
    restart: unless-stopped

  frontend:
    image: ghcr.io/jordanolivet/hytale-mod-lister-frontend:latest
    ports:
      - "3000:80"
    depends_on:
      backend:
        condition: service_healthy
    restart: unless-stopped

volumes:
  backend-data:
```

Start the application:

```bash
# With a CurseForge API key (recommended)
CURSEFORGE_API_KEY=your_key docker compose up -d

# Without API key (limited features)
docker compose up -d
```

The application will be available at `http://localhost:3000`

### Configuration

| Environment Variable | Description | Default |
|---------------------|-------------|---------|
| `CURSEFORGE_API_KEY` | CurseForge API key to retrieve mod metadata | (empty) |
| `TZ` | Timezone for the scheduler | UTC |

### Volumes

| Container Path | Description |
|----------------|-------------|
| `/app/mods` | Folder containing your mod files (.jar, .zip) |
| `/app/data` | Persistent data (metadata cache) |

## Local Development

### Prerequisites

- Node.js 22+
- .NET 10 SDK
- Docker (optional)

### Frontend

```bash
cd frontend
npm install
npm run dev
```

The dev server will be available at `http://localhost:5173`

### Backend

```bash
cd backend
dotnet restore
dotnet run --project HytaleModLister.Api
```

The API will be available at `http://localhost:5000`

### Local Docker Build

```bash
# Build and run with docker compose
docker compose up --build

# Or build separately
docker build -t hytale-modlister-frontend ./frontend
docker build -t hytale-modlister-backend ./backend/HytaleModLister.Api
```

## CI/CD

The project uses GitHub Actions for continuous integration and deployment.

### Pull Requests

Each PR must have a release label:
- `release:major` - Breaking changes
- `release:minor` - New features
- `release:patch` - Bug fixes
- `no-release` - No release (documentation, refactoring)

### Releases

When merging a PR with a `release:*` label, the workflow:
1. Calculates the new version
2. Builds and pushes Docker images to GHCR
3. Updates the VERSION file
4. Creates a Git tag and GitHub release

## Project Structure

```
.
├── backend/
│   └── HytaleModLister.Api/    # .NET API
│       ├── Dockerfile
│       └── ...
├── frontend/                    # SvelteKit application
│   ├── Dockerfile
│   ├── nginx.conf
│   └── ...
├── mods/                        # Mods folder (local for testing)
├── docker-compose.yml           # Docker Compose configuration
└── VERSION                      # Current version
```

## License

MIT
