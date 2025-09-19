## 2025-09-19
Última actualización: 2025-09-19

- Combate: el chequeo de precisión en `DamageResolver.ResolverAtaqueFisico` ahora aplica la penalización de Supervivencia cuando el flag `--precision-hit` está activo. Se toma `Precision` del ejecutor y se multiplica por el `FactorPrecision(etH, etS, etF)` derivado de las etiquetas de Hambre/Sed/Fatiga, con clamps centralizados vía `CombatBalanceConfig`. No cambia el daño base ni otros pasos; comportamiento es no intrusivo cuando no hay servicio/config cargada.
- Documentación: se reflejará en `Docs/Roadmap.md` y en la sección de combate de `Arquitectura_y_Funcionamiento.md` en la próxima pasada de documentación.
- UX Combate: se agregó un formateador de mensajes explicativos en `DamageResolver` que detalla el pipeline de daño sin alterar cálculos:
  - Físico: Base → Defensa (con nota de Penetración si está activa) → Mitigación → Crítico (nota) → Final.
  - Mágico: Base → Defensa Mágica (±Penetración) → Mitigación → Resistencia "magia" → Vulnerabilidad → Final.
- Mensajería: se mantiene la primera línea compacta para compatibilidad de pruebas, y se agrega una línea extra con el detalle didáctico cuando no hay evasión.
- Roadmap: ítem [5.14] creado con ejemplos, decisiones y próximos pasos (diseñar `CombatMessageFormatter` más formal y asserts de texto en [9.8]).

- Opciones de juego: se añadió en Menú Principal → Opciones la posibilidad de alternar en runtime:
  - Precisión (hit-check)
  - Penetración
  - Verbosidad de Combate (controla la línea didáctica del cálculo de daño)
  Esto complementa los flags CLI `--precision-hit`, `--penetracion` y `--combat-verbose`.


## 2025-09-18 — Estado del personaje: modo detallado y acceso por menú

- Se añadió un modo "detallado" al `EstadoPersonajePrinter` (`MostrarEstadoPersonaje(pj, bool detallado=false)`). Cuando está activo, se imprime una nueva sección "Equipo" listando slots: Arma, Casco, Armadura, Pantalón, Zapatos, Collar, Cinturón, Accesorio 1 y 2, con nombre del ítem y stats clave (Rareza/Perfección; para armas, Daño Físico/Mágico). La vista compacta se mantiene como predeterminada.
- `Juego.MostrarEstadoPersonaje` ahora expone un overload que acepta el flag `detallado` y el `Menú Fijo` incluye una opción separada para abrir el estado en modo detallado.
- Validación: build y suite en verde (58/58) tras los cambios, ver salida de tareas de build/test.

## 2025-09-18 — Gating de menú de ciudad, rediseño Estado y fix CS0234

- Corrección build: se resolvió el error intermitente `CS0234` en `RecoleccionService.cs` (referencia a `MiJuegoRPG.Personaje.NuevoObjeto` inexistente). La validación de herramienta ahora usa el tipo correcto `ObjetoConCantidad` y se agregó `using MiJuegoRPG.Personaje;` para simplificar la firma. Build limpio tras `dotnet clean` y suite verde.
- Gating de menú de ciudad: `Juego.MostrarMenuPorUbicacion` solo muestra el menú de ciudad si el sector es `Tipo:"Ciudad"` y además `EsCentroCiudad` o `CiudadPrincipal` son verdaderos. En otras partes de la ciudad se utiliza el menú de “Fuera de Ciudad”. Soporte de datos: `PjDatos/SectorData.cs` ahora tiene `Tipo` por defecto `"Ruta"` para evitar clasificaciones falsas cuando el JSON no especifica el tipo.
- Estado del personaje (UI): `EstadoPersonajePrinter` fue remaquetado con `UIStyle` para un aspecto profesional y compacto: encabezado/resumen, barras de Vida/Maná/Energía y XP, atributos con bonos agregados, y sección de supervivencia con etiquetas por umbral. En línea con la futura migración a Unity.
- Validación: `dotnet test --nologo` en verde (58/58). Se actualizó `Docs/Roadmap.md` con estos avances y se registró este cambio aquí para trazabilidad.

## 2025-09-17 — Fix NRE en Recolección (herramienta requerida)

- Se corrigió un `NullReferenceException` al ejecutar recolección en nodos con `Requiere` (p. ej., "Pico de Hierro") cuando partidas antiguas tenían `Inventario.NuevosObjetos == null`.
- Cambio: en `Motor/Servicios/RecoleccionService.cs` (`EjecutarAccion`), la validación de herramienta ahora usa null-safety (`?.`) y búsqueda case-insensitive sobre la lista local segura. Si falta la herramienta, se informa por UI y no rompe.
- Validación: build correcto y suite en verde (58/58).

