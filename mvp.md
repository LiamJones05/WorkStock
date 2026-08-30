# Workstock MVP Specification

## 1. Product Definition

**Working name:** Workstock

**Product type:** Multi-tenant SaaS web application and Progressive Web App (PWA)

**Initial target:** Independent tradespeople and small trade businesses, including plumbers, electricians, painters/decorators, bricklayers, carpenters/joiners, builders, tilers, roofers, heating engineers, landscapers and similar job-based trades.

The MVP should support both:
- Solo tradespeople who work alone and purchase or bring the supplies required for each job.
- Small trade businesses with a team of employees and a growing number of jobs.

**Primary objective:**

> Provide simple job management for tradespeople without requiring a complex CRM, ERP or enterprise field-service platform.

The MVP should replace fragmented operational processes such as spreadsheets, paper records, calendars, email threads, messaging apps and informal notes with one coherent workflow.

The product should focus on:

**Customer → Site → Job → Schedule → Items Required → Work → Completion**

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

Primarily used for:

- Owners
- Managers
- Office/admin users

Typical activities:

- Reviewing the schedule
- Managing customers and sites
- Creating and managing jobs
- Managing employees
- Reviewing documents and photos
- Viewing reports
- Organisation settings

The desktop experience should prioritise efficient administration and visibility across many jobs rather than requiring every workflow to be performed from a desktop.

## 3.3 Tablet experience

Designed for use in vans, workshops and on larger job sites.

Typical activities:

- Viewing the day's jobs
- Opening job/site information
- Updating status
- Reviewing items required
- Adding notes
- Uploading photographs
- Viewing documents

The tablet experience should provide a larger touch interface while retaining the same operational workflows as mobile.

## 3.4 Mobile experience

**Mobile is a primary operational platform for the MVP.**

The mobile interface is primarily intended for solo tradespeople and employees who may be working from a van, customer property or construction site.

The interface should provide prominent actions such as:

- View today's jobs
- View the job/site address
- Change status
- View or update items required
- Add note
- Upload photo
- View customer/site information
- Complete job

The mobile UI should not simply be a compressed desktop dashboard. Common job actions should require very few taps and should remain usable while the user is standing or moving around a work environment.

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

The core MVP workflow is:

```text
Customer enquiry
       ↓
Create / select customer
       ↓
Create / select site
       ↓
Create job
       ↓
Schedule job
       ↓
Add items required
       ↓
Assign employee (if applicable)
       ↓
Travel to / arrive at site
       ↓
Perform work
       ↓
Update status / add notes / photos
       ↓
Complete job
       ↓
Notify customer (if applicable)
```

For a solo tradesperson, the workflow may be as simple as:

```text
Customer
   ↓
Site
   ↓
Job
   ↓
Items Required
   ↓
Buy / bring supplies
   ↓
Complete work
```

For a small business:

```text
Customer
   ↓
Site
   ↓
Job
   ↓
Schedule
   ↓
Employee assignment
   ↓
Items Required
   ↓
Work
   ↓
Completion
```

The system should maintain relationships between:

```text
Customer
   │
   └── Sites
         │
         └── Jobs
               ├── Users / Assignments
               ├── Job Items
               ├── Documents / Photos
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
├── Users / Memberships
├── Customers
│   └── Sites
├── Jobs
│   ├── Assignments
│   ├── Items Required
│   ├── Documents / Photos
│   ├── Notes
│   └── Activities
├── Job Statuses / Types
└── Subscription
```

All business-owned records must contain an organisation relationship.

## Requirements

- Users belong to an organisation.
- Customers belong to an organisation.
- Jobs belong to an organisation.
- Job items belong to jobs and therefore indirectly to an organisation.
- Documents belong to an organisation.
- Activity records belong to an organisation.
- Inventory is not required for the MVP and is a future optional capability.
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
- Add items
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

# 9. Sites / Job Locations

A site represents the physical location where a customer's work is carried out.

This is separate from the customer because one customer may have multiple properties or job locations.

## Site fields

Potential fields:

- Unique ID
- Customer ID
- Name / reference
- Address line 1
- Address line 2
- City
- County
- Postcode
- Country
- Access instructions
- Site notes
- Created timestamp
- Updated timestamp

Example:

```text
Customer: Greenfield Property Management

Site A:
14 High Street

Site B:
82 Victoria Road
```

A site can have many jobs.

```text
Customer
   └── Site
        └── Jobs
```

---

# 10. Job Management

Jobs are the central operational entity.

## Job fields

Each job should contain:

- Unique ID
- Human-readable job number
- Customer ID
- Site ID
- Title
- Description
- Status
- Priority
- Assigned user / assignment information
- Scheduled start
- Scheduled end
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
- Customer reference
- Asset/device identifier
- Serial number
- Warranty status

These should not be expanded into a full asset-management system during the MVP.

---

# 11. Job Status Workflow

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

# 12. Job Views

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
6. Items Required
7. Notes
8. Documents/photos

Primary actions:

- Change status
- Add note
- Add item
- Upload photo

---

# 12.1 Job Items — Items Required

Job Items are part of the **MVP core**.

They represent the supplies, materials, tools, equipment or other items that a tradesperson expects to need for a job.

A Job Item does **not** require a stock record.

Example:

```text
JOB-00142

Items Required
-------------------------
22mm copper pipe     3m
22mm elbow            4
Isolation valve       1
PTFE tape             1 pack
```

## Job item fields

Each Job Item should contain:

- Unique ID
- Job ID
- Name
- Quantity
- Unit
- Notes
- Created timestamp
- Updated timestamp

The `Unit` should support common trade usage such as:

- item
- metre
- length
- box
- pack
- litre
- kg

A tradesperson can therefore use Workstock without maintaining any inventory.

## Future inventory integration

In a future version, a Job Item may optionally reference an Inventory Item:

```text
JobItem
   │
   └── InventoryItemId (optional)
```

This preserves the same workflow while allowing businesses with stock to add inventory tracking later.

---

# 13. Inventory Management — Future Capability

Inventory is intentionally **not part of the MVP**.

The MVP should work for a solo tradesperson who has no formal inventory system and simply needs to record what supplies, materials or other items are required for a job.

A future inventory capability may allow organisations to create reusable inventory items, maintain stock levels and link job items to stock.

Inventory should therefore be treated as an optional extension rather than a prerequisite for using Workstock.

---

# 14. Inventory Movements — Future Capability

Inventory movement tracking is also out of scope for the MVP.

A future implementation may record events such as:

```text
+50  Purchase received
-1   Used on JOB-00142
-3   Manual adjustment
```

This future capability should build on the existing job-item model rather than replacing it.

---

# 15. Jobs and Inventory — Future Capability

The MVP's Job Item is deliberately independent of inventory.

MVP:

```text
Job
 └── JobItem
       ├── Name
       ├── Quantity
       ├── Unit
       └── Notes
```

Future:

```text
Job
 └── JobItem
       ├── Name
       ├── Quantity
       ├── Unit
       ├── Notes
       └── InventoryItemId (optional)
```

This allows the same Workstock job to work for both:

- A solo tradesperson who buys or brings supplies as needed.
- A small business that keeps stock and wants to allocate materials to jobs.

---

# 16. Low-Stock Alerts — Future Capability

Low-stock alerts are not part of the MVP.

They become relevant when an organisation enables the future inventory capability.

Potential future behaviour:

```text
LOW STOCK

22mm copper pipe
8m remaining
Minimum: 10m
```

The existing Job Item model should remain usable without this capability.

---

# 17. Dashboard and Schedule

The dashboard should primarily answer:

> **"What do I need to do today?"**

For tradespeople, the schedule is more important than a traditional business KPI dashboard.

## Example

```text
Wednesday 26 August

08:00
Boiler Repair
Sarah Smith
12 High Street
Scheduled

10:30
Repaint Hallway
John Smith
45 Elm Road
Scheduled

13:00
Door Installation
82 Victoria Road
In Progress
```

The dashboard should surface:

- Today's jobs
- Upcoming jobs
- Overdue jobs
- Jobs awaiting action
- Jobs awaiting customer approval
- Jobs recently completed

Basic metrics may still be shown, but operational scheduling should be the primary focus.

# 18. Search

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

# 19. Documents and Attachments

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

# 20. Mobile Photo Upload

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

# 21. Customer Communication

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

# 22. Activity History

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

# 23. Responsive Design Requirements

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
- Items Required
- Photos
- Customer details

The responsive implementation should avoid requiring horizontal scrolling for normal mobile workflows.

---

# 24. PWA Requirements

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

# 25. Subscription Architecture

The MVP should support plan-based limits even if the payment system is introduced incrementally.

## Free

Initial proposed limits:

- 1 user
- 25 active jobs
- 50 customers
- 100 job items across active jobs
- 1 GB storage
- Basic dashboard

## Pro

Indicative:

**£12–£15/month**

Potential entitlements:

- 5 users
- Unlimited jobs
- Unlimited customers
- Unlimited job items
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

# 26. Billing

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

# 27. Data Model

An initial logical model is:

```text
organisations
    │
    ├── users / memberships
    │
    ├── customers
    │       └── sites
    │              └── jobs
    │                     ├── job_assignments
    │                     ├── job_items
    │                     ├── documents
    │                     ├── notes
    │                     └── activities
    │
    ├── job_statuses
    ├── job_types
    ├── documents
    ├── activities
    └── subscriptions

Future / optional:
    ├── inventory_items
    ├── inventory_movements
    └── suppliers
```

Potential table definitions should be refined during implementation.

---

# 28. API Structure

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
    POST /:id/items
    POST /:id/notes
    POST /:id/documents

/api/inventory  # future capability
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

# 29. Validation and Error Handling

All API input must be validated server-side.

Validation should cover:

- Required fields
- String lengths
- Email addresses
- Dates
- Numeric values
- Job item quantities
- Valid IDs
- Ownership/tenant access
- Allowed status transitions where applicable

The API should return consistent error responses.

Sensitive implementation details must not be exposed in production error messages.

---

# 30. Security Requirements

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

# 31. Out of Scope

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

### Optional future capabilities

The following are intentionally deferred until demand is validated:

- Inventory management
- Inventory movements
- Supplier management
- Stock levels
- Low-stock alerts
- Barcode scanning

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

# 32. Suggested Development Phases

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

## Phase 5 — Sites and Job Items

- Site CRUD
- Site/customer/job relationships
- Job item / items-required CRUD
- Job item quantities and units
- Mobile job-item workflow

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

# 33. MVP Acceptance Criteria

The MVP should not be considered complete merely because every screen exists.

A business should be able to perform the following without manual database intervention:

1. Create an organisation.
2. Create users and assign roles.
3. Create a customer.
4. Create a job for that customer.
5. Assign the job to an employee.
6. Update the job status.
7. Add notes and documents.
8. Add items required to a job.
9. Update and review required items on mobile/tablet.
10. Schedule jobs and view the day's work.
11. View relevant job/customer/site activity.
12. Search for customers, jobs and sites.
13. Upload job photographs/documents.
14. Use the main tradesperson workflow from a phone/tablet.
15. Upload a photograph from a mobile device.
16. Send a basic customer email.
17. Enforce organisation-level permissions.
18. Prevent cross-tenant data access.
19. Restrict features according to subscription entitlements.

---

# 34. Validation Targets

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

# 35. V2 Candidates

Only after MVP validation, investigate:

- Customer portal
- Inventory management
- Stock tracking and low-stock alerts
- Supplier management
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

# 36. Long-Term Product Direction

The long-term product could evolve from:

```text
Customers
   +
Sites
   +
Jobs
   +
Job Items
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

> **Simple job management for tradespeople, with optional business capabilities as they grow.**

The MVP should resist trying to become this entire platform immediately.

---

# 37. Non-Goals for Product Design

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

# 38. Initial Product Success Definition

The MVP is successful when a small business can say:

> **"I used to manage my jobs across WhatsApp, my calendar and notes. Now I use Workstock."**

That is the key outcome the first release should optimise for.
