# ── Stage 1: Build .NET API ───────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-build
WORKDIR /src

# Restore dependencies first (layer cache)
COPY src/Ledger.Core/Ledger.Core.csproj src/Ledger.Core/
COPY src/Ledger.Application/Ledger.Application.csproj src/Ledger.Application/
COPY src/Ledger.Infrastructure/Ledger.Infrastructure.csproj src/Ledger.Infrastructure/
COPY src/Ledger.Api/Ledger.Api.csproj src/Ledger.Api/
RUN dotnet restore src/Ledger.Api/Ledger.Api.csproj

# Copy source and publish
COPY src/ src/
RUN dotnet publish src/Ledger.Api/Ledger.Api.csproj \
    -c Release \
    -o /app/api \
    --no-restore

# ── Stage 2: Build React frontend ────────────────────────────────────────────
FROM node:20-alpine AS node-build
WORKDIR /client

# Install pnpm
RUN npm install -g pnpm

# Install dependencies first (layer cache)
COPY client/package.json client/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

# Copy source and build
COPY client/ ./
RUN pnpm build

# ── Stage 3: API runtime ──────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS api
WORKDIR /app
COPY --from=dotnet-build /app/api .

# Data directory for SQLite persistence (mount a volume here)
RUN mkdir -p /app/data
VOLUME ["/app/data"]

ENV ASPNETCORE_URLS=http://+:5050
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DataDir=/app/data

EXPOSE 5050
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget -qO- http://localhost:5050/health || exit 1

ENTRYPOINT ["dotnet", "Ledger.Api.dll"]

# ── Stage 4: Web (nginx + React SPA) ─────────────────────────────────────────
FROM nginx:alpine AS web
COPY --from=node-build /client/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
    CMD wget -qO- http://localhost/health || exit 1
