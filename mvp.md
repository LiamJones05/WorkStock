# Workstock MVP Specification

## 1. Product Definition

**Working name:** Workstock

**Product type:** Multi-tenant SaaS web application and Progressive Web App (PWA)

**Initial target:** Small repair, servicing, workshop and specialist trade businesses with approximately 1–20 employees.

**Primary objective:**

> Provide a simple operational system for managing customers, jobs, parts and inventory without requiring an ERP or enterprise field-service platform.

The MVP should replace fragmented operational processes such as spreadsheets, paper records, email threads and informal messaging with one coherent workflow.

The product should focus on:

**Customers → Jobs → Parts → Work → Completion**

---

# 2. Product Principles

The MVP should follow these principles.

## 2.1 Simplicity over breadth

The first version should solve a small number of business problems well.

It should not attempt to replace accounting software, CRMs, ERP systems and communication platforms simultaneously.

## 2.2 Operational usefulness over dashboards

Every major feature should help a user answer a practical question:

- What jobs need attention?
- What is currently being worked on?
- Which jobs are overdue?
- What parts have been used?
- What stock is low?
- What has happened to this job?
- What work have we done for this customer?

## 2.3 Desktop for management, mobile for execution

The desktop experience should optimise for information density and administration.

The mobile/tablet experience should optimise for quick actions by technicians.

## 2.4 SaaS-first architecture

The application must support multiple independent customer organisations from the beginning.

## 2.5 Auditable changes

Important operational changes should produce activity records.

## 2.6 Avoid premature automation

Automation and AI should only be introduced where they solve a validated problem.

---

# 3. Platform and Device Strategy

## 3.1 Primary platform

The MVP will be a **responsive web application** delivered as a **Progressive Web App**.

There will be one application codebase.

No separate native applications are required for the MVP.

## 3.2 Desktop experience

Optimised for:

- Owners
- Managers
- Office staff

Typical activities:

- Dashboard review
- Customer management
- Job management
- Inventory management
- Employee management
- Document management
- Reporting
- Settings

## 3.3 Tablet experience

Designed as a practical middle ground for workshops and service environments.

Typical activities:

- Viewing jobs
- Updating status
- Recording parts
- Adding notes
- Uploading photographs
- Viewing documents

## 3.4 Mobile experience

Optimised for technicians who are standing or moving around a work environment.

The interface should provide prominent actions such as:

- Change status
- Add note
- Add part
- Upload photo
- View customer
- Complete job

The mobile UI should not simply be a compressed desktop dashboard.

## 3.5 PWA requirements

The MVP should include:

- HTTPS
- Web App Manifest
- App icons
- Standalone display mode
- Responsive layouts
- Service worker
- Basic application-shell caching
- Installable experience on supported devices

Full offline data synchronisation is out of scope for the MVP.

---

# 4. Primary User Journey

The core user journey is:

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

The system should maintain relationships between:

```text
Customer
   │
   └── Job
         ├── Employee
         ├── Parts
         ├── Documents
         ├── Notes
         └── Activity history
```

---

# 5. Organisations / Tenancy

Workstock is a multi-tenant SaaS.

Each organisation represents one independent business account.

Conceptually:

```text
Organisation
│
├── Users
├── Customers
├── Jobs
├── Inventory
├── Documents
├── Activities
└── Subscription
```

All business-owned records must contain an organisation relationship.

## Requirements

- Users belong to an organisation.
- Customers belong to an organisation.
- Jobs belong to an organisation.
- Inventory belongs to an organisation.
- Documents belong to an organisation.
- Activity records belong to an organisation.
- API requests must be authorised against the authenticated user's organisation.
- Cross-tenant data access must be prevented at the application and database query level.

---

# 6. Authentication

The MVP should provide:

- Registration
- Login
- Logout
- Password hashing
- Email verification
- Password reset
- Session/token management
- Basic account/profile management

Potential later additions:

- Microsoft login
- Google login
- MFA
- SSO

These are not required for the MVP.

---

# 7. User Roles

The MVP has three primary roles.

## 7.1 Owner

Full organisation access.

Can:

- Manage organisation
- Manage users
- Manage subscription
- Manage customers
- Manage jobs
- Manage inventory
- Access reporting
- Change settings

## 7.2 Manager

Operational management access.

Can:

