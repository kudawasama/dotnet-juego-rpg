# Documentación (índice)

Este índice centraliza la documentación del proyecto y sirve como punto de entrada.

- [Roadmap (plan y estado)](./Roadmap.md)
- [Bitácora (historial por sesión)](./Bitacora.md)
- [Arquitectura y funcionamiento](./Arquitectura_y_Funcionamiento.md)
- [Configuración de progresión](./progression_config.md)
- [Guía de ejemplos (para principiantes)](./Guia_Ejemplos.md)

## Guía rápida

- Compilar todo:

```powershell
dotnet build
```

- Ejecutar pruebas:

```powershell
dotnet test --nologo
```

- Ejecutar el juego (desde la raíz del repo):

```powershell
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj
```

También puedes usar las tareas de VS Code del workspace:

- Build: "Build .NET project" → `dotnet build`
- Tests: "Compilar y ejecutar pruebas" → `dotnet test --nologo`
- Ejecutar juego: "Compilar y ejecutar juego para probar nombres de sectores"

## Referencia de CLI y herramientas

Puedes pasar opciones al ejecutable al final del comando `dotnet run --`.

Ejemplos generales:

```powershell
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj -- --help
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj -- --log-level=debug
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj -- --log-off
```

Validación de datos:

- Ejecutar validador básico y mostrar en consola:

```powershell
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj -- --validar-datos
```

- Generar reporte en la ruta por defecto (`PjDatos/validacion/`):

```powershell
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj -- --validar-datos=report
```

- Generar reporte en una ruta específica:

```powershell
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj -- --validar-datos=PjDatos/validacion/mi_reporte.txt
```

Reparación de materiales inválidos en sectores del mapa:

- Modo reporte (no modifica archivos):

```powershell
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj -- --reparar-materiales=report
```

- Modo escritura (aplica cambios) con ruta de reporte personalizada (opcional tras `;`):

```powershell
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj -- --reparar-materiales=write;PjDatos/validacion/materiales_reparacion.txt
```

Herramientas de QA de mapa:

- Normalizar bidireccionalidad de conexiones:

```powershell
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj -- --normalizar-conexiones
```

- Asignar biomas por bandas (opciones: `ol` = Océano Lejano, `oc` = Océano, ambas si no se especifica):

```powershell
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj -- --asignar-biomas
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj -- --asignar-biomas=ol,oc
```

- Hidratar nodos de recolección desde `DatosJuego/biomas.json` (opcional `=max`):

```powershell
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj -- --hidratar-nodos
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj -- --hidratar-nodos=5
```

Otros flags útiles:

- Mostrar ayuda y terminar:

```powershell
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj -- --help
```

- Control de logger desde CLI:

```powershell
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj -- --log-off
dotnet run --project MiJuegoRPG/MiJuegoRPG.csproj -- --log-level=info
```

Notas:

- Los reportes se guardan por defecto bajo `PjDatos/validacion/` si no se especifica ruta.
- `--log-off` tiene precedencia al inicio; luego puedes reactivarlo desde el menú Opciones.
- Las preferencias de logger se guardan por partida y se aplican al cargar.

## Convenciones

- Roadmap: visión, objetivos y próximos pasos por área. Sin entradas cronológicas.
- Bitácora: cambios diarios y decisiones. Enlazar al Roadmap cuando afecten prioridades.
- Arquitectura: contratos, pipelines, servicios y fórmulas relevantes.
- progression_config: parámetros de progresión y balance.
