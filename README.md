# ProductsApi

API REST en ASP.NET Core (.NET 10) para gestión de productos. CRUD sobre SQL Server usando únicamente stored procedures, más un endpoint de clima que consume la API pública de Open-Meteo.

## Cómo correrlo

Requisitos: .NET SDK 10.0 y SQL Server (lo probé en SQL Server Express con autenticación de Windows).

1. Clonar el repo:

```bash
git clone https://github.com/DuvanCodes/ProductsApi.git
```

2. Ejecutar `database/01_setup.sql` en la instancia. El script es idempotente: crea la base `ProductsDb`, la tabla y los 5 SPs.

```bash
sqlcmd -S localhost\SQLEXPRESS -E -i database/01_setup.sql
```

   Si se usa un cliente gráfico (DBeaver, etc.), ejecutarlo como script completo, no statement por statement: contiene separadores `GO`.

3. Si la instancia no es `localhost\SQLEXPRESS`, ajustar `DefaultConnection` en `ProductsApi/appsettings.json`.

4. Correr:

```bash
cd ProductsApi
dotnet run
```

La API queda en `http://localhost:5239`. En desarrollo hay UI interactiva en `http://localhost:5239/scalar/v1`, y el archivo `ProductsApi.http` trae requests de ejemplo para todo, incluidos los casos de error (400, 404).

## Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | /api/products | Lista productos activos (más recientes primero) |
| GET | /api/products/{id} | Producto por id (404 si no existe) |
| POST | /api/products | Crea producto (201 con el recurso completo) |
| PUT | /api/products/{id} | Actualiza (204, o 404 si no existe) |
| DELETE | /api/products/{id} | Borrado lógico (204; 404 si no existe o ya fue eliminado) |
| GET | /api/weather/{city} | Clima actual. Ciudades: cali, bogota, medellin, barranquilla, cartagena |

## Decisiones técnicas

**Dapper en lugar de EF Core.** Como todo el acceso a datos va por stored procedures, EF Core pierde lo que lo justifica (LINQ, change tracking, migraciones). Dapper hace el mapeo con mínimo overhead y me da control total sobre la llamada al SP. Todos los parámetros son tipados y no hay concatenación de SQL en ningún punto del proyecto.

**404 con `@@ROWCOUNT`, sin lectura previa.** Los SPs de update y delete devuelven las filas afectadas y el repositorio retorna `bool`, con eso el controller decide entre 204 y 404 en una sola llamada a la BD. Descarté el patrón de verificar con un GetById, además del round-trip extra, ese check-then-act tiene una race condition (el registro puede desaparecer entre la verificación y el update), por ende en el SP la verificación y la operación son atómicas.

**Integración de clima.** HttpClient tipado vía `IHttpClientFactory` con timeout de 10s, y respuesta cacheada por ciudad 10 minutos con `IMemoryCache` (el campo `fetchedAt` del DTO permite ver la edad del dato). Si Open-Meteo falla o no responde, la API devuelve 503 y este es un fallo de dependencia externa, no un 500 propio. La respuesta cruda del proveedor nunca sale del servicio; la API expone su propio DTO.

El borrado de productos es lógico (`IsActive = 0`) por lo que prefiero conservar el histórico antes que borrar filas. El manejo de errores va en un middleware global que responde ProblemDetails y deja el detalle real solo en el log.

Notas del template de .NET 10: desde .NET 9 ya no viene Swashbuckle, así que investigué y usé Scalar como UI sobre el OpenAPI nativo (solo en desarrollo) como se puede observar en el program. El SDK 10.0.301 además trae Microsoft.OpenApi 2.0.0 con un CVE alto reportado (GHSA-v5pm-xwqc-g5wc), por eso el `.csproj` pinea la 2.7.5 parcheada.

## Pendientes

Cosas que dejé por fuera por el alcance de la prueba: autenticación (JWT), paginación en el listado, tests, health checks, y retry con backoff en la llamada a Open-Meteo.