- Manage customers
- Manage jobs
- Manage inventory
- Review employee work
- View reporting
- View operational history

Cannot:

- Manage billing/subscription
- Delete the organisation
- Perform owner-only organisation settings

## 7.3 Employee

Operational execution access.

Can:

- View relevant jobs
- Update assigned jobs
- Add notes
- Add parts
- Upload documents/photos
- Change permitted statuses

Cannot:

- Manage users
- Manage subscription
- Change organisation settings
- Perform critical administrative actions

The exact permission matrix can be refined during implementation.

---

# 8. Customer Management

## Customer fields

Each customer should have:

- ID
- Name
- Company name (optional)
- Email
- Phone
- Address
- Notes
- Created timestamp
- Updated timestamp
- Last activity timestamp

## Customer relationships

A customer can have:

- Many jobs
- Many documents
- Many activities

## Customer page

The customer page should show:

```text
Customer details
       ↓
Contact information
       ↓
Active jobs
       ↓
Completed jobs
       ↓
Documents
       ↓
Recent activity
```

The intended question is:

> **"What work have we previously done for this customer?"**

---

# 9. Job Management

Jobs are the central operational entity.

## Job fields

Each job should contain:

- Unique ID
- Human-readable job number
- Customer ID
- Title
- Description
- Status
- Priority
- Assigned user
- Created timestamp
- Due date
- Completed timestamp
- Estimated price
- Actual price
- Internal notes
- Customer-facing notes
- Organisation ID

## Optional metadata

Potential fields that may be included if they prove useful:

- External reference
- Asset/device identifier
- Serial number
- Warranty status

These should not be expanded into a full asset-management system during the MVP.

---

# 10. Job Status Workflow

The default workflow is:

```text
New
 ↓
Received
 ↓
Diagnosing
 ↓
Awaiting Approval
 ↓
Awaiting Parts
 ↓
In Progress
 ↓
Testing
 ↓
Ready for Collection
 ↓
Completed
```

A separate:

```text
Cancelled
```

terminal state should exist.

## Status rules

A job should have exactly one active status.

Status changes should:

1. Validate the new status.
2. Update the job.
3. Create an activity record.
4. Record who made the change.
5. Record when the change occurred.

Example:

```text
23 Aug 2026 16:42
Sarah changed JOB-00142:
Testing → Ready for Collection
```

The MVP may allow limited customisation of statuses, but a visual workflow designer is out of scope.

---

# 11. Job Views

## Desktop

The main jobs area should support:

- Table/list view
- Search
- Status filtering
- Employee filtering
- Priority filtering
- Due-date filtering
- Sorting
- Quick status actions

Example:

| Job | Customer | Assigned | Status | Due |
|---|---|---|---|---|
| #142 | ABC Electronics | Sarah | Awaiting Parts | Today |
| #141 | Smith Ltd | Liam | In Progress | Today |
| #140 | John Smith | Sarah | Testing | Tomorrow |

## Mobile

The mobile job page should prioritise:

1. Job title/reference
2. Customer
3. Status
4. Due date
5. Assigned employee
6. Parts
7. Notes
8. Documents/photos

Primary actions:

- Change status
- Add note
- Add part
- Upload photo

---

# 12. Inventory Management

Inventory items represent physical parts, materials or consumables used by the business.

## Inventory fields

Each item should include:

- ID
- Name
- SKU
- Category
- Description
- Quantity
- Minimum stock level
- Unit cost
- Supplier
- Storage location
- Optional barcode
- Active status
- Created timestamp
- Updated timestamp

Example:

```text
HDMI Port
SKU: HDMI-001
Stock: 42
Minimum: 10
Unit cost: £1.20
Supplier: Example Electronics
```

---

# 13. Inventory Movements

Inventory should use movement records rather than silently changing quantities.

Examples:

```text
+50  Purchase received
-1   Used on JOB-00142
-3   Manual adjustment
+20  Stock delivery
```

## Inventory movement fields

Potential fields:

- ID
- Inventory item ID
- Organisation ID
- Quantity change
- Movement type
- Reference type
- Reference ID
- User ID
- Timestamp
- Notes

Possible movement types:

- Purchase
- Job consumption
- Manual adjustment
- Stock correction
- Return

This creates an audit trail for inventory.

---

# 14. Jobs and Parts

Users must be able to associate inventory items with jobs.

Example:

```text
JOB-00142

Screen              ×1
Adhesive            ×1
Thermal paste       ×0.1
```

The job-part record should contain enough information to establish:

- Which job consumed the part
- Which inventory item was consumed
- How much was consumed
- Who recorded it
- When it was recorded

The system should then create the corresponding inventory movement.

Example:

```text
HDMI Ports
42 → 41

Movement:
-1
Reason: JOB-00142 consumption
```

The implementation must account for insufficient stock and invalid quantities.

---

# 15. Low-Stock Alerts

Every inventory item may define a minimum stock threshold.

The system should identify:

- Low-stock items
- Out-of-stock items

Example:

```text
LOW STOCK

HDMI Ports
7 remaining
Minimum: 10
```

The dashboard should surface these items.

---

# 16. Dashboard

The dashboard should answer:

> **"What needs attention?"**

## Example overview

```text
17
Open Jobs

4
Due Today

7
Awaiting Parts

3
Low Stock

2
Ready for Collection
```

## Jobs requiring attention

Prioritise:

- Overdue jobs
- Jobs due today
- High-priority jobs
- Jobs awaiting action
- Jobs waiting for parts

## Inventory alerts

Show:

- Low-stock items
- Out-of-stock items

The MVP dashboard should remain operational rather than becoming a complete BI platform.

---

# 17. Search

A global search should support:

- Customers
- Jobs
- Job numbers
- Inventory
- SKUs
- Relevant notes where practical

Example query:

```text
ABC
```

Possible results:

```text
Customers
ABC Electronics

Jobs
JOB-00142 — MacBook repair

Inventory
ABC-USB-C-01
```

The search implementation should prioritise speed and usefulness over sophisticated semantic search.

---

# 18. Documents and Attachments

Documents can be associated with customers or jobs.

Examples:

- Diagnostic reports
- Quotes
- Customer photographs
- Repair photographs
- Supplier documents
- Reference documents

## MVP operations

Users can:

- Upload
- View metadata
- Download
- Delete
- Associate/re-associate where appropriate

## File storage

Files should be stored in object storage.

PostgreSQL should store metadata and references rather than large binary files.

---

# 19. Mobile Photo Upload

Mobile/tablet users should be able to capture or select images directly from the device and attach them to a job.

Example workflow:

```text
JOB-00142
     ↓
Add Photo
     ↓
Camera / Device
     ↓
Capture image
     ↓
Upload
     ↓
Image added to job
```

This is particularly relevant for repair and servicing environments.

---

# 20. Customer Communication

The MVP should support basic email notifications.

It should not attempt to become a full communication platform.

## Example

When a job reaches:

```text
Ready for Collection
```

the user can send:

> Your repair is ready for collection.

The email can include:

- Customer name
- Job number
- Job title
- Current status
- Customer-facing message
- Business name
- Business contact information

The initial release should focus on email.

SMS, WhatsApp and other channels are future features.

---

# 21. Activity History

Important actions should create activity records.

Examples:

```text
23 Aug 16:42
Sarah changed status:
Testing → Ready for Collection

23 Aug 15:21
1 × HDMI Port added to job

23 Aug 14:03
Status changed:
In Progress → Testing

22 Aug 10:32
Technician note added
```

## Activity record

Potential fields:

- ID
- Organisation ID
- User ID
- Entity type
- Entity ID
- Action type
- Description
- Timestamp
- Optional metadata

The purpose is operational visibility and accountability.

---

# 22. Responsive Design Requirements

The application must be usable at:

- Desktop monitor widths
- Laptop widths
- Tablet portrait/landscape
- Mobile portrait
- Mobile landscape

## Desktop-first areas

Prioritise information density for:

- Dashboard
- Jobs list
- Customers
- Inventory
- Reporting
- Organisation administration

## Mobile-first areas

Prioritise rapid interaction for:

- Job details
- Status changes
- Notes
- Parts
- Photos
- Customer details

The responsive implementation should avoid requiring horizontal scrolling for normal mobile workflows.

---

# 23. PWA Requirements

The MVP PWA should provide:

- `manifest.json`
- Application icons
- Theme/background metadata
- Standalone display
- Service worker
- Basic static asset caching
- HTTPS
- Installability where supported

The application should still treat the server as the source of truth.

## Offline

Full offline operation is **not** an MVP requirement.

