
# Bitácora de Desarrollo

## 2025-09-23 — Modularización de clases (normales y dinámicas)

- Se migraron todas las clases del juego a archivos individuales `.json` en subcarpetas por tipo (`basicas`, `avanzadas`, `especiales`), tanto para clases normales como dinámicas.
- **Clases normales**: definen los parámetros base, progresión y habilidades estándar de cada arquetipo. Son la referencia principal para el balance y la progresión general.
- **Clases dinámicas**: variantes adaptativas que pueden modificar requisitos, habilidades, progresión o condiciones de desbloqueo según el contexto del jugador, eventos o decisiones. Permiten mayor flexibilidad y personalización.
- Se recomienda mantener ambos tipos de archivos por ahora, para facilitar el testing, el balance y la migración futura a Unity. El sistema de carga puede priorizar la variante dinámica o la base según el flujo del juego.
- No se eliminó ningún archivo de clase existente; solo se modularizó y documentó la diferencia.

Última actualización: 2025-09-23

## 2025-09-20 — Habilidades por equipo y bono de set GM

- Se extendieron los DTOs de equipo no-arma (`ArmaduraData`, `CascoData`, `BotasData`, `PantalonData`, `CinturonData`, `CollarData`) con `HabilidadesOtorgadas` y `Efectos` opcionales.
- El generador (`GeneradorDeObjetos`) ahora mapea `HabilidadesOtorgadas` a las instancias runtime (`Objeto.HabilidadesOtorgadas`).
- Se implementó `Inventario.SincronizarHabilidadesYBonosSet(Personaje)` que al equipar/desequipar:
  - Otorga habilidades definidas por las piezas equipadas si el nivel del jugador ≥ `NivelMinimo`.
  - Aplica un bono simple de set GM por 2/4/6 piezas: +5000 Defensa, +5000 Ataque y +20000 Maná/Energía respectivamente.
- `Personaje.ObtenerBonificadorEstadistica` suma ahora `BonosTemporalesSet` y comprende alias comunes ("Defensa Física" → "Defensa").

Última actualización: 2025-09-20

## 2025-09-21

- Corrección: Los objetos de equipo no-arma ahora aplican sus bonificaciones al personaje.
- Clases afectadas: `Armadura`, `Botas`, `Pantalon`, `Cinturon`, `Collar`, `Casco`.
- Implementado `IBonificadorEstadistica` donde faltaba y estandarizado mapeo de claves case-insensitive.
- Claves soportadas:
- Defensa física: "Defensa", "DefensaFisica", "Defensa Física".
- Capacidad de carga: "Carga" en `Cinturon`.
- Recursos: "Energia" o "Mana" en `Collar`.
- Resultado: las piezas del set GM aportan sus defensas/carga/recursos al instante al estar equipadas.

- Mejora: Equipar objetos en todos los slots desde UI (“equipar todo” y acción individual) ahora soporta Casco, Pantalón, Botas (slot Zapatos), Collar y Cinturón con comparación básica de stats.

## 2025-09-23 — Estructura de carpetas para materiales de crafteo

- Se creó la carpeta `DatosJuego/Materiales` para organizar los materiales de crafteo de forma modular y escalable.
- Dentro de `Materiales` se generaron subcarpetas por especialidad, siguiendo el patrón `Mat_<Nombre>` según los tipos de recetas de crafteo:
  - Mat_Alquimista
  - Mat_Carpintero
  - Mat_Cocinero
  - Mat_Curtidor
  - Mat_Encantador
  - Mat_Herbolario
  - Mat_Herrero
  - Mat_Ingeniero
  - Mat_Joyero
  - Mat_Sastre
- Esta estructura permitirá separar materiales por especialidad y facilitará la futura carga y validación de datos.

Última actualización: 2025-09-23

## 2025-09-21 — Habilidades temporales por equipo + Sets data‑driven

- Ahora las habilidades otorgadas por piezas de equipo son temporales: se activan al equipar y se REMUEVEN al desequipar o perder el umbral de set.
- Se agregó `Personaje.HabilidadesTemporalesEquipo` para trackear y limpiar de forma segura.
- Se implementó `SetBonusService` (data-driven) que carga `DatosJuego/Equipo/sets/*.json` y aplica bonos/skills por umbral; `GM.json` define 2/4/6 piezas con los mismos valores previos.
- Se añadió `SetId` a DTOs de equipo (`ArmaData`, `ArmaduraData`, `CascoData`, `BotasData`, `PantalonData`, `CinturonData`, `CollarData`, `AccesorioData`) y a `Objetos/Objeto` para agrupar piezas.
- Se actualizó el generador para mapear `SetId` al runtime. Se marcó el set GM con `"SetId": "GM"` en sus JSON.

Última actualización: 2025-09-21

## 2025-09-21 — Modelo unificado de habilidades (datos + equipo/sets)

- Se unificó el modelo de habilidades para que todas provengan del mismo catálogo JSON (`DatosJuego/habilidades/**`).
- Nuevo `Habilidades/HabilidadLoader.cs` ahora tolera archivos lista u objeto, normaliza nombres y carga evoluciones/condiciones.
- Nuevo `HabilidadCatalogService` expone `Todas`, `ElegiblesPara(pj)` y `AProgreso(data)` para crear `Personaje.HabilidadProgreso` con requisitos y evoluciones.
- `Inventario.SincronizarHabilidadesYBonosSet` usa el catálogo al otorgar habilidades por piezas/sets: si la habilidad existe en datos, se instancia con sus evoluciones y requisitos; si no, se crea un progreso mínimo.
- `Personaje.SubirNivel()` intenta auto-desbloquear habilidades básicas elegibles por nivel/misiones/atributos mínimos (no intrusivo, tolerante a falta de datos).

Motivación: mantener consistencia entre habilidades aprendidas “in-world” y habilidades otorgadas por equipo/sets, favoreciendo un balance único y progresión lenta.

Última actualización: 2025-09-21

## 2025-09-22 — Tests de habilidades temporales y sets + doc unificada

- Nuevas pruebas unitarias (`MiJuegoRPG.Tests/HabilidadesYSetsLifecycleTests.cs`):
  
  - Verifican el ciclo de vida de habilidades otorgadas por equipo (se agregan al equipar y se quitan al desequipar).
  
  - Validan los umbrales del set GM (2/4/6 piezas) aplicando y limpiando bonos (`Defensa`, `Ataque`, `Mana`, `Energia`).
  
  - Cubren la elegibilidad básica del catálogo por nivel/atributos y la evolución por uso cuando la definición lo permite.
