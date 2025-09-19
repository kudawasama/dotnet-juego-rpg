# dotnet-juego-rpg

Centro de documentación y enlaces rápidos del proyecto. Para la guía completa visita:

- Documentación principal: [MiJuegoRPG/Docs/README.md](MiJuegoRPG/Docs/README.md)
- Roadmap: [MiJuegoRPG/Docs/Roadmap.md](MiJuegoRPG/Docs/Roadmap.md)
- Bitácora de cambios: [MiJuegoRPG/Docs/Bitacora.md](MiJuegoRPG/Docs/Bitacora.md)
- Arquitectura y funcionamiento: [MiJuegoRPG/Docs/Arquitectura_y_Funcionamiento.md](MiJuegoRPG/Docs/Arquitectura_y_Funcionamiento.md)
- Configuración de progresión: [MiJuegoRPG/Docs/progression_config.md](MiJuegoRPG/Docs/progression_config.md)
- Guía de ejemplos (principiantes): [MiJuegoRPG/Docs/Guia_Ejemplos.md](MiJuegoRPG/Docs/Guia_Ejemplos.md)
- Movimiento (cómo moverse): ver Menú de Rutas en [Flujo.txt](Flujo.txt)

Cómo compilar y probar (PowerShell):

```
dotnet build
dotnet test --nologo
```

Ejecutar el juego (desde carpeta `MiJuegoRPG/`):

```
dotnet run --project MiJuegoRPG.csproj
```

Flags útiles (ver más en la documentación principal):

- `--help` Muestra ayuda de CLI.
- `--log-level=debug|info|warn|error|off` Controla verbosidad.
- `--validar-datos[=report|<ruta>]` Ejecuta validadores y opcionalmente genera reporte.
- `--reparar-materiales=report|write[;ruta]` Herramienta QA para nodos de recolección.
