# Prompt Especializado: Combate

## Objetivo
Asistir en refactors y tuning del pipeline de daño y acciones de combate.

## Preguntas Diagnóstico
1. ¿Afecta orden de pasos (penetración, mitigación, crítico)?
2. ¿Se mantiene determinismo en pruebas?
3. ¿Se altera cálculo legacy o solo pipeline nuevo?
4. ¿Existen flags que condicionen el cambio?
5. ¿Se respeta progresión lenta (no inflar números)?

## Checklist
- Orden: Base → Pen → DEF → Mitig → Crítico → Vuln → Redondeo
- CritScalingFactor aplicado solo al extra
- Penetración reducida en crítico (factor configurable)
- Shadow mode compara sin side effects
- Verbose mensajes detrás de toggle

## No Hacer
- Mezclar lógica de UI
- Introducir efectos irreversibles sin flag
- Duplicar fórmulas en múltiples lugares
