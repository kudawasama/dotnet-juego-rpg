<!-- markdownlint-disable MD003 -->
PLAN DE REFACTORIZACIÓN Y PROGRESO
==================================

## Estado actual (resumen)

- Progreso estimado: 31/221 Hecho, 13/221 Parcial, 177/221 Pendiente (~14%).
- Sistemas base listos: ProgressionService, RandomService, PathProvider, GuardadoService y EventBus integrados.
- UI desacoplada: `IUserInterface` implementada con `ConsoleUserInterface` y `SilentUserInterface`; menús principales migrados.
- Combate: pipeline en progreso (`DamageResolver` con evasión/crit metadata), acciones básicas físicas/mágicas y uso de pociones integrados.
- Mundo y recolección: biomas con nodos hidratados por data; energía y rarezas operativas; encuentros data-driven con gating por kills/hora y cooldowns.
- Enemigos data-driven: estructura por bioma/nivel/categoría implementada para Bosque `nivel_1_3` con cuotas cumplidas; validador de datos activo.
- Documentación: `Docs/Bitacora.md` separada del Roadmap; arquitectura ampliada; tareas y estándares definidos en chatmode.

Más detalle en el snapshot extenso al final de este archivo: ver sección "ESTADO ACTUAL (snapshot)".

Estado de avance (resumen): 31/221 Hecho · 13/221 Parcial · 177/221 Pendiente
███████░░░░░░░░░░ 14% completado (estimado por ítems del roadmap)

Formato columnas: [ID] Estado | Área | Descripción breve | Próxima acción
Estados posibles: Pendiente, En curso, Parcial, Hecho, Bloqueado

Legend inicial: Solo la 1.x se empieza ahora para evitar cambios masivos de golpe.

## Tabla de contenidos

