# Prompt Especializado: Datos

## Objetivo

Mantener integridad y consistencia de catálogos JSON (equipo, materiales, habilidades, rarezas).

## Preguntas Diagnóstico

1. ¿Formato soporta objeto o lista? ¿Se normaliza internamente?
2. ¿Hay nombres duplicados? ¿Cómo se resuelven?
3. ¿Rareza está normalizada? ¿Fallback aplicado?
4. ¿Se protege contra archivos corruptos (warn + continuar)?
5. ¿Caching y overlay funcionan (primer base gana)?

## Checklist

- RarezaNormalizer aplicado
- Overlay reemplaza por Nombre (case-insensitive)
- Warnings agregados (no spam individual)
- Invalidate() limpia caché de forma segura
- Tests: load jerárquico, overlay, rareza normalization

## No Hacer

- Lanzar excepciones duras por datos faltantes
- Duplicar reglas en cada repo concreto
