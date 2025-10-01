<!-- Última actualización: 2025-09-23 -->
# Documentación detallada de progression.json

Este documento define, con precisión operativa, los parámetros de balance para la progresión de atributos por actividad y la regeneración de maná, incluyendo fórmulas en orden de aplicación, valores recomendados, ejemplos numéricos paso a paso, y notas de integración con los servicios del juego.

Referencias cruzadas:

- Arquitectura y pipeline: `./Arquitectura_y_Funcionamiento.md`
- Pruebas relacionadas: [`MiJuegoRPG.Tests/ProgressionServiceTests.cs`](../../MiJuegoRPG.Tests/ProgressionServiceTests.cs)
- Servicio: [`ProgressionService`](../Motor/Servicios/ProgressionService.cs)

---

## 1. Parámetros principales (visión y rangos)

Todos los valores aquí descritos residen en `DatosJuego/progression.json` y son consumidos por `ProgressionService` y servicios asociados (p. ej., `EnergiaService`, pipeline de combate para regeneración de maná). Los rangos propuestos son conservadores para mantener una progresión lenta.

- `ExpBaseRecoleccion`: base de experiencia fraccional por acción de recolección.
- Rango recomendado: 0.005 – 0.02
- Impacto: subida más rápida de atributos contextuales al recolectar.

- `ExpBaseEntrenamiento`: base por minuto virtual de entrenamiento.
- Rango recomendado: 0.005 – 0.02
- Impacto: es la vía controlada y lenta de subir atributos de forma dirigida.

- `ExpBaseExploracion`: micro-experiencia por explorar un sector.
- Rango recomendado: 0.001 – 0.005 (muy bajo para evitar “farmear” exploración)

### 1.1 Regeneración de Maná (en combate)

- `ManaRegenCombateBase`: puntos por turno antes de factores.
- Recomendado: 0.1 – 0.5
- `ManaRegenCombateFactor`: multiplicador por `Estadisticas.RegeneracionMana`.
- Recomendado: 0.01 – 0.05
- `ManaRegenCombateMaxPorTurno`: techo duro por turno (puntos).
- Recomendado: 1 – 2

### 1.2 Regeneración de Maná (fuera de combate)

- `ManaRegenFueraBase`: base por tick de descanso (puntos).
- Recomendado: 0.5 – 2.0
- `ManaRegenFueraFactor`: multiplicador por `Estadisticas.RegeneracionMana`.
- Recomendado: 0.02 – 0.08
- `ManaRegenFueraMaxPorTick`: techo por tick de descanso (puntos).
- Recomendado: 2 – 5

### 1.3 Escalados por Nivel (reducción con el nivel global)

Aplican como potencias $\text{valor}^{(Nivel-1)}$ y reducen la ganancia a niveles altos.

- `EscaladoNivelRecoleccion`: 1.03 – 1.10
- `EscaladoNivelEntrenamiento`: 1.03 – 1.10
- `EscaladoNivelExploracion`: 1.01 – 1.06

### 1.4 Piso mínimo de experiencia

- `factorMinExp`: piso para evitar converger a 0 con atributos altos o niveles elevados.
- Recomendado: 0.00005 – 0.0002

### 1.5 Índices por atributo (entrenamiento)

Índices mayores implican progreso más lento en entrenamiento (divisor adicional). Ajustar según importancia del atributo.

Ejemplo base:

```json
{
   "Indices": {
      "Fuerza": 3.0,
      "Inteligencia": 8.0,
      "Destreza": 3.5,
      "Suerte": 12.0,
      "Defensa": 5.5,
      "Vitalidad": 9.0,
      "Agilidad": 4.5,
      "Resistencia": 3.5,
      "Percepcion": 4.0
   }
}
```

---

## 2. Fórmulas exactas y orden de aplicación

Reglas generales:

- Salvo mención explícita, todos los clamps usan `Math.Clamp` con límites cerrados.
- El orden de operaciones importa y se detalla por actividad.
- Redondeo: la experiencia es fraccional y no se redondea; se acumula hasta cruzar el umbral de subida del atributo.

### 2.1 Recolección (por acción, por atributo afectado)

Fórmula base:

$\displaystyle \text{exp} = \max\Big(\text{factorMinExp}, \; \frac{\text{ExpBaseRecoleccion}}{(\text{EscaladoNivelRecoleccion})^{(Nivel-1)}} \cdot \frac{1}{1 + \frac{ValorActual}{10}}\Big)$

Orden:

1) Reducir por nivel global: $\text{baseNivel} = \frac{\text{ExpBaseRecoleccion}}{(\text{EscaladoNivelRecoleccion})^{(Nivel-1)}}$

2) Penalizar por valor actual del atributo: $\text{baseAttr} = \frac{\text{baseNivel}}{1 + ValorActual/10}$

3) Aplicar piso: $\text{exp} = \max(\text{baseAttr}, \text{factorMinExp})$

Notas:

- El multiplicador por minutos está fijado en 1 actualmente.

### 2.2 Entrenamiento (por minuto simulado)

$\displaystyle \text{expMinuto} = \max\Big(0.0001,\; \frac{\text{ExpBaseEntrenamiento}}{(\text{EscaladoNivelEntrenamiento})^{(Nivel-1)}\; \cdot \; \text{IndiceAtributo} \cdot (1 + 0.05\cdot ValorActual)}\Big)$

Orden:

1) Reducir por nivel global.

2) Dividir por `IndiceAtributo`.

3) Penalizar linealmente por `ValorActual` con pendiente 0.05.

4) Aplicar mínimo interno 0.0001.

### 2.3 Exploración (por sector descubierto)

$\displaystyle \text{basePercepcion} = \max\Big(0.00005,\; \frac{\text{ExpBaseExploracion}}{(\text{EscaladoNivelExploracion})^{(Nivel-1)}}\Big)$

Si es primera visita al sector: bonus a Agilidad = $0.5\,\times\,\text{basePercepcion}$.

### 2.4 Regeneración de Maná (turno de combate)

$\displaystyle \text{regen} = \mathrm{clamp}\Big(\text{ManaRegenCombateBase} + (\text{RegeneracionMana} \cdot \text{ManaRegenCombateFactor}),\; 0,\; \text{ManaRegenCombateMaxPorTurno}\Big)$

Fuera de combate (tick de descanso): fórmula análoga con parámetros `Fuera`.

---

## 3. Ejemplos numéricos paso a paso

Supuestos comunes: `Nivel=5`, `factorMinExp=0.0001`.

### 3.1 Recolección (subir Fuerza)

- Parámetros: `ExpBaseRecoleccion=0.01`, `EscaladoNivelRecoleccion=1.06`.
- Valor actual del atributo: `Fuerza=8`.

1) Reducción por nivel: $0.01 / 1.06^{4} \approx 0.01 / 1.2625 \approx 0.00792$

2) Penalización por atributo: $0.00792 / (1 + 8/10) = 0.00792 / 1.8 \approx 0.00440$

3) Piso: $\max(0.00440, 0.0001) = 0.00440$

Resultado: +0.00440 EXP a Fuerza.

### 3.2 Entrenamiento (subir Inteligencia)

- Parámetros: `ExpBaseEntrenamiento=0.012`, `EscaladoNivelEntrenamiento=1.07`, `Indice[Inteligencia]=8.0`.
- Valor actual: `Inteligencia=12`.

1) Reducción por nivel: $0.012 / 1.07^{4} \approx 0.012 / 1.3108 \approx 0.00916$

2) Dividir por índice: $0.00916 / 8 = 0.001145$

3) Penalización por valor: $0.001145 / (1 + 0.05\cdot 12) = 0.001145 / 1.6 \approx 0.000716$

4) Mínimo interno: $\max(0.000716, 0.0001) = 0.000716$

Resultado: +0.000716 EXP por minuto a Inteligencia.

### 3.3 Exploración (Percepción y Agilidad)

- Parámetros: `ExpBaseExploracion=0.002`, `EscaladoNivelExploracion=1.03`.

1) Base: $0.002 / 1.03^{4} \approx 0.002 / 1.1255 \approx 0.00178$

2) Piso: $\max(0.00178, 0.00005) = 0.00178$

3) Primera visita: Agilidad recibe bonus adicional de $0.5\times 0.00178 = 0.00089$.

Resultado: +0.00178 a Percepción; +0.00089 a Agilidad (si primera visita).

### 3.4 Regeneración de maná (combate)

