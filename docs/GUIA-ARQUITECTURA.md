# Guía de Arquitectura y Patrones — Control Taxi

> **Para:** Guadalupe (desarrolladora)
> **Objetivo:** que toda implementación nueva siga el mismo molde. Si respetas esta guía,
> el código queda ordenado, testeable y fácil de mantener. **Lee esto antes de escribir código.**

---

## 1. La idea en una frase

Usamos **Clean Architecture** (arquitectura por capas) con **MVVM** en la interfaz.
La regla sagrada es: **las dependencias apuntan hacia adentro**. El negocio nunca depende
de la base de datos ni de la pantalla.

```
  Desktop (WPF)  ───►  Application  ───►  Domain
       │                   ▲
       └─► Infrastructure ─┘
                  │
                  └──────────────────►  Domain
```

- **Domain** no conoce a NADIE (ni EF Core, ni WPF, ni internet).
- **Application** solo conoce a Domain.
- **Infrastructure** implementa lo que Application pide (BD, seguridad, APIs).
- **Desktop** junta todo y muestra la UI.

Si alguna vez te dan ganas de usar `DbContext` dentro de un ViewModel, o de poner una regla
de negocio en el XAML: **detente**. Eso rompe la arquitectura.

---

## 2. Para qué sirve cada proyecto

| Proyecto | Qué va aquí | Qué NUNCA va aquí |
|----------|-------------|-------------------|
| `ControlTaxi.Domain` | Entidades, reglas de negocio, enums, `Result`/`Error` | EF Core, SQL, HttpClient, WPF |
| `ControlTaxi.Application` | Casos de uso (servicios), interfaces, DTOs, validaciones | Implementaciones concretas (EF, archivos, red) |
| `ControlTaxi.Infrastructure` | `DbContext`, repositorios, migraciones, hashing, APIs | Lógica de negocio, ViewModels |
| `ControlTaxi.Desktop` | Vistas (XAML), ViewModels, navegación, arranque/DI | Reglas de negocio, SQL |
| `ControlTaxi.Tests` | Pruebas de Domain/Application/Infrastructure | — |

---

## 3. Patrones que usamos (y por qué)

| Patrón | Dónde vive | Para qué |
|---|---|---|
| **Result** | `Domain/SharedKernel/Result.cs` | No devolver `null` ni lanzar excepciones en flujos esperados. Cada operación dice si fue éxito o fracaso. |
| **Repository + Unit of Work** | `Application/Common/Interfaces` + `Infrastructure/.../Repositories` | Acceso a datos limpio y transaccional. Nada de SQL a mano. |
| **Dominio rico** | Entidades de `Domain/Entities` | La regla vive en la entidad (ej. `RelacionTicketTaxista.PagarDejada()`), no en el servicio. |
| **MVVM** | `Desktop/ViewModels` + `Desktop/Views` | La pantalla (XAML) no sabe de lógica; el ViewModel no sabe de botones. |
| **Dependency Injection** | `App.xaml.cs` + `DependencyInjection.cs` de cada capa | Todo se inyecta por constructor. No se hace `new` de servicios. |
| **Validación** | `FluentValidation` en `Application/Features/.../*Validator.cs` | Validar la entrada ANTES de tocar la BD. |

---

## 4. Reglas de oro (no negociables)

1. **El Domain no tiene `using` de EF Core, WPF ni nada externo.** Si lo necesitas, algo está mal ubicado.
2. **Todo servicio de Application devuelve `Result` o `Result<T>`.** Nunca `null` para indicar fallo.
3. **Las reglas de negocio van en la entidad** (métodos como `PagarDejada`, `LigarTaxista`), no en el servicio ni el ViewModel.
4. **Las propiedades de las entidades tienen `private set`.** Se cambian solo por métodos con nombre y con validación. (Mira `RelacionTicketTaxista`.)
5. **Nunca guardes contraseñas en claro.** Usa `IPasswordHasher`.
6. **Todo lo que falle se registra con `ILogger`.** Nada de `catch` vacíos que se traguen el error.
7. **Cada cambio de esquema = una migración EF.** Nada de crear tablas con SQL suelto.
8. **Toda lógica de negocio nueva lleva su prueba** en `ControlTaxi.Tests`.
9. **El idioma del código es español** (nombres de clases, métodos, mensajes), igual que el resto.

---

## 5. Receta: cómo agregar un módulo nuevo (paso a paso)

Vamos a usar como ejemplo el módulo **Relaciones** (ya implementado) para que veas cada pieza.
Sigue estos 6 pasos **en orden** (de adentro hacia afuera):

