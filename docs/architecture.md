# Workstock architecture and delivery notes

## Implemented MVP boundary

The working MVP follows the newer product specification: customer → site → job → schedule → items required → work → completion. Inventory is intentionally not required for an organisation to use the product.

The repository now contains a React/Vite PWA frontend and an ASP.NET Core / PostgreSQL API. The API owns tenancy and authorisation; the browser never supplies an organisation ID. Authentication uses randomly generated opaque bearer sessions, stored as SHA-256 hashes so sessions can be revoked without exposing bearer tokens in the database.

## Current tables

| Area | Tables |
| --- | --- |
| tenancy and access | `Organisations`, `Users`, `UserSessions`, `Subscriptions` |
| customer work | `Customers`, `Sites`, `Jobs`, `JobStatuses`, `JobAssignments`, `JobItems`, `JobNotes` |
| accountability and files | `Activities`, `Documents` |

Every business-owned operational table has an `OrganisationId`. All controllers scope queries to the authenticated session's organisation; employees additionally need an assignment to read or change a job.

## Deliberately designed future tables

| Future capability | Proposed tables / key fields |
| --- | --- |
| inventory | `InventoryItems(OrganisationId, Sku, Name, Unit, ReorderPoint)`, `InventoryBalances(InventoryItemId, LocationId?, Quantity)`, `InventoryMovements(InventoryItemId, JobId?, Type, Quantity, OccurredAt)` |
| suppliers and purchasing | `Suppliers`, `PurchaseOrders`, `PurchaseOrderLines` |
| billing | `BillingCustomers`, `Invoices`, `InvoiceLines`, `PaymentProviderEvents` |
| integrations | `IntegrationConnections` (encrypted credential reference only), `WebhookDeliveries`, `OutboxMessages` |
| multi-location | `Locations`, then optional `LocationId` on inventory balances, users and jobs |
| auth hardening | `PasswordResetTokens`, `EmailVerificationTokens`, `MfaCredentials` |

When inventory arrives, `JobItems.InventoryItemId` can be nullable. Job items must continue to preserve the original human-readable name, quantity and unit so historical job records do not change when inventory is renamed or removed.

## Local run

1. Copy `.env.example` to `.env` and set a local password.
2. Run `docker compose up --build`.
3. Open `http://localhost:8080` and create the first organisation/account.

The Compose setup exposes PostgreSQL only to `127.0.0.1`, keeps database/uploads in named volumes, waits for database health, and applies EF migrations in the development stack. For API-only development, run `dotnet run --project backend/Workstock.Api` and `npm run dev` in `frontend`.

## Same-network device testing

The Docker web service publishes `0.0.0.0:8080`, so phones and tablets on the same Wi-Fi can use the app through the host machine.

1. Start the stack with `docker compose up --build`.
2. Find the host machine's IPv4 address with `ipconfig`.
3. Open `http://<host-ip-address>:8080` from the phone or tablet.

The frontend and API are served from the same origin through nginx, so authenticated API calls and uploaded image previews work from other devices. If the page does not load, allow Docker Desktop or port `8080` through Windows Firewall for private networks.

## Production checklist

- Terminate TLS at a managed load balancer/reverse proxy and set `Security__ForceHttps=true`.
- Set `Database__AutoMigrate=false`; execute `dotnet ef database update` as a single CI/CD migration job before rolling out replicas.
- Replace the local document volume with a private S3-compatible storage adapter and short-lived, access-checked download URLs.
- Put PostgreSQL on a managed private service with automated point-in-time recovery, encryption, and restore testing.
- Store configuration and database credentials in a secret manager, rotate them, and never use the sample `.env` in production.
- Run multiple stateless API replicas behind a load balancer. Move long-running emails and image processing to a queue/outbox worker.
- Add central structured logs, error tracking, metrics, alerting, database connection-pool monitoring, and readiness checks.
- Set explicit CORS origins; never use a wildcard with credentialed traffic.

## Security posture and next hardening increments

Implemented protections include PBKDF2-SHA512 passwords, server-revocable opaque sessions, server-side role checks, organisation-scoped queries, parameterised EF Core queries, input validation, auth rate limiting, audit activity, content-type/size-limited uploads, and non-public local storage paths.

Before a public launch, add email verification and password reset delivery, account lockout/backoff keyed by user and IP, CSRF protection if moving to cookie sessions, malware scanning for uploads, a content-security policy, security headers at the edge, independent penetration testing, and automated tenant-isolation tests.
