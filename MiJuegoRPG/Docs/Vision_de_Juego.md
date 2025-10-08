# Visión de Juego (North Star) — MiJuegoRPG

Última actualización: 2025-10-08

Este documento captura la intención creativa y técnica del juego para alinear decisiones de diseño, datos y código. Es la brújula que usamos al priorizar, evaluar trade-offs y mantener coherencia durante la evolución y la futura migración a Unity.

## Pilares de Diseño

- Progresión lenta, decisiones con peso: cada mejora debe sentirse ganada; los riesgos importan.
- Dificultad justa: castiga errores, recompensa preparación y lectura del entorno.
- Economía austera: el oro es escaso; existen sinks claros y fuentes limitadas, sin granjas triviales.
- Mundo hostil pero legible: señales suficientes para anticipar peligros; la información es un recurso.
- Datos primero: JSON como fuente de verdad; gameplay data-driven, código como orquestador.
- Determinismo para probar, variación para jugar: RNG inyectable en tests; ruido controlado en runtime.
- Modularidad y portabilidad: núcleo de dominio puro, UI e I/O como adaptadores; compatible con Unity 2022 LTS.

## Fantasía del Jugador y Ritmo

- Arco: de sobreviviente novato → explorador competente → líder de caravana/maestro artesano.
- Estilos viables: cazador de trofeos, comerciante de rutas, artesano de alto nivel, explorador de biomas raros.
- Ritmo por fases:
  - Early: escasez fuerte, aprendizaje de sistemas, rutas seguras cortas.
  - Mid: especialización, primeras sinergias de set, apertura de biomas medianos.
  - Late: optimización de rutas/riesgos, economía avanzada, encargos/facciones.

## Lo que queremos y lo que no

- Queremos: elecciones tácticas claras, lectura del estado, progresión sentida, economía tensa, muerte que enseña.
- No queremos: “grindeo” vacío, inflaciones descontroladas, HUD ruidoso, snowball irrecuperable.

## Mecánicas Clave (resumen)

- Combate por turnos táctico (orden fijo):
  1) Daño base y modificadores
  2) Crítico (critChance 0..1, critMultiplier ≥ 1)
  3) Resistencias elementales por tipo
  4) Penetración afecta solo la mitigación (no el bruto)
- Estados: sangrado/quemadura/aturdimiento; separar daño por turno (DoT) del control; stacking con límites.
- Puntos de Acción (PA) — modelo principal: ejecución encadenada y dinámica por acciones con costos variables; las acciones se acumulan (oculto) para perfilar el estilo del jugador.
- Puntos de Acción (PA) — modelo principal: ejecución encadenada y dinámica por acciones con costos variables; las acciones se acumulan (oculto) para perfilar el estilo del jugador. Los PA escalan con estadísticas/atributos y nivel del PJ (p. ej., inicio 3 PA → late ~6 PA si las estadísticas son adecuadas).
- Supervivencia: hambre/sed/fatiga/temperatura con efectos suaves pero acumulativos.
- Recolección y biomas: nodos por bioma, riesgos por distancia; materiales únicos por especialidad.
- Facciones y reputación: desbloqueos, precios, encargos; umbrales configurados.
- Crafteo: recetas, estaciones y perfección de ítems influida por rareza/meta.

### Escalado de PA por estadísticas (visión)

- Objetivo: reflejar el crecimiento del PJ en la capacidad de encadenar acciones sin romper la economía del turno.
- Modelo propuesto: `PA = clamp(BasePA + f(Atributos, Estadísticas, Nivel), 1, PAMax)` con pesos moderados para Agilidad/Destreza/Nivel.
- Compatibilidad con recursos: cada Acción consume PA; las Habilidades además consumen Maná. El diseño debe evitar combinaciones que vacíen Maná en 1 turno con PA altos.

### Progresión por acciones (visión)

- Acumulación oculta de acciones: cada acción realizada suma progreso a trazas internas que definen el estilo del jugador.
- Desbloqueo y evolución de habilidades: ciertas habilidades se desbloquean/evolucionan por uso/acciones (p. ej., mucho "Correr" + "Empujar" → "Embestida").
- Clases y profesiones: se desbloquean por condiciones de mundo (NPCs/Misiones) y alineación de acciones; títulos reflejan maestrías alcanzadas.
- Economía/objetos/enemigos: adaptan costes, precios, loot y comportamientos a la lógica de acciones y estilos.