- Documentación: se agregó la sección “Habilidades (modelo unificado)” en `Docs/Arquitectura_y_Funcionamiento.md` explicando loader, catálogo, runtime y la temporalidad de habilidades por equipo/sets.

Motivación: asegurar consistencia entre habilidades aprendidas y las otorgadas por equipo/sets, y evitar regresiones en la aplicación de bonos de set.

Última actualización: 2025-09-22

## 2025-09-22 — Endurecimiento de pruebas de habilidades

- Se ajustó `MiJuegoRPG.Tests/HabilidadesYSetsLifecycleTests.cs` para no asumir el desbloqueo de evoluciones específicas tras N usos, ya que en los datos reales pueden requerir múltiples condiciones en AND.
- Nueva aserción: se verifica que la habilidad haya subido al menos un nivel (`Nivel >= 2`) tras 60 usos, lo que refleja de forma estable el progreso por uso sin acoplarse a definiciones de evolución.
- Resultado: suite completa en verde (66/66).

Última actualización: 2025-09-22

## 2025-09-22 — Prueba de habilidades por set (GM)

- Se agregó una prueba unitaria `SetGM_HabilidadesPorUmbral_SeAplicanYSeLimpian` que valida que, si un set define habilidades en `DatosJuego/Equipo/sets/*.json`, estas se otorguen temporalmente al alcanzar los umbrales y se limpien al perder piezas.
- La prueba consulta dinámicamente el servicio `SetBonusService` para obtener las habilidades definidas por umbral, evitando acoplarse a datos específicos. Con el set GM actual (solo bonos), la prueba pasa sin encontrar habilidades (no falla por ausencia de definiciones).

Última actualización: 2025-09-22

## 2025-09-22 — Habilidades normales usables en combate (mapper)

- Nuevo `Motor/Servicios/HabilidadAccionMapper.cs`: mapea habilidades aprendidas (por `Id`/`Nombre`) a acciones de combate (`IAccionCombate`) existentes: `Ataque Físico`, `Ataque Mágico`, `Aplicar Veneno`. Soporta sinónimos y normaliza claves.
- Nueva acción `Motor/Acciones/AccionCompuestaSimple.cs`: envoltorio que permite ajustar `CostoMana` y `CooldownTurnos` en runtime y delegar la ejecución a una acción base.
- `CombatePorTurnos` → opción “Habilidad”: ahora lista las habilidades aprendidas usables (vía mapper), muestra coste/CD, verifica recursos/cooldowns con `ActionRulesService` y ejecuta con `TryEjecutarAccion`. Tras usarlas, registra progreso con `GestorHabilidades`.
- Estado: build PASS; tests PASS (67/67). Este cambio habilita contenido básico jugable con habilidades del catálogo sin atar la UI a tipos concretos.

Última actualización: 2025-09-22

## 2025-09-22 — AccionId en habilidades + demo y set GM

- Se añadió el campo opcional `AccionId` a `HabilidadData` y se actualizó `HabilidadAccionMapper` para preferirlo cuando esté presente, manteniendo el fallback por Id/Nombre con sinónimos.
- Nuevo archivo de datos `DatosJuego/habilidades/habilidades_mapper_demo.json` que define `descarga_arcana` con `AccionId: "ataque_magico"` y `CostoMana: 8` (más una evolución de ejemplo).
- El set GM (`DatosJuego/Equipo/sets/GM.json`) ahora otorga la habilidad `descarga_arcana` al alcanzar 4 piezas, además de los bonos previos. Esta habilidad es TEMPORAL y se remueve al bajar del umbral (vía `Inventario.SincronizarHabilidadesYBonosSet`).

Motivación: reducir ambigüedad en el mapeo habilidad→acción, acelerar pruebas de habilidades de catálogo en combate y demostrar la ruta de habilidades por set de forma visible.

Última actualización: 2025-09-22

## 2025-09-22 — Migración de habilidades físicas a archivos individuales

- Migración realizada de `Hab_Fisicas.json` a carpeta temática `DatosJuego/habilidades/Hab_Fisicas/` con un archivo por habilidad (p. ej., `GolpeFuerte.json`).
- Compatibilidad: el loader de habilidades (`Habilidades/HabilidadLoader.cs`) soporta ambos formatos (lista agregada u objeto por archivo) con `PropertyNameCaseInsensitive`.
- Impacto: facilita QA, revisión por PR y balance granular sin cambios en código de consumo.
- Limpieza: se retiró el archivo agregado original tras confirmar la carga per-file.

Referencias

- Roadmap → Estado actual: habilidades per-file y AccionId/mapper.
- Arquitectura_y_Funcionamiento → “Habilidades (modelo unificado)” (nota de migración per-file).

Última actualización: 2025-09-22

## 2025-09-22 — Catálogo de Acciones (data-driven) y plan de desbloqueo oculto

- Se creó `DatosJuego/acciones/acciones_catalogo.json` con acciones genéricas del juego (explorar, dialogar, observar NPC, robar intento, craftear, desmontar, entrenar, viajar, descansar, meditar, aceptar/rechazar misión, reputación, etc.).
- Objetivo: habilitar un sistema de desbloqueo de habilidades basado en el estilo de juego, con condiciones ocultas definidas por acciones (`Condiciones[]` en habilidades pueden referenciar `Accion`).
- Diseño propuesto de runtime: método único `RegistrarAccion(string accionId)` que suma progreso a todas las habilidades del personaje con condiciones que coincidan y dispara el desbloqueo cuando se cumple la `Cantidad`.
- Integración prevista: hooks en combate (mover+ataque como `CorrerGolpear`), interacción con NPC (dialogar/observar/robar), mundo (explorar/descubrir secreto) y sistemas (crafteo/recolección/entrenamiento).
- Beneficios: flexibilidad para añadir/modificar acciones y requisitos sin tocar el código; progresión lenta, desafiante y personalizada.

Referencias

- Roadmap → 7.a Sistema de Acciones data-driven.
- Arquitectura_y_Funcionamiento → pendiente de anotar contrato y convenciones.

Última actualización: 2025-09-22

## 2025-09-22 — Fix pruebas: colisión de nombre `Personaje`

