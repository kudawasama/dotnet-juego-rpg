# Prompt Especializado: Review

## Objetivo

Checklist sistemático de revisión de cambios antes de merge.

## Preguntas Diagnóstico

1. ¿SRP respetado? ¿Clase hace más de una cosa?
2. ¿Nulos tratados? ¿Existe riesgo de NullReference en caminos comunes?
3. ¿Hay duplicación evitable?
4. ¿Logs son claros y no ruidosos?
5. ¿Datos siguen siendo la fuente de verdad?

## Checklist

- SRP
- Null-safety
- Duplicación evaluada
- Logs nivel correcto
- Data-driven verificado
- Tests actualizados
- Docs sincronizadas (Bitácora/Roadmap/Arquitectura)

## No Hacer

- Aprobar sin ejecutar tests
- Mezclar refactor + feature grande en un commit
