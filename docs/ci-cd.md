# CI/CD

The workflows live in `.github/workflows`.

## CI (`ci.yml`)

Runs on every pull request and on pushes to `main`.

| Job | Steps |
| --- | --- |
| **Backend** | `dotnet restore` → `dotnet build -c Release` → `dotnet test` for all five suites. Integration and functional tests start Postgres, Redis, RabbitMQ and Azurite with Testcontainers, which works on `ubuntu-latest` because it has Docker. TRX results and coverage are uploaded as the `backend-test-results` artifact. |
| **Frontend** | In `frontend/`: `bun install --frozen-lockfile` → `bun run lint` → `bun run typecheck` → `bun run build` |
| **Docker** | Builds both images (`src/Web.Api/Dockerfile` and `frontend/Dockerfile`) without pushing, after the two jobs above pass. Layers are cached in the GitHub Actions cache. |

Older runs on the same branch are cancelled when a new commit arrives.

## CD (`cd.yml`)

Publishes images to GitHub Container Registry:

- `ghcr.io/<owner>/<repo>/api`
- `ghcr.io/<owner>/<repo>/frontend`

It runs:

- after CI succeeds on `main`, tagging the images `latest` and `sha-<short>`;
- on version tags `vX.Y.Z`, tagging the images `X.Y.Z` and `X.Y`;
- on demand through **Run workflow**.

It only publishes images. Deploying them to a host is left to the target platform.

### Configuration

The front end inlines its public URLs at build time. Set these **repository variables** (Settings → Secrets and variables → Actions → Variables) so the published image points at your deployed API:

| Variable | Example |
| --- | --- |
| `NEXT_PUBLIC_SERVER_URL` | `https://api.example.com` |
| `NEXT_PUBLIC_APP_URL` | `https://app.example.com` |

The `frontend` container also needs `AUTH_SECRET` at runtime, and optionally `UPSTASH_REDIS_REST_URL` / `UPSTASH_REDIS_REST_TOKEN`. The API container takes its usual configuration (connection strings, `Jwt:Secret`, `Stocks:ApiKey`).

Pushing uses the built-in `GITHUB_TOKEN`; no extra secrets are needed. New packages are private by default; change their visibility under the repository's **Packages** if needed.

## Dependabot (`dependabot.yml`)

- Weekly grouped minor/patch updates for NuGet (`/`) and Bun (`/frontend`).
- Monthly updates for GitHub Actions and the Docker base images.
