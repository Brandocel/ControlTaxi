# CLAUDE.md — Instrucciones para Claude Code

> Antes de implementar o modificar cualquier cosa en este proyecto, **lee y respeta**:
> 1. [`AGENTS.md`](AGENTS.md) — reglas obligatorias resumidas (válidas para todos los agentes).
> 2. [`docs/GUIA-ARQUITECTURA.md`](docs/GUIA-ARQUITECTURA.md) — la guía detallada con la receta paso a paso.

Las reglas de `AGENTS.md` aplican igual aquí. Lo más importante:

- **Clean Architecture, dependencias hacia adentro.** `Desktop`/`Infrastructure` → `Application` → `Domain`. El `Domain` no depende de nada externo (ni EF Core, ni WPF).
- **Todo servicio de `Application` devuelve `Result`/`Result<T>`.** Nunca `null` ni excepciones para flujos esperados.
- **Las reglas de negocio van en la entidad del Domain**, no en servicios ni ViewModels.
- **MVVM estricto:** ViewModels con CommunityToolkit.Mvvm; vistas solo con Binding; code-behind vacío.
- **Acceso a datos solo por `IRepository<T>` + `IUnitOfWork`.** Cada cambio de esquema = migración EF.
- **Logs con `ILogger`; nada de `catch` vacíos.** Contraseñas solo con `IPasswordHasher`.
- **Idioma del código: español.**
- **Estilo UI centralizado** en `src/ControlTaxi.Desktop/Themes/ControlTaxiTheme.xaml`: reutiliza sus recursos, no hardcodees colores.

**Módulo de referencia para copiar la estructura: `Relaciones`** (Domain → Application → Infrastructure → Desktop → Tests).

Verifica siempre antes de terminar: `dotnet build` (sin warnings) y `dotnet test` (en verde).