- [1. FUNDAMENTOS](#1-fundamentos-infra--enumeraciones--servicios-base)
- [2. EVENTOS Y DESACOPLAMIENTO](#2-eventos-y-desacoplamiento)
- [3. PROGRESIÓN Y ATRIBUTOS](#3-progresión-y-atributos)
- [4. RECOLECCIÓN Y MUNDO](#4-recolección-y-mundo)
- [5. COMBATE](#5-combate)
- [6. MISIONES Y REQUISITOS](#6-misiones-y-requisitos)
- [7. REPOSITORIOS / DATA](#7-repositorios--data)
- [8. UI / PRESENTACIÓN](#8-ui--presentación)
- [9. TESTING](#9-testing)
- [10. LIMPIEZA / QUALITY](#10-limpieza--quality)
- [11. CLASES DINÁMICAS / PROGRESIÓN AVANZADA](#11-clases-dinámicas--progresión-avanzada)
- [12. REPUTACIÓN](#12-reputación)
- [14. MIGRACIÓN / INTEGRACIÓN UNITY](#14-migración--integración-unity)
- [13. ADMIN / HERRAMIENTAS QA](#13-admin--herramientas-qa)
- [15. OBJETOS / CRAFTEO / DROPS](#15-objetos--crafteo--drops)
- [16. ESTADO POR ARCHIVO / MÓDULO](#16-estado-por-archivo--módulo-inventario-actual)
- [17. HABILIDADES Y MAESTRÍAS](#17-habilidades-y-maestrías)
- [18. ITEMIZACIÓN AVANZADA](#18-itemización-avanzada)
- [19. ECONOMÍA Y SINKS](#19-economía-y-sinks)
- [20. MUNDO DINÁMICO Y EXPLORACIÓN](#20-mundo-dinámico-y-exploración)
- [24. LOCALIZACIÓN (i18n)](#24-localización-i18n)
- [22. LOGROS Y RETOS](#22-logros-y-retos)
- [23. GUARDADO VERSIONADO Y MIGRACIONES](#23-guardado-versionado-y-migraciones)
- [25. PERFORMANCE Y CACHING](#25-performance-y-caching)
- [26. ACCESIBILIDAD Y QoL](#26-accesibilidad-y-qol)
- [27. SUPERVIVENCIA Y SISTEMAS REALISTAS](#27-supervivencia-y-sistemas-realistas)

> Nota: La bitácora cronológica se movió a `Docs/Bitacora.md` para mantener este documento enfocado en plan y estado.

## Próximos pasos (prioridad sugerida)

- [5.8] Pipeline de daño (etapa A): chequeo de acierto (Precision vs Evasion) ya integrado de forma opcional en `DamageResolver`. El `Ataque Mágico` ahora también fluye por el resolver (sin paso de precisión) unificando metadatos y mensajería. NUEVO: penetración integrada (reducción de defensa efectiva antes de mitigaciones) detrás del flag `--penetracion`.
- [9.8] Tests pipeline combate: cobertura base creada (hit/miss/crit) con `RandomService.SetSeed` y dummies deterministas; verificación del orden en daño mágico (Defensa→Mitigación→Resistencia→Vulnerabilidad) y físico (Defensa→Mitigación). NUEVO: pruebas de penetración física y mágica (defensa reducida antes de mitigaciones/resistencias) y gating por toggle. Añadidos casos de interacción Crítico + Penetración (físico y mágico) validando `DanioReal` y `FueCritico`. p_hit integra penalización de Supervivencia cuando `--precision-hit` está activo (ver 27.4), pendiente añadir tests dedicados.
- [5.10]/[3.4] Integrar stats de combate: usar `Precision`, `CritChance`, `CritMult`, `Penetracion` de `Estadisticas` (defaults ya presentes) y parametrizar en JSON (`progression.json`) las curvas/caps. Añadir caps sugeridos en `Docs/progression_config.md`.
- [5.13] Mensajería unificada: canalizar todos los mensajes de combate vía `ResultadoAccion` para evitar duplicados.
- [5.14] Texto de combate didáctico/expresivo: ampliar los mensajes de combate para explicar brevemente el cálculo (mitigación, resistencias, vulnerabilidades, crítico y penetración) y el porqué del daño final con ejemplos tipo "Jugador hace 12 de daño; Enemigo reduce 20% por defensa y 10% por mitigación". Integrado un primer formateador en `DamageResolver` y toggle `--combat-verbose` para controlar la verbosidad.
  
  Avance: se agregó control en runtime desde Menú Principal → Opciones para alternar Verbosidad de Combate (ON/OFF) además del flag CLI `--combat-verbose`.
  Avance 2 (tests): se añadieron pruebas que validan presencia del detalle didáctico cuando está ON (físico y mágico) y ausencia cuando hay evasión/fallo por precisión. Ver `CombatVerboseMessageTests`.
- [10.6] Validación de datos: extender `DataValidatorService` a esquemas de objetos/drops/armas con rangos y referencias cruzadas.
- [7.1]/[15.1] Repos JSON: consolidar objetos/materiales/balances bajo `IRepository<T>` con caché e invalidación.
- [5.2] Refactor a cola de acciones en `CombatePorTurnos` tras estabilizar el pipeline.
- [10.5] Documentación de arquitectura: sección ampliada con fórmulas de estadísticas, encuentros, energía, supervivencia y clases dinámicas (ver `Docs/Arquitectura_y_Funcionamiento.md`).

## 1. FUNDAMENTOS (Infra / Enumeraciones / Servicios base)

[1.1] Hecho | Organización | Crear este archivo de tracking | Archivo creado y actualizado periódicamente
[1.2] Hecho | Enumeraciones | Definir enums: Atributo, TipoRecoleccion, OrigenExp | En uso en ProgressionService y RecoleccionService
[1.3] Hecho | Servicio | Crear ProgressionService (sólido) con método AplicarExpAtributo | Recolección + entrenamiento + exploración centralizados (tests pendientes 9.x)
[1.4] Hecho | Servicio | RandomService centralizado (inyectable) | Reemplazados todos los usos de new Random() en dominio
[1.5] Hecho | Limpieza | Sustituir strings mágicos de recolección por enum | Menú y acción usan TipoRecoleccion
[1.6] Hecho | Guardado | Completar GuardadoService (ya scaffold) | Integrado en Juego y Menús, reemplaza GestorArchivos

## 2. EVENTOS Y DESACOPLAMIENTO

[2.1] Hecho | Infra | EventBus simple (pub/sub en memoria) | EventBus.cs + integración ProgressionService
[2.2] Hecho | Progresión | Emitir eventos en subidas de nivel / atributo / misión | Atributos, nivel y misión integrados
[2.3] Parcial | UI | Sustituir Console directa por IUserInterface | Interfaz creada + ConsoleUserInterface + SilentUserInterface para pruebas; InputService delega a Juego.Ui para lectura/pausa. Juego permite inyectar UI vía UiFactory. Progreso: Juego, menús modulares, MenusJuego y Program migrados; Servicios migrados: RecoleccionService, EnergiaService, EstadoPersonajePrinter y MotorMisiones. Pendiente: limpiar Console.* en dominio (Personaje, Inventario, CombatePorTurnos, Objetos y Gestores) y herramientas.

## 3. PROGRESIÓN Y ATRIBUTOS

[3.1] Hecho | Dominio | Unificar experiencia de atributos en estructura (ExpAtributo) | Implementado ExpAtributo
[3.2] Hecho | Dominio | Migrar Personaje a diccionario <Atributo, ExpAtributo> | Personaje.ExperienciaAtributos + migración legacy
[3.3] Hecho | Balance | Parametrizar fórmula en ProgressionConfig (JSON) | progression.json actualizado con escalados y factorMinExp + documentación añadida
[3.4] Pendiente | Stats de combate | Añadir parámetros de progresión para `Precision`, `CritChance`, `CritMult`, `Penetracion` y `Stamina` | Definir en `progression.json` curvas/base/caps por nivel/atributos; integrarlo en cálculo de `Estadisticas`.

## 4. RECOLECCIÓN Y MUNDO

[4.1] Hecho | Servicio | RecoleccionService (mover RealizarAccionRecoleccion + MostrarMenuRecoleccion) | Menú y ejecución centralizados
[4.2] Hecho | Data | Añadir tiempos de respawn y rarezas a nodos | Base lista: Rareza y ProduccionMin/Max en biomas, CooldownBase soportado, hidratación de nodos por nombre desde bioma (tolerante a acentos), etiqueta de producción [Prod X–Y] en UI y telemetría ligera en recolección. Contrato JSON de Materiales normalizado: se reemplazaron tuplas por DTO `MaterialCantidad { Nombre, Cantidad }` para evitar lecturas "0x" y nombres vacíos. Pendiente de balance fino por bioma/rareza (ver 15.7).
[4.3] Hecho | Energía | Integrar coste dinámico según herramienta y bioma | energia.json + cálculo en EnergiaService (modificadores por tipo/herramienta/bioma/atributos/clase)
[4.4] Hecho | UX | Menú híbrido con filtros + búsqueda + cooldown + fallo | Implementado en RecoleccionService
[4.5] Hecho | Robustez | Null-safety y validación de herramienta requerida | Se corrigió un NRE en nodos con `Requiere` cuando `Inventario.NuevosObjetos == null` (partidas antiguas). La comprobación de herramienta ahora es segura y robusta: coincidencia exacta case-insensitive y fallback por coincidencia parcial ("Pico de Hierro" satisface "Pico").

## 5. COMBATE

[5.1] Parcial | Dominio | Definir IAccionCombate + ResultadoAccion | Interfaz `IAccionCombate` y DTO `ResultadoAccion` creados. Acciones básicas implementadas: `AtaqueFisicoAccion` y `AtaqueMagicoAccion`. Integración inicial en `CombatePorTurnos` vía menú Habilidad. Acceso al combate desde mundo habilitado: opción "Combatir" en `MenuFueraCiudad` y encuentros aleatorios activos en `ExplorarSector`.
[5.2] Pendiente | Dominio | Refactor CombatePorTurnos a cola de acciones | Tras 5.1
[5.3] Parcial | Estados | Implementar IEfecto (veneno, sangrado, buff) | Base lista: `IEfecto` creado e integrado a `CombatePorTurnos` con tick por turno y expiración; `EfectoVeneno` implementado y aplicable vía acción (coste de maná). Pendiente: sangrado/hemorragia, aturdimiento y buffs; reglas de stacking/resistencias.
[5.4] Pendiente | Balance | Escalado por velocidad (orden dinámico) | Tras 5.2
[5.5] Hecho | Flujo | Uso de pociones en combate (selección, confirmación, consumo) | Integrado como `IAccionCombate` (`UsarPocionAccion`) y ejecutado vía helper `TryEjecutarAccion` en `CombatePorTurnos`. Aplica patrón de gating (no perder turno si no hay pociones, selección inválida o cancelación). Mensajería unificada por UI. Tests cubren uso y consumo de stack.
[5.6] Parcial | Habilidades | Menú “Habilidad” (Ataque Físico/Mágico) con selección de objetivo | MVP integrado en CombatePorTurnos; arquitectura `IAccionCombate` lista (5.1). Pendiente: costes de recursos/cooldowns (17.3) y más acciones.

[5.7] Hecho | Resistencias | Inmunidades/mitigaciones por enemigo | Se añadió a `Enemigo` soporte de `Inmunidades` (por clave, ej. "veneno") y `MitigacionFisicaPorcentaje`/`MitigacionMagicaPorcentaje` aplicadas tras la defensa. `AplicarVenenoAccion` ahora respeta la inmunidad de no-muertos (zombi/esqueleto). Resultado: peleas más duras y coherentes con fantasía de mundo hostil.

[5.8] Parcial | Pipeline de daño | `DamageResolver` con pasos (`IDamageStep`): Hit/Evasión, Crítico, Defensa, Penetración, Mitigación, Resistencias, Aplicación de daño, OnHit/OnKill |

Estado:

- Parcial (MVP no intrusivo activo con flags).

Descripción:

- Diseñar contrato y ensamblar pasos en orden determinista; centralizar mensajería en un único resultado (`ResultadoAccion`).

Decisiones/Conclusiones:

- `DamageResolver` mínimo mejorado y `AtaqueFisicoAccion` usan el resolver.
- Se registran metadatos de crítico/evasión.
- Chequeo de precisión opcional (flag `--precision-hit`) solo para físico.
- Penetración integrada (flag `--penetracion`) propagando el valor mediante `CombatAmbientContext` para reducir defensa efectiva antes de mitigaciones. Orden validado por tests.

Próxima acción:

- Formalizar contrato `IDamageStep` y ensamblado de pasos.
- Unificar mensajes al 100% vía `ResultadoAccion`.
[5.9] Pendiente | Iniciativa y orden | Turnos por Velocidad/Agilidad + ruido RNG, penalizados por Carga de equipo | Definir cálculo e integrar en `CombatePorTurnos`.
[5.10] Parcial | Precisión/Crítico/Penetración | `Precision`, `CritChance`, `CritMult`, `Penetracion` en `Estadisticas` base y uso en pipeline a través de flags `--precision-hit` y `--penetracion` |

Estado:

- Parcial (stats disponibles; flags operativos; caps documentados en `Docs/progression_config.md`).

Decisiones/Conclusiones:

- El chequeo de precisión aplica solo a físico y está desactivado por defecto.
- Crítico forzado en tests cuando `CritChance >= 1.0` para determinismo.
- Penetración reduce defensa antes de mitigaciones cuando el flag está activo.

Próxima acción:

- Integrar caps/curvas desde `DatosJuego/progression.json` en `Estadisticas`.
- Extender estos campos a data de armas (7.4) y añadir validación.
[5.11] Pendiente | Stamina/Poise | Recurso `Stamina` para acciones físicas y `Poise` para aturdimientos | Costes por arma/peso; caída de Poise causa `Stun` 1 turno. Integración con acciones y estados.
[5.12] Pendiente | Estados avanzados | Sangrado/Aturdimiento/Buffs data-driven | Extender `IEfecto`, resistencias y stacking; aplicar en pipeline y UI.
[5.13] Parcial | Mensajería unificada | Centralizar mensajes de combate |

Estado:

- Parcial (mensajería centralizada en `DamageResolver`; enemigos en `CombatePorTurnos` ya usan resolver; quedan limpiezas menores en rutas legacy de habilidades).

Descripción:

- Unificar la salida de mensajes de combate para que provenga exclusivamente de `ResultadoAccion.Mensajes` compuesto por `DamageResolver`.

Decisiones/Conclusiones:

- Se eliminaron `Console/UI.WriteLine` directos en `Personaje` y `Enemigo` durante ataques/evadir/recibir daño para evitar duplicados.
- `CombatePorTurnos`: el turno de enemigos ahora invoca `DamageResolver.ResolverAtaqueFisico(enemigo, jugador)` y muestra solo `res.Mensajes`.
- Logger sigue registrando eventos en nivel Debug para soporte de QA sin afectar al jugador.

Próxima acción:

- Revisar y limpiar rutas legacy en `Habilidades/*` que imprimen a consola directamente (sustituir por acciones/resolver) y añadir asserts de texto en pruebas relevantes.

[5.14] Parcial | UX Combate | Texto de combate didáctico/expresivo |

Estado:

- Parcial (se añadió un formateador básico en `DamageResolver` que agrega una línea explicativa adicional para físico y mágico, sin alterar el cálculo ni las primeras líneas).

Descripción:

- Mejorar el feedback con mensajes más ricos y explicativos. Ejemplo objetivo:
  - "Jugador hace 12 de daño; Enemigo mitiga 20% por defensa y 10% por armadura. Crítico x1.5 aplicado. Daño final: 12".
  - "Hechizo impacta por 40; Resistencia magia 30% reduce a 28; Vulnerabilidad +20% eleva a 33.6 → 34. Daño final: 34".
- Debe integrarse con `DamageResolver` y `ResultadoAccion.Mensajes` para evitar duplicados.
- Respetar `UIStyle` (subtítulos, bullets) y un nivel de verbosidad configurable (compacto versus detallado) desde opciones o flag.

Decisiones/Conclusiones:

- Los mensajes explican el orden real del pipeline: Defensa → Penetración → Mitigación → Resistencias/Vulnerabilidades → Crítico → Redondeos. Evitar términos ambiguos.
- No cambiar el balance; solo la narrativa del cálculo.

Próxima acción:

- Añadir opción de UI/menú para activar/desactivar verbosidad en runtime respetando `UIStyle`.
- Añadir 2–3 asserts de texto en pruebas (9.8) para fijar expectativas y evitar regresiones.

## 6. MISIONES Y REQUISITOS

[6.1] Pendiente | Dominio | Reemplazar strings requisitos por IRequisito | Base
[6.2] Pendiente | Dominio | Reemplazar recompensas por IRecompensa | Tras 6.1
[6.3] Pendiente | Flujo | Cadena de misiones con grafo (prerequisitos) | Tras 6.1

## 7. REPOSITORIOS / DATA

[7.1] Pendiente | Infra | `IRepository<T>` genérico JSON | Base
[7.2] Pendiente | Infra | Repos específico Misiones / Enemigos / Objetos | Tras 7.1
[7.3] Pendiente | Cache | Carga diferida + invalidación | Tras 7.2
[7.4] Pendiente | Esquema objetos | Incluir en JSON de armas/armaduras campos `Precision`, `CritChance`, `CritMult`, `Penetracion`, `DurabilidadBase` | Compatibilidad retro con campos faltantes; validación en 10.6

### 7.x Organización de Enemigos por Bioma/Nivel/Categoría (nuevo)

- Hecho | Bosque nivel_1_3 | Estructura por categorías creada: `normal/`, `elite/`, `jefe/`, `campo/`, `legendario/`, `unico/`, `mundial/`. Limpieza lógica del root: el loader ahora ignora JSONs ubicados directamente en la raíz de `nivel_*` bajo `enemigos/por_bioma` para evitar doble carga (mientras se completa la eliminación física).
- Hecho | Bosque nivel_1_3 | Cuotas por categoría alcanzadas: 10 normales, 10 élite, 5 jefes, 3 de campo, 2 legendarios, 2 únicos, 1 mundial.
- Hecho | Documentación | `DatosJuego/enemigos/README.md` ampliado con estructura sugerida y plantilla JSON.
- Hecho | Validación | `DataValidatorService` reconstruido; ahora valida enemigos respetando la convención de ignorar JSON en la raíz de `nivel_*` y aplica rangos estrictos para `Mitigacion*` [0..0.9], `ResistenciasElementales` [0..0.9], `SpawnChance` [0..1] y `SpawnWeight>0`. Se agregó reporte opcional `--validar-datos=report`.
- Pendiente | Replicar | Extender la misma organización a otros biomas (`montana/`, `pantano/`, etc.) y rangos de nivel (`nivel_4_6/`, ...). Checklist:

  - Crear subcarpetas por categoría en cada `nivel_X_Y`.
  - Migrar JSONs existentes a su carpeta por `Categoria`.
  - Completar cuotas mínimas por categoría (mismos números que bosque) o marcar excepciones justificadas.
  - Ejecutar `DataValidatorService.ValidarEnemigosBasico()` y revisar advertencias.
  - Actualizar `Roadmap.md` con progreso y `enemigos/README.md` si surgen nuevas convenciones.

Convenciones adicionales:

- Nombres variantes: cuando un mismo arquetipo exista en categorías distintas, el `Nombre` debe incluir sufijo entre paréntesis, por ejemplo: `Cuervo de Tres Ojos (Élite)`, `Lobo Alfa (Jefe)`. Esto evita colisiones en validadores y facilita filtros por texto. Los `Tags` deben incluir `variante:elite|jefe|...`.
- Estructura de carpetas: todos los enemigos bajo `por_bioma/<bioma>/<nivel_*>` deben residir en una subcarpeta de categoría. Los JSON en la raíz de `nivel_*` serán ignorados por el loader hasta su eliminación definitiva.

Política vigente (elemental):

- NO se permiten valores negativos en `ResistenciasElementales` (mitigación) — rango válido [0..0.9]. Valores negativos históricos fueron normalizados a `0.0`.
- Las debilidades se modelan explícitamente con `VulnerabilidadesElementales { tipo: factor }`, aplicadas como multiplicador post-mitigación. Rango permitido y validado: `[1.0 .. 1.5]` (conservador para progresión lenta).
- Soporte inicial implementado para el canal genérico `"magia"`. En futuras iteraciones se ampliará a elementos específicos (fuego/hielo/rayo/veneno/...) cuando el pipeline identifique el elemento del golpe.

Bitácora movida: Las entradas cronológicas de esta sección fueron movidas a `Docs/Bitacora.md`.

## 8. UI / PRESENTACIÓN

[8.1] Hecho | Abstracción | IUserInterface (WriteLine, ReadOption, Confirm) | Interfaz creada + adaptadores: ConsoleUserInterface y SilentUserInterface (para tests); InputService usa la UI para leer opciones/números y pausar. Añadido InputService.TestMode para evitar bloqueos en tests. Juego expone UiFactory para inyección. Logger central agregado y enlazado a la UI. Migradas salidas principales en Juego (menú, viaje, recolección, mazmorra, rutas) y GeneradorEnemigos. Menús migrados: MenuCiudad, MenuFueraCiudad, MenuRecoleccion, MenuFijo, MenuAdmin, MenuEntreCombate, MenusJuego y Program.cs. Pendiente: unificar colores/estilo.
[8.2] Pendiente | Menús | Refactor menús a comandos (Command Pattern) | Tras 8.1
[8.3] Parcial | Estilo | Colores y layout unificados | Etiquetas de reputación colorizadas en ciudad/tienda/NPC/misiones; Recolección (híbrida), Energía, Estado del Personaje y Misiones ya usan la UI unificada. Añadido utilitario `UIStyle` (encabezados y subtítulos) y aplicado en `MenusJuego.MostrarMenuPrincipalFijo`, `Inventario`, `MenuCiudad`, `MenuFueraCiudad`, `MenuFijo`, `MenuRecoleccion` y menú inicial (`Program.cs`). Avance: `CombatePorTurnos` migrado a `IUserInterface` y `UIStyle` (encabezados/subtítulos, hints y estado), con submenús para Habilidades y uso de Pociones. Avance 2: `Inventario` y `MotorInventario` migrados a `IUserInterface` + `UIStyle` (listado numerado, encabezados, pausas por UI). Avance 3: Recompensas de enemigos (drops/oro/exp) muestran feedback por UI y añaden drops al inventario del jugador. Avance 4: El estado de combate ahora muestra Maná del jugador y Efectos activos por combatiente con turnos restantes. NUEVO: `EstadoPersonajePrinter` fue rediseñado con un layout profesional (resumen superior, barras de Vida/Maná/Energía y XP, atributos compactos con bonos, sección de supervivencia con etiquetas) usando `UIStyle`. NUEVO-2: se añadió modo "detallado" opcional (toggle) que incluye sección "Equipo" (slots: Arma, Casco, Armadura, Pantalón, Zapatos, Collar, Cinturón, Accesorio 1/2) con nombre del ítem y stats clave (Rareza/Perfección y, en armas, Daño Físico/Mágico). Acceso rápido desde el `Menú Fijo`: opción separada "Estado (detallado)" junto a la vista compacta.
NUEVO-3: Menú Principal incluye "Opciones" con toggles runtime para Logger, Precisión (hit-check), Penetración y Verbosidad de Combate.

### 8.x Correcciones de gating de menús por sector (nuevo)

- Hecho | Menú de ciudad solo en centro: `Juego.MostrarMenuPorUbicacion` ahora muestra el menú de ciudad únicamente cuando `SectorData.Tipo == "Ciudad"` y además `EsCentroCiudad == true` o `CiudadPrincipal == true`. Cualquier otra parte de ciudad (`ParteCiudad`) utiliza el menú de “Fuera de Ciudad”. Evita mostrar el menú de ciudad al estar en entradas/periferias.
- Hecho | Default seguro de tipo: `PjDatos/SectorData.cs` cambia el valor por defecto de `Tipo` a `"Ruta"` (antes `"Ciudad"`). Así, si el JSON omite el campo, no se clasifica erróneamente como ciudad.
- Pendiente | Prueba de integración: añadir un test que cargue Bairan (`8_23.json`) y verifique que al moverse a un sector marcado como parte de ciudad sin `EsCentroCiudad`, el menú que aparece es el de “Fuera de Ciudad”.
[8.4] Pendiente | Theming | Servicio de estilo (UIStyleService) con paleta y helpers (títulos, listas, etiquetas) | Facilita unificación visual y futura migración a UI de Unity. Base ligera `UIStyle` creada; falta paleta configurable y aplicación global.

## 9. TESTING

[9.1] Hecho | Infra | Crear proyecto tests xUnit | Proyecto MiJuegoRPG.Tests creado (xUnit) y referenciado en la solución. Config actualizado: copia recursiva de `MiJuegoRPG/DatosJuego/**` al output de pruebas para alinear con estructura por bioma/nivel/categoría de enemigos y otros datos.
[9.2] Hecho | Test | Mapa.MoverseA casos | Tres casos cubiertos: inicialización (CiudadPrincipal), adyacencias y movimiento válido/ inválido + descubrimiento
[9.3] Hecho | Test | GeneradorEnemigos nivel y drops | Tests deterministas con RandomService.SetSeed y filtro por nivel; E/S aislada a %TEMP% y opción DesactivarPersistenciaDrops para evitar escribir JSONs reales.
[9.4] Hecho | Test | ProgressionService fórmula | Explorar (Percepción+Agilidad), Entrenamiento con subida y Recolección por tipo
[9.5] Hecho | Test | Recolección energía y requisitos | Cooldown por nodo: aplicar y limpiar al entrar sector (persistencia multisector)
[9.6] Hecho | Test | EncuentrosService: MinKills y ventanas horarias (incluye cruce de medianoche) | Pruebas unitarias que ejercitan gating por kills y por HoraMin/HoraMax con control de hora en `Juego` y `RandomService.SetSeed` para determinismo
[9.7] Hecho | Test | EncuentrosService: Chance/Prioridad y Cooldown | Pruebas unitarias verifican activación por `Chance` (1.0 y 0.0), desempate por `Prioridad` y bloqueo temporal por `CooldownMinutos` con proveedor de fecha/hora inyectado, incluyendo expiración de cooldown.
[9.8] Parcial | Test | Pipeline de combate |

Estado:

- Parcial (suite en verde; cobertura base de Hit/Miss/Crítico, orden de pasos y penetración con toggle).

Descripción:

- Cobertura: Hit/Miss/Crítico con `RandomService.SetSeed` y dummies deterministas; verificación del orden en daño mágico (Defensa→Mitigación→Resistencia→Vulnerabilidad) y físico (Defensa→Mitigación).
- Pruebas de penetración (físico y mágico) aplicando reducción de defensa antes de mitigaciones/resistencias y gating por `--penetracion`.
- NUEVO: casos de interacción Crítico + Penetración (físico y mágico) validando `DanioReal` y `FueCritico`. Caps centralizados cubiertos con `EstadisticasCapsTests`.

Decisiones/Conclusiones:

- Los tests fijan el orden exacto del pipeline y valores esperados, evitando regresiones de balance.

Próxima acción:

- Formalizar `IDamageStep` y ensamblado de pasos en `DamageResolver`.
- Unificar mensajería al 100% vía `ResultadoAccion` (5.13) y ampliar asserts de texto en pruebas.
- Integrar pruebas unitarias para `Supervivencia.FactorPrecision` afectando $p_{hit}$ (ya integrado en código) bajo `--precision-hit`.
- Añadir pruebas que lean `StatsCaps` custom desde `progression.json` para validar clamps data-driven.
 - Extender asserts de verbosidad para contemplar nota de Penetración cuando `--penetracion` está ON.
[9.9] Pendiente | Test | Estados avanzados | Aplicación/decadencia/stacking de Sangrado/Aturdimiento/Buffs y resistencias.
[9.10] Pendiente | Test | Supervivencia | Tick de hambre/sed/fatiga/temperatura; penalizaciones por umbral y multiplicadores por contexto/bioma.

## 10. LIMPIEZA / QUALITY

[10.1] Hecho | Rutas | Centralizar rutas en PathProvider | Servicio PathProvider agregado; refactors en Juego, ProgressionService, EnergiaService, ReputacionService, ReputacionPoliticas, ShopService, MenusJuego, MotorMisiones, GestorArmas, GestorPociones, GestorMateriales, GuardadoService, CreadorPersonaje, TestGeneradorObjetos
[10.2] Hecho | Random | Sustituir usos dispersos | RecoleccionService y BiomaRecoleccion usan RandomService; agregado SetSeed(int) para tests deterministas
[10.3] Pendiente | Nombres | Uniformar nombres archivos (GeneradorObjetos vs GeneradorDeObjetos) | Revisión
[10.4] Pendiente | Comentarios | Podar comentarios redundantes | Continuo
[10.5] Hecho | Documentación | README arquitectura modular | `Docs/Arquitectura_y_Funcionamiento.md` ampliado con fórmulas exactas (stats), detalles de Encuentros/Energía/Supervivencia y clases dinámicas. README corto en raíz creado (`/README.md`) con enlaces a Docs, Roadmap, Bitácora, Arquitectura, Progresión, Guía de Ejemplos y Flujo.
[10.6] Parcial | Validación Data | Validador JSON referencial (IDs de mapa, facciones, misiones, objetos) + pruebas | Base creada: `DataValidatorService.ValidarReferenciasBasicas()` verifica IDs de sectores en `facciones_ubicacion.json` contra el mapa cargado; flag CLI `--validar-datos` ejecuta el validador al inicio y reporta errores/advertencias sin detener el juego. Ampliado: valida `misiones.json` (IDs, `SiguienteMisionId`, dependencias `Condiciones: "Completar X"`, `UbicacionNPC` si es ID de sector) y `npc.json` (IDs de sector en `Ubicacion` y existencia de `Misiones`). Implementado reporte opcional: `--validar-datos=report` (o `--validar-datos=<ruta>`) guarda salida en `PjDatos/validacion/`. Nuevo: `ValidarEnemigosBasico()` recorre `DatosJuego/enemigos` y comprueba duplicados por `Nombre/Id`, rangos de mitigación [0..0.9], y campos básicos; informa NoMuertos sin `veneno:true` explícito (se aplica por defecto en runtime). Siguiente: cubrir repos de objetos y esquemas por archivo.
[10.9] Hecho | Validación Data | Detectar materiales vacíos/invalidos en `nodosRecoleccion` de sectores | `DataValidatorService` amplía validación recorriendo sectores y reportando `{}` o materiales con `Nombre` vacío o `Cantidad <= 0`. Ayuda a higiene de datos y evita comportamientos raros en recolección.
[10.7] Parcial | Higiene Git | Decidir si versionar juego.db; si no, añadir a .gitignore y documentar | Se excluyeron mapas JPG pesados (Mapa*.jpg) mediante .gitignore para evitar límites de GitHub (>100MB). Pendiente decidir el estatus de DatosCompartidos/juego.db (ignorar o versionar con migraciones) y documentarlo.
[10.8] Hecho | Null-safety | Endurecer accesos a `mapa.UbicacionActual` | Añadidos null-checks en `Juego.MostrarTienda`, `ExplorarSector` (rama Materiales) y `MostrarMenuRutas` (logs de depuración). Limpieza menor en `GuardarPersonaje` para evitar ifs duplicados. Reduce warnings CS8602 intermitentes en IDE.
[10.10] Hecho | Reparación Data | Reparador automático de `nodosRecoleccion` con materiales inválidos | Nueva herramienta `Herramientas/ReparadorMateriales.cs` recorre `DatosJuego/mapa/SectoresMapa` y elimina materiales nulos, con `Nombre` vacío o `Cantidad <= 0`; normaliza listas null a `[]`. Flags CLI añadidos en `Program.cs`: `--reparar-materiales=report[;ruta]` (dry-run, no modifica archivos, genera reporte) y `--reparar-materiales=write[;ruta]` (aplica cambios + reporte). Reportes se guardan por defecto en `PjDatos/validacion/materiales_reparacion_*.txt`. Integrado con `PathProvider` y contrato real `PjDatos.SectorData`/`Motor.NodoRecoleccion`/`MaterialCantidad`. Tests existentes (36/36) se mantienen verdes.
[10.11] Pendiente | Documentación | Especificación pipeline de combate | Diagrama y pseudocódigo con orden de pasos; contratos `IDamageStep` y `ResultadoAccion`; criterios de éxito/edge cases.
[10.12] Pendiente | Documentación | Manual de tuning de supervivencia | Guía para `DatosJuego/config/supervivencia.json` (tasas, umbrales, climas, consumos) y cómo validar con pruebas.

## 11. CLASES DINÁMICAS / PROGRESIÓN AVANZADA

[11.1] Hecho | Atributo Extra | Agregar 'Oscuridad' a AtributosBase | Disponibles requisitos y clases oscuras futuras
[11.2] Hecho | Evaluación Requisitos | ClaseDinamicaService: nivel, clasesPrevias, clasesAlguna, exclusiones, atributos, estadísticas, actividad, reputación, misiones múltiple/única, objeto único | Lógica centralizada CumpleHardRequirements
[11.3] Hecho | Bonos Iniciales | Aplicar AtributosGanados al desbloquear clase (incluye Oscuridad) | Método AplicarBonosAtributoInicial
[11.4] Hecho | Desbloqueo Emergente | Score parcial (PesoEmergenteMin) | Dataset aún no lo usa (seguir monitoreo)
[11.5] Hecho | Reputación Facción | Campo ReputacionFaccionMin en ClaseData + check | Evaluado en ClaseDinamicaService
[11.6] Pendiente | Bonificadores Globales | Servicio unificador (XP.*, Drop.*, Energia.*) | Diseñar BonosGlobalesService
[11.7] Hecho | Clamp Atributos | Evitar negativos / límites soft-hard | Aplicado en bonos de clase y menú admin

## 12. REPUTACIÓN

[12.1] Hecho | Persistencia | Reputacion global y por facción en Personaje | Campos Reputacion / ReputacionesFaccion
[12.2] Hecho | Servicio | ReputacionService (modificar global/facción + reevaluar clases) | Integrado en Juego
[12.3] Hecho | Umbrales | reputacion_umbrales.json + eventos y avisos | ReputacionService publica EventoReputacionUmbral*
[12.4] Hecho | Alineación Negativa | Feedback visual y gating por reputación negativa | Etiquetas compactas colorizadas + gating en NPC y tienda alineado a bandas; políticas en JSON
[12.5] Pendiente | Métricas | Tracking de cambios reputación para balance | Requiere logger/telemetría ligera
[12.6] Hecho | Tienda ↔ Reputación | Ganancia por compra (+1/100 oro) y venta (+1/200 oro); descuentos por rep global/facción | Lógica centralizada en ShopService (GetPrecioCompra/Venta, PuedeAtender); MenusJuego solo UI; facciones_ubicacion.json data-driven (fallback activo); unificación a IDs de mapa en curso

## 14. MIGRACIÓN / INTEGRACIÓN UNITY

[14.1] Pendiente | Abstracciones | Separar estrictamente dominio (lógica) de presentación (UI) | Facilitar portar a Unity sin reescribir núcleo
[14.2] Pendiente | Carga Datos | Diseñar conversor JSON → ScriptableObjects (plan de tool) | Pipeline de datos para Unity
[14.3] Pendiente | Servicios | Adaptadores de IUserInterface/Logger a Unity UI/Console | Reuso de menús y mensajes
[14.4] Pendiente | Tiempo/Juego | Servicio de tiempo (tick/update) desacoplado de Console loop | Integración con game loop de Unity
[14.5] Pendiente | Input | Adaptar InputService a sistemas de input (teclado/control) | Capa de entrada unificada

## 13. ADMIN / HERRAMIENTAS QA

[13.1] Hecho | Menú Admin | Separado del menú principal (opción 5) | Aísla flujos de jugador
[13.2] Hecho | Ajustes Directos | TP, reputación global/facción, verbose reputación, nivel +/- | MenuAdmin opciones 1–6
[13.3] Hecho | Atributos | Modificar atributo individual con recálculo y reevaluación clases | Opción 7
[13.4] Hecho | Diagnóstico | Listar clases (motivos bloqueo), atributos+stats, habilidades, inventario, resumen integral | Opciones 8–12
[13.5] Hecho | Forzar Clase | Desbloqueo manual (override) con aplicación de bonos y reevaluación | Opción 13 en MenuAdmin
[13.6] Hecho | Export Snapshot | Guardar resumen integral a archivo (logs/admin) | Opción 14 en MenuAdmin
[13.7] Hecho | Batch Atributos | Parser múltiple (fuerza+5,int+3) | Opción 7 soporta entrada batch
[13.8] Pendiente | Seguridad | Flag para ocultar menú admin en build release | Config build / preprocesador
[13.9] Hecho | QA Mapas | Generador de conexiones cardinales (N/E/S/O) desde `mapa.txt` | Herramienta `Herramientas/GeneradorConexiones.cs` agrega adyacencias bidireccionales a `Conexiones` sin sobrescribir otras. Mantiene datos y evita duplicados. Añadido `NormalizarBidireccionalidad` y flag `--normalizar-conexiones`.
[13.10] Hecho | QA Mapas | Validador de mapa con BFS de conectividad | `Herramientas/ValidadorSectores.cs` ahora usa `PjDatos.SectorData`, valida IDs, bidireccionalidad y reporta sectores inalcanzables desde `ciudadPrincipal` (fallback primer sector). Útil para garantizar mundo navegable.
[13.11] Hecho | Tiempo del mundo | Control de hora/minutos desde menú admin | Añadido en MenuAdmin opción 15: `+/-N` para ajustar minutos o `h=HH` para fijar hora del día. Nuevos métodos en `Juego`: `AjustarMinutosMundo(int)` y `EstablecerHoraDelDia(int)`. Facilita QA de encuentros con gating por `HoraMin/HoraMax` en `EncuentrosService`.
[13.11] Hecho | Mundo | Generador de biomas por bandas | `Herramientas/GeneradorBiomas.cs` asigna `bioma` según distancia a bordes (Oceano Lejano/Oceano/Interior) y fuerza `tipo:"Ruta"` en no-ciudades. Flag `--asignar-biomas[=ol,oc]`.
[13.12] Hecho | Mundo | Hidratador de nodos de recolección | `Herramientas/HidratadorNodos.cs` crea `nodosRecoleccion` en sectores sin definir a partir de `DatosJuego/biomas.json`, preservando existentes, con límite `max`. Flag `--hidratar-nodos[=max]`.
[13.10] Hecho | QA Mapas | Validador de mapa con BFS de conectividad | `Herramientas/ValidadorSectores.cs` ahora usa `PjDatos.SectorData`, valida IDs, bidireccionalidad y reporta sectores inalcanzables desde `ciudadPrincipal` (fallback primer sector). Útil para garantizar mundo navegable.

## 15. OBJETOS / CRAFTEO / DROPS

[15.1] Pendiente | Data | Esquema común de objetos/materiales (JSON) + repositorios | Consolidar GestorArmas/Materiales/Pociones bajo repos JSON; IDs únicos, Rareza, NivelRequerido, BonosAtributo/Stats, DurabilidadBase (opcional). Integrar con PathProvider y validar con 10.6
[15.2] Hecho | Drops Enemigos | Tablas de botín por enemigo (base) + modificadores por sector/bioma/dificultad | `EnemigoData.Drops` soporta `Tipo/Nombre/Rareza(texto)/Chance/CantidadMin/Max/UniqueOnce`. Runtime: `GeneradorEnemigos` mapea probabilidades + metadatos de cantidad y UniqueOnce; `Enemigo.DarRecompensas` aplica sorteo con clamps anti-farming (máx 3 por kill, 5 para rarezas bajas) y respeta `UniqueOnce` persistiendo claves en `PjDatos/drops_unicos.json` vía `DropsService` integrado en `GuardadoService`. Tests usan `GeneradorEnemigos.DesactivarPersistenciaDrops` para evitar escritura real.
[15.3] Pendiente | Drops Mapa | Tablas de botín por sector (cofres/eventos ambientales) | Archivo loot/sectores.json; gating por reputación/llaves/misiones; sincronizar con IDs de sector
[15.4] Pendiente | Crafteo | Sistema de recetas (recetas.json) + blueprints desbloqueables | Requisitos por atributos/habilidad/misiones; coste de energía/tiempo; chance de fallo; calidad resultante; estaciones de trabajo por ciudad/ubicación
[15.5] Pendiente | Desmontaje | Desmontar objetos para recuperar materiales | Rendimiento según skill y estado del objeto; pérdida parcial en fallos; economía anti-exploit
[15.6] Pendiente | Durabilidad/Repair | Degradación y reparación con materiales | Integrar con EnergiaService; estaciones de reparación; opcional pero recomendado para progresión lenta
[15.6.1] Pendiente | Servicio de reparación | `RepairService` con costes por calidad/nivel/materiales/herramientas | UI en tienda/taller; hooks para durabilidad en combate y recolección.
[15.7] Pendiente | Balance | Rareza, caps y cooldowns | Límites por nodo/sector, cooldown por crafteo avanzado, protección contra rachas RNG (bad luck protection)
[15.8] Pendiente | Economía | Integración con ShopService | Precios dinámicos de materiales/crafteados según reputación y facción; stock rotativo por ciudad
[15.9] Pendiente | Testing | Determinismo y contratos | Tests de drop tables y crafteo con RandomService.SetSeed; validación de contratos JSON (10.6)
[15.10] Pendiente | Telemetría | Métricas de crafting/drops | Tasas de éxito, consumo de materiales, progresión de skill de artesanía para balance futuro

## 16. ESTADO POR ARCHIVO / MÓDULO (inventario actual)  

Nota: Este punto es un inventario de estado por carpeta/archivo, pensado como apéndice operativo. Por eso su formato es distinto al del resto de secciones numeradas (1–15, 17–27), que siguen el esquema por ítems [ID] con estado.

Agrupado por carpeta. Hecho = estable/usable; Parcial = base hecha pero faltan migraciones UI/tests/balance; Pendiente = por implementar/migrar.

- Interfaces (Hecho):

  - Interfaces/IUserInterface.cs, IUsable.cs, IInventariable.cs, ICombatiente.cs, IBonificadorAtributo.cs

- Servicios (mayoría Hecho):

  - Hecho: Motor/Servicios/{EventBus, RandomService, ProgressionService, PathProvider, Logger, ConsoleUserInterface, SilentUserInterface, ReputacionService, ReputacionPoliticas, ClaseDinamicaService}
  - Hecho: Motor/Servicios/RecoleccionService.cs, EnergiaService.cs
  - Parcial: Motor/Servicios/GuardadoService.cs (flujos interactivos UI por migrar)

- Motor core:

  - Hecho: Motor/{Juego, Mapa, MapaLoader, Ubicacion, MotorRutas}
  - Parcial: Motor/CreadorPersonaje.cs (UI ya adaptada en parte)
  - Parcial: Motor/AvisosAventura.cs, GestorDesbloqueos.cs (conectar a UI/Logger)

- Menús (Hecho salvo Combate/Inventario):

  - Hecho: Motor/Menus/{MenuCiudad, MenuFueraCiudad, MenuRecoleccion, MenuFijo, MenuAdmin}, MenusJuego.cs, MenuEntreCombate.cs
  - Pendiente: Integración de estilo unificado en todos (8.3)

- Combate (Parcial):

  - Parcial: Motor/CombatePorTurnos.cs (UI unificada + menús de Habilidad y uso de Pociones; selección de objetivo; mensajes centralizados)
  - Pendiente: Motor/MotorCombate.cs (sin cambios funcionales)
  - Pendiente/Parcial: Habilidades/{AtaqueFisico, AtaqueMagico, Hechizo, Habilidad, GestorHabilidades, HabilidadLoader} (faltan arquitectura de acciones 5.1, estados/orden por Velocidad y UI)

- Inventario y Personaje:

  - Dominio Hecho: Personaje/{Personaje, AtributosBase, ExpAtributo, Estadisticas, Clase, ClaseData, HabilidadProgreso, FuenteBonificador}
  - UI/Flujos Hecho: Motor/MotorInventario.cs, Personaje/Inventario.cs migrados a `IUserInterface` + `UIStyle`; confirmación al usar pociones; mensajes consistentes

- Enemigos (Hecho base):

  - Enemigos/{Enemigo, EnemigoEstandar, Goblin, GranGoblin} + PjDatos/EnemigoData.cs; GeneradorEnemigos.cs (tests verdes)

- Objetos y materiales:

  - Modelos Hecho: Objetos/{Objeto, ObjetoJsonConverter, EnumsObjetos, Material, Arma, Armadura, Casco, Botas, Cinturon, Collar, Pantalon, Accesorio, Pocion}
  - Gestores Parcial: Objetos/{GestorArmas, GestorMateriales, GestorPociones} (migrar logs a Logger y UI para feedback)
  - Generador De Objetos Parcial: Motor/GeneradorDeObjetos.cs + Motor/TestGeneradorObjetos.cs

- Datos Pj (mappers/modelos de data) Hecho:

  - PjDatos/{AccesorioData, ArmaData, ArmaduraData, BotasData, CinturonData, CollarData, PantalonData, Categoria, Familia, SectorData, Rareza, ClasesData, ClasesData.cs, personajeData.cs, GuardaPersonaje.cs, PersonajeSqliteService.cs}

- Comercio (Hecho):

  - Comercio/{ShopService, PriceService} con reputación integrada y PathProvider

- Crafteo (Pendiente):

  - Crafteo/CraftingService.cs (esqueleto; dependerá de 15.x)

- Herramientas / Datos (Parcial):

  - Herramientas/{ValidadorSectores, ReparadorSectores} (útiles; integrar en QA/CI)
  - DatosJuego/mapa/GeneradorSectores.cs (tool de generación; añadir tests/validación)

- Program/Entrypoint (Hecho):

  - Program.cs migrado a UI

## 17. HABILIDADES Y MAESTRÍAS

[17.1] Pendiente | Progresión por uso | Subir skill por tipo de arma/armadura/habilidad; bonifica precisión/daño/defensa | Integrar con ProgressionService y RandomService
[17.2] Pendiente | Árboles por arquetipo | Guerrero/Explorador/Mago con sinergias por atributos | Data JSON y evaluador de requisitos
[17.3] Parcial | Costes y recursos | Mana/Concentración y cooldowns; recuperación y pociones | Cooldowns base implementados: `IAccionCombate.CooldownTurnos`, gestión por combatiente a través de `ActionRulesService` (chequeo, aplicación y avance por turno). Coste de maná aplicado en acciones que lo requieren (p. ej., Veneno, Ataque Mágico). Verificación y consumo de recursos centralizados: `ActionRulesService.TieneRecursos` y `ConsumirRecursos` integrados en `CombatePorTurnos` antes de ejecutar acciones (tests añadidos). Regeneración de maná en combate implementada con acumulación fraccional según `progression.json` (`ManaRegenCombateBase`, `ManaRegenCombateFactor`, `ManaRegenCombateMaxPorTurno`) y feedback en UI. UX: el submenú Habilidad muestra coste/CD y sufijos dinámicos ("CD n" / "Sin maná"), marca no disponibles con `[X]` y no consume turno si se intenta una acción inválida/no disponible. Recuperación de maná fuera de combate integrada en descanso de ciudad (`MenuCiudad`) basada en `progression.json` (`ManaRegenFueraBase`, `ManaRegenFueraFactor`, `ManaRegenFueraMaxPorTick`) y documentada en `progression_config.md`. Uso de pociones en combate unificado bajo `TryEjecutarAccion` con patrón de gating. Pendiente: extender a recursos adicionales (p. ej., Concentración) y considerar cooldown por categoría de consumible.
[17.4] Pendiente | Gating | Requisitos por nivel de skill/atributo/clase/reputación | Validación en uso de skills

## 18. ITEMIZACIÓN AVANZADA

[18.1] Pendiente | Afijos | Prefijos/Sufijos con rangos y rareza | Generador ponderado; validación de compatibilidades
[18.2] Pendiente | Únicos/Sets | Objetos con propiedades fijas y bonos por set | Data-driven; bonus 2/3/4 piezas
[18.3] Pendiente | Sockets/Gemas | Inserción/extracción con coste y riesgo | Interacción con crafteo y durabilidad
[18.4] Pendiente | Calidad | Calidad del ítem que escala stats y precio | Afecta reparación y drop rate
[18.5] Pendiente | Forja/Mejora | Mejora con probabilidad de fallo/retroceso | Integración con CraftingService

## 19. ECONOMÍA Y SINKS

[19.1] Pendiente | Costes de viaje | Oro/energía por rutas largas/peligrosas | Balance con reputación/facción
[19.2] Pendiente | Entrenamiento avanzado | Tarifas en entrenadores por rango | Requiere reputación/licencias
[19.3] Pendiente | Reparación y mantenimiento | Tasas crecientes según nivel/calidad | Vinculado a 15.6
[19.4] Pendiente | Impuestos/Peajes | Zonas con peaje o tasa de comercio | Afecta ShopService
[19.5] Pendiente | Licencias/Gremios | Acceso a crafteo avanzado/áreas | Gating de sistemas
[19.6] Pendiente | Stock rotativo/eventos | Escasez/bonanza por ciudad/facción | Data eventos económicos

## 20. MUNDO DINÁMICO Y EXPLORACIÓN

[20.1] Hecho | Encuentros | Tablas por bioma con pesos (Nada/Botín/Materiales/NPC/Combate comunes/bioma/MiniJefe/MazmorraRara) | `EncuentrosService` integrado con tablas por defecto (Bosque/Montaña) y hook en `Juego.ExplorarSector`. Añadidos contadores de muertes persistentes en `Personaje` (global y por bioma), etiquetado `Enemigo.Tag` y registro automático en recompensas; generación de enemigos ahora acepta filtro por tipo (`rata|lobo`), aplicando `res.Param`. Minijefe condicionado a `MinKills` ya usa los contadores reales. Hecho: tablas externalizadas a `DatosJuego/eventos/encuentros.json` y carga automática en runtime; `Juego.ExplorarSector` usa el resolver con modificadores por atributos (Suerte/Agilidad/Percepción/Destreza) y bonus por kills extra; añadido gating por franja horaria (`HoraMin/HoraMax`) con manejo de ventanas que cruzan medianoche. Nuevo: probabilidades independientes por evento (`Chance`) con priorización opcional (`Prioridad`), y soporte de `CooldownMinutos` para evitar repetición inmediata de eventos raros; por ejemplo, un MiniJefe con `MinKills=13`, `Chance=0.1` y `CooldownMinutos=60`. Persistencia: cooldowns de encuentros se guardan/cargan vía `GuardadoService` en `PjDatos/cooldowns_encuentros.json`. Pendiente: ampliar biomas/sectores y añadir condiciones por clima y reputación. Añadidas pruebas de contratos de encuentros (9.6, 9.7).
[-] Hecho | Balance de no-muertos | Zombis más duros | Zombis (y no-muertos afines) ahora son inmunes al veneno, mitigan 25% del daño físico tras defensa y ganan +15% HP por defecto. Se añadió un leve escalado contextual si el jugador es novato (≤2) y sin equipo, reforzando la progresión lenta y la necesidad de planificación/objetos.
[-] Fix | Exploración | NRE al explorar en sectores con `Region` nula/vacía | Endurecidas comprobaciones en `Juego.ExplorarSector` (rama Materiales) y en `TablaBiomas.GenerarNodosParaBioma` para no invocar generación sin bioma válido. Ahora, si no hay nodos personalizados y el bioma es vacío, se muestra "No hay nodos de recolección disponibles." en lugar de fallar.
[20.2] Pendiente | Trampas/llaves | Cerraduras, llaves y trampas con detección | Usa Percepción/Agilidad
[20.3] Pendiente | Eventos ambientales | Cofres, santuarios, anomalías con cooldown | Ver 15.3 loot por sector
[20.4] Pendiente | Clima/Condiciones | Modificadores de encuentros por clima y hora (golpe de calor/frío) | Vincular con `supervivencia.json` (27.x) y bioma.

## 21. MISIONES CON CONSECUENCIAS

[21.1] Pendiente | Grafo con ramas | Rutas exclusivas por facción/decisiones | Impacta reputación y tiendas
[21.2] Pendiente | Recompensas significativas | Blueprints/llaves/accesos en lugar de solo oro/XP | Data y gating
[21.3] Pendiente | Persistencia de impacto | Cambios en NPC/stock/hostilidad | Servicio de mundo persistente

## 22. LOGROS Y RETOS

[22.1] Pendiente | Sistema de logros | Tracking de hitos y retos | Export/telemetría opcional
[22.2] Pendiente | Retos (ironman) | Muerte permanente/no usar X/tiempo límite | Flags de partida
[22.3] Pendiente | Recompensas leves | Cosméticos/QoL leve para no romper balance | Evitar pay-to-win

## 23. GUARDADO VERSIONADO Y MIGRACIONES

[23.1] Pendiente | Versionado | Save y datasets con versión | Comparador al cargar
[23.2] Pendiente | Migradores | Pasos entre versiones | Backups automáticos
[23.3] Pendiente | Compresión/Rotación | Archivos compactos y rotación de backups | Configurable

## 24. LOCALIZACIÓN (i18n)

[24.1] Pendiente | Desacoplar textos | Recursos por clave | Sustituir literales gradualmente
[24.2] Pendiente | Idiomas | Plantillas ES/EN | Selección de idioma en opciones
[24.3] Pendiente | Longitudes UI | Revisar cortes/colores por idioma | Con UIStyleService

## 25. PERFORMANCE Y CACHING

[25.1] Pendiente | Índices repos | Búsquedas por ID y nombre | Repositorios 7.x
[25.2] Pendiente | Lazy/Caché | Carga diferida e invalidación | Calentamiento en menús
[25.3] Pendiente | Reducir E/S | Minimizar lecturas de JSON | Batch y snapshots
[25.4] Pendiente | Pooling | Reutilizar entidades temporales (enemigos/efectos) | Combate y generación

## 26. ACCESIBILIDAD Y QoL

[26.1] Pendiente | Paleta accesible | Colorblind-safe en UIStyleService | Pruebas visuales
[26.2] Hecho | Verbosidad | Niveles de detalle en UI/Logger | Agregado `MenuOpciones` (Menú Principal → Opciones) para alternar Logger ON/OFF y cambiar `LogLevel` (Error/Warn/Info/Debug) en runtime. Preferencias por partida persistidas en `Personaje` (`PreferenciaLoggerEnabled`, `PreferenciaLoggerLevel`), aplicadas al crear/cargar personaje. Flags CLI en `Program` (`--log-level=debug|info|warn|error|off`, `--log-off`, `--help`) siguen disponibles y tienen precedencia al inicio (si `--log-off`, se respeta apagado al cargar). Documentación de flags añadida a `README_EXPLICACION.txt`.
[26.3] Parcial | Confirmaciones | Acciones destructivas requieren confirmación | Confirmación añadida al uso de pociones en Inventario y en combate; pendiente extender a descartar/desmontar/venta y otras operaciones.

## 27. SUPERVIVENCIA Y SISTEMAS REALISTAS

[27.1] Parcial | Infraestructura | Configuración data-driven de supervivencia (hambre/sed/fatiga/temperatura) | Archivo `DatosJuego/config/supervivencia.json` creado; modelos `PjDatos/SupervivenciaConfig.cs` y servicio `SupervivenciaService` añadidos (carga y acceso). Aún sin cableado en bucles del juego para no romper tests.
[27.2] Pendiente | Estados jugador | Añadir estados de Hambre, Sed, Fatiga, Temperatura al `Personaje` (persistentes) | Definir acumuladores 0..1 (hambre/sed/fatiga) y TempActual (°C), con inicialización segura y migración.
[27.3] Pendiente | Tick de mundo | Aplicar `TickSupervivencia` en exploración/viaje/comercio/entrenamiento/combate | Factores por acción desde `supervivencia.json` (Multiplicadores por contexto) y por bioma/clima.
[27.4] Parcial | Penalizaciones | Reducir precisión/evasión/regen según umbrales | Implementado: factores de penalización en `SupervivenciaService` (`FactorEvasion`, `FactorRegen`, `FactorPrecision` listo para usar). Aplicado a: evasión del jugador (`Personaje.IntentarEvadir`) y regeneración de maná (en combate y fuera) en `ActionRulesService`. Pendiente: afectar precisión del atacante cuando se integre el chequeo de acierto en el pipeline; penalizaciones a atributos y regeneración de vida/energía cuando se formalicen servicios correspondientes.
[27.5] Pendiente | Consumos | Integrar consumo de `Comida/Bebida/Medico` desde inventario | Reducción de hambre/sed/fatiga/sangrado/infección según `Consumo` del JSON; feedback UI y confirmaciones.
[27.6] Pendiente | Refugios/Hogueras | Beneficios en descanso y mitigación de temperatura | `BonosRefugio` del JSON y acciones de campamento (crear hoguera, descansar), con requerimientos de materiales.
[27.7] Pendiente | Clima y bioma | Temperatura y oscilación por bioma y hora | `ReglasPorBioma` del JSON; variación diurna/nocturna, eventos (ola de calor/frío) básicos.
[27.8] Pendiente | Enfermedades | Sangrado e Infección como efectos de estado | Integrar con `IEfecto` (5.3) y botiquín/antibióticos en `Consumo`.
[27.9] Parcial | UI/Feedback | Indicadores de hambre/sed/fatiga/temperatura | `EstadoPersonajePrinter` muestra barras (% y 10 segmentos) y etiquetas por umbral (OK/ADVERTENCIA/CRÍTICO) para Hambre/Sed/Fatiga, y estado de temperatura (FRÍO/CALOR/HIPOTERMIA/GOLPE DE CALOR/CONFORT) usando `SupervivenciaService`. Replicado un indicador compacto en `MenuCiudad` y `MenuFueraCiudad` vía `UIStyle.SurvivalCompact` (colores + % + etiqueta). Añadido evento `EventoSupervivenciaUmbralCruzado` publicado desde `SupervivenciaRuntimeService.ApplyTick` y suscrito en `Juego` para mostrar avisos al cruzar umbrales. Pendiente: sonidos/efectos visuales y configuración de severidad/colores en `UIStyleService`.
[27.10] Pendiente | Tests | Unit tests de progresión de barras y penalizaciones | Deterministas con `RandomService.SetSeed`; escenarios por bioma y por contexto.

ESTADO ACTUAL (snapshot):

- Fundamentos base completos (1.1–1.6). GuardadoService reemplaza llamadas directas a GestorArchivos en Juego y Menús.
- ProgressionService extendido: recolección, entrenamiento y micro EXP de exploración integrada en movimiento (MostrarMenuRutas).
- Clave ExpBaseExploracion añadida a progression.json para balance.
- Personaje migrado a ExperienciaAtributos (3.1, 3.2) con campos legacy ignorados y migración automática.
- Mapas: selección inicial por CiudadPrincipal funcionando.
- Menú de rutas aplica experiencia de exploración correctamente.
- Sistema de clases dinámicas completo (11.2) con reputación integrada (12.1/12.2).
- Menú Admin implementado (13.1–13.4) facilita balance y QA.

- Testing: proyecto xUnit (MiJuegoRPG.Tests) con 34/34 PASS. Cubiertos: Mapa, ProgressionService, Recolección (cooldown multisector), GeneradorEnemigos (determinista por nivel y filtro) y EncuentrosService (MinKills y ventanas horarias, incluyendo cruce de medianoche). Determinismo reforzado con `RandomService.SetSeed`, `SilentUserInterface` en pruebas y proveedor de hora inyectable `EncuentrosService.HoraActualProvider`. Paralelización de xUnit desactivada para evitar interferencias.
- UI: Base de IUserInterface implementada con ConsoleUserInterface; InputService ya delega lectura/pausas a Juego.Ui y soporta TestMode (evita bloqueos en tests).
- Random: centralización completa y SetSeed disponible para determinismo en pruebas.

- Recolección: nodos de sector se hidratan automáticamente con datos del bioma por nombre (acento-insensible), evitando nodos sin materiales. Producción respeta rangos ProduccionMin/Max y se muestra como etiqueta [Prod X]/[Prod X–Y] en el menú. La cantidad añadida a inventario coincide con lo producido. Telemetría ligera añade logs `[Recolectar][OK/FALLO]` con detalles.

- Logger/CLI/UI: añadidos flags `--log-level` y `--log-off` para controlar logger desde CLI. Nuevo `MenuOpciones` permite cambiarlo en runtime y persistir por partida; al cargar, `Juego` aplica preferencias del jugador (respetando `--log-off` si fue pasado).

- Reputación: bandas y colores parametrizados (DatosJuego/config/reputacion_bandas.json).
- Políticas de bloqueo centralizadas (NPC/Tienda) en servicio `ReputacionPoliticas` con config en `DatosJuego/config/reputacion_politicas.json`.
- Menús muestran etiquetas compactas de reputación con color y valor numérico en Ciudad, Tienda, NPCs y Misiones.
- Gating por reputación negativa activo y alineado a bandas en NPC y Tienda.
- Normalización de ubicaciones a IDs de sector aplicada en menús y tienda (compatibilidad con nombres durante migración de datos).
- Rutas centralizadas: nuevo `PathProvider` define carpetas canónicas de DatosJuego/PjDatos; eliminado código ad-hoc de combinaciones de rutas en servicios clave.

### Data-driven Enemigos (nuevo)

- Hecho | Enemigos por archivo | Se agrega carpeta `DatosJuego/enemigos/` y loader que lee todos los `*.json` (lista u objeto) con fallback a `DatosJuego/enemigos.json`.
- Hecho | Esquema extendido `PjDatos.EnemigoData` con: `Inmunidades {string:bool}`, `MitigacionFisicaPorcentaje`, `MitigacionMagicaPorcentaje`, `Tags[]`, `Id?`.
- Hecho | Spawn | Campos `SpawnChance` (0..1) y `SpawnWeight` (>0) agregados a `EnemigoData` y usados en la selección de `GeneradorEnemigos` (pre-filtro por chance y sorteo ponderado por weight). Retrocompatibilidad con selección uniforme.
- Hecho | Elemental | `ResistenciasElementales {tipo:0..0.9}`, `VulnerabilidadesElementales {tipo:1.0..1.5}` y `DanioElementalBase {tipo:int}` mapeados a `Enemigo` con helpers; vulnerabilidades aplicadas post-mitigación al canal `magia`.
- Hecho | Equipo inicial | Soporte para `EquipoInicial.Arma` (por nombre) con warnings si no existe en catálogo.
- Hecho | Drops por enemigo | `EnemigoData.Drops[]` con chance por ítem, cantidades (min/max) y `UniqueOnce` persistente. Tipos soportados: `material|arma|pocion`. Clamps anti-farming aplicados.
- Hecho | Mapeo en `Motor/GeneradorEnemigos`: aplica inmunidades/mitigaciones/tags desde data; default por `Familia.NoMuerto` -> `veneno` inmune si no está definido.
- Hecho | `PathProvider.EnemigosDir()` para resolver carpeta canónica.
- Parcial | Unificación catálogo objetos | Aún falta consolidar repos JSON de objetos (15.1) para eliminar stubs y warnings.

MÉTRICAS / OBS (para futura instrumentación ligera):

- Clases desbloqueadas por sesión y top motivos bloqueo.
- Frecuencia ajustes reputación (detección abuso admin).
- Atributos manualmente más alterados (apoyo tuning progresión).

PRÓXIMOS PASOS SUGERIDOS (reordenados tras avances):

1. (4.2) Balance de recolección: aplicar Rareza en tasas y cooldowns (raros con cooldown mayor), revisar rangos ProduccionMin/Max por bioma y ajustar con telemetría. Mantener progresión lenta y gating por herramientas/atributos.
2. (2.3/8.3) Completar migración UI: CombatePorTurnos, Inventario/Personaje, Gestores (Armas/Pociones/Materiales), GuardadoService interactivo; introducir UIStyleService.
3. (7.1) `IRepository<T>` base (LoadAll, SaveAll, GetById) + implementación JSON simple (sin cache) para Misiones como piloto.
4. (26.2) Verbosidad desde UI: COMPLETADO. Documentación y `--help` implementados.
5. (10.6) Validador JSON: verificar referenciales (IDs de mapa, facciones, misiones, objetos) + test de contrato por archivo. Extender a repos de objetos (armas/materiales/pociones).
6. (12.5) Métricas reputación: contador de eventos de reputación y cambios por facción; export opcional a CSV/JSON.
7. (Datos) Completar unificación a IDs de mapa en npc.json y facciones_ubicacion.json; mantener compatibilidad durante migración.
8. (14.x) Preparación Unity: esqueleto de adaptadores (UI/Logger/Input) y script de conversión JSON→SO (diseño).
9. (15.1–15.3) Iniciar base de objetos/drops: definir esquema JSON y repos; piloto con 1 enemigo y 1 sector; tests de contrato.
10. (15.4) Esqueleto de crafteo: recetas mínimas + consumo de energía + fallo/éxito; UI básica integrada.
11. (15.5–15.6) Dismantle y reparación: flujo y balance inicial; logs para telemetría (15.10).

NOTAS RIESGO / DEPENDENCIAS:

- Persistir cooldown requiere definir formato (epoch segundos o ISO8601) y limpiar cooldowns expirados al cargar.
- IUserInterface debe entrar antes de colorear UI (8.3) para evitar rehacer cambios.
- Repositorios: migrar uno (Misiones) antes de aplicar a Enemigos/Objetos para validar patrón.

— Fin snapshot actualizado —

Bitácora movida: La bitácora de sesiones fue reubicada en `Docs/Bitacora.md`.

## 2025-09-17 — Documentación detallada

- Se reforzó `progression_config.md` con fórmulas en KaTeX, ejemplos numéricos paso a paso, orden de clamps y contrato JSON sugerido.
- Se amplió `Arquitectura_y_Funcionamiento.md` con contratos (interfaces/DTOs), pipeline de combate por etapas, referencias cruzadas a `Flujo.txt`, y apéndice de firmas.
- Mantener política de “fuente única” y enlaces cruzados desde `Docs/README.md`.
- `Docs/README.md` ahora incluye enlaces profundos directos a secciones específicas de `Flujo.txt` (menús) y de `Arquitectura_y_Funcionamiento.md` (pipeline/contratos), para navegación de un clic.
