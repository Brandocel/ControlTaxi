# Control Taxi — Desktop (WPF + Clean Architecture)

Reescritura desde cero del POS "Control Taxi" como aplicación de escritorio Windows,
con arquitectura por capas, patrones de diseño y logging robusto.

## Stack

- **.NET 9** · **WPF** (UI nativa Windows)
- **MVVM** con `CommunityToolkit.Mvvm`
- **EF Core 9** + **SQL Server** (acceso a datos con migraciones)
- **Serilog** (logs con rotación diaria)
- **FluentValidation** (validación de casos de uso)
- **xUnit + FluentAssertions** (pruebas)

## Arquitectura (Clean Architecture)

Las dependencias apuntan **hacia adentro**. El dominio no conoce a nadie.

```
src/
├── ControlTaxi.Domain          → Entidades + reglas puras. CERO dependencias externas.
│   ├── Common/        BaseEntity, AuditableEntity, IAuditable
│   ├── SharedKernel/  Result, Error  (manejo de errores sin excepciones)
│   ├── Entities/      Usuario (+ se irán agregando: Operacion, Venta, Pago...)
│   └── Enums/
│
├── ControlTaxi.Application      → Casos de uso, interfaces, validaciones, DTOs.
│   ├── Common/Interfaces/  IRepository, IUnitOfWork, IPasswordHasher, ICurrentUser
│   └── Features/Auth/       LoginRequest/Result, IAuthService, AuthService, LoginValidator
│
├── ControlTaxi.Infrastructure   → EF Core, repos, seguridad. Implementa Application.
│   ├── Persistence/   DbContext, Configurations, Repository, UnitOfWork, Migrations, DbInitializer
│   └── Security/      Pbkdf2PasswordHasher
│
└── ControlTaxi.Desktop (WPF)    → Views (XAML) + ViewModels + DI host (Serilog).
    ├── App.xaml.cs    Generic Host, DI, Serilog, manejo global de excepciones
    ├── Views          LoginWindow, MainWindow (shell)
    ├── ViewModels     LoginViewModel, ShellViewModel
    └── Services        CurrentUserService (sesión)

tests/
└── ControlTaxi.Tests            → Pruebas de Domain/Application/Infrastructure
```

## Patrones aplicados

| Patrón | Dónde |
|---|---|
| Result pattern | `Domain/SharedKernel/Result.cs` — sin nulls ni excepciones en flujos esperados |
| Repository + Unit of Work | `Application/Common/Interfaces` + `Infrastructure/Persistence/Repositories` |
| MVVM | `Desktop/ViewModels` + `Desktop/Views` |
| Dependency Injection | `App.xaml.cs` (Generic Host) + `DependencyInjection.cs` por capa |
| Auditoría automática | `ControlTaxiDbContext.SaveChangesAsync` |

## Cómo correr

1. Edita la cadena de conexión en `src/ControlTaxi.Desktop/appsettings.json`.
2. Compilar y ejecutar:
   ```
   dotnet run --project src/ControlTaxi.Desktop
   ```
3. Al primer arranque crea la BD (migraciones) y un usuario **admin / admin123**.

## Logs

`%LOCALAPPDATA%\ControlTaxi\logs\controltaxi-YYYYMMDD.log` (rotación diaria, 30 días).

## Pruebas

```
dotnet test
```

## Migraciones EF

```
dotnet ef migrations add <Nombre> --project src/ControlTaxi.Infrastructure --startup-project src/ControlTaxi.Infrastructure --output-dir Persistence/Migrations
```

## Pendiente (roadmap de módulos)

Login + shell ya funcionan como **slice vertical de referencia**. Faltan, siguiendo el
mismo patrón (Entidad → Repo → Servicio Application → ViewModel → View):
Registro Diario · Ventas/Remisiones · Pagos · Gastos · Cortes · Comisiones ·
Catálogos · Gafetes · Usuarios · Relaciones/Dejadas · Sincronización App Móvil.
