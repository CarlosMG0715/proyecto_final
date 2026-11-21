# Granja Lechera — API Web en .NET 10 con arquitectura en capas

Taller de Lenguajes de Programación (Universidad Santiago de Cali). API REST para gestionar una granja lechera:
producción de leche, transformación en queso o yogurt, inventario por lotes con vencimiento, ventas con regla FIFO
y reportes. Parte de la estructura de `ApiSampleFinal` (capas, AutoMapper, Swagger, EF Core + SQLite,
CORS y `appsettings`).

## Tecnologías

- .NET 10, ASP.NET Core Web API, C#
- Entity Framework Core 10.0.15 con **SQLite** (también está referenciado el proveedor de SQL Server)
- AutoMapper 16.3.0 · Swashbuckle (Swagger/OpenAPI) 6.4.0
- xUnit para las pruebas unitarias (sobre SQLite en memoria)

## Estructura de la solución

```text
Granja.sln
├── Granja.Domain          Entidades, enums, excepciones de dominio e interfaces (sin dependencias externas)
├── Granja.Infrastructure  GranjaDbContext, configuraciones EF, repositorios, UnitOfWork, SystemClock
├── Granja.Services        Servicios (casos de uso), DTOs, validaciones, AutoMapper, lógica FIFO
├── Granja.Web             Controllers, Program.cs (raíz de composición), manejo de errores, Swagger
└── Granja.Tests           Pruebas unitarias xUnit
```

Dependencias: `Web → Services → Infrastructure → Domain`. El dominio no conoce a ninguna otra capa; Infrastructure
implementa las interfaces definidas en el dominio. Web solo usa Infrastructure en `Program.cs` para registrar
dependencias. Los controllers no usan el `DbContext` ni contienen lógica de negocio.

## Modelo de dominio

| Entidad | Atributos principales |
|---|---|
| `GranjaLechera` | `Id`, `Nombre`, `NumeroVacas` (n), `LitrosPorVaca` (l), `Parametros` |
| `ParametroProducto` | `Tipo` (Queso/Yogurt), `Rendimiento` (litros por unidad), `VidaUtilDias`, `Precio` |
| `Vaca` | `Id`, `Nombre`, `Activa` |
| `ProduccionLeche` | `Fecha`, `VacaId`, `Litros` (única por fecha y vaca) |
| `LoteProducto` | `Tipo`, `FechaProduccion`, `FechaVencimiento`, `LitrosUsados`, `CantidadInicial`, `CantidadDisponible`, `CantidadDanada` + reglas `EstaVencido`, `Descontar`, `MarcarComoDanado` |
| `Venta` | `Fecha`, `Tipo`, `PrecioUnitario`, `Cantidad`, `Total`, `Detalles` |
| `VentaDetalle` | `LoteId`, `FechaProduccion`, `Cantidad` (trazabilidad por lote) |

Mapa del diagrama del enunciado: Farm → `GranjaLechera`, Cow → `Vaca`, Milk → `ProduccionLeche`,
Inventory → conjunto de `LoteProducto`, Product (Yogurt/Cheese) → `LoteProducto` + `TipoProducto`, Venta → `Venta`.

Los valores sugeridos (n = 10, l = 15 L, queso 10 L/kg y 30 días, yogurt 1 L/L y 15 días, precios) se cargan como
datos iniciales de la base y **se editan con `PUT /api/granja`**; no están fijos en el código.

## Cómo ejecutar

Requisitos: SDK de .NET 10 y la herramienta `dotnet-ef`:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.15
```

Desde la carpeta raíz:

```powershell
dotnet restore
dotnet build

# 1) Generar la migración inicial (la primera vez)
dotnet ef migrations add Inicial `
  --project Granja.Infrastructure `
  --startup-project Granja.Web `
  --context GranjaDbContext `
  --output-dir Data/Migrations

# 2) Ejecutar (aplica las migraciones y crea granja.db)
dotnet run --project Granja.Web
```

Swagger: `https://localhost:57199/swagger` (o `http://localhost:57200/swagger`).

- Si ya había ejecutado la API antes de crear la migración, borre `Granja.Web/granja.db` para que la base se cree
  desde la migración.
- Mientras no existan migraciones, `Program.cs` crea el esquema con `EnsureCreated()`; cuando existen, usa `Migrate()`.
- Para SQL Server: cambiar `UseSqlite` por `UseSqlServer` en `Granja.Infrastructure/DependencyInjection.cs`, ajustar
  la cadena de conexión y generar de nuevo las migraciones.

## Pruebas

```powershell
dotnet test
```

Cubren las pruebas mínimas del punto 9 del taller y el escenario de referencia completo (sección 8):