Potential V2 behaviour:

```text
Server
  ↕
Local cache/database
  ↕
Technician UI
```

with queued changes and conflict resolution.

---

# 24. Subscription Architecture

The MVP should support plan-based limits even if the payment system is introduced incrementally.

## Free

Initial proposed limits:

- 1 user
- 25 active jobs
- 50 customers
- 100 inventory items
- 1 GB storage
- Basic dashboard

## Pro

Indicative:

**£12–£15/month**

Potential entitlements:

- 5 users
- Unlimited jobs
- Unlimited customers
- Unlimited inventory
- Email notifications
- Higher storage limit
- Advanced filtering/reporting

## Business

Indicative:

**£30–£50/month**

Potential future entitlements:

- 15+ users
- Advanced permissions
- Multiple locations
- API access
- Integrations
- Advanced audit functionality

Pricing is provisional and must be validated.

---

# 25. Billing

A payment provider such as Stripe can be introduced for subscription billing.

The application should conceptually support:

```text
Organisation
    ↓
Subscription
    ↓
Plan
    ↓
Entitlements / Limits
```

The frontend should not be trusted to enforce plan limits.

Entitlement checks must occur server-side.

---

# 26. Data Model

An initial logical model is:

```text
organisations
    │
    ├── users
    │
    ├── customers
    │       └── jobs
    │              ├── job_parts
    │              ├── documents
    │              └── activities
    │
    ├── inventory_items
    │       └── inventory_movements
    │
    ├── documents
    │
    ├── activities
    │
    └── subscriptions
```

Potential table definitions should be refined during implementation.

---

# 27. API Structure

A REST API is sufficient for the MVP.

Potential endpoints:

```text
/api/auth
    POST /register
    POST /login
    POST /logout
    POST /forgot-password
    POST /reset-password

/api/users
    GET /
    GET /:id
    POST /
    PATCH /:id
    DELETE /:id

/api/customers
    GET /
    GET /:id
    POST /
    PATCH /:id
    DELETE /:id

/api/jobs
    GET /
    GET /:id
    POST /
    PATCH /:id
    DELETE /:id
    POST /:id/status
    POST /:id/parts
    POST /:id/notes
    POST /:id/documents

/api/inventory
    GET /
    GET /:id
    POST /
    PATCH /:id
    DELETE /:id
    POST /:id/movements

/api/documents
    GET /:id
    POST /
    DELETE /:id

/api/activity
    GET /

/api/organisation
    GET /
    PATCH /

/api/subscription
    GET /
    POST /checkout
    POST /portal
```

Exact API design should be established before implementation.

---

# 28. Validation and Error Handling

All API input must be validated server-side.

Validation should cover:

- Required fields
- String lengths
- Email addresses
- Dates
- Numeric values
- Inventory quantities
- Valid IDs
- Ownership/tenant access
- Allowed status transitions where applicable

The API should return consistent error responses.

Sensitive implementation details must not be exposed in production error messages.

---

# 29. Security Requirements

Security is important because the product will contain business data.

Minimum requirements:

- Secure password hashing
- Secure authentication/session handling
- Server-side authorisation
- Tenant isolation
- Parameterised database queries
- Input validation
- Rate limiting where appropriate
- Secure secrets/environment configuration
- HTTPS
- Secure file uploads
- Safe file access
- Access-controlled documents
- Audit logging
- Minimal sensitive data collection
- Secure production configuration

Particular care must be taken with multi-tenant query construction.

A user should never be able to supply an arbitrary organisation/customer/job ID and access another organisation's records.

---

# 30. Out of Scope

The MVP explicitly excludes:

### Applications

- Native iOS
- Native Android
- Native Windows
- Native macOS

### Connectivity

- Full offline mode
- Offline conflict resolution
- Background data synchronisation

### Business functions

- Accounting
- Payroll
- Full invoicing
- POS
- Full CRM
- Route optimisation
- Advanced scheduling
- Supplier marketplace

### Communication

- SMS
- WhatsApp
- Live customer chat

### Intelligence

- AI assistant
- AI document analysis
- AI forecasting
- Semantic knowledge system

### Enterprise functionality

- Complex RBAC
- Multi-location support
- SSO
- Enterprise integrations
- Large public API

These should only be introduced based on validated demand.

---

# 31. Suggested Development Phases

## Phase 1 — Foundation