- Problema: en `MiJuegoRPG.Tests/AccionRegistryTests.cs` el identificador `Personaje` colisionaba entre el namespace y la clase (`CS0118`), impidiendo compilar las pruebas.
- Solución: se añadió un alias explícito al tipo `Personaje` (`using PersonajeEnt = MiJuegoRPG.Personaje.Personaje;`) y se actualizó la instanciación a `new PersonajeEnt("Tester")`.
- Resultado: build PASS; tests PASS (69/69) tras ejecutar `dotnet build` y `dotnet test --nologo`.
- Notas: no hay cambios en lógica de producción; es un ajuste local del archivo de pruebas.

Última actualización: 2025-09-22

## 2025-09-22 — Plan de implementación del Sistema de Acciones (Fase 1 · MVP)

- Objetivo: pasar del diseño al código con un servicio `AccionRegistry` (o `ProgressionTracker`) capaz de registrar acciones del juego y avanzar condiciones de habilidades definidas en datos (`Condiciones[]` → `Tipo: "accion"`).
- Contrato propuesto:
  - `RegistrarAccion(string accionId, Personaje pj, object? contexto = null)`: suma progreso a todas las condiciones que referencian `accionId`. Si una habilidad cumple todas sus condiciones, se desbloquea y se notifica por UI (mensaje sutil configurable).
  - `GetProgreso(Personaje pj, string habilidadId, string accionId): int` (diagnóstico/QA).
- Persistencia: `Personaje` almacenará un mapa compacto `{ habilidadId: { accionId: cantidad } }`. Guardado/lectura se integran a `GuardadoService` sin romper saves previos.
- Hooks iniciales:
  - Combate: detectar movimiento+ataque → registrar `CorrerGolpear`.
  - NPC: ver ficha o interactuar sin comerciar → `ObservarNPC`.
  - Mundo: descubrir un sector por primera vez → `ExplorarSector`.
  - Opcionales (si es trivial): `Craftear`, `Recolectar` usando servicios existentes.
- Pruebas: unitarias del servicio (progreso, ids desconocidos, desbloqueo) y una integración que desbloquee una habilidad de demo tras N registros con `RandomService.SetSeed` para determinismo.
- Documentación: actualizar `Docs/Arquitectura_y_Funcionamiento.md` con el contrato, ejemplos y convenciones (IDs, frecuencia, límites).

Próximos pasos:

1) Implementar el servicio y persistencia mínima.
2) Añadir los hooks en combate/NPC/mundo.
3) Escribir las pruebas y ajustar datos de demo si es necesario.

Última actualización: 2025-09-22

## 2025-09-21 — Refuerzo Set GM (per-item v2)

- Se incrementaron y bloquearon parámetros del set GM para facilitar QA extremo y verificaciones de pipeline:
  - Armadura: DEF=80.000; `NivelMin/Max=200`; `PerfeccionMin/Max=100`.
  - Casco: DEF=30.000; `NivelMin/Max=200`; `PerfeccionMin/Max=100`.
  - Pantalón: DEF=30.000; `NivelMin/Max=200`; `PerfeccionMin/Max=100`.
  - Botas: DEF=25.000; `NivelMin/Max=200`; `PerfeccionMin/Max=100`.
  - Collar: `BonificacionDefensa=20.000`, `BonificacionEnergia=50.000`; `NivelMin/Max=200`; `PerfeccionMin/Max=100`.
  - Cinturón: `BonificacionCarga=15.000`; `NivelMin/Max=200`; `PerfeccionMin/Max=100`.
- Motivación: pruebas de alto nivel, validación de bonificadores de equipo no-arma y stress de mensajería de combate.
- Validación: build PASS; tests PASS (63/63). Menú Admin → 22 (`gm:set`) equipa en bloque y aplica bonos (defensa, energía, carga) preservando ratios de recursos.

Última actualización: 2025-09-21

## 2025-09-20 — Menú Admin: clases

- Listar clases ahora separa en: Desbloqueadas, Disponibles (cumplen requisitos pero no obtenidas) y Bloqueadas (con motivos detallados).
- Forzar clase: muestra todas las clases con índice y estado; se puede seleccionar por número o por nombre. Aplica bonos iniciales y reevalúa cadenas.

- NUEVO: si la clase ya estaba desbloqueada, ahora se ofrecen opciones al forzar:
  - Retomar como ACTIVA (no vuelve a sumar bonos); útil para reactivar una clase previamente obtenida.
  - Reaplicar bonos iniciales (ACUMULATIVO) con confirmación explícita; opcionalmente se puede marcar también como ACTIVA.
  - El listado indica el estado [ACTIVA] junto al nombre cuando corresponde.

- Fix: se forzó la carga de `ClaseDinamicaService.Cargar()` desde `MenuAdmin` antes de reflejar `defs`, para evitar listas vacías en algunas rutas de ejecución.
- Fix datos: corregidos campos `MisionUnica` y `ObjetoUnico` en `DatosJuego/clases_dinamicas.json` (eran `{}` y ahora son cadenas vacías), evitando problemas de deserialización.

Última actualización: 2025-09-20

## 2025-09-21 — Restauración completa MenuAdmin + opción 21

- Recuperado `Motor/Menus/MenuAdmin.cs` desde la última versión buena (opciones 1–20 totalmente funcionales: reputación, nivel, atributos batch, listados, exportar snapshot, tiempo del mundo, cooldowns y drops únicos).
- Corregidos artefactos de líneas partidas (tokens como `CultureInfo`, `StringComparison`, `System.Reflection`) que habían quedado corruptos y causaban CSxxxx.
- Reintegrada la opción 21: “Cambiar clase ACTIVA (sin rebonificar)”. Permite alternar entre clases desbloqueadas sin sumar bonos iniciales nuevamente. Mantiene el ratio de maná al recalcular estadísticas.
- Build/Test: PASS (63/63). Validado con `dotnet build` y `dotnet test --nologo`.

Próximos pasos:

- Auto-activar clase por defecto al cargar partida si `Clase==null` y `ClasesDesbloqueadas` no está vacía (tomar la primera por orden alfabético; respetar estilo de juego en el futuro).
- Documentar en `Arquitectura_y_Funcionamiento.md` la diferencia entre desbloquear clase (aplica bonos) y “clase activa” (no rebonifica); incluir advertencias de progresión lenta.

Última actualización: 2025-09-21

## 2025-09-21 — Auto-activación de clase por defecto al cargar

