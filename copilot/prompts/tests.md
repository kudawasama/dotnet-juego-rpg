# Prompt Especializado: Tests

## Objetivo
Garantizar cobertura mínima determinista y significativa.

## Preguntas Diagnóstico
1. ¿Existen casos feliz, edge y fallback?
2. ¿RNG controlado con SetSeed?
3. ¿Asserts expresivos (FluentAssertions futuro)?
4. ¿Evitar pruebas frágiles dependientes de logs volátiles?
5. ¿Se mide interacción entre componentes (integration) donde aplica?

## Checklist
- Seed fijo
- Nombrado claro
- Evitar sleeps / waits
- Datos de prueba aislados (carpeta temporal)
- Teardown limpia recursos

## No Hacer
- Tests que dependan de orden incidental de archivos
- Afirmar textos completos si basta un token estable
