# Workstock

> Simple job, customer, inventory and workflow management for small operational businesses.

Workstock is a planned multi-tenant SaaS platform for small repair, servicing, workshop and specialist trade businesses that have outgrown spreadsheets, paper records and scattered messaging, but do not need the complexity of a full ERP or enterprise field-service platform.

The product is designed as a **responsive web application and Progressive Web App (PWA)**. Managers and owners can use the desktop-oriented interface to manage the business, while technicians can use the mobile/tablet experience to update jobs, record parts, add notes and upload photos while working.

## Product Vision

Small operational businesses often run their day-to-day work across a mixture of:

- Spreadsheets
- Paper records
- Email
- Messaging applications
- Shared folders
- Staff members' individual knowledge

Workstock aims to provide a focused operational layer between these informal systems and heavyweight enterprise software.

The initial product focuses on one core workflow:

**Customer → Job → Parts → Work → Completion**

The longer-term vision is to become an operating system for small operational businesses, gradually expanding into workflow automation, reporting, customer portals, integrations and intelligent business insights.

## Target Market

The initial target market is small businesses with approximately **1–20 employees**, particularly:

- Repair shops
- Electronics repair businesses
- Equipment servicing companies
- Workshops
- Specialist trades
- Small technical service businesses
- Small businesses that manage physical jobs and parts

The product may eventually expand into other operational SMB sectors once the initial market has been validated.

## Core Value Proposition

> **Manage customers, jobs, parts and inventory in one simple system — without the complexity or cost of an ERP.**

Workstock should make it possible for a small business to replace a collection of spreadsheets and manual processes with one coherent system without requiring an IT department or lengthy implementation.

## Platform

Workstock will initially be a:

**Responsive web application + Progressive Web App**

There will be no separate native mobile or desktop applications during the MVP.

### Desktop / Laptop

Primarily designed for owners and managers who need to:

- Review the operational dashboard
- Manage jobs
- Manage customers
- Manage inventory
- Review activity
- Upload documents
- Manage employees
- Configure the organisation

### Tablet / Mobile

Primarily designed for technicians and employees who need to:

- View assigned jobs
- Update job status
- Add notes
- Record parts
- Upload photographs
- View customer information
- Complete jobs

The mobile interface should be intentionally designed for quick actions rather than being a desktop interface simply scaled down to a small screen.

## MVP Scope

The MVP provides the following core functionality:

- Multi-tenant organisations
- Authentication and account management
- User roles and permissions
- Customer management
- Job management
- Configurable job statuses
- Job assignment
- Job priority and due dates
- Inventory management
- Inventory movement history
- Parts consumption against jobs
- Low-stock alerts
- Operational dashboard
- Global search
- Customer and job documents/attachments
- Mobile photo upload
- Basic customer email notifications
- Activity history / audit trail
- Responsive PWA experience
- Freemium subscription architecture

See [`mvp.md`](./mvp.md) for the complete product and technical MVP specification.

## Typical Workflow

A typical repair/service job should be manageable through the following flow:

```text
Customer enquiry
       ↓
Create customer
       ↓
Create job
       ↓
Assign employee
       ↓
Diagnose / perform work
       ↓
Record parts
       ↓
Update job status
       ↓
Complete testing
       ↓
Notify customer
       ↓
Complete job
```

The system maintains the relationship between the customer, job, employee, parts, documents and activity history.

## Example

A business creates:

```text
JOB-00142
Customer: ABC Electronics
Job: MacBook Pro screen replacement
Status: Awaiting Parts
Priority: Normal
Assigned: Sarah
Due: 28 August
```

The technician later records:

```text
Screen ×1
Adhesive ×1
```

Workstock records the consumption against the job and updates inventory accordingly.

Once the job is completed, the customer can receive a basic status notification and the full operational history remains associated with the job.

## High-Level Architecture

The planned architecture is:

```text
┌──────────────────────────────┐
│ React / Vite PWA             │
│ Responsive UI                │
│ Desktop + Tablet + Mobile    │
└──────────────┬───────────────┘
               │ HTTPS / REST
               ▼
┌──────────────────────────────┐
│ ASP.NET Core REST API        │
│ Authentication               │
│ Authorisation                │
│ Business Logic               │
│ Validation                   │
└──────────────┬───────────────┘
               │
       ┌───────┴────────┐
       ▼                ▼
┌──────────────┐  ┌────────────────┐
│ PostgreSQL   │  │ Object Storage │
│ Application  │  │ Documents      │
│ Data         │  │ Photos         │
└──────────────┘  └────────────────┘
```

### Planned technology stack

**Frontend**

- React
- Vite
- React Router
- Axios
- Lucide
- Recharts where useful
- Responsive CSS / CSS Modules

**Backend**

- ASP.NET Core
- Entity Framework Core
- REST API
- Server-side validation
- Authentication and authorisation

**Database**

- PostgreSQL

**Storage**

- S3-compatible object storage for documents and photos

**Deployment**

