# dotnet-juego-rpg

Centro de documentación y enlaces rápidos del proyecto. Para la guía completa visita:

- Documentación principal: [MiJuegoRPG/Docs/README.md](MiJuegoRPG/Docs/README.md)
- Roadmap: [MiJuegoRPG/Docs/Roadmap.md](MiJuegoRPG/Docs/Roadmap.md)
- Bitácora de cambios: [MiJuegoRPG/Docs/Bitacora.md](MiJuegoRPG/Docs/Bitacora.md)
- Arquitectura y funcionamiento: [MiJuegoRPG/Docs/Arquitectura_y_Funcionamiento.md](MiJuegoRPG/Docs/Arquitectura_y_Funcionamiento.md)
- Configuración de progresión: [MiJuegoRPG/Docs/progression_config.md](MiJuegoRPG/Docs/progression_config.md)
- Guía de ejemplos (principiantes): [MiJuegoRPG/Docs/Guia_Ejemplos.md](MiJuegoRPG/Docs/Guia_Ejemplos.md)
 Movimiento (cómmo moverse): ver Menú de Rutas en [Flujo de Menús](MiJuegoRPG/Docs/Flujo.md)

Cómo compilar y probar (PowerShell):

```md
dotnet build
dotnet test --nologo
```

Ejecutar el juego (desde carpeta `MiJuegoRPG/`):

## Uso del Agente (Copilot / Reglas Internas)

Fuente única de reglas: `copilot/agent.md`.

Pasos recomendados al iniciar una sesión de trabajo:

1. Abrir `copilot/agent.md` y leer cambios recientes (sección Roadmap de Infra).
2. Consultar prompts especializados según el tipo de tarea:
    - Combate: `copilot/prompts/combate.md`
    - Datos / Repos: `copilot/prompts/datos.md`
    - Infra / CI / Estilo: `copilot/prompts/infra.md`
    - Tests: `copilot/prompts/tests.md`
    - Review: `copilot/prompts/review.md`

3. Añadir comentarios guía en código cuando proceda:

Planilla Sugerida:

```csharp
    @<agente>
    Contexto:
    Objetivo(s):
    Cambios previstos:
    Código/Fragmentos (si aplica):
    Restricciones / Invariantes:
    Preguntas específicas:
    Riesgos percibidos:
    Siguiente acción esperada del agente (opcional):

```csharp
// @AgenteCombate: validar interacción crítico vs penetración antes de tunear
// @AgenteDatos: revisar duplicados en overlay
```md
4. Tras aplicar cambios relevantes a núcleo / datos: actualizar Bitácora y Roadmap.

## CI

Workflow básico (build + test) en `.github/workflows/ci.yml`. El badge se añadirá al estabilizar la rama principal.

```md
dotnet run --project MiJuegoRPG.csproj
```

Flags útiles (ver más en la documentación principal):

- `--help` Muestra ayuda de CLI.
- `--log-level=debug|info|warn|error|off` Controla verbosidad.
- `--validar-datos[=report|<ruta>]` Ejecuta validadores y opcionalmente genera reporte.
- `--reparar-materiales=report|write[;ruta]` Herramienta QA para nodos de recolección.
