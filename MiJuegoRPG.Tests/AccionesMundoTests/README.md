# Acciones de Mundo - Suite de Tests

Esta carpeta contiene los tests unitarios e integración para el sistema de Acciones de Mundo (Energía + Tiempo).

## 📋 Estructura de Tests

### Tests Unitarios

1. **ZonePolicyServiceTests.cs** (Tarea A)
   - Carga de `config/zonas_politicas.json`
   - Políticas por tipo de zona (Ciudad, ParteCiudad, Ruta)
   - Fallback seguro para acciones no definidas
   - Sin RNG (solo lectura de configuración)

2. **ActionWorldCatalogServiceTests.cs** (Tarea B)
   - Carga de `acciones/acciones_mundo.json`
   - Validación de defaults: `energia=1`, `tiempo=1`, `cooldown=0`
   - Requisitos por clase y atributos
   - Manejo de acciones inexistentes

3. **DelitosServiceTests.cs** (Tarea C)
   - Carga de `config/delitos.json`
   - Aplicación de consecuencias (reputación, multas)
   - Acumulación de múltiples delitos
   - RNG determinista con `RandomService.SetSeed`

4. **WorldActionExecutorTests.cs** (Tarea D)
   - Consumo de Energía
   - Avance de tiempo del mundo (minutos)
   - Bloqueos por recursos insuficientes
   - Sistema de cooldowns

### Tests de Integración

1. **AccionesMundoIntegrationTests.cs** (Tarea E)
   - Flujo completo: políticas + catálogo + delitos + recursos
   - Escenarios:
     - Robar en Ruta (éxito sin detección)
     - Robar en Ruta (detectado, consecuencias)
     - Robar en Ciudad (bloqueado por política)
     - Requisitos no cumplidos
     - Cooldown tras primera ejecución
   - RNG determinista para reproducibilidad

## 🎯 Criterios de Aceptación

- ✅ Cobertura mínima: 80% de servicios de Acciones de Mundo
- ✅ Determinismo: `RandomService.SetSeed` para escenarios con probabilidades
- ✅ No rompe suite existente: 131/131 tests deben seguir PASS
- ✅ Nomenclatura consistente: `[Collection("Sequential")]` por determinismo
- ✅ Documentación XML en cada test explicando escenario

## 🔬 Convenciones de Tests

### Patrón AAA (Arrange-Act-Assert)

Todos los tests siguen el patrón Arrange-Act-Assert estricto con comentarios claros:

```csharp
[Fact]
public void NombreDescriptivo_Condicion_ResultadoEsperado()
{
    // Arrange - Preparar servicios, datos y contexto
    var service = new ZonePolicyService();
    service.CargarPoliticas();

    // Act - Ejecutar la operación bajo prueba
    var resultado = service.ObtenerPolitica("Ciudad", "robar_intento");

    // Assert - Verificar el resultado esperado
    Assert.NotNull(resultado);
    Assert.False(resultado.Permitido);
}
```

### Determinismo RNG

Para tests que involucran probabilidades (detección, multas):

```csharp
var rng = new RandomService();
rng.SetSeed(12345); // Semilla fija para reproducibilidad
var service = new DelitosService(rng);
```

### Paralelización

Todos los tests usan `[Collection("Sequential")]` para evitar interferencias por estado compartido (archivos JSON, RNG global).

## 📊 Cobertura Objetivo

| Servicio | Cobertura Objetivo | Casos Críticos |
|----------|-------------------|----------------|
| ZonePolicyService | 90% | Política bloqueada, permitida, fallback |
| ActionWorldCatalogService | 85% | Defaults, requisitos, acción inexistente |
| DelitosService | 85% | Consecuencias, acumulación, RNG |
| WorldActionExecutor | 80% | Energía, tiempo, cooldowns, bloqueos |
| Integración | 75% | Flujos completos end-to-end |

## 🚀 Ejecutar Tests

### Suite completa

```powershell
dotnet test --nologo
```

### Solo Acciones de Mundo

```powershell
dotnet test --filter "FullyQualifiedName~AccionesMundoTests" --nologo
```

### Con cobertura

```powershell
dotnet test --collect:"XPlat Code Coverage" --nologo
```

## 📝 Notas de Implementación

**Estado actual**: Tests diseñados y listos para implementación.

**Pendiente**:

- Implementar servicios reales (`ZonePolicyService`, `ActionWorldCatalogService`, etc.)
- Crear DTOs (`ActionWorldDef`, `ZonePolicyResult`, `WorldActionResult`, etc.)
- Añadir campos al `Personaje` (`CooldownsAccionesMundo`, etc.)
- Crear contexto `MundoContext` con reloj del mundo

**Feature flag**: Todo el sistema estará detrás de `GameplayToggles.AccionesMundoEnabled` (OFF por defecto).

## 🔗 Referencias

- Diseño: `Docs/Vision_de_Juego.md` → "Acciones de Mundo (Energía + Tiempo)"
- Arquitectura: `Docs/Arquitectura_y_Funcionamiento.md` → Sección MVP
- Datos: `Docs/Resumen_Datos.md` → Secciones 28–30
- Ejemplos: `Docs/Guia_Ejemplos.md` → Acciones de Mundo

---

Última actualización: 2025-10-15
