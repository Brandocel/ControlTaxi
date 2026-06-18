# AGENTS.md — Reglas para agentes de IA (Codex, Claude Code, etc.)

> **LEE ESTO ANTES DE ESCRIBIR O MODIFICAR CÓDIGO EN ESTE PROYECTO.**
> Si vas a implementar algo, primero abre y respeta [`docs/GUIA-ARQUITECTURA.md`](docs/GUIA-ARQUITECTURA.md).
> Este archivo es el resumen obligatorio; la guía es el detalle.

## Qué es este proyecto
POS de escritorio **Control Taxi**: **WPF + .NET 9**, **Clean Architecture**, **MVVM**, **EF Core + SQL Server**.
Solución en 4 capas + pruebas:
`ControlTaxi.Domain` → `ControlTaxi.Application` → `ControlTaxi.Infrastructure` → `ControlTaxi.Desktop` (+ `ControlTaxi.Tests`).

## Regla sagrada: dependencias hacia adentro
`Desktop` y `Infrastructure` → `Application` → `Domain`. **El Domain no depende de nadie.**
- ❌ NUNCA uses `DbContext`, SQL, `HttpClient` ni `System.Windows` dentro de `Domain` o de un ViewModel.
- ❌ NUNCA pongas reglas de negocio en el XAML, el code-behind ni el ViewModel.
- ✅ La regla de negocio vive en la **entidad** del Domain.

## Reglas de oro (no negociables)
1. El `Domain` no tiene `using` de EF Core, WPF ni nada externo.
2. Todo servicio de `Application` devuelve `Result` / `Result<T>` — **nunca `null` para indicar fallo, ni excepciones en flujos esperados**.
3. Reglas de negocio en la entidad (ej. `RelacionTicketTaxista.PagarDejada()`), no en servicios ni ViewModels.
4. Propiedades de entidades con `private set`; se cambian por métodos con nombre y validación.
5. Contraseñas SOLO con `IPasswordHasher` (PBKDF2). Jamás en claro.
6. Todo fallo se registra con `ILogger`. Prohibido `catch` vacío que se trague el error.
7. Cada cambio de esquema = una **migración EF**. Nada de SQL suelto para crear tablas.
8. Toda lógica de negocio nueva lleva su **prueba** en `ControlTaxi.Tests`.
9. Acceso a datos vía `IRepository<T>` + `IUnitOfWork`. Nada de SQL a mano.
10. Inyección por constructor (DI). No se hace `new` de servicios.
11. **Idioma del código: español** (clases, métodos, mensajes), igual que el resto.

## Cómo implementar un módulo (orden obligatorio: de adentro hacia afuera)
1. **Domain**: entidad + reglas + enums.
2. **Application**: DTOs/records, `I<Modulo>Service`, validador FluentValidation, servicio (devuelve `Result`, registra). Regístralo en `Application/DependencyInjection.cs`.
3. **Infrastructure**: `IEntityTypeConfiguration<T>`, `DbSet` en `ControlTaxiDbContext`, migración EF.
4. **Desktop**: `<Modulo>ViewModel` (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`), `<Modulo>View.xaml` (solo Binding), registrar en `App.xaml.cs`, navegación en `ShellViewModel` + `DataTemplate` en `MainWindow.xaml`.
5. **Tests**: prueba la entidad (sin BD) y el servicio (BD en memoria).

👉 **Módulo de referencia a copiar: `Relaciones`.** Cuando dudes, imita esa estructura exacta.

## Estilo visual (UI)
Hay un sistema de estilos centralizado en `src/ControlTaxi.Desktop/Themes/ControlTaxiTheme.xaml`.
- **Reutiliza** los estilos y brushes del tema (colores de marca, botones, inputs, tarjetas, DataGrid).
- ❌ No hardcodees colores hex en cada vista; usa los recursos (`{StaticResource ...}`).
- Marca HOKA: teal `#0F8F83`, oscuro `#143F55`, acento dorado `#F0BD67`.

## Antes de terminar (checklist)
- [ ] `dotnet build` sin advertencias.
- [ ] `dotnet test` en verde (incluye tus pruebas nuevas).
- [ ] Servicio y ViewModel registrados en DI.
- [ ] Migración creada si tocaste el esquema.
- [ ] El módulo abre desde el menú y funciona de punta a punta.

## Comandos
```bash
dotnet build
dotnet run --project src/ControlTaxi.Desktop
dotnet test
dotnet ef migrations add <Nombre> --project src/ControlTaxi.Infrastructure --startup-project src/ControlTaxi.Infrastructure --output-dir Persistence/Migrations
```

## Qué NO hacer
- No reintroducir el patrón del sistema viejo (SQL crudo, `Dictionary<string,object>`, leer `INFORMATION_SCHEMA` en runtime, campos `static` mutables, `catch` que devuelven `null`).
- No agregar dependencias pesadas sin justificación.
- No romper la dirección de dependencias entre capas.
- No mezclar idiomas en el código.