### Paso 1 — Domain: la entidad y sus reglas
Crea la entidad en `src/ControlTaxi.Domain/Entities/`. Hereda de `AuditableEntity`
(trae `CreadoEn/CreadoPor/ModificadoEn/ModificadoPor` automáticos).

- Propiedades con `private set`.
- Constructor que valida lo obligatorio.
- Métodos con nombre de negocio que cambian el estado (no setters públicos).
- Si hay estados, exprésalos como propiedad calculada o enum.

📄 Ejemplo: [`RelacionTicketTaxista.cs`](../src/ControlTaxi.Domain/Entities/RelacionTicketTaxista.cs)
— mira cómo `Estatus` se calcula solo y `PagarDejada()` tiene su guarda.

### Paso 2 — Application: interfaz, DTOs, validador y servicio
En `src/ControlTaxi.Application/Features/<Modulo>/`:

1. **DTOs / requests** (records): lo que entra y sale del servicio. Nunca expongas la entidad a la UI directamente.
2. **Interfaz** `I<Modulo>Service` con métodos que devuelven `Result`.
3. **Validador** FluentValidation para cada request de escritura.
4. **Servicio** que implementa la interfaz: valida → usa `IUnitOfWork` → llama métodos de la entidad → `SaveChangesAsync` → devuelve `Result`. Inyecta `ILogger` y registra.

📄 Ejemplo: [`IRelacionesService.cs`](../src/ControlTaxi.Application/Features/Relaciones/IRelacionesService.cs) y
[`RelacionesService.cs`](../src/ControlTaxi.Application/Features/Relaciones/RelacionesService.cs).

5. Regístralo en [`Application/DependencyInjection.cs`](../src/ControlTaxi.Application/DependencyInjection.cs):
   ```csharp
   services.AddScoped<IRelacionesService, RelacionesService>();
   ```

### Paso 3 — Infrastructure: configuración EF y migración
1. Crea la configuración en `src/ControlTaxi.Infrastructure/Persistence/Configurations/`
   (`IEntityTypeConfiguration<T>`): tabla, llaves, longitudes, índices, y **`Ignore`** de las
   propiedades calculadas (las que no se guardan).
   📄 Ejemplo: [`RelacionTicketTaxistaConfiguration.cs`](../src/ControlTaxi.Infrastructure/Persistence/Configurations/RelacionTicketTaxistaConfiguration.cs)
2. Agrega el `DbSet` en [`ControlTaxiDbContext.cs`](../src/ControlTaxi.Infrastructure/Persistence/ControlTaxiDbContext.cs).
3. Genera la migración:
   ```
   dotnet ef migrations add <NombreDescriptivo> --project src/ControlTaxi.Infrastructure --startup-project src/ControlTaxi.Infrastructure --output-dir Persistence/Migrations
   ```
   La migración se aplica sola al arrancar (lo hace `DbInitializer`).

### Paso 4 — Desktop: ViewModel
En `src/ControlTaxi.Desktop/ViewModels/` crea `<Modulo>ViewModel` heredando de `ObservableObject`
(CommunityToolkit.Mvvm):
- Propiedades con `[ObservableProperty]`.
- Acciones con `[RelayCommand]` (¡async!).
- Inyecta el `I<Modulo>Service`, `ICurrentUser` y `ILogger`.
- El ViewModel **solo** llama al servicio y mueve datos a propiedades. Cero reglas de negocio.

📄 Ejemplo: [`RelacionesViewModel.cs`](../src/ControlTaxi.Desktop/ViewModels/RelacionesViewModel.cs).

### Paso 5 — Desktop: Vista (XAML)
En `src/ControlTaxi.Desktop/Views/` crea `<Modulo>View.xaml` (UserControl). Usa **Binding** a las
propiedades y comandos del ViewModel. Nada de lógica en el code-behind.

📄 Ejemplo: [`RelacionesView.xaml`](../src/ControlTaxi.Desktop/Views/RelacionesView.xaml).

### Paso 6 — Conectar navegación y DI
1. En [`App.xaml.cs`](../src/ControlTaxi.Desktop/App.xaml.cs) registra el ViewModel:
   ```csharp
   services.AddTransient<RelacionesViewModel>();
   ```
2. En [`MainWindow.xaml`](../src/ControlTaxi.Desktop/MainWindow.xaml) agrega el `DataTemplate`
   (mapea ViewModel → Vista) y un botón en el menú.