## Economía en Tensión

- Fuentes: botín acotado, encargos, comercio de oportunidad, venta de artesanías.
- Sinks: reparación, consumibles, tarifas de viaje/peaje, estaciones de crafteo, favores de facción.
- Precios: data-driven. `PriceService` aplica multiplicadores por rareza/meta y reputación. Objetivo: prevenir inflación.

## Datos y Validación

- JSON con claves snake_case; C# en PascalCase. Schemas obligatorios para catálogos.
- Rarezas dinámicas (string) con fallback seguro; logs de advertencia ante claves desconocidas.
- Cambios breaking en catálogos deben fallar CI salvo migrador explícito.

## Principios Técnicos

- Capas: Game.Core (dominio), Game.App (presentación), Infra (datos/servicios externos).
- DI y Logging: Microsoft.Extensions.*; categorías por subsistema.
- RNG inyectable para determinismo en pruebas.
- Analyzers: NetAnalyzers + StyleCop; .editorconfig como contrato de estilo.
- Evitar LINQ en loops críticos; preferir `for` indexado, caching por turno.

## Contratos Esenciales (C# mínimo útil)

Interfaz RNG inyectable (para combate, drops, encuentros):

```csharp
namespace Game.Core.Randomness;

public interface IRandomSource
{
    // Retorna un doble en [0,1)
    double NextDouble();
    // Entero en [min, max)
    int Next(int minValue, int maxValue);
}
```

Cálculo de Puntos de Acción (esqueleto, C# 10, .NET 6):

```csharp
namespace Game.Core.Combat;

public static class ActionPointService
{
    public static int ComputePA(int basePa, int agilidad, int destreza, int nivel, int paMax = 6)
    {
        var pa = basePa
                 + (agilidad / 30)
                 + (destreza / 40)
                 + (nivel / 10);
        if (pa < 1) pa = 1;
        if (pa > paMax) pa = paMax;
        return pa;
    }
}
```

Orden de operaciones de daño (contrato de éxito): resultado y métricas intermedias disponibles para UI/logs.

```csharp
namespace Game.Core.Combat;

public sealed class DamageResult
{
    public double Base { get; init; }
    public double TrasMitigacion { get; init; }
    public bool EsCritico { get; init; }
    public bool Impacto { get; init; }
    public double Final { get; init; }
}
```

## Heurísticas de Decisión

- Si no mejora legibilidad o decisiones del jugador, no entra.
- Si complica balance sin añadir variedad significativa, descártalo.
- Primero datos/config; luego código.
- Prioriza features que se puedan probar de forma determinista.

## Indicadores para Balancear

- Tiempo a primer upgrade significativo (minutos objetivo: 20–30).
- Tasa de muertes en early (objetivo 10–25%).
- Ratio sinks/sources de oro (objetivo ≥ 0.9 early, ~1.0 mid).
- Porcentaje de encuentros “fáciles/justos/duros” (tramos 40/40/20 con bioma base).

## Experiencias Objetivo (ejemplos concretos)

- Rutas: elegir entre camino seguro (menos botín) o atajo peligroso (mejor botín) según estado y provisiones.
- Equipamiento: reemplazo puntual es relevante; sets completos son raros y planificados.
- Crafteo: una receta de nivel medio requiere componentes de 2 biomas y una estación específica.
- Facciones: subir reputación abre mejores precios y encargos únicos; caer en negativa cierra rutas.

## Glosario Breve

- Perfección: calidad [0..100] que escala stats/precio; deriva de rareza y del ítem.
- Meta de rareza: pesos, rango de perfección y multiplicadores derivados.
- Overlay de datos: personalizaciones bajo `PjDatos/` que sobreescriben JSON base.

## Próximas Ideas (Backlog de Diseño)

- Caravanas con escolta y eventos de ruta.
- Clima con impacto suave en precisión/visibilidad.
- Talleres de facción con perks situacionales.
- Contrabando: rutas de alto riesgo/beneficio condicionadas por reputación.

---

Notas para implementación: mantener este documento sincronizado con `Docs/Arquitectura_y_Funcionamiento.md` y `Docs/Roadmap.md`. Cambios de visión major requieren entrada en Bitácora y, si aplica, migrador de datos.