- Docker
- HTTPS
- Production cloud hosting

The exact infrastructure provider is intentionally not fixed in the MVP specification.

## Initial Data Model

The initial domain model is expected to include:

```text
organisations
users
customers
jobs
job_statuses
job_parts
inventory_items
inventory_movements
documents
activities
subscriptions
```

Every business-owned operational record must be associated with an organisation to enforce tenant isolation.

## Security Principles

Security is a first-class requirement because Workstock will store business and customer information.

The implementation should include:

- Secure password hashing
- Strong authentication/session handling
- Server-side authorisation checks
- Organisation-level tenant isolation
- Input validation
- Parameterised database queries
- Secure file handling
- HTTPS in production
- Secure secret management
- Rate limiting where appropriate
- Audit/activity logging for important actions
- Least-privilege access to uploaded files
- Safe error handling without leaking sensitive information

Security requirements will become more detailed as the implementation progresses.

## Freemium Business Model

The intended business model is subscription SaaS with a genuinely useful free tier.

### Free

Potential starting limits:

- 1 user
- 25 active jobs
- 50 customers
- 100 inventory items
- 1 GB storage
- Basic dashboard

### Pro

Indicative target:

**£12–£15/month**

Potential features:

- Up to 5 users
- Unlimited customers
- Unlimited jobs
- Unlimited inventory
- Customer email notifications
- Additional storage
- Advanced filtering/reporting

### Business

Indicative future target:

**£30–£50/month**

Potential features:

- 15+ users
- Advanced permissions
- Multiple locations
- API access
- Integrations
- Advanced audit features

Pricing is provisional and should be validated through actual customer interviews and market testing.

## Out of Scope for the MVP

The following are deliberately excluded from the first release:

- Native iOS application
- Native Android application
- Native Windows/macOS applications
- Full offline operation
- Accounting
- Payroll
- Full invoicing/accounting suite
- Full CRM
- Route optimisation
- Advanced scheduling
- POS functionality
- Supplier marketplace
- SMS integrations
- WhatsApp integrations
- Customer live chat
- Advanced workflow builder
- AI assistant
- AI document analysis
- Advanced analytics
- Multi-location management
- Complex enterprise RBAC
- Large integration ecosystem

These may become future features if validated by customer demand.

## Development Principles

Workstock should be developed around a few principles:

### Build for the first customer, not the entire market

The MVP should solve a narrow problem extremely well before broadening its scope.

### Keep the workflow simple

The product should be easier to understand than the software it replaces.

### Mobile actions should be fast

Technicians should be able to update a job with a few taps.

### Preserve operational history

Important changes should be recorded rather than silently overwritten.

### Design for SaaS from the beginning

Multi-tenancy, authentication, subscription limits and data isolation should be part of the architecture rather than retrofitted later.

### Avoid premature complexity

New functionality should be justified by an actual customer problem.

## Validation Targets

The MVP should be considered commercially validated in stages.

### Stage 1

Get approximately **3 real businesses** using the product regularly.

### Stage 2

Get at least **1 business willing to pay** for continued use.

### Stage 3

Reach approximately **10 paying businesses**.

### Stage 4

Analyse usage and customer requests to determine the highest-value V2 functionality.

The primary validation question is:

> **Will a real small business replace its existing spreadsheet/manual workflow with Workstock?**

## Potential V2 Direction

Once the core product has real users and meaningful operational data, potential extensions include:

- Customer portal
- Offline technician workflows
- Barcode scanning
- Quotes and invoices
- Supplier management
- Advanced scheduling
- Workflow automation
- Business analytics
- Automatic stock forecasting
- Integrations
- Advanced reporting
- Intelligent operational insights
- AI-assisted search and summaries

The long-term direction is:

```text
                    WORKSTOCK
                        │
       ┌────────────────┼────────────────┐
       │                │                │
     Jobs           Customers        Inventory
       │                │                │
       └────────────────┼────────────────┘
                        │
                   Documents
                        │
                    Workflow
                        │
                  Automation
                        │
                   Reporting
                        │
               Customer Portal
                        │
                 Integrations
                        │
                  AI Insights
```

## Project Status

**Current status:** MVP foundation implemented. The customer/site/job workflow, tenant-aware API, role checks, sessions, audit history, document upload foundation, PWA shell and Docker development stack are present; see [`docs/architecture.md`](./docs/architecture.md) for setup, deployment and the intentionally deferred data model.

The project is currently at the product-definition stage. The MVP specification should be treated as the primary scope reference before implementation begins.

## Documentation

- [`README.md`](./README.md) — Project overview, product vision and high-level technical direction
- [`mvp.md`](./mvp.md) — Detailed MVP product, functional and technical specification

## License

The project licence has not yet been selected.

If the software is eventually commercialised as a proprietary SaaS product, the repository should use an appropriate proprietary/commercial licence and should not expose production secrets or customer data.

---

**Workstock** — *simple operational software for businesses that have outgrown spreadsheets.*