3. En [`ShellViewModel.cs`](../src/ControlTaxi.Desktop/ViewModels/ShellViewModel.cs) agrega el
   comando `Navegar<Modulo>` que pone el ViewModel en `CurrentView`.

### Paso 7 — Pruebas
En `tests/ControlTaxi.Tests/<Modulo>/`:
- Prueba la **entidad** (reglas, estados, guardas) — sin base de datos.
- Prueba el **servicio** con base en memoria (`UseInMemoryDatabase`).

📄 Ejemplos: [`RelacionTicketTaxistaTests.cs`](../tests/ControlTaxi.Tests/Relaciones/RelacionTicketTaxistaTests.cs)
y [`RelacionesServiceTests.cs`](../tests/ControlTaxi.Tests/Relaciones/RelacionesServiceTests.cs).

Corre todo con:
```
dotnet test
```

---

## 6. Flujo de una operación (ejemplo: cobrar una dejada)

Así viaja una acción del usuario por las capas. Memoriza este recorrido:

```
Usuario hace clic "Cobrar dejada"
   │
   ▼
RelacionesViewModel.PagarDejadaCommand        (Desktop)   ── solo llama al servicio
   │
   ▼
IRelacionesService.PagarDejadaAsync(...)       (Application) ── valida, coordina, Result + log
   │
   ▼
relacion.PagarDejada(usuario, ticket)          (Domain)    ── la REGLA vive aquí
   │
   ▼
IUnitOfWork.SaveChangesAsync()                 (Infrastructure) ── guarda en SQL Server
   │
   ▼
Result.Success()  ──► el ViewModel muestra "Dejada cobrada"
```

---

## 7. Convenciones de nombres

| Cosa | Convención | Ejemplo |
|---|---|---|
| Entidad | sustantivo singular | `Taxista`, `RelacionTicketTaxista` |
| Servicio | `I<Modulo>Service` + `<Modulo>Service` | `IRelacionesService` |
| Request/DTO | `record` con sufijo claro | `GuardarRelacionRequest`, `RelacionRowDto` |
| Validador | `<Request>Validator` | `GuardarRelacionValidator` |
| ViewModel | `<Modulo>ViewModel` | `RelacionesViewModel` |
| Vista | `<Modulo>View` | `RelacionesView` |
| Configuración EF | `<Entidad>Configuration` | `TaxistaConfiguration` |
| Prueba | `<Clase>Tests` | `RelacionesServiceTests` |

---

## 8. Errores y logs

- **Errores esperados** (validación, no encontrado, conflicto): se devuelven como `Result.Failure(Error.X("mensaje"))`. No lances excepciones para esto.
- **Errores inesperados** (la BD se cayó, un bug): se registran con `_logger.LogError(ex, ...)`. En la UI ya hay un manejador global que evita que la app se cierre.
- Los logs se guardan en `%LOCALAPPDATA%\ControlTaxi\logs\` (un archivo por día, se conservan 30 días).
- Usa los niveles bien: `LogInformation` para acciones normales (login, guardado), `LogWarning` para algo raro pero manejado, `LogError` para fallos reales.

---

## 9. Checklist antes de dar por terminado un módulo

- [ ] La entidad tiene sus reglas adentro y propiedades con `private set`.
- [ ] El servicio devuelve `Result` y registra con `ILogger`.
- [ ] Hay validador para cada request de escritura.
- [ ] Hay configuración EF + migración generada.
- [ ] El servicio y el ViewModel están registrados en DI.
- [ ] La vista usa Binding (code-behind vacío).
- [ ] Hay pruebas de la entidad y del servicio, y `dotnet test` pasa en verde.
- [ ] `dotnet build` sin advertencias.
- [ ] El módulo se abre desde el menú y funciona de punta a punta.

---

## 10. Comandos útiles

```bash
# Compilar todo
dotnet build

# Correr la app
dotnet run --project src/ControlTaxi.Desktop

# Correr las pruebas
dotnet test

# Crear una migración
dotnet ef migrations add <Nombre> --project src/ControlTaxi.Infrastructure --startup-project src/ControlTaxi.Infrastructure --output-dir Persistence/Migrations

# Deshacer la última migración (si aún no se aplicó)
dotnet ef migrations remove --project src/ControlTaxi.Infrastructure --startup-project src/ControlTaxi.Infrastructure
```

---

**Resumen mental:** Domain (reglas) → Application (coordina con Result) → Infrastructure (guarda) → Desktop (muestra).
De adentro hacia afuera, siempre. Cuando dudes, copia el módulo **Relaciones**: es el ejemplo a seguir.
