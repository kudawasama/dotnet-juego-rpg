# Prompt Especializado: Infra

## Objetivo
Estandarizar estructura, CI, analyzers y soporte futuro Unity.

## Preguntas Diagnóstico
1. ¿Compatibilidad C#10 preservada?
2. ¿Se añadieron analyzers sin romper build?
3. ¿Workflow CI cubre build+test?
4. ¿Archivos legacy marcados como tales?
5. ¿Se evita lógica en raíz (scripts dispersos)?

## Checklist
- .editorconfig presente
- Analyzers referenciados (warnings controlados)
- CI YAML con cache NuGet
- README con instrucciones agente
- Scripts legacy migrados o eliminados

## No Hacer
- Tratar warnings como errors prematuramente
- Introducir toolchains no necesarias