- Parámetros: `ManaRegenCombateBase=0.2`, `ManaRegenCombateFactor=0.03`, `Max=1.5`.
- `Estadisticas.RegeneracionMana = 0.01 \cdot Inteligencia = 0.12` (para `Inteligencia=12`).

1) Suma: $0.2 + (0.12 \cdot 0.03) = 0.2 + 0.0036 = 0.2036$

2) Clamp: $\mathrm{clamp}(0.2036, 0, 1.5) = 0.2036$

---

## 4. Contrato de datos (JSON) y defaults sugeridos

Estructura mínima en `DatosJuego/progression.json`:

```json
{
   "ExpBaseRecoleccion": 0.01,
   "ExpBaseEntrenamiento": 0.01,
   "ExpBaseExploracion": 0.002,

   "EscaladoNivelRecoleccion": 1.06,
   "EscaladoNivelEntrenamiento": 1.06,
   "EscaladoNivelExploracion": 1.03,

   "factorMinExp": 0.0001,

   "ManaRegenCombateBase": 0.2,
   "ManaRegenCombateFactor": 0.03,
   "ManaRegenCombateMaxPorTurno": 1.5,

   "ManaRegenFueraBase": 1.0,
   "ManaRegenFueraFactor": 0.05,
   "ManaRegenFueraMaxPorTick": 3.0,

    "Indices": { },
    "StatsCaps": {
       "PrecisionMax": 0.95,
       "CritChanceMax": 0.50,
       "CritMultMin": 1.25,
       "CritMultMax": 1.75,
       "PenetracionMax": 0.25
    }
}
```

Reglas de validación:

- No permitir negativos en bases ni factores.
- `EscaladoNivel* > 1.0`.
- `ManaRegen*Max >= ManaRegen*Base`.
- `Indices`: claves deben ser atributos válidos; valores `> 0`.
- `StatsCaps` (opcional): si no está presente, se usan defaults conservadores indicados arriba. Valores fuera de rango se clampean a `[0..1]` donde aplique.

---

## 5. Integraciones con servicios y orden de lectura

- `ProgressionService`: aplica las fórmulas de 2.x para otorgar EXP a atributos tras acciones de recolección, entrenamiento y exploración.
- `EnergiaService`: usa `Indices` y bonificadores de clases (ver `Arquitectura_y_Funcionamiento.md` §5.1 y §15) para coste y progresión relacionada.
- `CombatePorTurnos`/`ActionRulesService`: consultan los parámetros de regeneración de maná (combate/fuera) combinados con penalizaciones de Supervivencia.
- Habilidades del catálogo: cuando una `HabilidadData` define `CostoMana`, el `HabilidadAccionMapper` lo aplica envolviendo la acción base (`AccionCompuestaSimple`). Si existen evoluciones con `CostoMana`, se toma el mínimo entre la base y evoluciones desbloqueadas. Esto mantiene consistencia entre datos y runtime.

---

## 6. Pruebas unitarias recomendadas (9.4)

- Recolección: al aumentar `Nivel` de 1 a 10, la EXP por acción disminuye en ≥ 20% (tolerancia por configuración).
- Entrenamiento: al duplicar `ValorActual` del atributo, la EXP por minuto se reduce aproximadamente a la mitad cuando el término `1 + 0.05·Valor` domina.
- Exploración: primera visita otorga +50% a Agilidad respecto a Percepción base.
- Regeneración: nunca supera los `MaxPorTurno/PorTick` y responde linealmente al cambiar `RegeneracionMana`.

Para determinismo: usar `RandomService.SetSeed` cuando se combine con acciones aleatorias (no aplica directamente a estas fórmulas, pero sí a pipelines que las rodean).

---

## 7. Invariantes de balance y guías de tuning

- Mantener `ExpBase*` bajos para evitar saltos de poder; priorizar ajustar `EscaladoNivel*` para controlar progresión mid/late.
- Preferir aumentar ligeramente `Indices` de atributos dominantes antes que bajar drásticamente `ExpBaseEntrenamiento`.
- Revisar tiempos “1→5” y “5→10” por atributo tras cambios; documentar variaciones en la Bitácora.

Procedimiento:

1) Cambiar un único parámetro.

2) Correr 10–15 acciones representativas y medir.

3) Registrar en `Docs/Bitacora.md` con fecha y motivación.

---

## 8. Elementales: resistencias y vulnerabilidades

