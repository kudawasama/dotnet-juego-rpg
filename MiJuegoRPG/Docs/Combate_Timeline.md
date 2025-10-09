# Sistema de Combate Simultáneo Determinista (Núcleo)

## Pipeline por Tick
1. Incrementar CurrentTick.
2. Ordenar acciones por ActionOrderKey (ScheduledTick, PhaseOrdinal, PriorityTier, SpeedScore DESC, Sequence, ActorId).
3. Para cada acción no finalizada:
   - Si la fase actual completó su duración -> transicionar.
   - En transición Cast -> Impact: ejecutar OnImpact, emitir CombatEvent(ActionImpact) y DamageApplied (si aplica), pasar a Recovery.
4. Efectos periódicos (pendiente de implementación) aplican si su NextTick == CurrentTick.
5. Limpiar acciones Finished/Cancelled.
6. Verificar condiciones de victoria / KO (pendiente fase 2).

## Determinismo
- RNG particionado: cada stream (Crit, Proc, Dot, Ai) deriva de BaseSeed + constante.
- Orden estable: al empatar todas las claves anteriores se cae a Sequence/ActorId.
- Hash de replay: `CombatEventLog.ComputeDeterministicHash()` para comparar simulaciones.

## Extensiones Planeadas
- Canalización (Phase Channel) para hechizos mantenidos.
- Interrupciones: política por fase + fuerza.
- DOT/HOT con tick scheduling exacto (sin drift acumulado).
- Fixed-point interno para daño (int escalado) reemplazando dobles.
- Evento Death y mutual KO policy.

## Principios
- Sin LINQ en hot path crítico (salvo en tests / depuración).
- Acceso datos O(1); evitar asignaciones por tick innecesarias.
- Cada cambio en orden o hashing debe acompañarse de prueba.

## ActionOrderKey
```
struct ActionOrderKey {
  int ScheduledTick;   // tick objetivo del impacto para Cast, tick actual para Impact
  byte PhaseOrdinal;   // orden natural de ActionPhase
  byte PriorityTier;   // reglas de diseño (defensa < ataque rápido < pesado < magia) redefinidas por número
  int SpeedScore;      // mayor primero (se invierte comparación)
  int Sequence;        // primero en entrar gana en empate
  int ActorId;         // último desempate
}
```

## Eventos Iniciales
- ActionImpact
- DamageApplied

(Se añadirán: ActionStart, ActionCancelled, Death, DotTick, BuffApplied...)

## Próximas Tareas Recomendadas
1. Añadir CombatEventType.Death y pipeline de KO.
2. Añadir canalizaciones + DOT scheduling.
3. Implementar logger binario compacto para replays.
4. A/B shadow con motor legacy para validación de balance.