- Mejora QoL: al cargar una partida, si el personaje tiene clases desbloqueadas pero no tiene una clase activa (`Clase == null`), ahora se auto-activa la primera por orden alfabético. Esta operación NO rebonifica; solo marca la activa.
- Implementación: `Motor/Juego.cs` dentro de `CargarPersonaje()` asigna `pj.Clase = new Clase { Nombre = seleccion }` y recalcula `Estadisticas` preservando el ratio de Maná.
- UI: se informa con un mensaje de sistema indicando la clase seleccionada y recordando que puede cambiarse en Menú Admin → opción 21.
- Validación: build y pruebas ejecutadas tras el cambio; estado reportado abajo.

Última actualización: 2025-09-21

## 2025-09-21 — Menú Admin: opción 22 (dar objeto/equipo/material)

- Se añadió la opción 22 en `MenuAdmin`: "Dar objeto/equipo/material por nombre".
- Flujo: ingresa `[tipo:]nombre` (p. ej., `arma:Espada de Hierro`, `material:Madera`, `pocion:Poción Pequeña`, o solo `Collar de Energía`). Si omites tipo, busca en todo el catálogo y muestra coincidencias.
- Para equipo, se respeta el esquema v2: rareza normalizada, rango de perfección por rareza y rangos de nivel/estadística cuando están en JSON; se calcula el valor final con la base Normal=50% (`valorFinal = round(valorBase * (Perfeccion / 50.0))`).
- Tras entregar, si es equipo, ofrece equiparlo inmediatamente.
- Objetivo: agilizar QA de data de objetos y pruebas de balance sin depender del RNG de drops.

Validación

- Build: PASS.
- Tests: PASS (63/63).

Última actualización: 2025-09-21

## 2025-09-21 — Fix loader de Equipo per-item y rutas PjDatos

- Se endureció `GeneradorObjetos.CargarListaDesdeCarpeta<T>` para aceptar tanto listas como objetos individuales por archivo usando `JsonDocument`, con `PropertyNameCaseInsensitive` y logs tolerantes por elemento. Esto evita errores como “The JSON value could not be converted to List 'ArmaData' ” al leer archivos per-item bajo `DatosJuego/Equipo/<tipo>/**.json`.
- `GestorPociones.CargarPociones` ahora resuelve rutas con `PathProvider.PjDatosPath` y, si no encuentra el archivo en PjDatos, hace fallback automático a `DatosJuego/pociones/pociones.json`.
- `GestorMateriales.CargarMateriales` ahora usa `PathProvider.PjDatosPath` y valida existencia con un mensaje claro.
- Resultado: Carga automática de equipo vuelve a listar correctamente los ítems encontrados sin ruido; menú admin opción 22 ya puede encontrar pociones y materiales cuando existen en sus ubicaciones previstas.

Última actualización: 2025-09-21

## 2025-09-21 — Normalización ítems GM (per-item v2)

- `DatosJuego/Equipo/armas/Estada_GM.json`: adaptado al esquema Arma v2. Se corrigió `Rareza` a `Legendaria`, se limitaron `RarezasPermitidasCsv` a valores válidos, `Efectos`/`HabilidadesOtorgadas` convertidos a listas de objetos, y `Requisitos` a diccionario `{"Nivel": 1}`. Mantiene `Tags` para marcar uso GM.
- `DatosJuego/Equipo/armaduras/armadura_GM.json`: reescrito a esquema Armadura v2 con claves y tipos correctos (`Nombre`, `Defensa`, `Nivel`, `TipoObjeto`, `Rareza`, `Perfeccion`). El resto de atributos se trasladó a `Tags`/`Descripcion`.
- Resultado: el loader per-item ya no ignora estos archivos; quedan disponibles en el menú admin (opción 22) para QA.

Última actualización: 2025-09-21

## 2025-09-21 — Build desbloqueada y verificación Estada_GM

- Se cerró el proceso colgado `MiJuegoRPG.exe` que bloqueaba la copia de `apphost.exe` (errores MSB3027/MSB3021). Tras terminar el proceso, la solución compiló correctamente.
- Verificación de datos: `DatosJuego/Equipo/armas/Estada_GM.json` cumple el esquema `ArmaData v2`:
  - Claves y tipos válidos (enteros para daño/niveles/valor; `Rareza: "Legendaria"`; `RarezasPermitidasCsv` consistente; `Perfeccion=100` con `Min/Max=100`).
  - `Efectos` y `HabilidadesOtorgadas` en forma de lista de objetos con propiedades reconocidas.
  - `Requisitos` expresado como diccionario (`{"Nivel": 1}`) y metadatos (`Descripcion`, `Tags`).
- Resultado: el menú Admin opción 22 puede otorgar el arma “Estada del Creador” y la armadura GM previamente normalizada sin ser ignoradas por el loader.

Última actualización: 2025-09-21

## 2025-09-21 — Set GM completo (per-item)

- Añadidos JSON per-item para el set GM complementario a la armadura y arma ya existentes:
  - `Equipo/cascos/casco_GM.json` (Casco)
  - `Equipo/botas/botas_GM.json` (Botas)
  - `Equipo/cinturones/cinturon_GM.json` (Cinturón)
  - `Equipo/collares/collar_GM.json` (Collar)
  - `Equipo/pantalones/pantalon_GM.json` (Pantalón)
- Esquema: v2 compatible con `*Data.cs` respectivos (`Defensa` o bonificaciones según tipo, `Nivel`, `TipoObjeto`, `Rareza`, `Perfeccion`, metadatos). Rareza `Ornamentada`, `Perfeccion=100`, `Durabilidad=-1`, `Valor/ValorVenta=0`, y `Tags` con `gm`/`divino`.
- Cómo otorgarlos (Menú Admin → 22):
  - `casco:Casco DIVINO GM GOD`
  - `botas:Botas DIVINO GM GOD`
  - `cinturon:Cinturon DIVINO GM GOD`
  - `collar:Collar DIVINO GM GOD`
  - `pantalon:Pantalon DIVINO GM GOD`
- Validación: build PASS; los datos se copian al output. Opción 22 permite buscarlos por nombre o por tipo:nombre.

Última actualización: 2025-09-21

## 2025-09-21 — Menú 22: atajo para set GM

- Se añadió un atajo en Menú Admin → opción 22: escribir `gm:set` o `gm:set-completo` entrega automáticamente todas las piezas GM (arma, armadura, casco, botas, cinturón, collar, pantalón) y ofrece equiparlas en bloque.
- Implementación: `Motor/Menus/MenuAdmin.cs` → método `EntregarSetGMCompleto()` invocado cuando el input coincide con los alias (`gm:set`, `gm set`, `gm:todo`, `gm todo`).
- Validación: build PASS.