- `ResistenciasElementales { tipo: 0..0.9 }`: mitigación adicional post-defensa. Negativos no permitidos.
- `VulnerabilidadesElementales { tipo: [1.0..1.5] }`: multiplicador post-mitigación. Si falta un tipo, se asume `1.0`.
- Estado actual: canal genérico `"magia"` para daño mágico. Futuro: fuego/hielo/rayo/veneno cuando el golpe incluya etiqueta elemental.

Nota pipeline: aplicar en el paso 5 del orden descrito en `Arquitectura_y_Funcionamiento.md` §4 (tras defensa/penetración y mitigaciones físicas/mágicas).

---

## 9. Roadmap vinculado

- 5.10: parametrizar curvas y caps para `Precision`, `CritChance`, `CritMult`, `Penetracion` aquí y consumir en el pipeline de combate.
- 9.4: ampliar suite de pruebas de progresión (ver §6).

Notas vinculadas a equipo (15.4):

- La generación de equipo usa una base de perfección Normal=50% para escalar valores: $valor_{final} = \operatorname{round}(valor_{base} \cdot (Perfeccion/50.0))$.
- La aparición por rareza es ponderada y ahora data-driven a través de `DatosJuego/Equipo/rareza_pesos.json`.

---

## 10. Caps y curvas recomendadas (combate)

Los siguientes límites son conservadores para mantener una progresión lenta y exigente. Deben reflejarse en el cálculo de `Estadisticas` y (cuando aplique) en el pipeline de combate. Donde corresponda, se documenta el flag CLI que habilita la etapa.

- `Precision` (usa `--precision-hit` en físico):
- Rango efectivo recomendado: $[0.00 .. 0.95]$.
- Fórmula de impacto propuesta: $p_{hit} = clamp(0.35 + Precision - k\cdot Evasion,\ 0.20,\ 0.95)$ con $k \in [1.0, 1.2]$.

- `CritChance` y `CritMult`:
- `CritChance` recomendado: $[0.00 .. 0.50]$.
- `CritMult` recomendado: $[1.25 .. 1.75]$.
- Política de test: si `CritChance >= 1.0` se fuerza crítico para pruebas deterministas (solo si hay daño real > 0).

- `Penetracion` (usa `--penetracion`):
- Rango efectivo recomendado: $[0.00 .. 0.25]$ para jugador early/mid; clamps defensivos del runtime a $[0.00 .. 0.90]$ por seguridad.
- Aplicación en pipeline: `defensaEfectiva = round(defensa * (1 - Penetracion))` ANTES de mitigaciones/resistencias.

Sugerencia de mapeo desde atributos (ejemplo base, a ajustar en `Estadisticas`):

- `Precision = clamp(0.01·Destreza + 0.005·Percepcion, 0, 0.95)`
- `CritChance = clamp(0.01·(Destreza + Suerte), 0, 0.5)`
- `CritMult = clamp(1.5 + 0.001·Sabiduría, 1.25, 1.75)`
- `Penetracion = clamp(0.002·Destreza, 0, 0.25)`

Notas de integración:

- Los flags de juego (`--precision-hit`, `--penetracion`) controlan la activación de etapas sin alterar balance por defecto.
- Para Unity, mantener estos caps en un `ScriptableObject` espejo para ajustes en editor.

---

Última actualización: 2025-09-17 — Parametrización 3.4 + Caps de combate (5.10).

## Changelog de balance (datos relacionados)

- 2025-09-16: Se añadieron y organizaron enemigos del bioma `bosque` (`nivel_1_3`) por categorías (normal/elite/jefe/campo/legendario/unico/mundial). Las recompensas de EXP y Oro de estos enemigos se mantuvieron conservadoras para respetar la progresión lenta del proyecto. No se modifican parámetros de `progression.json` en esta iteración.

- 2025-09-16: Convención de variantes adoptada: cuando un arquetipo existe en múltiples categorías, el campo `Nombre` lleva sufijo `(Élite, Jefe, etc.)` y se añade `Tags` con `variante:*`. Además, el loader ignora archivos JSON ubicados directamente en la raíz de `nivel_*` bajo `enemigos/por_bioma` (solo se consideran los de subcarpetas de categoría). Medida temporal para evitar doble carga mientras se completa la limpieza física de archivos.
