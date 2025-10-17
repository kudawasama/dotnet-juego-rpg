# Agente del Proyecto (Fuente Única)

> LEGACY: El archivo `.github/chatmodes/MIJuego.chatmode.md` queda como referencia histórica. Cualquier cambio nuevo de reglas va aquí.

## Principios

- Data-driven primero (JSON fuente de verdad).
- Compatibilidad C#10 (Unity 2022 LTS target) – evitar features posteriores.
- Resiliencia: degradar, nunca explotar por datos incompletos.
- Progresión lenta + dificultad justa: evitar escalados explosivos.
- Evitar duplicación: factorizar si se repiten ≥2 veces bloques no triviales.

## Flujo Operativo del Agente

1. Identificar intención.
2. Leer artefactos relevantes (código + docs) antes de asumir.
3. Micro‑plan en bullets (solo pasos necesarios).
4. Ejecutar cambios mínimos (patch). Añadir tests si toca lógica.
5. Validar: build + tests afectados + JSON válido.
6. Documentar (Bitácora / Roadmap / Arquitectura) si es feature/refactor núcleo.
7. Resumen (acciones, estado quality gates, próximos pasos breves).

## Quality Gates

- Build PASS.
- Tests afectados verdes (mínimo: caso feliz + edge + fallback con `RandomService.SetSeed`).
- Sin referencias nuevas a enums rareza.
- Logs sin spam (agrupar warnings repetidos).
- No regresión de performance evidente (loops críticos sin LINQ pesado).

## Comentarios Guiados (@Agente)

Insertar en código:

```csharp
// @AgenteCombate: validar orden penetración antes de mitigación
// @AgenteDatos: asegurar primer archivo base gana, overlay reemplaza
// @AgenteTests: mantener seed determinista
```

El agente los usará como anclas de revisión rápida.

### Agentes soportados

| Etiqueta            | Ámbito / Uso principal                                      |
|---------------------|-------------------------------------------------------------|
| `@AgenteCombate`    | Reglas de pipeline de daño, orden pasos, penetración, crit. |
| `@AgenteDatos`      | Carga/merge de catálogos JSON, normalización, overlays.     |
| `@AgenteTests`      | Determinismo, seeds, cobertura mínima, fixtures.            |
| `@AgenteInfra`      | Build, pipelines CI, tooling, performance infra.            |
| `@AgenteReview`     | Notas de auditoría o verificación manual pendiente.        |

Convención: comentarios siempre en una sola línea iniciando con `// @AgenteX:` para que se puedan grep-ear fácilmente (`grep -R "@Agente"`). Extiende esta tabla si se añaden nuevas categorías; evita alias redundantes.

## Repositorios Jerárquicos (Equipo/Materiales)

- Base recursiva (objeto o lista) → primer nombre gana.
- Overlay (`*_overlay.json` o `<tipo>.json`) reemplaza por `Nombre` (case-insensitive).
- Rareza normalizada con `RarezaNormalizer` (alias → forma canónica).
- Fallback rareza desconocida: peso=1, perfección 50–50, log warn (agregado).

## Combat Pipeline (estado actual)

Orden estable: BaseDamage → Hit/Evasión (si toggle) → Penetración → Defensa → Mitigación% → Crítico (escalado F) → Vulnerabilidad/Elemento → Redondeo.
Flags: `--damage-shadow`, `--damage-live`, penetración toggle, verbose.

## Documentación Obligatoria Cuándo

- Feature núcleo (combate, progresión, repos data, validadores).
- Refactor que cambia contratos.
- Cambio formato JSON.
- Balance con impacto o deprecación pública.

Bitácora (plantilla):

```markdown
### YYYY-MM-DD — <Resumen>
<Qué cambió / Por qué / Impacto>
```

Roadmap: editar solo fila implicada.

## Tests

- Determinismo: `RandomService.SetSeed` o inyectar RNG controlado.
- Tres mínimos: feliz / edge / fallback degradado.
- Nombrar claramente (GivenWhenThen opcional).

## Performance

- Evitar LINQ en loops de combate.
- Reutilizar estructuras temporales si se generan masivamente.
- Cargar catálogos una sola vez; invalidar manual.

## No Hacer

- Duplicar reglas de este archivo en prompts especializados.
- Introducir features > C#10.
- Registrar cada micro‑warn por ítem desconocido (agrupar).

## Prompts Especializados

Ver `copilot/prompts/`:

- combate.md
- datos.md
- infra.md
- tests.md
- review.md

## Roadmap de Infra Agente (interno)

- [x] Agent base
- [ ] Completar prompts
- [ ] Integrar coverage en CI
- [ ] Endurecer analyzers (posterior)

---
Última actualización: (placeholder)