Última actualización: 2025-09-21

## 2025-09-20 — Migración accesorios a v2

- Se migraron los accesorios `anillo_de_poder.json` y `anillo_de_proteccion.json` al esquema v2 compatible.
- Cambios clave:

  - Rareza normalizada a "Normal" como baseline; se agregó `RarezasPermitidasCsv` con el conjunto completo permitido.
  - Rango de `PerfeccionMin/Max` (10–100) y `NivelMin/Max` (1–4/5) para generación data-driven.
  - Se añadió `Descripcion` y se mantienen las bonificaciones base como máximos a 100% de perfección.

- Build y pruebas: PASS (63/63). Validador de datos: OK sin errores.

Próximos pasos:

- Añadir validador específico de equipo (rango coherente, rarezas válidas, duplicados por nombre) — planificado.

Última actualización: 2025-09-20

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

### Pruebas añadidas — Verbosidad de combate (9.8)

- Nuevas pruebas en `MiJuegoRPG.Tests/CombatVerboseMessageTests.cs` que validan la presencia del detalle didáctico cuando `GameplayToggles.CombatVerbose` está ON y el ataque impacta:
  - Físico: verifica fragmentos "Base", "Defensa efectiva", "Mitigación" y "Daño final:".
  - Mágico: además de lo anterior, incluye "Defensa mágica efectiva", "Resistencia magia" y "Vulnerabilidad".
- Caso negativo: cuando el ataque es evadido/falla por precisión (`--precision-hit` con `Precision=0`), no se agrega la línea de detalle aunque la verbosidad esté ON.
- Determinismo: se usa `RandomService.SetSeed` en los escenarios con RNG y se restablecen los toggles para no contaminar otras pruebas.

### Datos de Equipo — Loader per-item y rareza ponderada

- Se implementó `GeneradorObjetos.CargarEquipoAuto()` que lee recursivamente JSON por ítem bajo `DatosJuego/Equipo/<tipo>/**.json` (objeto o lista). Si no hay subcarpetas, cae a los archivos agregados por tipo (`armas.json`, `Armaduras.json`, etc.) para mantener compatibilidad.
- Selección con progresión lenta: por defecto `GeneradorObjetos.UsaSeleccionPonderadaRareza = true` usa pesos conservadores (Rota=50, Pobre=35, Normal=20, Superior=7, Rara=3, Legendaria=1, Ornamentada=1). Se puede desactivar si se requiere uniformidad en pruebas. NUEVO: estos pesos ahora son configurables desde `DatosJuego/Equipo/rareza_pesos.json` (acepta formato objeto o lista con `{Nombre,Peso}`); al iniciar, `CargarEquipoAuto()` los intenta leer y aplica si son válidos.
- Documentación: añadido `DatosJuego/Equipo/README.md` con estructura, ejemplos y notas.

### Migrador de Equipo per-item (incluye armas con esquema legado)

- Nueva herramienta `Herramientas/MigradorEquipoPerItem.cs` capaz de dividir los agregados por tipo en archivos individuales por ítem dentro de `DatosJuego/Equipo/<tipo>/`.
- Compatibilidad armas.json: se añadió un parser tolerante que soporta claves históricas (`DañoFisico`/`DañoMagico`, `Categoria`/`TipoObjeto`, `Rareza: Comun/PocoComun`) y normaliza a `ArmaData` (`Daño`, `Tipo` inferido por nombre/categoría, `Rareza` mapeada a `Normal`/`Superior`).
- Ejecución:

  - Reporte: `--migrar-equipo=report` → muestra un estimado de archivos a crear.
  - Aplicar: `--migrar-equipo=write` → crea los JSON por ítem. Resultado de hoy: 29 archivos generados (armas, armaduras, accesorios, botas, cinturones, collares, pantalones). Errores: 0.

- Infra: se añadieron helpers tolerantes para leer propiedades JSON (`GetPropertyOrDefault`, `GetIntPropertyOrDefault`) usados solo por el migrador.
- Tarea VS Code corregida: `Ejecutar TestGeneradorObjetos` ahora apunta a `MiJuegoRPG\MiJuegoRPG.csproj` correctamente.

### Generación de accesorios — base Normal=50% y rarezas configurables

- Se ajustó la fórmula de escalado por Perfección para que la base sea la rareza Normal=50%. Factor: `valorFinal = round(valorBase * (Perfeccion / 50.0))`. Esto endurece la progresión temprana y refuerza el carácter lento del juego.
- Rarezas permitidas por ítem: si `Rareza` trae CSV o `RarezasPermitidasCsv` está definido, se filtra la elección a ese conjunto y se usa selección ponderada global. Si hay rango de `PerfeccionMin/Max`, se intersecta con el rango impuesto por la rareza elegida.
- Archivo de configuración: `DatosJuego/Equipo/rareza_pesos.json` permite modificar la probabilidad relativa de aparición por rareza sin recompilar. Formatos soportados:
  - Objeto: `{ "Rota": 50, "Pobre": 35, ... }`
  - Lista: `[{ "Nombre": "Rota", "Peso": 50 }, ... ]`
  Notas: claves normalizadas toleran acentos y alias ("Comun" → "Normal").

Extensión global de selección ponderada y base de perfección

- La selección ponderada por rareza ya no es exclusiva de accesorios; se aplica también a armaduras, cascos, botas, cinturones, collares y pantalones cuando `GeneradorObjetos.UsaSeleccionPonderadaRareza = true` (por defecto).
- La fórmula de base Normal=50% (`valorBase * (Perfeccion / 50.0)`) se estandarizó para todo el equipo (daño/defensa/bonificaciones). Con `Perfeccion=50` no hay cambio; valores inferiores/superiores escalan en consecuencia.

Configuración desde `DatosJuego/config` (preferido) y fallback

- Ahora el generador intenta primero leer configuración global desde `DatosJuego/config`:
  - `rareza_pesos.json`: pesos de aparición por rareza (objeto o lista). Si no se encuentra, se intenta `DatosJuego/Equipo/rareza_pesos.json`.
  - `rareza_perfeccion.json`: rangos de perfección por rareza (tres formatos soportados: arrays, objeto Min/Max, lista). Si falta, se usan defaults conservadores en código.