- Repository setup
- Frontend project
- Backend project
- PostgreSQL
- Docker development environment
- Environment management
- Database migrations
- Basic CI
- Authentication foundation

## Phase 2 — Tenancy and Users

- Organisations
- User membership
- Roles
- Tenant isolation
- Account settings

## Phase 3 — Customers

- Customer CRUD
- Customer list
- Search
- Customer detail page
- Customer job history

## Phase 4 — Jobs

- Job CRUD
- Job numbering
- Statuses
- Assignment
- Priorities
- Due dates
- Notes
- Job detail page
- Activity records

## Phase 5 — Inventory

- Inventory CRUD
- Stock levels
- Stock movements
- Job parts
- Automatic consumption
- Low-stock alerts

## Phase 6 — Documents

- Object-storage integration
- Uploads
- Downloads
- Job/customer attachments
- Mobile photo upload

## Phase 7 — Dashboard

- Operational metrics
- Jobs requiring attention
- Inventory alerts
- Basic filtering

## Phase 8 — Communications

- Email provider
- Customer status emails
- Business notification settings

## Phase 9 — PWA

- Manifest
- Icons
- Service worker
- Installability
- Responsive polish
- Mobile workflow refinement

## Phase 10 — Monetisation

- Plans
- Entitlements
- Usage limits
- Stripe integration
- Subscription management

## Phase 11 — Hardening

- Security review
- Tenant isolation testing
- Validation testing
- Error handling
- Performance testing
- Backup/restore verification
- Production deployment

---

# 32. MVP Acceptance Criteria

The MVP should not be considered complete merely because every screen exists.

A business should be able to perform the following without manual database intervention:

1. Create an organisation.
2. Create users and assign roles.
3. Create a customer.
4. Create a job for that customer.
5. Assign the job to an employee.
6. Update the job status.
7. Add notes and documents.
8. Add inventory items.
9. Associate inventory with a job.
10. Record part consumption.
11. See the inventory quantity change.
12. See low-stock warnings.
13. View relevant job/customer activity.
14. Search for customers, jobs and inventory.
15. Use the main technician workflow from a phone/tablet.
16. Upload a photograph from a mobile device.
17. Send a basic customer email.
18. Enforce organisation-level permissions.
19. Prevent cross-tenant data access.
20. Restrict features according to subscription entitlements.

---

# 33. Validation Targets

The first goal is not scale.

It is validation.

## Stage 1 — Usage

Get approximately:

**3 real businesses**

using Workstock regularly.

## Stage 2 — Payment

Get at least:

**1 business willing to pay**

for continued access.

## Stage 3 — Repeatability

Reach:

**10 paying businesses**

and identify common requests and usage patterns.

## Primary validation question

> **Will a real small business replace its existing spreadsheet/manual workflow with Workstock?**

Secondary questions:

- Which feature brings them into the product?
- Which feature makes them return?
- Which features are ignored?
- What do they still manage outside Workstock?
- What would they pay for?
- What prevents adoption?
- Which sector has the strongest product fit?

---

# 34. V2 Candidates

Only after MVP validation, investigate:

- Customer portal
- Barcode scanning
- Offline technician mode
- Quotes
- Invoicing
- Supplier management
- Advanced scheduling
- Multiple locations
- Workflow automation
- Advanced reports
- Inventory forecasting
- Email/SMS/WhatsApp integrations
- Public API
- AI-assisted search
- AI-generated job/customer summaries
- Operational anomaly detection

These are potential extensions rather than commitments.

---

# 35. Long-Term Product Direction

The long-term product could evolve from:

```text
Customers
   +
Jobs
   +
Inventory
```

into:

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

The long-term proposition is:

> **The operating system for small operational businesses.**

The MVP should resist trying to become this entire platform immediately.

---

# 36. Non-Goals for Product Design

The product should not compete by having more buttons or more modules.

It should compete on:

- Ease of adoption
- Speed of common actions
- Clear operational visibility
- Sensible defaults
- Mobile usability
- Transparent pricing
- Low administrative overhead
- Reliable operational history

A business should be able to start using Workstock without training sessions or consultants.

---

# 37. Initial Product Success Definition

The MVP is successful when a small business can say:

> **"We used to manage this in Excel and WhatsApp. Now we use Workstock."**

That is the key outcome the first release should optimise for.