## 2025-09-17 — Mensajería de combate unificada vía DamageResolver

- Se eliminaron impresiones directas a UI en `Personaje.AtacarFisico/AtacarMagico` y `Personaje.RecibirDanio*`, así como en `Enemigo.AtacarFisico/AtacarMagico`. En su lugar, la mensajería de combate se centraliza en `DamageResolver`, que compone `ResultadoAccion.Mensajes`.
- `CombatePorTurnos`: el turno de los enemigos ahora usa `DamageResolver.ResolverAtaqueFisico(enemigo, jugador)` y muestra únicamente los mensajes de `ResultadoAccion`, evitando duplicados e inconsistencias (por ejemplo, “0 de daño”).
- Logs de depuración se mantienen mediante `Logger.Debug(...)` en puntos clave (evadidos y aplicación de daño) sin afectar la UI del jugador.
- Resultado: suite en verde (58/58). Documentación pendiente de reflejar el estado de [5.13] en el Roadmap.

## 2025-09-17 — Remaquetado de Roadmap (Combate/Testing)

- Se reestructuraron las secciones `5. COMBATE` (ítems [5.8], [5.10], [5.13]) y `9. TESTING` (ítem [9.8]) en `Docs/Roadmap.md` para mejorar legibilidad y trazabilidad.
- Nuevo formato por ítem: sub-bloques "Estado", "Descripción", "Decisiones/Conclusiones" y "Próxima acción". Objetivo: lectura rápida en GitHub y status claro por bloque.
- No hay cambios en runtime ni en firmas de código; es puramente documental. Se cuidó la compatibilidad con `markdownlint` (MD032/MD007) y se mantuvieron enlaces/identificadores originales.

## 2025-09-17 — Penetración en pipeline de combate (flag `--penetracion`)

- Se implementó la etapa de Penetración en el pipeline de daño de forma no intrusiva y opcional (desactivada por defecto). Al habilitar el flag CLI `--penetracion`, la defensa efectiva del objetivo se reduce en proporción a la `Estadisticas.Penetracion` del atacante ANTES de aplicar mitigaciones/resistencias.
- Técnica utilizada: contexto ambiental `CombatAmbientContext` que transporta la penetración del atacante durante la ejecución del ataque sin modificar firmas públicas. `DamageResolver` establece el valor de forma temporal alrededor de `AtacarFisico/AtacarMagico` cuando el ejecutor es `Personaje`.
- Receptores actualizados: `Enemigo.RecibirDanioFisico/Magico` y `Personaje.RecibirDanioFisico/Magico` leen la penetración del contexto y calculan `defensaEfectiva = round(defensaBase * (1 - pen))` antes de `Mitigacion*`. El daño mágico mantiene el orden: Defensa→Mitigación→Resistencia(`magia`)→Vulnerabilidad.
- CLI y toggles: se añadió `GameplayToggles.PenetracionEnabled` en `Program.cs` y el flag `--penetracion` con ayuda en `--help`. El flag `--precision-hit` se mantuvo y se aclaró que aplica al ataque físico.
- Pruebas unitarias: `PenetracionPipelineTests` con escenarios deterministas y expectativas numéricas:
  - Físico con pen: defensa 30, pen 20%, mitigación 10% sobre daño 100 → `DanioReal = 68`.
  - Mágico con pen: defensa mágica 20, resistencia `magia` 30%, vulnerabilidad 1.2, pen 25% → `DanioReal = 71`.
  - Toggle OFF: mismo caso físico inicial pero con `--penetracion` desactivado → `DanioReal = 63`.
- Resultado: build y suite de pruebas en verde (total 52). Documentación sincronizada en `Docs/Roadmap.md` ([5.8], [5.10], [9.8]). Pendiente: caps/curvas de `Penetracion`/`CritChance`/`CritMult`/`Precision` en `Docs/progression_config.md`.

## 2025-09-17 — Caps de combate centralizados y tests Crit+Pen

- Se creó `Motor/Servicios/CombatBalanceConfig.cs` para cargar caps opcionales (`StatsCaps`) desde `DatosJuego/progression.json` con defaults conservadores (`PrecisionMax=0.95`, `CritChanceMax=0.50`, `CritMult∈[1.25,1.75]`, `PenetracionMax=0.25`).
- `Personaje/Estadisticas` ahora aplica clamps centralizados a `Precision`, `CritChance`, `CritMult` y `Penetracion` usando el servicio anterior (sin cambiar fórmulas base).
- Se añadieron pruebas `EstadisticasCapsTests` verificando que valores extremos respetan los caps por defecto.
- Roadmap actualizado ([9.8]) para reflejar casos de interacción Crítico + Penetración (físico y mágico) que validan orden, `DanioReal` y flag `FueCritico`.
- `Docs/progression_config.md` incluye ahora la sección `StatsCaps` en el contrato JSON y reglas de validación.