- Objetivo: centralizar balance y mantener compatibilidad con el esquema previo de Equipo/.

### Armas v2 (rangos + rarezas permitidas + metadatos)

- Se extendió `PjDatos/ArmaData.cs` con campos opcionales para un esquema de armas más rico: rangos (`NivelMin/Max`, `PerfeccionMin/Max`, `DañoMin/Max`), canales (`DañoFisico/DañoMagico`, `DañoElemental`), crítico/penetración/precisión/velocidad, bonificadores, efectos, habilidades, requisitos y economía (valor compra/venta), además de peso/durabilidad/tags/descripcion. Compatibilidad total con el formato anterior.
- `GeneradorDeObjetos.GenerarArmaAleatoria` ahora:
  - Filtra rarezas por `RarezasPermitidasCsv` si existe y elige ponderado dentro de ese subconjunto.
  - Interseca `PerfeccionMin/Max` con el rango por rareza configurado.
  - Soporta `NivelMin/Max` y `DañoMin/Max`; aplica escalado por perfección usando `MidpointRounding.AwayFromZero`.
- Datos migrados (muestra): `DatosJuego/Equipo/armas/espada_oxidada.json` y `espada_de_hierro.json` actualizados al esquema extendido (manteniendo claves legacy) con Rareza base en "Normal" y `RarezasPermitidasCsv` para controlar el espacio de tiradas.
- Documentación: `DatosJuego/Equipo/README.md` ampliado con el esquema "Arma v2" y ejemplo completo.

Próximos pasos sugeridos:

- Extender el generador para interpretar `DañoFisico/DañoMagico` y `DañoElemental` en la instancia si están definidos en el JSON (hoy se usa `danioBase` representativo para el constructor de `Arma`).
- Añadir validador de datos de equipo que verifique rangos, rarezas y duplicados por `Nombre`.

## 2025-09-18

Última actualización: 2025-09-18

### Estado del personaje: modo detallado y acceso por menú

- Se añadió un modo "detallado" al `EstadoPersonajePrinter` (`MostrarEstadoPersonaje(pj, bool detallado=false)`). Cuando está activo, se imprime una nueva sección "Equipo" listando slots: Arma, Casco, Armadura, Pantalón, Zapatos, Collar, Cinturón, Accesorio 1 y 2, con nombre del ítem y stats clave (Rareza/Perfección; para armas, Daño Físico/Mágico). La vista compacta se mantiene como predeterminada.
- `Juego.MostrarEstadoPersonaje` ahora expone un overload que acepta el flag `detallado` y el `Menú Fijo` incluye una opción separada para abrir el estado en modo detallado.
- Validación: build y suite en verde (58/58) tras los cambios, ver salida de tareas de build/test.

### Gating de menú de ciudad, rediseño Estado y fix CS0234

- Corrección build: se resolvió el error intermitente `CS0234` en `RecoleccionService.cs` (referencia a `MiJuegoRPG.Personaje.NuevoObjeto` inexistente). La validación de herramienta ahora usa el tipo correcto `ObjetoConCantidad` y se agregó `using MiJuegoRPG.Personaje;` para simplificar la firma. Build limpio tras `dotnet clean` y suite verde.
- Gating de menú de ciudad: `Juego.MostrarMenuPorUbicacion` solo muestra el menú de ciudad si el sector es `Tipo:"Ciudad"` y además `EsCentroCiudad` o `CiudadPrincipal` son verdaderos. En otras partes de la ciudad se utiliza el menú de “Fuera de Ciudad”. Soporte de datos: `PjDatos/SectorData.cs` ahora tiene `Tipo` por defecto `"Ruta"` para evitar clasificaciones falsas cuando el JSON no especifica el tipo.
- Estado del personaje (UI): `EstadoPersonajePrinter` fue remaquetado con `UIStyle` para un aspecto profesional y compacto: encabezado/resumen, barras de Vida/Maná/Energía y XP, atributos con bonos agregados, y sección de supervivencia con etiquetas por umbral. En línea con la futura migración a Unity.
- Validación: `dotnet test --nologo` en verde (58/58). Se actualizó `Docs/Roadmap.md` con estos avances y se registró este cambio aquí para trazabilidad.

## 2025-09-17

Última actualización: 2025-09-17

### Fix NRE en Recolección (herramienta requerida)

- Se corrigió un `NullReferenceException` al ejecutar recolección en nodos con `Requiere` (p. ej., "Pico de Hierro") cuando partidas antiguas tenían `Inventario.NuevosObjetos == null`.
- Cambio: en `Motor/Servicios/RecoleccionService.cs` (`EjecutarAccion`), la validación de herramienta ahora usa null-safety (`?.`) y búsqueda case-insensitive sobre la lista local segura. Si falta la herramienta, se informa por UI y no rompe.
- Validación: build correcto y suite en verde (58/58).

### Mensajería de combate unificada vía DamageResolver

- Se eliminaron impresiones directas a UI en `Personaje.AtacarFisico/AtacarMagico` y `Personaje.RecibirDanio*`, así como en `Enemigo.AtacarFisico/AtacarMagico`. En su lugar, la mensajería de combate se centraliza en `DamageResolver`, que compone `ResultadoAccion.Mensajes`.
- `CombatePorTurnos`: el turno de los enemigos ahora usa `DamageResolver.ResolverAtaqueFisico(enemigo, jugador)` y muestra únicamente los mensajes de `ResultadoAccion`, evitando duplicados e inconsistencias (por ejemplo, “0 de daño”).
- Logs de depuración se mantienen mediante `Logger.Debug(...)` en puntos clave (evadidos y aplicación de daño) sin afectar la UI del jugador.
- Resultado: suite en verde (58/58). Documentación pendiente de reflejar el estado de [5.13] en el Roadmap.

### Remaquetado de Roadmap (Combate/Testing)

- Se reestructuraron las secciones `5. COMBATE` (ítems [5.8], [5.10], [5.13]) y `9. TESTING` (ítem [9.8]) en `Docs/Roadmap.md` para mejorar legibilidad y trazabilidad.
- Nuevo formato por ítem: sub-bloques "Estado", "Descripción", "Decisiones/Conclusiones" y "Próxima acción". Objetivo: lectura rápida en GitHub y status claro por bloque.
- No hay cambios en runtime ni en firmas de código; es puramente documental. Se cuidó la compatibilidad con `markdownlint` (MD032/MD007) y se mantuvieron enlaces/identificadores originales.

### Penetración en pipeline de combate (flag `--penetracion`)

- Se implementó la etapa de Penetración en el pipeline de daño de forma no intrusiva y opcional (desactivada por defecto). Al habilitar el flag CLI `--penetracion`, la defensa efectiva del objetivo se reduce en proporción a la `Estadisticas.Penetracion` del atacante ANTES de aplicar mitigaciones/resistencias.
- Técnica utilizada: contexto ambiental `CombatAmbientContext` que transporta la penetración del atacante durante la ejecución del ataque sin modificar firmas públicas. `DamageResolver` establece el valor de forma temporal alrededor de `AtacarFisico/AtacarMagico` cuando el ejecutor es `Personaje`.
- Receptores actualizados: `Enemigo.RecibirDanioFisico/Magico` y `Personaje.RecibirDanioFisico/Magico` leen la penetración del contexto y calculan `defensaEfectiva = round(defensaBase * (1 - pen))` antes de `Mitigacion*`. El daño mágico mantiene el orden: Defensa→Mitigación→Resistencia(`magia`)→Vulnerabilidad.
- CLI y toggles: se añadió `GameplayToggles.PenetracionEnabled` en `Program.cs` y el flag `--penetracion` con ayuda en `--help`. El flag `--precision-hit` se mantuvo y se aclaró que aplica al ataque físico.
- Pruebas unitarias: `PenetracionPipelineTests` con escenarios deterministas y expectativas numéricas:
  - Físico con pen: defensa 30, pen 20%, mitigación 10% sobre daño 100 → `DanioReal = 68`.
  - Mágico con pen: defensa mágica 20, resistencia `magia` 30%, vulnerabilidad 1.2, pen 25% → `DanioReal = 71`.
  - Toggle OFF: mismo caso físico inicial pero con `--penetracion` desactivado → `DanioReal = 63`.
- Resultado: build y suite de pruebas en verde (total 52). Documentación sincronizada en `Docs/Roadmap.md` ([5.8], [5.10], [9.8]). Pendiente: caps/curvas de `Penetracion`/`CritChance`/`CritMult`/`Precision` en `Docs/progression_config.md`.

### Caps de combate centralizados y tests Crit+Pen

- Se creó `Motor/Servicios/CombatBalanceConfig.cs` para cargar caps opcionales (`StatsCaps`) desde `DatosJuego/progression.json` con defaults conservadores (`PrecisionMax=0.95`, `CritChanceMax=0.50`, `CritMult∈[1.25,1.75]`, `PenetracionMax=0.25`).
- `Personaje/Estadisticas` ahora aplica clamps centralizados a `Precision`, `CritChance`, `CritMult` y `Penetracion` usando el servicio anterior (sin cambiar fórmulas base).
- Se añadieron pruebas `EstadisticasCapsTests` verificando que valores extremos respetan los caps por defecto.
- Roadmap actualizado ([9.8]) para reflejar casos de interacción Crítico + Penetración (físico y mágico) que validan orden, `DanioReal` y flag `FueCritico`.
- `Docs/progression_config.md` incluye ahora la sección `StatsCaps` en el contrato JSON y reglas de validación.

### Fix pruebas: alias de tipo `Personaje`

- Se corrigió un error de compilación intermitente en `MiJuegoRPG.Tests/CritPenetracionInteractionTests.cs` (CS0118: `Personaje` espacio de nombres usado como tipo) introduciendo un alias explícito: `using PersonajeModel = MiJuegoRPG.Personaje.Personaje;` y reemplazando instancias de `new Personaje(...)` por `new PersonajeModel(...)`.
- Resultado: la suite vuelve a verde estable (55/55) en ejecuciones repetidas.

### Docs: Arquitectura y caps centralizados

- `Docs/Arquitectura_y_Funcionamiento.md`: sección 3 actualizada para reflejar que `Precision`, `CritMult` y `Penetracion` se clampean vía `CombatBalanceConfig` (fuente `DatosJuego/progression.json` → `StatsCaps`).
- Añadida subsección 3.2 con fórmula en KaTeX, defaults y enlace a `Docs/progression_config.md`.

### Tests: orden Defensa→Mitigación→Resistencias/Vulnerabilidades

- Se añadieron pruebas `DamagePipelineOrderTests` que validan, de forma determinista, el orden de aplicación en el pipeline de daño no intrusivo:
  - Mágico: Defensa → Mitigación → Resistencia (canal "magia") → Vulnerabilidad.
  - Físico: Defensa → Mitigación.
  - Escenarios adicionales: mágico sin vulnerabilidad (solo defensa/mitigación/resistencia) y mágico solo con vulnerabilidad.
- Metodología: atacante plano que aplica 100 de daño; `EnemigoEstandar` configurado con valores controlados; se verifica `DanioReal` por delta de vida vía `DamageResolver` y se comparan resultados esperados (redondeos con `MidpointRounding.AwayFromZero`).
- Roadmap actualizado ([9.8]) para reflejar el avance de cobertura; próximos pasos: penetración, caps desde `progression.json` y centralización total de mensajería.

### Ataque Mágico unificado vía DamageResolver

- `AtaqueMagicoAccion` ahora invoca `DamageResolver.ResolverAtaqueMagico`, manteniendo el cálculo de daño actual pero unificando metadatos y mensajería: `DanioReal` por delta de vida, `FueEvadido` cuando no aplica daño y `FueCritico` bajo la misma política que el físico (crítico forzado si `CritChance>=1.0` en pruebas).
- Documentación sincronizada: `Docs/Arquitectura_y_Funcionamiento.md` refleja que el mágico también fluye por el resolver; `Docs/Roadmap.md` actualizado en [5.8]/[9.8].
- Calidad: se corrigieron avisos de `markdownlint` (MD007) en listas anidadas de `Arquitectura_y_Funcionamiento.md`.

### Combate: precisión opcional y pruebas

- Se introdujo chequeo de precisión opcional en `DamageResolver` previo al ataque físico, controlado por el flag CLI `--precision-hit` (variable `GameplayToggles.PrecisionCheckEnabled`).
- Mensajería y metadatos: cuando falla por precisión se marca `FueEvadido=true` y se emite un mensaje de fallo; cuando `CritChance>=1.0` en `Personaje`, el crítico se fuerza para pruebas deterministas.
- Se ajustó el cálculo de `DanioReal` en `ResultadoAccion` tomando la diferencia de vida del objetivo antes/después del ataque para reflejar mitigaciones.
- Pruebas añadidas (`AccionesCombateTests`): `AtaqueFisico_PrecisionToggle_AlFallarNoHayDano` y `AtaqueFisico_CriticoForzado_SeMarcaCritico`. Todas las pruebas existentes se mantienen verdes (47/47).
- Balance por defecto sin cambios: el flag está desactivado por defecto.

