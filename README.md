# Director-MESH

Central control plane for managing site-to-site TRE (Trusted Research Environment) connectivity over [NetBird MESH](https://netbird.io).

Operators define data planes, sites, services, and ACL rules through a web UI. The system generates per-site configurations (HAProxy config + Kubernetes manifests) that are versioned, sent for local approval, then applied to each TRE endpoint.

---

## Architecture

```
┌─────────────────────────────────────────────┐
│              Director-MESH (this repo)       │
│                                             │
│  ┌─────────┐  ┌──────────┐  ┌───────────┐  │
│  │DataPlane│→ │  Site    │→ │ Service   │  │
│  └─────────┘  └──────────┘  └───────────┘  │
│                                    │        │
│  ┌──────────────────────────┐      ↓        │
│  │       ACL Rules          │ ┌──────────┐  │
│  │ Outbound ──→ Inbound     │ │SiteConfig│  │
│  └──────────────────────────┘ └────┬─────┘  │
└───────────────────────────────────┼─────────┘
                                    │ approve + download
                          ┌─────────▼──────────┐
                          │   TRE Site (K8s)    │
                          │  ┌──────────────┐   │
                          │  │  HAProxy pod │   │
                          │  │  (bridge)    │   │
                          │  └──────┬───────┘   │
                          │         │            │
                          │  ┌──────▼───────┐   │
                          │  │  NetBird     │   │
                          │  │  peer agent  │   │
                          │  └──────────────┘   │
                          └────────────────────┘
```

### Core concepts

| Concept | Description |
|---|---|
| **Data Plane** | Logical network segment. Sites and ACL rules belong to a data plane. Maps to a NetBird network. |
| **Site / TRE** | An endpoint (e.g. a university TRE) that hosts one or more services. |
| **Service** | A network service on a site — either **Inbound** (receives traffic, proxied to a local K8s service) or **Outbound** (sends traffic to a remote inbound service). Each service registers as its own **individual NetBird peer**, so NetBird routes traffic directly between service endpoints rather than between sites. |
| **ACL Rule** | Allows or denies an Outbound service at one site from reaching an Inbound service at another. |
| **Site Configuration** | A versioned snapshot of HAProxy config + K8s manifests for a site, sent for local approval before being applied. |

### Configuration lifecycle

```
Generate → Sent → Approved → Applied
                ↘ Rejected
```

---

## Quick start (Docker)

**Prerequisites:** Docker Desktop

```bash
docker compose up
```

| Service | URL | Credentials |
|---|---|---|
| Director-MESH web | http://localhost:5000 | `admin` / `admin` |
| Keycloak admin | http://localhost:8080 | `admin` / `admin` |
| PostgreSQL | localhost:5432 | `director_mesh` / `director_mesh_pass` |

On first boot Keycloak takes ~30–60 seconds to initialise and import the realm. The web container waits for it automatically.

> **Fresh start:** If you've run the stack before and need to reset, destroy volumes first:
> ```bash
> docker compose down -v
> docker compose up
> ```

---

## Local development (without Docker)

**Prerequisites:** .NET 10 SDK, PostgreSQL 16, a running Keycloak instance

```bash
# 1. Start only the infrastructure
docker compose up postgres keycloak -d

# 2. Run the web app (EF migrations run automatically on startup)
cd src/DirectorMesh.Web
dotnet run
```

The app will be available at `http://localhost:5000` (or the port shown in terminal output).

---

## Configuration

Settings live in [src/DirectorMesh.Web/appsettings.json](src/DirectorMesh.Web/appsettings.json). Override via environment variables or `appsettings.Development.json`.

| Key | Default | Description |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | local postgres | PostgreSQL connection string |
| `Keycloak:Authority` | `http://localhost:8080/realms/director-mesh` | Public Keycloak URL (browser-visible) |
| `Keycloak:InternalAuthority` | *(unset)* | Internal Docker URL for backchannel calls (set by docker-compose) |
| `Keycloak:ClientId` | `director-mesh-web` | OIDC client ID |
| `Keycloak:ClientSecret` | `director-mesh-secret` | OIDC client secret |
| `NetBird:ApiBase` | `https://api.netbird.io/api` | NetBird management API base URL |
| `NetBird:ApiToken` | *(empty)* | NetBird API token |

### Keycloak in Docker — how it works

The browser redirects to Keycloak at `http://localhost:8080` (port-mapped). The web container cannot reach `localhost:8080` from inside Docker, so `Keycloak:InternalAuthority=http://keycloak:8080` is set in docker-compose. A `KeycloakBackchannelHandler` transparently rewrites all server-side OIDC calls (token exchange, userinfo) to use the internal service name.

---

## Project structure

```
director-mesh/
├── src/
│   └── DirectorMesh.Web/
│       ├── Controllers/          # MVC controllers (CRUD + workflow actions)
│       ├── Data/
│       │   ├── ApplicationDbContext.cs
│       │   └── Migrations/
│       ├── Models/
│       │   ├── Domain/           # DataPlane, Site, NetworkService, AclRule, SiteConfiguration
│       │   └── ViewModels/       # Form view models with validation
│       ├── Services/
│       │   ├── ConfigurationGeneratorService.cs   # Generates HAProxy + K8s YAML
│       │   ├── NetBirdService.cs                  # NetBird management API client
│       │   └── KeycloakBackchannelHandler.cs      # Docker OIDC backchannel fix
│       └── Views/                # Razor views (Bootstrap 5 sidebar UI)
├── keycloak/
│   └── director-mesh-realm.json  # Auto-imported Keycloak realm (client, user)
├── postgres/
│   └── init.sh                   # Creates the keycloak database on first boot
├── docker-compose.yml
└── DirectorMesh.slnx
```

---

## Typical workflow

1. **Create a Data Plane** — e.g. *Production Mesh*, linked to a NetBird network ID
2. **Register Sites** — add each TRE endpoint (name, description, data plane)
3. **Define Services** per site — each service becomes its own NetBird peer:
   - *Inbound*: a service this site exposes (e.g. `Postgres` on TCP/5432, proxied to `postgres-svc:5432`). Set its NetBird peer name, DNS name, and IP address.
   - *Outbound*: a connection this site makes. Set `Remote NetBird DNS Name` to the DNS name of the target inbound service peer.
4. **Create ACL Rules** — link an Outbound service (source) to an Inbound service (destination) with `Allow`
5. **Generate Config** — from a site's detail page, click *Generate Config*
6. **Send → Approve → Apply** — walk the config through its lifecycle; download the HAProxy `.cfg` and K8s `.yaml` artifacts to deploy at the site

---

## Site deployment

Each site runs a HAProxy pod in Kubernetes that bridges local services to the NetBird overlay network.

The generated K8s manifest deploys:
- A `Deployment` running `haproxy:2.8-alpine`
- A `Service` exposing inbound ports
- A `ConfigMap` reference for the HAProxy config (apply the downloaded `.cfg` as a ConfigMap separately)

```bash
# At the TRE site — apply the generated artifacts
kubectl create namespace director-mesh
kubectl create configmap director-mesh-haproxy-config \
  --from-file=haproxy.cfg=haproxy-mysite-v3.cfg \
  -n director-mesh
kubectl apply -f k8s-mysite-v3.yaml
```

---

## NetBird integration

The `NetBirdService` connects to the NetBird management API. Set `NetBird:ApiToken` to a personal access token from your NetBird account.

### Per-service peer model

Each `NetworkService` registers as its own independent NetBird peer. This means:
- An **Inbound** service gets a `NetBirdPeerName`, `NetBirdDnsName`, and `NetBirdIpAddress` — the address that remote outbound services connect to.
- An **Outbound** service sets `RemoteNetBirdDnsName` to the DNS name of the target inbound service's NetBird peer.
- NetBird routes traffic directly between service peers; there is no single site-level peer.

Supported API operations (called when pushing configs):
- Create peer groups
- Create access policies (maps to ACL rules)
- Delete policies

---

## Development notes

### Database migrations

```bash
cd src/DirectorMesh.Web

# Add a new migration
dotnet ef migrations add <MigrationName> --output-dir Data/Migrations

# Apply to a running database
dotnet ef database update
```

Migrations also run automatically on application startup.

### Adding a new Keycloak user

Log in to the Keycloak admin console at `http://localhost:8080` with `admin/admin`, navigate to the `director-mesh` realm → Users → Add user.

To change the default `admin` user password, edit [keycloak/director-mesh-realm.json](keycloak/director-mesh-realm.json) before first boot, or update it directly in the Keycloak console.