## 2025-09-17 — Fix pruebas: alias de tipo `Personaje`

- Se corrigió un error de compilación intermitente en `MiJuegoRPG.Tests/CritPenetracionInteractionTests.cs` (CS0118: `Personaje` espacio de nombres usado como tipo) introduciendo un alias explícito: `using PersonajeModel = MiJuegoRPG.Personaje.Personaje;` y reemplazando instancias de `new Personaje(...)` por `new PersonajeModel(...)`.
- Resultado: la suite vuelve a verde estable (55/55) en ejecuciones repetidas.

## 2025-09-17 — Docs: Arquitectura y caps centralizados

- `Docs/Arquitectura_y_Funcionamiento.md`: sección 3 actualizada para reflejar que `Precision`, `CritMult` y `Penetracion` se clampean vía `CombatBalanceConfig` (fuente `DatosJuego/progression.json` → `StatsCaps`).
- Añadida subsección 3.2 con fórmula en KaTeX, defaults y enlace a `Docs/progression_config.md`.

## 2025-09-17 — Tests: orden Defensa→Mitigación→Resistencias/Vulnerabilidades

- Se añadieron pruebas `DamagePipelineOrderTests` que validan, de forma determinista, el orden de aplicación en el pipeline de daño no intrusivo:
  - Mágico: Defensa → Mitigación → Resistencia (canal "magia") → Vulnerabilidad.
  - Físico: Defensa → Mitigación.
  - Escenarios adicionales: mágico sin vulnerabilidad (solo defensa/mitigación/resistencia) y mágico solo con vulnerabilidad.
- Metodología: atacante plano que aplica 100 de daño; `EnemigoEstandar` configurado con valores controlados; se verifica `DanioReal` por delta de vida vía `DamageResolver` y se comparan resultados esperados (redondeos con `MidpointRounding.AwayFromZero`).
- Roadmap actualizado ([9.8]) para reflejar el avance de cobertura; próximos pasos: penetración, caps desde `progression.json` y centralización total de mensajería.

## 2025-09-17 — Ataque Mágico unificado vía DamageResolver

- `AtaqueMagicoAccion` ahora invoca `DamageResolver.ResolverAtaqueMagico`, manteniendo el cálculo de daño actual pero unificando metadatos y mensajería: `DanioReal` por delta de vida, `FueEvadido` cuando no aplica daño y `FueCritico` bajo la misma política que el físico (crítico forzado si `CritChance>=1.0` en pruebas).
- Documentación sincronizada: `Docs/Arquitectura_y_Funcionamiento.md` refleja que el mágico también fluye por el resolver; `Docs/Roadmap.md` actualizado en [5.8]/[9.8].
- Calidad: se corrigieron avisos de `markdownlint` (MD007) en listas anidadas de `Arquitectura_y_Funcionamiento.md`.

## 2025-09-17 — Combate: precisión opcional y pruebas

- Se introdujo chequeo de precisión opcional en `DamageResolver` previo al ataque físico, controlado por el flag CLI `--precision-hit` (variable `GameplayToggles.PrecisionCheckEnabled`).
- Mensajería y metadatos: cuando falla por precisión se marca `FueEvadido=true` y se emite un mensaje de fallo; cuando `CritChance>=1.0` en `Personaje`, el crítico se fuerza para pruebas deterministas.
- Se ajustó el cálculo de `DanioReal` en `ResultadoAccion` tomando la diferencia de vida del objetivo antes/después del ataque para reflejar mitigaciones.
- Pruebas añadidas (`AccionesCombateTests`): `AtaqueFisico_PrecisionToggle_AlFallarNoHayDano` y `AtaqueFisico_CriticoForzado_SeMarcaCritico`. Todas las pruebas existentes se mantienen verdes (47/47).
- Balance por defecto sin cambios: el flag está desactivado por defecto.

## 2025-09-17 — Expansión documental detallada

