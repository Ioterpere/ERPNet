# ERPNet

Sistema ERP didáctico desarrollado con .NET 10 y Blazor WebAssembly. El objetivo del proyecto es demostrar la implementación práctica de **Clean Architecture**, patrones de diseño modernos y un stack tecnológico completo en un contexto realista: la gestión interna de una empresa.

> **Nota**: Este proyecto está pensado como portfolio y referencia de aprendizaje. Se prioriza la claridad del código sobre la optimización prematura.

---

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Backend API | ASP.NET Core 10, EF Core 10 |
| Base de datos | SQL Server 2022 |
| Autenticación | JWT Bearer + Refresh Tokens |
| Frontend | Blazor WebAssembly (InteractiveAuto) |
| Almacenamiento de archivos | MinIO (S3-compatible) |
| Mensajería | RabbitMQ 4 |
| Email | MailKit + MailHog (dev) |
| Validación | FluentValidation 12 |
| Documentación API | Scalar (OpenAPI 3.1) |
| Testing | xUnit + NSubstitute |
| Infraestructura dev | Docker Compose |

---

## Arquitectura

El proyecto sigue **Clean Architecture** con dependencias estrictamente unidireccionales:

```
┌─────────────────────────────────────────────────────────────┐
│  ERPNet.Web.Blazor  (BFF :7100)  +  ERPNet.Web.Blazor.Client│
│                   Blazor InteractiveAuto                      │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTP (ApiClient tipado / NSwag)
┌────────────────────────▼────────────────────────────────────┐
│                    ERPNet.Api  (:7268)                        │
│   Controllers · Middleware · JWT · Rate Limiting · MinIO      │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                 ERPNet.Application                            │
│   Interfaces · Services · FluentValidation · Mailing         │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                  ERPNet.Domain                                │
│      Entities · Value Objects · Repository Interfaces         │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│               ERPNet.Infrastructure                           │
│   EF Core · Repositories · RabbitMQ · MinIO · SMTP           │
└─────────────────────────────────────────────────────────────┘

ERPNet.ApiClient  ←  cliente HTTP generado por NSwag desde OpenAPI
ERPNet.Testing    ←  xUnit + NSubstitute + DbSeeder
```

---

## Puesta en marcha

### 1. Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Node.js (opcional, para recompilar el tema Sass de Bootstrap)

### 2. Levantar los servicios de infraestructura

```bash
docker compose up -d
```

Esto levanta:

| Servicio | URL |
|---|---|
| SQL Server | `localhost,1434` |
| MinIO (consola) | http://localhost:9001 |
| RabbitMQ (UI) | http://localhost:15672 |
| MailHog (bandeja dev) | http://localhost:8025 |

Credenciales por defecto en `docker-compose.yml` y `appsettings.json`.

### 3. Aplicar migraciones y seed inicial

```bash
# Desde la raíz del repositorio
dotnet run --project ERPNet.Testing -- seed
```

Esto crea el esquema, inserta las entidades estáticas (Recursos, TiposMantenimiento, Secciones) y un usuario administrador inicial.

### 4. Ejecutar la API

```bash
dotnet run --project ERPNet.Api
```

La API queda disponible en `https://localhost:7268`.
Documentación interactiva: `https://localhost:7268/scalar`

### 5. Ejecutar la aplicación Blazor

```bash
dotnet run --project ERPNet.Web.Blazor
```

Abre `https://localhost:7100` en el navegador.
Credenciales del administrador generadas en el paso 3 (ver salida del seeder).

---

## Estructura del proyecto

```
ERPNet/
│
├── ERPNet.Domain/
│   ├── Entities/              # 20 entidades (Usuario, Empleado, Rol, Maquinaria…)
│   ├── Common/
│   │   ├── BaseEntity.cs      # Auditoría + soft delete
│   │   └── Values/            # Value Objects: Email, Dni
│   ├── Enums/                 # RecursoCodigo, Alcance, Plataforma…
│   └── Repositories/          # Interfaces (IUnitOfWork, IUsuarioRepository…)
│
├── ERPNet.Application/
│   ├── Common/
│   │   ├── Interfaces/        # IRolService, IUsuarioService…
│   │   ├── DTOs/              # DTOs de capa Application (con deps de Domain)
│   │   └── *.cs               # Implementaciones de servicios
│   ├── Auth/                  # AuthService, UsuarioContext, PermisoUsuario
│   └── Mailing/               # IMailService, modelos de email, plantillas Razor
│
├── ERPNet.Infrastructure/
│   ├── Database/
│   │   ├── ErpNetDbContext.cs  # DbContext con soft delete global + auditoría
│   │   ├── Configurations/    # IEntityTypeConfiguration por entidad
│   │   ├── Repositories/      # Implementaciones de repositorios
│   │   └── Migrations/
│   ├── FileStorage/           # MinioFileStorageService + thumbnails WebP
│   ├── Mailing/               # SmtpEmailSender (MailKit) + RazorViewRenderer
│   └── Messaging/             # RabbitMqPublisher<T>, QueueHelper, Consumer
│
├── ERPNet.Api/
│   ├── Controllers/           # 8 controllers: Auth, Usuarios, Roles, Empleados…
│   ├── Middleware/            # UsuarioContextMiddleware, ControlAccesoMiddleware
│   ├── Attributes/            # [Recurso], [RequierePermiso], [SinPermiso]
│   └── Filters/               # ValidationFilter (FluentValidation automático)
│
├── ERPNet.ApiClient/          # Cliente tipado generado por NSwag (MSBuild)
│   └── ERPNet.Api.json        # Spec OpenAPI commiteada (visible en PRs)
│
├── ERPNet.Web.Blazor/         # BFF server-side (:7100)
│   └── Bff/
│       └── BffApiClient.cs    # Proxy autenticado: inyecta JWT en cada llamada
│
├── ERPNet.Web.Blazor.Client/  # App Blazor WASM
│   └── Components/
│       ├── Common/            # Modal, ItemSelector, Tooltip, NavMenu…
│       ├── Layout/            # MainLayout (sidebar + topbar), EmptyLayout
│       └── Pages/Admin/       # Usuarios.razor, Roles.razor
│
├── ERPNet.Testing/
│   ├── Seeders/DbSeeder.cs    # Seed completo para desarrollo
│   └── Tests/                 # 5 clases xUnit (unit + architecture tests)
│
└── docker-compose.yml
```

---

## Conceptos y patrones demostrados

### Backend

| Patrón | Dónde buscarlo |
|---|---|
| Clean Architecture | Estructura de proyectos y referencias |
| Result<T> (Railway-oriented) | `ERPNet.Application/Common/Result.cs` → `BaseController.FromResult()` |
| Repository + Unit of Work | `Domain/Repositories/` + `Infrastructure/Database/` |
| Soft delete global (EF) | `ErpNetDbContext.cs` → `ConfigureConventions` + query filters |
| Value Objects (EF converters) | `Domain/Common/Values/` + `Infrastructure/Database/Converters/` |
| FluentValidation automático | `Api/Filters/ValidationFilter.cs` + validators junto a DTOs |
| JWT + Refresh Token rotation | `Application/Auth/AuthService.cs` |
| Reuse detection en tokens | `Infrastructure/Database/Repositories/RefreshTokenRepository.cs` |
| Rate limiting por IP y email | `Api/Program.cs` → `AddRateLimiter` |
| Control de acceso por recurso | `Api/Middleware/ControlAccesoMiddleware.cs` + `[Recurso]` |
| Alcance (Propio/Sección/Global) | `Domain/Enums/Alcance.cs` + `PermisoRolRecurso` |
| FileStorage genérico | `ArchivoBaseController<TEntidad,TEnum>` + `IHasArchivos<TEnum>` |
| Thumbnails automáticos | `MinioFileStorageService.cs` → ImageSharp 300×300 WebP |
| Mensajería async (RabbitMQ) | `Infrastructure/Messaging/` + `EmailConsumer.cs` |
| Email con plantillas Razor | `Infrastructure/Mailing/RazorViewToStringRenderer.cs` |
| Auditoría automática | `ErpNetDbContext.SaveChanges` → timestamps + LogService |
| Architecture tests | `ERPNet.Testing/Tests/ArchitectureTests.cs` |
| Cliente HTTP tipado (NSwag) | `ERPNet.ApiClient/` + MSBuild target en `.csproj` |

### Frontend (Blazor)

| Patrón | Dónde buscarlo |
|---|---|
| BFF | `ERPNet.Web.Blazor/Bff/BffApiClient.cs` |
| Menús dinámicos desde API | `ERPNet.Web.Blazor.Client/Components/Common/NavMenu.razor` |

---

---

## Modelo de control de acceso

```
Recurso (tabla estática, seeded desde enum RecursoCodigo)
  └── PermisoRolRecurso (pivot)
        ├── CanCreate / CanEdit / CanDelete
        └── Alcance: Propio | Seccion | Global

Middleware evalúa: [Recurso(RecursoCodigo.Empleados)] + HTTP verb
→ CanCreate = POST, CanEdit = PUT, CanDelete = DELETE
```

---

## Ejecutar los tests

```bash
dotnet test ERPNet.Testing
```

Los architecture tests verifican que:
- `ERPNet.Domain` no referencia Infrastructure ni Application
- Los servicios no referencian directamente EF Core

---

## Roadmap de funcionalidades pendientes

- [ ] Gestión de Vacaciones (flujo Pendiente → Aprobado/Rechazado)
- [ ] Turnos y Marcajes con detección de incidencias
- [ ] Módulo de Maquinaria y Órdenes de Mantenimiento
- [ ] Dashboard con métricas en tiempo real

---

## Licencia

Proyecto de uso educativo y portfolio. Sin licencia comercial.
