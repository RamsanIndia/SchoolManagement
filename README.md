🧩 Business Domains Covered (Modular Monolith)

Student Management

Attendance (Biometric + Manual)

Examination & Results

Fee & Payment Management

HRMS (Employees, Payroll, Leave)

User, Role & Permission Management

Notification System (Email / SMS / Push)

Tenant Management (SaaS)

🏗️ System Architecture Overview (Modular Monolith)
Client (React / Mobile)
        ↓
ASP.NET Core Web API (Single Deployable Application)
        ↓
-------------------------------------------------
|            Modular Monolithic Core             |
-------------------------------------------------
| School | Students | Attendance | Exams | Fees |
| HRMS | Users | Notifications | Tenant |
-------------------------------------------------
        ↓
PostgreSQL Database (Tenant Aware)

✅ Single deployment unit
✅ Internally separated by business modules
✅ Can evolve to microservices later

🧠 Architectural Principles

Separation of Concerns

Dependency Inversion

Module Isolation (No tight coupling between modules)

High Testability

Internal Event-based Communication

Future Microservices Extraction Ready

🏢 Multi-Tenancy Design (SaaS Ready)
Tenant Strategy

Shared Database + TenantId (Current)

Schema-per-tenant / DB-per-tenant (Future)

Tenant Resolution

JWT Claim (tenant_id)

HTTP Header (X-Tenant-Id)

Subdomain-based (Future)

Data Isolation

TenantId present in all core entities

EF Core Global Query Filters

Zero cross-tenant data leakage

modelBuilder.Entity<Student>()
    .HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
🧱 Clean Architecture Structure
├── API Layer
│   └── Controllers, Middleware, Authorization
├── Application Layer
│   └── CQRS (Commands, Queries, Handlers)
├── Domain Layer
│   └── Entities, Value Objects, Business Rules
├── Infrastructure Layer
│   └── External Services, Background Jobs
└── Persistence Layer
    └── EF Core, Repositories, Unit of Work

✔ Domain logic is framework-independent
✔ Infrastructure is replaceable without breaking business logic

🔁 CQRS Implementation

Commands → Write operations (Transactional)

Queries → Read operations (Optimized)

MediatR used for request handling

✅ Improves maintainability
✅ Supports future scaling

🔐 Security Architecture
Authentication

JWT Bearer Tokens

Tenant-aware claims

Configurable token expiry

Authorization

Role-Based Access Control (RBAC)

Menu-Level Permissions

Action-based access (Read / Write / Delete)

[MenuPermission("Students", "Write")]
public async Task<IActionResult> CreateStudent()
📦 Modular Monolith Modules Overview
Module	Responsibility
SchoolManagement	Core admin, menus, roles
StudentManagement	Student lifecycle
Attendance	Attendance & biometrics
Examination	Exams & results
FeeManagement	Fees & payments
HRMS	Employees & payroll
UserManagement	Authentication & users
Notification	Email/SMS/Push
TenantManagement	SaaS tenant control

👉 module has:

Domain

Application

Infrastructure

Persistence layer (within the same solution)

🗄️ Data Architecture

Primary DB: PostgreSQL

ORM: Entity Framework Core 8

Patterns Used

Repository

Unit of Work

📡 Event-Driven (Within Modular Monolith)

Domain Events

Outbox Pattern

Azure Service Bus

Use Cases:

Attendance marked → Notify parents

Fee paid → Update student status

Employee onboarded → Auto create user

☁️ Cloud & DevOps Readiness

Docker-ready (Single container now)

Azure App Service deployment

Azure Key Vault for secrets

CI/CD via Azure DevOps / GitHub Actions

Application Insights monitoring

🧪 Quality & Best Practices

Async/await everywhere

FluentValidation

Global exception handling

Structured logging (Serilog)

SOLID principles

High testability

📈 Non-Functional Capabilities
Aspect	Status
Scalability	Modular scaling
Availability	99.9% ready
Security	Enterprise-grade
Performance	Optimized
Maintainability	High
🔮 Roadmap
Phase 1

API Gateway

Refresh Tokens

Internal event-driven communication

Phase 2

Redis caching

Database per module

Advanced reporting

Phase 3

Extract modules to Microservices

Kubernetes deployment

AI-based analytics

Mobile application

👨‍💻 Author

Naveen Kumar
Senior Backend Developer | .NET Core | Azure | Clean Architecture

📌 Expertise

Modular Monolith & Microservices Architecture

SaaS Multi-Tenant Systems

Azure Cloud & DevOps

High-Performance Backend Systems
