# Copilot Instructions — Prisma Decision API

This repository contains two sub-projects that work together:

- **`PrismaDotnetApi/`** — A C# ASP.NET Core Web API (Clean Architecture)
- **`PrismaFastApi/`** — A Python FastAPI service for Bayesian inference / influence-diagram solving

The .NET API calls the Python API over HTTP for all solver operations.

---

## Architecture

### .NET — Clean Architecture layers

| Layer | Project | Responsibility |
|---|---|---|
| Presentation | `PrismaApi.Api` | Controllers, attributes, security policies |
| Application | `PrismaApi.Application` | Services, repositories, mapping, background jobs |
| Domain | `PrismaApi.Domain` | Entities, DTOs, interfaces, constants |
| Infrastructure | `PrismaApi.Infrastructure` | EF Core context, caching, DB utilities |

Dependencies always point inward. Infrastructure and Application implement interfaces defined in Domain.

### Python — FastAPI layers

| Layer | Location | Responsibility |
|---|---|---|
| Routes | `src/routes/` | FastAPI routers, request/response handling |
| Services | `src/services/` | Business and solver logic |
| DTOs | `src/dtos/` | Pydantic models |
| Utils | `src/utils/` | Shared helpers |

---

## C# Conventions

### Naming

- **Classes, interfaces, methods, properties, enums**: PascalCase
- **Interfaces**: `I` prefix — `IProjectService`, `ICrudRepository<T, TId>`
- **Async methods**: `*Async` suffix — `GetAsync()`, `CreateAsync()`
- **Private fields**: `_camelCase` — `_projectRepository`, `_cache`
- **Method parameters & locals**: camelCase
- **`CancellationToken` parameter**: always named `ct`
- **Constants** in static classes: PascalCase — `MaxShortStringLength`

### DTOs

Each entity has a family of DTOs using inheritance:

```csharp
public class ProjectDto { /* shared base fields */ }
public class ProjectCreateDto : ProjectDto { /* POST body */ }
public class ProjectIncomingDto : ProjectDto { /* PUT body */ }
public class ProjectOutgoingDto : ProjectDto { /* response */ }
public class PopulatedProjectDto : ProjectDto { /* response with nested entities */ }
```

JSON serialization uses `[JsonPropertyName("snake_case")]` attributes.

### Entities

```csharp
// Hierarchy
IBaseEntity<TId>
  └── BaseEntity          // Id, CreatedAt, UpdatedAt
        └── AuditableEntity  // + CreatedById, UpdatedById, navigation properties

public class Project : AuditableEntity
{
    // EF Core fluent config lives in the entity class
    public static void OnModelConfiguring(ModelBuilder modelBuilder) { ... }

    // Navigation collections initialised inline
    public ICollection<ProjectRole> ProjectRoles { get; set; } = new List<ProjectRole>();
}
```

### Repository pattern

```csharp
// Generic base interface — defined in Domain
public interface ICrudRepository<TEntity, TId>
{
    Task<TEntity?> GetByIdAsync(TId id, bool withTracking = true,
        Expression<Func<TEntity, bool>>? filterPredicate = null, CancellationToken ct = default);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken ct);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct);
    Task DeleteAsync(TEntity entity, CancellationToken ct);
    // ...
}

// Entity-specific interface extends the generic one
public interface IProjectRepository : ICrudRepository<Project, Guid>
{
    Task<ICollection<Project>> GetProjectsWhereUserHasAccess(string userId, CancellationToken ct);
}
```

The `filterPredicate` parameter is used for row-level authorization — pass it instead of filtering after the fact.

### Service pattern

```csharp
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IMemoryCache _cache;

    public ProjectService(IProjectRepository projectRepository, IMemoryCache cache)
    {
        _projectRepository = projectRepository;
        _cache = cache;
    }

    // Every public method: async, accepts UserOutgoingDto for authorization, accepts CancellationToken
    public async Task<List<ProjectOutgoingDto>> CreateAsync(
        List<ProjectCreateDto> dtos,
        UserOutgoingDto userDto,
        CancellationToken ct = default)
    { ... }
}
```

- All methods are `async Task<T>`.
- Every mutating method receives the current `UserOutgoingDto` for authorization checks.
- Methods accept and return `List<Dto>` (batch operations).
- Use `IMemoryCache` for caching; invalidate on mutations.

### Controller pattern

```csharp
[ApiController]
[Route("")]
public class ProjectsController : PrismaBaseEntityController
{
    [HttpPost("projects")]
    public async Task<ActionResult<List<ProjectOutgoingDto>>> CreateProjects(
        [FromBody] List<ProjectCreateDto> dtos,
        CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        await BeginTransactionAsync(ct);
        try
        {
            var result = await _projectService.CreateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }
}
```

- Always inherit `PrismaBaseController` or `PrismaBaseEntityController`.
- `[LoadUser]` and `[ApiExceptionFilter]` are applied on the base controller — do not add them individually.
- Always wrap write operations in a transaction (Begin / Commit / Rollback in catch).
- Retrieve the current user with `HttpContext.GetLoadedUser()`.
- Return `ActionResult<List<Dto>>` — never raw types.

### Mapping pattern

Conversions live in static extension-method classes named `*MappingExtensions`:

```csharp
public static class ProjectMappingExtensions
{
    public static ProjectOutgoingDto ToOutgoingDto(this Project entity) { ... }
    public static List<ProjectOutgoingDto> ToOutgoingDtos(this IEnumerable<Project> entities)
        => entities.Select(e => e.ToOutgoingDto()).ToList();
    public static Project ToEntity(this ProjectCreateDto dto, UserOutgoingDto user) { ... }
}
```