- producción = n × l y no se registra dos veces para la misma fecha;
- no se transforman más litros que los disponibles del día;
- la venta toma primero el lote más antiguo y, si no alcanza, continúa con el siguiente (varios detalles);
- un lote vencido nunca se vende aunque sea el más antiguo;
- una venta con existencias insuficientes no modifica el inventario;
- al procesar vencimientos lo disponible pasa a dañado y el reporte lo refleja;
- reportes de ventas, leche por día, producto por día/total/dañado y reporte de vacas.

La colección de pruebas manuales está en `Granja.Web/Granja.http` (escenario de referencia paso a paso) y también
se puede usar Swagger.

## Endpoints

| Método | Ruta | Descripción | Respuestas |
|---|---|---|---|
| GET | `/api/granja` | Configuración (n, l) y parámetros por producto | 200 |
| PUT | `/api/granja` | Actualiza n, l, rendimiento, vida útil y precio | 200, 400 |
| GET | `/api/vacas` | Lista las vacas | 200 |
| POST | `/api/produccion` | Registra la producción del día (n × l; opcionalmente litros por vaca) | 201, 400, 409 |
| GET | `/api/produccion?desde=&hasta=` | Producción por día | 200, 400 |
| POST | `/api/produccion/transformaciones` | Transforma leche de una fecha en un lote de queso o yogurt | 201, 400, 409 |
| GET | `/api/inventario?fecha=&tipo=` | Lotes y resumen del inventario | 200 |
| POST | `/api/inventario/vencimientos?fecha=` | Procesa vencimientos (disponible → dañado) | 200 |
| GET | `/api/inventario/por-vencer?dias=3&fecha=` | Alerta de lotes próximos a vencer (extensión) | 200, 400 |
| POST | `/api/ventas` | Registra una venta (FIFO, atómica) | 201, 400, 409 |
| GET | `/api/ventas/{id}` | Consulta una venta con su detalle por lote | 200, 404 |
| GET | `/api/ventas?desde=&hasta=&tipo=` | Lista de ventas | 200, 400 |
| GET | `/api/reportes/ventas?desde=&hasta=&tipo=` | Ventas del período y total general | 200, 400 |
| GET | `/api/reportes/leche-diaria?desde=&hasta=` | Leche producida, transformada y no aprovechada por día | 200, 400 |
| GET | `/api/reportes/productodiario?desde=&hasta=` | Producto elaborado por día y tipo | 200, 400 |
| GET | `/api/reportes/producto-total` | Producido, vendido, disponible y dañado por producto | 200 |
| GET | `/api/reportes/productodanado?desde=&hasta=` | Producto dañado por lote y tipo | 200, 400 |
| GET | `/api/reportes/vacas?desde=&hasta=` | Promedio, vaca que más/menos produce y vacas bajo el promedio | 200, 400 |

Fechas en formato ISO `yyyy-MM-dd`. Los errores se devuelven como `ProblemDetails`
(400 validación, 404 no encontrado, 409 regla de negocio, 500 inesperado).

## Decisiones de diseño

- **Reglas en el dominio**: `LoteProducto` calcula su vencimiento y cantidad inicial, decide si está vencido y
  descuenta/marca como dañado; `Venta` acumula cantidad y total desde sus detalles.
- **FIFO atómico** (`VentaService.RegistrarVentaAsync`): se consultan los lotes vendibles por `FechaProduccion`
  ascendente, se verifica la existencia total y recién entonces se descuenta. Venta y lotes se guardan con un único
  `SaveChanges` (`IUnitOfWork`), por lo que una venta rechazada (409) no deja cambios.
- **Vencimiento**: un lote está vencido cuando `fecha > FechaVencimiento`; el último día de vida útil todavía se vende.
  Los lotes vencidos se excluyen de la venta aunque no se haya ejecutado `POST /api/inventario/vencimientos`; ese
  proceso es el que mueve la cantidad a dañada.
- **Producción por vaca**: `n` vacas activas; `PUT /api/granja` las sincroniza (se dan de baja las sobrantes sin borrar
  su historial). Por defecto cada vaca produce `l` litros; el cuerpo opcional `vacas` permite registrar litros distintos
  para alimentar el reporte de vacas, y las demás producen `l`.
- **SQLite y decimales**: SQLite guarda `decimal` como texto y no puede compararlos ni sumarlos en SQL, así que los
  repositorios filtran por tipo y fechas en la base y las cantidades se evalúan en memoria.
- **Hora actual**: `IClock` (`SystemClock`) entrega la fecha por defecto; todas las operaciones aceptan una fecha explícita.

## Nota sobre las anotaciones a mano del enunciado

Las notas manuscritas de la página 4 mencionan 150 g de queso por litro y vencimiento a 2 semanas, mientras que la
tabla del enunciado y el escenario de referencia usan 10 L = 1 kg y 30/15 días. Se tomaron como referencia la tabla y el
escenario (los reportes esperados dependen de ellos). Como son parámetros configurables, para aplicar las notas basta
con un `PUT /api/granja` con `rendimiento` ≈ 6.6667 (150 g/L) y `vidaUtilDias` = 14 para cada producto.