### Expansión documental detallada

- `progression_config.md`: se añadieron fórmulas en KaTeX, ejemplos numéricos paso a paso, orden de aplicación (clamps) y contrato JSON sugerido con defaults. Se incluyeron pruebas recomendadas y guías de tuning.
- `Arquitectura_y_Funcionamiento.md`: se profundizó en contratos (interfaces/DTOs), pipeline de combate con orden exacto y límites, referencias a `Flujo.txt`, y apéndice de firmas públicas. Además, se documentó el estado actual (MVP) del pipeline de combate: precisión opcional activable por `--precision-hit`, cálculo de `DanioReal` por delta de vida y forzado de crítico (`CritChance>=1.0`) para pruebas. Objetivo: facilitar onboarding y migración a Unity.
- Roadmap: anotación del hito documental y recordatorio de política de “fuente única”.
- Índice: `Docs/README.md` actualizado con enlaces profundos a secciones clave de `Flujo.txt` (menús) y `Arquitectura_y_Funcionamiento.md` (pipeline/contratos) para navegación rápida.

#### Navegación y anclas

- Se añadieron encabezados H1 en `Flujo.txt` para permitir enlaces directos por sección (menús y flujo del juego).
- `Arquitectura_y_Funcionamiento.md` ahora enlaza a cada sección específica de `Flujo.txt` (Inicio, Menú Principal, Ciudad, Fuera de Ciudad, Misiones/NPC, Rutas, Combate, Entre Combates, Menú Fijo).

Este documento registra cambios cronológicos por sesión. El `Roadmap.md` mantiene el plan por áreas y los próximos pasos.

### Resumen del día (2025-09-17)

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

Última actualización: 2025-09-16

- Tests/Infra:
  - Ajustado `MiJuegoRPG.Tests.csproj` para copiar recursivamente `MiJuegoRPG/DatosJuego/**` al output de pruebas. Resuelve errores MSB3030 por reorganización de enemigos (bioma/nivel/categoría). Suite de pruebas en verde: 45/45 PASS.
  - Verificado build de solución post-cambio: ambos proyectos compilan correctamente.
- Documentación/Quality:
  - Normalizadas viñetas/indentación en Roadmap (correcciones markdownlint) y sincronizada la sección 9 con la nueva configuración de assets.
- Datos/Enemigos/Elemental (estado):
  - Loader recursivo de enemigos con convención de ignorar JSON en la raíz de `nivel_*` ya activo.
  - `VulnerabilidadesElementales {1.0..1.5}` integrado y documentado; aplicado en daño mágico post-mitigación.

## 2025-09-15

Última actualización: 2025-09-15

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

## 2025-09-20

Última actualización: 2025-09-20

- Datos/Equipo — Armas v2: se migró `DatosJuego/Equipo/armas/vara_de_fuego.json` al esquema extendido “Arma v2” con:
  - Base Normal=50%, `RarezasPermitidasCsv: "Superior, Rara, Legendaria"`.
  - Rangos: `NivelMin/Max: 3–5`, `PerfeccionMin/Max: 55–90`, `DañoMin/Max: 13–17`.
  - Metadatos: crítico (6% x1.6), penetración 4%, precisión 2%, velocidad 1.0, bonus de Inteligencia +2, efecto OnHit “Quemadura” (25% por 2 turnos), requisitos {Inteligencia:5, Nivel:3}, economía (`ValorVenta:9`), peso 1.2, durabilidad 65 y tags [baston, fuego].
  - Se mantiene `Rareza: "Normal"` como baseline para escalado y compatibilidad.
- Build/Test: verificados localmente.
  - Build PASS: `dotnet build` OK.
  - Tests PASS: `dotnet test --nologo` OK (63/63).
- Documentación: no se requieren cambios adicionales; `DatosJuego/Equipo/README.md` ya documenta el esquema Arma v2.

### Migración adicional — Esquema v2 para equipo no-arma

Última actualización: 2025-09-20

- Se migraron los siguientes ítems al esquema v2 (rangos Nivel/Perfección/Stat, RarezasPermitidasCsv, metadatos básicos):
  
  - Botas: `botas_de_tela.json`, `botas_de_tela_2.json` (antes parciales: `botas_de_cuero*.json` ya migradas).
  - Cinturones: `cinturon_de_cuero.json`, `cinturon_de_cuero_2.json`, `cinturon_de_hierro.json`, `cinturon_de_hierro_2.json`.
  - Collares: `collar_de_energia.json`, `collar_de_proteccion.json`.
  - Pantalones: `pantalon_de_cuero.json`, `pantalon_de_cuero_2.json`, `pantalon_de_tela.json`, `pantalon_de_tela_2.json`.

- Notas de balance y generación:
  
  - Baseline Normal=50% para escalado: $valor_{final} = \operatorname{round}(valor_{base} \cdot (Perfeccion/50.0))$.
  - Rarezas permitidas por ítem: cuando `RarezasPermitidasCsv` está presente, la rareza se elige ponderada dentro del subconjunto permitido (pesos configurables en `DatosJuego/Equipo/rareza_pesos.json`).
  - Rangos por rareza: al elegir rareza, se intersecta `PerfeccionMin/Max` del ítem con el rango de perfección configurado para esa rareza (`rareza_perfeccion.json` o defaults conservadores).

- Verificación rápida:
  
  - No hubo cambios de código en esta tanda (solo datos). La compilación/pruebas deberían mantenerse verdes. Si el entorno local no tiene `dotnet` en PATH, ejecutar manualmente en una consola con SDK instalado.

- Próximos pasos:
  
  - Accesorios (anillos) pendientes de migrar a v2 opcional (ya soportado por DTO y generador).
  - Añadir validador de datos de equipo (rangos/rareza/duplicados) en `DataValidatorService`.

Próximos pasos sugeridos:

- Extender el consumo de `DañoFisico/DañoMagico` y `DañoElemental` en la instancia `Arma` cuando el JSON los defina.
- Añadir validador de Equipo (rangos de nivel/daño/perfección y rarezas permitidas) y chequeo de duplicados por `Nombre`.