Never put mapping logic in controllers or services directly.

### Testing

- xUnit with `[Fact]` (single case) and `[Theory]` + `[InlineData]` (parameterised).
- Tests share infrastructure via `IClassFixture<PrismaApiFixture>` and `[Collection(nameof(PrismaCollection))]`.
- Test method naming: `MethodName_StateUnderTest_ExpectedResult` — e.g. `GetProject_ReturnsProject`, `GetProjectWithoutAccess_ReturnsNotFound`.
- Use `TestClientGetAsync<T>` / `TestClientPostAsync<T>` extension helpers rather than raw `HttpClient`.
- Assert HTTP status code first, then payload properties.

---

## Python Conventions

### Naming

- **Classes**: PascalCase — `SolverService`, `DecisionTreeCreator`
- **Functions & methods**: snake_case — `find_optimal_decisions`, `build_inference_engine`
- **Variables & parameters**: snake_case — `project_id`, `issue_dtos`
- **Module files**: snake_case — `solver_service.py`, `decision_dtos.py`
- **Constants / enum values**: UPPER_SNAKE_CASE

### DTOs (Pydantic)

Mirror the C# DTO family pattern:

```python
class IssueDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    project_id: uuid.UUID

class IssueIncomingDto(IssueDto):
    type: str

class IssueOutgoingDto(IssueDto):
    type: str
    decision: Optional[DecisionOutgoingDto]
    uncertainty: Optional[UncertaintyOutgoingDto]
```

### FastAPI routes

```python
router = APIRouter(tags=["solvers"])

@router.post("/solvers/project/{project_id}")
async def get_optimal_decisions(
    issues: list[IssueOutgoingDto],
    edges: list[EdgeOutgoingDto],
    solver_service: SolverService = Depends(get_solver_service),
) -> SolutionDto:
    return await solver_service.find_optimal_decision_pyagrum_from_dtos(issues, edges)
```

- Use `Depends()` for all service injection.
- Declare the return type annotation on every route handler — FastAPI uses it for serialization and OpenAPI docs.
- One `APIRouter` per resource file.

---

## Python Code Quality Standards

These standards apply to all new and modified Python code. Existing code may not yet meet them — always apply them when touching a file.

### Typing — use modern Python 3.11+ style

Use built-in generic types and `typing.Optional` for nullable values. **Never** import `List`, `Dict`, `Tuple`, or `Set` from `typing` — use the built-in lowercase equivalents instead.

```python
# Wrong
from typing import List
def process(items: List[str]) -> str | None: ...

# Correct
from typing import Optional
def process(items: list[str]) -> Optional[str]: ...
```

Use `Optional[T]` for nullable values rather than `T | None`. Use `Union[X, Y]` for non-nullable unions of multiple types.

Every function and method **must** have a return type annotation, including `-> None` for void functions and `-> Self` where appropriate.

Always import `Optional` and `Union` from `typing`. Only import other items from `typing` that have no built-in equivalent: `TypeVar`, `Protocol`, `overload`, `TYPE_CHECKING`, `cast`, `Final`, `Literal`, `TypeAlias`.

### No `Any`

Avoid `Any` as a type. Use `TypeVar`, specific unions, `object`, or `Protocol` instead. When interfacing with untyped third-party libraries (e.g. `pyagrum`), constrain `# type: ignore` to the narrowest possible line and add a comment explaining why.

```python
# Wrong
def get_labels(node: Any) -> Any: ...

# Correct
def get_labels(node: int | str) -> tuple[str, ...]: ...
```

### Avoid imperative for-loops for collection transforms

Use comprehensions and `itertools` for building, filtering, and transforming collections.

**Flattening** — use `itertools.chain.from_iterable`, not a for-loop with `.extend()`:

```python
# Wrong — side-effect-in-comprehension antipattern
states: list[OptionOutgoingDto] = []
[states.extend(issue.decision.options) for issue in issues if issue.decision]

# Correct
from itertools import chain
states = list(chain.from_iterable(
    issue.decision.options for issue in issues if issue.decision is not None
))
```

**Filtering + transforming** — use a comprehension or `map`/`filter`:

```python
# Wrong
result = []
for issue in issues:
    if issue.type == "Decision":
        result.append(issue.id)

# Correct
result = [issue.id for issue in issues if issue.type == "Decision"]
```

**Finding the first match** — use `next()` with a default:

```python
# Wrong — allocates the full filtered list
state = [s for s in states if str(s.id) == state_id][0]

# Correct
state = next((s for s in states if str(s.id) == state_id), None)
```

Imperative loops are acceptable when the body has genuine side effects (e.g. mutating a CPT in-place via a third-party API), is too complex to read as a comprehension, or when building a result that requires intermediate mutable state across iterations.

### No mutable default arguments

```python
# Wrong — the default list is shared across all calls
async def get_solutions(self, evidence: list[list[uuid.UUID]] = []) -> list[SolutionDto]: ...

# Correct
async def get_solutions(self, evidence: Optional[list[list[uuid.UUID]]] = None) -> list[SolutionDto]:
    if evidence is None:
        evidence = []
```

### No class-level mutable state

Mutable collections must be instance attributes, initialised in `__init__`, not class-level attributes (which are shared across all instances):

```python
# Wrong
class Manager:
    all_ids: set[str] = set()  # shared across all instances!

# Correct
class Manager:
    def __init__(self) -> None:
        self.all_ids: set[str] = set()
```

### Async

- Route handlers and service methods that perform I/O must be `async def`.
- Do not mix sync blocking calls inside `async def` — use `asyncio.to_thread()` if you must call a sync library.
- Do not use `asyncio.sleep(0)` as a polling mechanism.
