# Documentación de progression.json

Este archivo define parámetros de balance para la progresión de atributos por actividad.

## Parámetros Principales

- ExpBaseRecoleccion: Base de experiencia fraccional aplicada a atributos al realizar acciones de recolección. Valor típico: 0.005 – 0.02.
- ExpBaseEntrenamiento: Base para el cálculo de experiencia por minuto virtual de entrenamiento. Típico: 0.005 – 0.02.
- ExpBaseExploracion: Micro experiencia otorgada al explorar un sector. Valores muy pequeños (0.001 – 0.005) recomendados.

### Regeneración de Maná (en combate)

- ManaRegenCombateBase: Regeneración base por turno (en puntos) antes de aplicar factores. Recomendado bajo (0.1 – 0.5) para mantener la progresión lenta.
- ManaRegenCombateFactor: Factor multiplicador por `Estadisticas.RegeneracionMana` del personaje. Valores típicos: 0.01 – 0.05.
- ManaRegenCombateMaxPorTurno: Tope duro de maná que puede recuperarse por turno (en puntos). Recomendado 1–2 para evitar spam de habilidades.

### Regeneración de Maná (fuera de combate)

- ManaRegenFueraBase: Regeneración base por tick de descanso (puntos). Más alta que en combate pero conservadora (0.5 – 2.0).
- ManaRegenFueraFactor: Factor multiplicador por `Estadisticas.RegeneracionMana` en descanso. Usar valores bajos (0.02 – 0.08).
- ManaRegenFueraMaxPorTick: Tope por tick de descanso. Recomendar 2–5 para mantener ritmo lento.

## Escalados por Nivel

Estos factores reducen la ganancia conforme el personaje sube de nivel global.

- EscaladoNivelRecoleccion: Potencia usada como Math.Pow(valor, Nivel-1). Aumentar reduce la ganancia a niveles
altos. Rango sugerido: 1.03 – 1.10.
- EscaladoNivelEntrenamiento: Igual que el anterior pero para entrenamiento. Rango sugerido: 1.03 – 1.10.
- EscaladoNivelExploracion: Similar para exploración. Más bajo para no penalizar demasiado. Rango sugerido: 1.01 – 1.06.

## FactorMinExp

- factorMinExp: Piso mínimo de experiencia fraccional para evitar que llegue a cero en atributos muy altos o niveles elevados. Rango sugerido: 0.00005 – 0.0002.

## Indices (por atributo)

Cada atributo tiene un índice que aumenta su coste relativo de progresión en entrenamiento (divisor). Valores altos = progreso más lento. Ajustar según importancia del atributo.

Ejemplo base incluido:

```json

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
```

## Fórmulas Resumidas

1. Recolección (por acción, por atributo afectado):
   exp = ExpBaseRecoleccion / ( (EscaladoNivelRecoleccion^(Nivel-1)) * (1 + ValorActual/10) )
   Luego se multiplica por minutos (actualmente 1) y se aplica un mínimo factorMinExp.

2. Entrenamiento (por minuto):
   expMinuto = ExpBaseEntrenamiento / (EscaladoNivelEntrenamiento^(Nivel-1) × IndiceAtributo × (1 + ValorActual × 0.05))
   Si expMinuto < 0.0001 se aplica ese mínimo interno.

3. Exploración:
   basePercepcion = ExpBaseExploracion / (EscaladoNivelExploracion^(Nivel-1)) con mínimo interno 0.00005.
   Bonus primera visita: +50% de basePercepcion a Agilidad.

4. Regeneración de Maná (por turno en combate):
   regen = clamp( ManaRegenCombateBase + (RegeneracionMana * ManaRegenCombateFactor), 0, ManaRegenCombateMaxPorTurno )
   Donde `RegeneracionMana` proviene de `Estadisticas` del personaje.

## Ajuste Inicial Recomendado

1. Jugar 10–15 acciones de cada tipo (recolección / entrenamiento / exploración) y observar tiempos de subida de un atributo bajo (1–3) a ~5.
2. Si sube demasiado rápido (< 2 minutos equivalentes) aumentar ligeramente Escalado correspondiente (ej. 1.05 → 1.07) o bajar ExpBase.
3. Si es demasiado lento (> 10 minutos) hacer lo inverso.
4. No tocar varios parámetros a la vez sin registrar cambios.

## Próximos Pasos Relacionados

- Agregar pruebas unitarias (9.4) que validen que al subir de nivel la experiencia por acción disminuye dentro de un rango esperado.
- Ajustar ExpBaseExploracion tras observar ritmo de descubrimiento de sectores.

### Nota sobre Resistencias/Vulnerabilidades Elementales

- `ResistenciasElementales { tipo: 0..0.9 }`: mitigación adicional post-defensa. No se permiten valores negativos (rechazados por el validador).
- `VulnerabilidadesElementales { tipo: factor }`: multiplicador de daño aplicado post-mitigación. Rango permitido: [1.0 .. 1.5] (conservador para progresión lenta). Si falta el tipo, se asume 1.0.
      - Implementado inicialmente para el canal genérico `"magia"` en daño mágico. En futuras iteraciones se expandirá a más tipos (fuego, hielo, rayo, veneno, etc.) cuando el pipeline identifique el elemento del golpe.

Nota combate (5.10 pendiente): En próximas iteraciones se parametrizarán en progression.json curvas/caps para `Precision`, `CritChance`, `CritMult` y `Penetracion` para integrarse en el pipeline de daño. Mantener valores conservadores (progresión lenta) y documentar cambios en este archivo.

---
Última actualización: Parametrización 3.3 completada.

## Changelog de balance (datos relacionados)

- 2025-09-16: Se añadieron y organizaron enemigos del bioma `bosque` (`nivel_1_3`) por categorías (normal/elite/jefe/campo/legendario/unico/mundial). Las recompensas de EXP y Oro de estos enemigos se mantuvieron conservadoras para respetar la progresión lenta del proyecto. No se modifican parámetros de `progression.json` en esta iteración.
- 2025-09-16: Convención de variantes adoptada: cuando un arquetipo existe en múltiples categorías, el campo `Nombre` lleva sufijo `("Élite", "Jefe", etc.)` y se añade `Tags` con `variante:*`. Además, el loader ignora archivos JSON ubicados directamente en la raíz de `nivel_*` bajo `enemigos/por_bioma` (solo se consideran los de subcarpetas de categoría). Medida temporal para evitar doble carga mientras se completa la limpieza física de archivos.