- `progression_config.md`: se añadieron fórmulas en KaTeX, ejemplos numéricos paso a paso, orden de aplicación (clamps) y contrato JSON sugerido con defaults. Se incluyeron pruebas recomendadas y guías de tuning.
- `Arquitectura_y_Funcionamiento.md`: se profundizó en contratos (interfaces/DTOs), pipeline de combate con orden exacto y límites, referencias a `Flujo.txt`, y apéndice de firmas públicas. Además, se documentó el estado actual (MVP) del pipeline de combate: precisión opcional activable por `--precision-hit`, cálculo de `DanioReal` por delta de vida y forzado de crítico (`CritChance>=1.0`) para pruebas. Objetivo: facilitar onboarding y migración a Unity.
- Roadmap: anotación del hito documental y recordatorio de política de “fuente única”.
- Índice: `Docs/README.md` actualizado con enlaces profundos a secciones clave de `Flujo.txt` (menús) y `Arquitectura_y_Funcionamiento.md` (pipeline/contratos) para navegación rápida.

### Navegación y anclas

- Se añadieron encabezados H1 en `Flujo.txt` para permitir enlaces directos por sección (menús y flujo del juego).
- `Arquitectura_y_Funcionamiento.md` ahora enlaza a cada sección específica de `Flujo.txt` (Inicio, Menú Principal, Ciudad, Fuera de Ciudad, Misiones/NPC, Rutas, Combate, Entre Combates, Menú Fijo).

Este documento registra cambios cronológicos por sesión. El `Roadmap.md` mantiene el plan por áreas y los próximos pasos.

## 2025-09-17

- Hecho
  - Separación de la bitácora a este documento y limpieza de `Docs/Roadmap.md` (mantener roadmap sin historia cronológica).
  - README unificado: se eliminó `MiJuegoRPG/README_EXPLICACION.txt` y se consolidó `MiJuegoRPG/Docs/README.md` como índice principal.
  - Añadida sección de referencia de CLI/herramientas en `Docs/README.md` (validadores, reparadores, QA de mapa, logger).
  - Creada `Docs/Guia_Ejemplos.md` con ejemplos para principiantes y enlazada desde el índice.
  - Actualizado `Flujo.txt` para reflejar el flujo real implementado (inicio, menús de ciudad/fuera de ciudad, rutas, misiones/NPC, combate, menú fijo y entre combates), incluyendo notas de reputación/supervivencia/logger.
  - Conexión de documentación: README raíz con enlaces clicables a Docs/ y `Flujo.txt`. En `Docs/README.md` añadida sección “Estudia el juego (fuente única)” y política de no duplicación.
- En progreso
  - Sincronización de documentación y enlaces cruzados (Docs/README, Arquitectura).
- Decisiones
  - Mantener la bitácora fuera del Roadmap para reducir ruido y facilitar lectura del plan.

---

## 2025-09-16

- Tests/Infra:
  - Ajustado `MiJuegoRPG.Tests.csproj` para copiar recursivamente `MiJuegoRPG/DatosJuego/**` al output de pruebas. Resuelve errores MSB3030 por reorganización de enemigos (bioma/nivel/categoría). Suite de pruebas en verde: 45/45 PASS.
  - Verificado build de solución post-cambio: ambos proyectos compilan correctamente.
- Documentación/Quality:
  - Normalizadas viñetas/indentación en Roadmap (correcciones markdownlint) y sincronizada la sección 9 con la nueva configuración de assets.
- Datos/Enemigos/Elemental (estado):
  - Loader recursivo de enemigos con convención de ignorar JSON en la raíz de `nivel_*` ya activo.
  - `VulnerabilidadesElementales {1.0..1.5}` integrado y documentado; aplicado en daño mágico post-mitigación.

## 2025-09-15

- Combate → Pipeline de daño (5.8):
  - `DamageResolver` ahora anota evasión: cuando el daño retornado es 0 (por chequeo de `IEvadible` en `AtacarFisico/AtacarMagico`), se marca `ResultadoAccion.FueEvadido = true` y se agrega mensaje “¡El objetivo evadió el ataque!”.
  - Se mantiene comportamiento no intrusivo: el cálculo de daño sigue delegado al ejecutor; no se alteraron fórmulas ni balance actual.
- DTO de resultado: `ResultadoAccion` conserva flags `FueCritico` y ahora refleja también la evasión.
- Acciones: `AtaqueFisicoAccion` ya usa `DamageResolver` (sin cambiar mensajes existentes salvo añadir el de evasión cuando aplica).
- Tests: corregido constructor de `Personaje` en pruebas (`new Personaje("Heroe")`) y añadido caso determinista de evasión (objetivo que siempre evade). Suite de pruebas ejecutada con 4/4 PASS.
- Build: solución compilada en Debug sin errores.

---

Plantilla para futuras sesiones

- Hecho:
- En progreso:
- Decisiones:
- Siguientes pasos:
