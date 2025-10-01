# Arquitectura y Funcionamiento (Documento Vivo)

## Rarezas y probabilidades de aparición (MIGRADO A STRINGS DINÁMICO)

Estado actual (2025-09-30):

- El sistema YA NO requiere agregar rarezas a un enum. El enum `Rareza` en `EnumsObjetos.cs` queda como legado y no es fuente de verdad para nuevas rarezas (deprecado para extensiones futuras).
- La única fuente dinámica de pesos y rangos de perfección es la configuración JSON en `DatosJuego/config/rareza_pesos.json` y `DatosJuego/config/rareza_perfeccion.json` (formats tolerantes: objeto, lista, arrays).
- Todas las clases runtime de equipo y el `GeneradorObjetos` operan con `string rareza` y consultan `RarezaConfig` para:
  - Pesos de selección ponderada.
  - Rango de perfección por rareza.
  - Multiplicadores opcionales.
- Si una rareza no existe en la config: se aplica fallback seguro (peso=1, perfección base 50–50) y se emite advertencia (no excepción dura).
- Las probabilidades/ pesos admiten enteros o decimales y pueden añadirse nuevas rarezas (p.ej. `Epica`, `Mistica`) sin tocar código.

Formato recomendado (ejemplo `rareza_pesos.json` actual, con `Epica` y valores vigentes):

```json
{
  "Rota": 20,
  "Pobre": 35,
  "Normal": 50,
  "Superior": 10,
  "Rara": 3,
  "Epica": 2,
  "Legendaria": 1,
  "Ornamentada": 0.1
}
```

Nota: el orden en el archivo no es relevante; los pesos se normalizan al cargar. Valores más altos = más comunes.

Formato alterno listado:

```json
[
  { "Nombre": "Rota", "Peso": 50 },
  { "Nombre": "Pobre", "Peso": 35 }
]
```

Rangos de perfección (`rareza_perfeccion.json`) soportan:

```json
{ "Normal": [40,60], "Rara": [70,90] }
```

o

```json
{ "Rara": { "Min": 70, "Max": 90 } }
```

o lista

```json
[{ "Nombre": "Rara", "Min": 70, "Max": 90 }]
```

Reglas vigentes:

- Base de escalado: `Normal = 50%` → factor = Perfeccion/50.0 para daño/defensa/bonos.
- Intersección: el generador intersecta rangos del ítem (`PerfeccionMin/Max`) con el rango global de la rareza. Si intersección vacía → fallback al rango global.
- Nueva rareza: agregar en ambos archivos (pesos y perfección). No modificar enum.
- Enum legado: puede retirarse cuando no existan callers que lo consuman (marcado con comentario `[DEPRECATED]`).

Histórico (pre 2025-09-30): el modelo exigía mantener correspondencia estricta con un enum; esa sección se conserva en Bitácora (entradas 2025-09-24) para trazabilidad pero ya no aplica como política.

Última actualización: 2025-09-30 — Migración completa a rarezas dinámicas.

### 2025-09-23: Robustez en menú admin (clases)

Se reforzó la función `MotivosBloqueoClase` para que todos los accesos a propiedades y colecciones sean seguros ante valores nulos o datos incompletos.
Esto garantiza que el menú de administración de clases nunca lanza excepción y siempre muestra los requisitos, aunque falte información en los datos o definiciones.
El cambio es retrocompatible y no afecta la lógica de progresión ni el desbloqueo de clases.

#### 2025-09-23: Inserción masiva de armas de enemigos

Se automatizó la detección y creación de todas las armas referenciadas por enemigos en los JSON de enemigos.
Ahora, cada arma referenciada existe en `armas.json` con rareza y estructura válidas, evitando errores de combate por datos faltantes.
El proceso es repetible y puede adaptarse a futuras expansiones de enemigos o biomas.
Validado con build y pruebas unitarias (PASS).

Última actualización: 2025-09-23

# MiJuegoRPG — Arquitectura y Funcionamiento (Estudio Detallado)

## (LEGACY / OBSOLETO) Validación estricta de enums

Esta política aplicaba antes de la migración a rarezas dinámicas (strings + JSON). El enum `Rareza` quedó como legado y ya no es fuente de verdad. Se mantiene la sección sólo para trazabilidad histórica. El flujo actual: rarezas se validan contra `RarezaConfig` (JSON) y el enum se eliminará tras confirmación de no-usos.
Última actualización legado: 2025-09-23 (marcado obsoleto 2025-09-30)

## Introducción y Documentos Relacionados

Objetivo: documentar con nivel de ingeniería la estructura, el flujo y las reglas del juego para facilitar mantenimiento, onboarding y futura migración a Unity. Este documento sirve como guía viva y complementa `Roadmap.md` y `progression_config.md`.

Documentos relacionados

- Roadmap (plan y estado): `./Roadmap.md`
- Bitácora (historial): `./Bitacora.md`
- Config de progresión: `./progression_config.md`
- Flujo de juego (menús): [`../../Flujo.txt`](../../Flujo.txt)
- Inicio: [`INICIO DEL JUEGO`](../../Flujo.txt#inicio-del-juego-programcs)
- Menú Principal: [`MENÚ PRINCIPAL DEL JUEGO`](../../Flujo.txt#menu-principal-del-juego-juegoiniciar)
- Ciudad: [`MENÚ DE CIUDAD`](../../Flujo.txt#menu-de-ciudad-menuciudad)
- Fuera de ciudad: [`MENÚ FUERA DE CIUDAD`](../../Flujo.txt#menu-fuera-de-ciudad-menufueraciudad)
- Misiones/NPC: [`MENÚ DE MISIONES Y NPC`](../../Flujo.txt#menu-de-misiones-y-npc-menusjuegomostrarmenumisionesnpc)
- Rutas: [`MENÚ DE RUTAS`](../../Flujo.txt#menu-de-rutas-juegomostrarmenurutas)
- Combate: [`MENÚ DE COMBATE`](../../Flujo.txt#menu-de-combate-base-actual)
- Entre combates: [`MENÚ ENTRE COMBATES`](../../Flujo.txt#menu-entre-combates-menuentrecombate)
- Menú fijo: [`MENÚ FIJO`](../../Flujo.txt#menu-fijo-accesible-desde-ciudadfueracombate)

Tabla de contenidos

1. Visión general del sistema
1. Núcleo del dominio
1. Progresión y atributos
1. Habilidades (modelo unificado)
1. Combate (pipeline y estados)
- Nota: Nuevo `DamagePipeline` disponible en modo sombra (flag `--damage-shadow`) comparando resultados sin alterar gameplay; reemplazo total planificado tras calibración.
1. Recolección y mundo
1. Objetos, inventario y comercio
1. Repositorio jerárquico de equipo (base + overlay)
1. Misiones y encuentros
1. Supervivencia (hambre/sed/fatiga/temperatura)
1. UI y presentación
1. Datos, validación y guardado
1. Testing y determinismo
1. Migración a Unity (consideraciones)
1. Problemas conocidos y edge cases
1. Ejemplos prácticos (recetas de uso)
1. Apéndice de contratos (interfaces y DTOs)

---

## 1. Visión general del sistema

### Modularidad de materiales y drops de enemigos (2025-09-23)

### Sistema de Puntos de Acción (PA) – Fase 1 (2025-09-30)

Objetivo: introducir cálculo determinista de PA por turno para el futuro modo de acciones encadenadas sin alterar aún el combate existente.

Fórmula actual:

PA = BasePA + floor(Agilidad / 30) + floor(Destreza / 40) + floor(Nivel / 10) + BonosEquipo + BuffsPA - DebuffsPA

En Fase 1: BonosEquipo = 0; BuffsPA = 0; DebuffsPA = 0 (placeholders hasta cablear sistemas asociados).

Clamp: PA ∈ [1, PAMax] (por defecto PAMax=6). Config editable en `DatosJuego/config/combat_config.json`.

Archivo nuevo: `CombatConfig` (carga tolerante, fallback a defaults). Servicio puro: `ActionPointService.ComputePA`.

Integración: pendiente – se activará con flag `ModoAcciones` en fases posteriores.

Motivación: escala lenta, decisiones tácticas (pocas acciones de alto impacto vs varias ligeras). Evita explosión combinatoria temprana.

Última actualización: 2025-09-30.

### Pipeline de Daño (Fase 2 aislado – 2025-09-30)

### Meta de Rarezas (Unificación Escalas) – 2025-09-30

Cada rareza se define en dos JSON: `rareza_pesos.json` (peso/frecuencia) y `rareza_perfeccion.json` (rango calidad). A partir de ellos `RarezaConfig` construye `RarezaMeta` con:

- PerfMin/PerfMax/PerfAvg → `BaseStatMult = PerfAvg / 100`.
- Probabilidad = peso / sumaPesos.
- Escasez = 1 / prob; normalizada logarítmicamente a [0..1].
- `PriceMult = BaseStatMult * (1 + K * ScarcityNorm)` con K=0.6 (cap 2.0).

`RarezaHelper.MultiplicadorBase/Precio/Drop` consultan esta meta (fallback seguro si rareza desconocida). Eliminados hardcodes previos; agregar rareza nueva sólo necesita editar los JSON.

### Repositorio jerárquico de equipo (base + overlay) – 2025-10-01

Objetivo: unificar la carga de datos de equipo en una capa resiliente y extensible, desacoplando el generador legacy de la estructura física de archivos.

Principios:
- Base recursiva: se recorren carpetas por tipo (ej. `DatosJuego/Equipo/armaduras/**`) aceptando archivos cuya raíz sea objeto o lista.
- Overlay opcional: archivos en `PjDatos/` (por convención `<tipo>_overlay.json` o `<tipo>s.json`) reemplazan entradas existentes por `Nombre` (case-insensitive) sin modificar la base.
- Primer archivo base gana: si dos archivos base definen el mismo `Nombre`, se conserva la primera aparición para evitar efectos de orden no deterministas.
- Tolerancia a errores: archivo corrupto → `Warn` y continuar; nunca abortar carga total.
- Normalización de rarezas: cada DTO pasa por `RarezaNormalizer.Normalizar()` (alias históricos: `Raro→Rara`, `Epico→Epica`, `Normal/Comun→Comun`, etc.).
- Fallback seguro: si falta `Rareza` se asigna `Comun`; si faltan rangos de perfección se usan los de la rareza global.

Orden de precedencia:
1. Primer registro válido encontrado en base (por carpeta recursiva).
2. Overlay (jugador) reemplaza totalmente la entrada por nombre.

Beneficios:
- Fuente única fiable usada por generación, validadores y futuros sistemas (crafteo, economía).
- Simplifica pruebas: tests de repos no dependen del generador completo.
- Facilita migración progresiva (se pueden usar repos para piezas ya migradas y legacy para el resto sin romper flujo).

Estado actual (2025-10-01): Migrados `MaterialRepository`, `ArmaRepository`, `ArmaduraRepository`, `BotasRepository`, `CascosRepository`, `CinturonesRepository`, `CollaresRepository` (integrados gradualmente en `GeneradorObjetos.CargarEquipoAuto`). Pendiente migrar: Pantalones, Accesorios, Pociones.

Próximos pasos técnicos:
- Extraer clase base reutilizable (`HierarchicalOverlayRepository<T>`) para factorizar `CargarBase` / `AplicarOverlay` y reducir duplicación.
- Añadir validador cruzado (rango de perfección coherente, rarezas válidas, nombres duplicados) consolidado.
- Consolidar logs de rarezas desconocidas (agrupar por rareza y count) para reducir ruido.

Riesgos y mitigación:
- Duplicados en overlay: el último en el mismo archivo ganará; se recomienda validación pre-carga (futuro validador).
- Cambios masivos de estructura: al ser data-driven, solo requiere agregar archivos; la lógica de repos soporta ambas raíces (objeto/lista) sin modificaciones.

Motivación: consistencia economía, balance incremental y evitar divergencias manuales.

Última actualización: 2025-09-30.

Estado: servicio `DamagePipeline` independiente, no altera aún `DamageResolver`.

Orden inmutable:

1. BaseDamage (DB)
2. Hit/Evasión (HitChance = clamp(PrecisionBase + PrecisionExtra – Evasion, MinHit, 1.0))
3. Penetración (DEF * (1 - Pen))
4. Resta de defensa (min 1 si impacta)
5. Mitigación porcentual (afterDef * (1 - MIT))
6. Crítico (afterMit * CritMult si Crit)
7. Vulnerabilidad/Elemento (afterCrit * Vuln)
8. Redondeo (AwayFromZero) + mínimo 1

Estructura:
`DamagePipeline.Request` (parámetros explícitos) → `DamagePipeline.Calcular` → `Result` (FinalDamage, flags y métricas intermedias).

Motivación: determinismo testable y facilidad para insertar pasos futuros (`IDamageModifierStep`).

No hay mutación directa de vida todavía; el consumo lo realizará un adaptador de combate posterior con logs integrados.

Próximo paso: envolver `DamageResolver` para usar pipeline cuando se active flag experimental.

Última actualización: 2025-09-30.

Todos los materiales referenciados como drops de enemigos cuentan ahora con archivos `.json` individuales en la subcarpeta correspondiente (ejemplo: `Mat_Cocina`).

Esto permite:

- Integración directa con el sistema de crafteo, cocina y progresión.
- Validación y QA automáticos desde el loader, evitando referencias huérfanas.
- Escalabilidad para añadir nuevos materiales, recetas y drops sin modificar el código fuente.
- Trazabilidad y documentación completa de cada material, su rareza, origen y usos.

Esta modularidad es clave para la futura migración a Unity y para mantener la progresión lenta y desafiante definida en `progression_config.md`.

Patrón de carga (Materiales): Base jerárquica (`DatosJuego/Materiales/**`) + overlay jugador (`PjDatos/materiales.json`) con sustitución por Nombre (case-insensitive). Archivos aceptan objeto único o lista; campos tolerantes a mayúsculas/minúsculas y alias (`especialidad` → `Categoria`). Rareza normalizada temprano; errores por archivo degradan (log Warn) sin abortar carga global.

Organización por capas con enfoque data-driven. Piezas principales (enlaces a implementación real):

- Equipo: `DatosJuego/Equipo/` soporta JSON por ítem en subcarpetas por tipo (armas/armaduras/...) con carga recursiva vía `GeneradorObjetos.CargarEquipoAuto()` y fallback a archivos agregados por tipo. Selección aleatoria puede ser ponderada por rareza.
**Equipo v2 (no-armas)**: Los tipos de equipo como armaduras, botas, cascos, cinturones, collares, pantalones y accesorios pueden declarar campos opcionales como `RarezasPermitidasCsv`, `PerfeccionMin/Max`, `NivelMin/Max` y rangos de estadística (por ejemplo, `DefensaMin/Max`, `Bonificacion*Min/Max`). El generador intersecta los rangos de perfección por rareza con los declarados por el ítem. La escala usa `Normal = 50%` con redondeo `AwayFromZero`.
- Nota: el set GM usa estos campos para bloquear `NivelMin/Max=200` y `PerfeccionMin/Max=100` en todas las piezas no-arma, y definir bonificaciones elevadas coherentes con su rol de QA.
- Bonos de equipo (contrato): las piezas implementan `IBonificadorEstadistica.ObtenerBonificador(string estadistica)`. Claves soportadas (case-insensitive): Defensa física = {"Defensa", "DefensaFisica", "Defensa Física"}; Capacidad de carga = {"Carga"}; Recursos = {"Energia", "Mana"}. El personaje suma estos bonos en runtime a través de `Personaje.ObtenerBonificadorEstadistica`.
- QA de objetos desde Admin: `MenuAdmin` incluye la opción 22 para entregar un objeto/equipo/material/poción por nombre (busca en catálogos JSON). Útil para validar definiciones y balance rápidamente; permite equipar de inmediato tras conceder.
- Esquema v2 de equipo: los DTOs de Armadura/Botas/Casco/Cinturón/Collar/Pantalón admiten rangos `NivelMin/Max`, `PerfeccionMin/Max` y `DefensaMin/Max` (o `Bonificacion*Min/Max`), además de `RarezasPermitidasCsv` y metadatos (`Valor/ValorVenta`, `Peso`, `Durabilidad`, `Descripcion`, `Tags`). Compatibles con el formato legado; los campos son opcionales. Ver ejemplos en `DatosJuego/Equipo/*/*.json` y notas en `Docs/Roadmap.md` (2025-09-20).
- Herramientas/QA: validadores, generadores y reparadores.

Diagrama conceptual (texto):

Jugador/Enemigo → Acciones → [`DamageResolver`](../Motor/Servicios/DamageResolver.cs) → [`ResultadoAccion`](../Interfaces/ResultadoAccion.cs) → UI
                          ↑                                       |
                          |                                       ↓
               ProgressionService ← Eventos ← GuardadoService

Metas clave:

- Progresión lenta y desafiante (balance conservador).
- Modularidad y futura migración a Unity.
- Determinismo en pruebas (`RandomService.SetSeed`).

## 2. Núcleo del dominio

Entidades y responsabilidades:

- `Personaje`: atributos, estadísticas derivadas, inventario, reputaciones, clases dinámicas; implementa `ICombatiente` e `IEvadible`. Ver [`Estadisticas`](../Personaje/Estadisticas.cs) y [`Atributo`](../Dominio/Atributo.cs).
- `Enemigo`: comportamiento similar a `Personaje` con mitigaciones, inmunidades, `Drops`. Ver [`Enemigo`](../Enemigos/Enemigo.cs).
- Acciones de combate: `AtaqueFisico`, `AtaqueMagico`, `UsarPocion`, `AplicarEfectos`; exponen `IAccionCombate.Ejecutar` y devuelven `ResultadoAccion`.
- Efectos: `IEfecto` con ticks por turno y expiración (p. ej., veneno).

Interfaces clave (contratos ejecutivos):

- [`ICombatiente`](../Interfaces/ICombatiente.cs):
  - Métodos: `AtacarFisico(ICombatiente objetivo)`, `AtacarMagico(ICombatiente objetivo)`, `RecibirDanioFisico(int d)`, `RecibirDanioMagico(int d)`.
  - Props: `int Vida`, `int VidaMaxima`, `int Mana`, `int ManaMaximo`, `double DefensaFisica`, `double DefensaMagica`.
- [`IEvadible`](../Interfaces/IEvadible.cs):
  - `bool IntentarEvadir(bool esMagico)` — devuelve true si evita; aplica penalización de hechizos.
- [`IAccionCombate`](../Interfaces/IAccionCombate.cs):
  - `ResultadoAccion Ejecutar(CombateContext ctx)`; metadatos `CooldownTurnos`, `CostoMana`.

DTOs relevantes:

- [`ResultadoAccion`](../Interfaces/ResultadoAccion.cs): `{ string Mensaje, int DanioReal, bool FueCritico, bool FueEvadido, bool ObjetivoDerrotado, ... }`
- `Estadisticas`: derivadas de `AtributosBase`; ver §3.2 y [`Estadisticas`](../Personaje/Estadisticas.cs).

## 3. Progresión y atributos

Controlado vía `progression_config.md`/`progression.json`.

Atributos base (del código): `Fuerza`, `Destreza`, `Vitalidad`, `Agilidad`, `Suerte`, `Defensa`, `Resistencia`, `Sabiduría`, `Inteligencia`, `Fe`, `Percepcion`, `Persuasion`, `Liderazgo`, `Carisma`, `Voluntad`, `Oscuridad`.

### 3.1 Estadísticas derivadas (fórmulas actuales)

- Salud = `10·Vitalidad`
- Maná = suma ponderada de múltiples atributos (Int, Sab, Fe, Vol, Car, Lid, Vit, Fue, Des, Agi, Suer, Def, Res, Perc, Pers)
- Energía = `10·Agilidad`
- Ataque = `0.01·(Fuerza + Destreza)`
- DefensaFisica = `0.01·(Defensa + Vitalidad)`
- PoderMagico = `0.01·(Inteligencia + Sabiduría)`
- DefensaMagica = `0.01·(Resistencia + Sabiduría)`
- RegeneracionMana = `0.01·Inteligencia`
- Evasion = `0.01·(Agilidad + Suerte)`
- Critico (chance) = `0.01·(Destreza + Suerte)`
- Precision = `ClampPrecision(0.01·Destreza + 0.005·Percepcion)`
- CritMult = `ClampCritMult(1.5 + 0.001·Sabiduría)`
- Penetracion = `ClampPenetracion(0.002·Destreza)`

Notas: los clamps se aplican de forma centralizada a través de `CombatBalanceConfig` cargando caps desde `DatosJuego/progression.json` (sección opcional `StatsCaps`). Si el archivo no define `StatsCaps`, se usan defaults conservadores. Cap de evasión efectiva en `IntentarEvadir` = 0.5 (previo a RNG), penalización 0.8 para hechizos. Equipo y supervivencia modifican estas cifras.

### 3.2 Caps de combate (data‑driven)

## 3.3 Modularización de clases (normales y dinámicas)

> **Actualización 2025-09-23**

- Todas las clases del juego se migraron a archivos individuales `.json` en subcarpetas por tipo (`basicas`, `avanzadas`, `especiales`), tanto para clases normales como dinámicas.
- **Clases normales**: definen los parámetros base, progresión y habilidades estándar de cada arquetipo. Son la referencia principal para el balance y la progresión general.
- **Clases dinámicas**: variantes adaptativas que pueden modificar requisitos, habilidades, progresión o condiciones de desbloqueo según el contexto del jugador, eventos o decisiones. Permiten mayor flexibilidad y personalización.
- Se recomienda mantener ambos tipos de archivos por ahora, para facilitar el testing, el balance y la migración futura a Unity. El sistema de carga puede priorizar la variante dinámica o la base según el flujo del juego.
- No se eliminó ningún archivo de clase existente; solo se modularizó y documentó la diferencia.

---

## 4. Habilidades (modelo unificado)

> Nota (2025-09-22): Las habilidades físicas fueron migradas de un archivo único (`Hab_Fisicas.json`) a archivos individuales por habilidad bajo `DatosJuego/habilidades/Hab_Fisicas/`. El loader soporta ambos formatos (lista u objeto por archivo). Esta organización facilita QA, versionado y balance granular.

Objetivo: todas las habilidades del juego comparten la misma fuente de verdad (JSON en `DatosJuego/habilidades/**`), ya sean aprendidas “in-world” por el personaje o concedidas temporalmente por equipo/sets. Esto garantiza consistencia en requisitos, coste y evoluciones.

Piezas clave

- Loader: `Habilidades/HabilidadLoader.cs` carga archivos en formato objeto o lista, con `PropertyNameCaseInsensitive`, y normaliza campos. Soporta:
  
  - `AtributosNecesarios` y `CostoMana`.
  
  - `Evoluciones` con `Condiciones` (por ejemplo, `NvHabilidad`, `NvJugador`, `Misiones` o etiquetas de logro futuras).
- Catálogo: `HabilidadCatalogService` expone:
  
  - `Todas`: habilidades disponibles en datos.
  
  - `ElegiblesPara(Personaje)`: filtra por condiciones básicas (nivel, misión, atributos mínimos) — extensible.
  
  - `AProgreso(HabilidadData)`: crea la instancia runtime `Personaje.HabilidadProgreso` con evoluciones y requisitos.
- Runtime Personaje:
  
  - `Personaje.Habilidades: Dictionary<string,HabilidadProgreso>` — habilidades aprendidas/activas.
  
  - `Personaje.UsarHabilidad(id)`: incrementa experiencia, verifica costes/atributos y llama a `RevisarEvolucionHabilidad` para desbloquear evoluciones cuando cumplan sus condiciones.
  
  - `Personaje.SubirNivel()`: intenta auto-desbloquear habilidades elegibles del catálogo (no intrusivo).

Habilidades otorgadas por equipo/sets (temporales)

- `Objeto.HabilidadesOtorgadas`: lista de referencias `{ Id, NivelMinimo }` declaradas en el JSON del ítem. Al equiparlo, se agregan temporalmente si el PJ cumple el nivel.
- `Inventario.SincronizarHabilidadesYBonosSet(Personaje)`: agrega o quita habilidades y aplica bonos de set. La sincronización usa el catálogo cuando la habilidad existe en data, manteniendo un único modelo. Las habilidades otorgadas por equipo/sets se rastrean en `Personaje.HabilidadesTemporalesEquipo` y se eliminan al desequipar o perder umbrales de set.
- Sets data-driven: `Motor/Servicios/SetBonusService.cs` carga `DatosJuego/Equipo/sets/*.json` y aplica bonificaciones y habilidades por umbral (ej.: 2/4/6 piezas). El matching se realiza por `SetId` del objeto o por coincidencia de nombre.

Habilidades jugables en combate (mapper)

- `Motor/Servicios/HabilidadAccionMapper.cs`: servicio de mapeo tolerante que traduce `HabilidadProgreso` (Id/Nombre) a una `IAccionCombate` concreta. Incluye sinónimos comunes para Físico/Mágico/Veneno y puede envolver una acción base en `AccionCompuestaSimple` para ajustar `CostoMana` y `CooldownTurnos` según el catálogo de la habilidad.
- Mapeo explícito (opcional): `HabilidadData` incorpora `AccionId` para mapear de forma determinista una habilidad a una acción (por ejemplo, `"ataque_magico"`). El mapper prefiere `AccionId` y, si no está presente, cae a sinónimos por Id/Nombre.
- Menú de combate (`CombatePorTurnos`, opción Habilidad): lista solo las habilidades que el mapper considera usables, mostrando coste y cooldown actuales. Antes de ejecutar, usa `ActionRulesService` para chequear recursos y cooldowns; en éxito registra el uso con `GestorHabilidades` para dar EXP y posibles subidas de nivel/evoluciones.

Contratos mínimos

- `HabilidadCatalogService`:
  
  - `IReadOnlyList<HabilidadData> Todas { get; }`
  
  - `IEnumerable<HabilidadData> ElegiblesPara(Personaje pj)`
  
  - `HabilidadProgreso AProgreso(HabilidadData data)`
- `Inventario.SincronizarHabilidadesYBonosSet(Personaje pj)` — llamado tras equipar/desequipar para mantener consistencia de habilidades y sets.

Notas de diseño

- Progresión lenta: los requisitos iniciales son conservadores (niveles/atributos), y las evoluciones se desbloquean con uso frecuente o hitos (niveles/combate). Esto favorece exploración y planificación.
- Temporalidad clara: las habilidades de equipo/sets no persisten si se quita el origen. Se evita confusión visual limpiando silenciosamente al sincronizar.
- Compatibilidad: existe un fallback por nombre “GM” para el set legado cuando faltan definiciones JSON, minimizando regresiones.
Fuente de verdad: `DatosJuego/progression.json` → sección opcional `StatsCaps` (ver `Docs/progression_config.md`).

- Servicio: `Motor/Servicios/CombatBalanceConfig.cs` proporciona `ClampPrecision`, `ClampCritChance`, `ClampCritMult` y `ClampPenetracion`.
- Defaults actuales (si faltan en JSON):
  - `PrecisionMax = 0.95`
  - `CritChanceMax = 0.50`
  - `CritMultMin = 1.25`, `CritMultMax = 1.75`
  - `PenetracionMax = 0.25`

Ejemplo (KaTeX):

$\text{Precision} = \min(0.95,\ 0.01\cdot Destreza + 0.005\cdot Percepcion)$

$\text{CritMult} = \operatorname{clamp}(1.5 + 0.001\cdot Sabidur\'ia,\ 1.25,\ 1.75)$

$\text{Penetraci\'on} = \min(0.25,\ 0.002\cdot Destreza)$

## 5. Combate (pipeline y estados)

Estado actual (MVP implementado):

- Precisión opcional: `DamageResolver` realiza un chequeo de acierto previo en ataques físicos cuando se lanza el juego con el flag `--precision-hit`. Si el atacante es `Personaje`, usa `Estadisticas.Precision` y, de fallar, retorna `FueEvadido=true` y `DanioReal=0` sin invocar `AtacarFisico`. Ver [`DamageResolver`](../Motor/Servicios/DamageResolver.cs), [`Program.cs`](../Program.cs) y [`Estadisticas`](../Personaje/Estadisticas.cs).
- Cálculo de daño real: `DanioReal` se computa como delta de vida del objetivo (VidaAntes − VidaDespués), garantizando coherencia post-defensa/mitigación. Mensajes de combate usan `DanioReal` para evitar desalineación con la UI.
- Crítico determinista en pruebas: si `CritChance >= 1.0` en `Personaje`, `FueCritico=true` forzado para escenarios de test; en juego normal se usa la probabilidad conservadora y solo aplica si `DanioReal > 0`. Ver [`ResultadoAccion`](../Interfaces/ResultadoAccion.cs).

- Penetración opcional: si se lanza con `--penetracion`, el `DamageResolver` propagará la `Penetracion` del atacante (si es `Personaje`) a través de un contexto ambiental [`CombatAmbientContext`](../Motor/Servicios/CombatAmbientContext.cs). Los receptores (`Enemigo`/`Personaje`) reducen su defensa efectiva antes de mitigar: `defensaEfectiva = round(defensa * (1 - pen))`. Orden respetado: Físico → Defensa/Penetración → Mitigación; Mágico → Defensa/Penetración → Mitigación → Resistencia(`magia`) → Vulnerabilidad. El flag está desactivado por defecto para no alterar el balance legacy.

Unificación de acciones:

- El `Ataque Mágico` también pasa por `DamageResolver` (sin paso de precisión), manteniendo el cálculo de daño actual pero unificando metadatos (`DanioReal`, `FueEvadido`, `FueCritico`) y mensajería.

Orden de pipeline propuesto (futuro inmediato):

1) Hit/Evasión: $p_{hit} = clamp(0.35 + Precision_{att} - k·Evasion_{obj},\ 0.20,\ 0.95)$, con $k \in [1.0, 1.2]$.
      - Aplicar factor de Supervivencia: $p_{hit} *= FactorPrecision$.
2) Crítico: si RNG < `CritChance`, multiplicar por `CritMult`; caps: `CritChance ≤ 0.5`, `CritMult ∈ [1.25, 1.75]`.
3) Defensa/Penetración: reducir defensa por `Penetracion` y mitigar. Implementación actual detrás de `--penetracion` usando `CombatAmbientContext`.
4) Mitigaciones del objetivo: físicas/mágicas.
5) Elementales: resistencias (0..0.9) y vulnerabilidades (1.0..1.5) por canal (`magia` hoy).
6) Aplicar daño y efectos OnHit/OnKill.
7) Registrar en `ResultadoAccion` y presentar en UI.

Nota práctica (MVP actual):

- `DamageResolver` incorpora un chequeo de precisión opcional previo al ataque físico (ver flag CLI). Si falla, corta la ejecución con `FueEvadido=true` y `DanioReal=0`.
- Crítico: si `CritChance >= 1.0` en `Personaje`, el crítico se considera forzado (útil para pruebas deterministas); en runtime normal se aplica probabilidad y multiplicador con clamps conservadores.
- Mensajería: los mensajes se generan en base a `DanioReal` y flags (`FueEvadido`, `FueCritico`) para mantener coherencia; `CombatePorTurnos` imprime a través de la UI.
- Verbosidad de combate: además del flag `--combat-verbose`, puede alternarse en runtime desde Menú Principal → Opciones → "Verbosidad de Combate". Cuando está ON, `DamageResolver` agrega una línea didáctica explicando los pasos del cálculo (Defensa→Mitigación→Resistencias/Vulnerabilidades→Crítico→Daño final).
- Pruebas unitarias: `CombatVerboseMessageTests` valida la presencia de esta línea cuando hay impacto y su ausencia cuando el ataque es evadido o falla (por precisión). El gating se respeta siempre: sin `CombatVerbose` no se emite el detalle.

Ejemplo práctico (flag de precisión activado):

1) Ejecutar con `--precision-hit`.
2) `AtaqueFisico` → `DamageResolver.ResolverAtaqueFisico` → chequeo de $p_{hit}$.
3) Si RNG ≥ $p_{hit}$: `FueEvadido=true`, `DanioReal=0`; si RNG < $p_{hit}$: procede el cálculo de daño, evalúa crítico y aplica sobre el objetivo.
4) `ResultadoAccion` se devuelve a `CombatePorTurnos` y la UI muestra el mensaje.

Fórmula de impacto (propuesta, usada por el MVP con $k=1.0$): $p_{hit} = clamp(0.35 + Precision - 1.0\cdot Evasion,\ 0.20,\ 0.95)$

Ejemplo práctico (flag de penetración activado):

1) Ejecutar con `--penetracion`.
2) `AtaqueFisico`/`AtaqueMagico` → `DamageResolver` envuelve la llamada de ataque con `CombatAmbientContext.WithPenetracion(pen)` donde `pen = clamp(Estadisticas.Penetracion, 0, 0.9)`.
3) En el receptor, se calcula `defensaEfectiva = round(defensa * (1 - pen))` y luego se aplican mitigaciones (y resistencias/vulnerabilidades si es mágico).
4) `ResultadoAccion.DanioReal` refleja el delta de vida post-mitigación y los mensajes lo usan para coherencia.
Edge cases y decisiones:

- Daño mínimo = 1 si impacta y tras mitigaciones queda > 0 (salvo inmunidades explícitas).
- Evasión duplicada: consolidar en un solo chequeo en el resolver (evitar doble miss).
- Overkill: clamp vida a 0; marcar `ObjetivoDerrotado`.

## 6. Sistema de Acciones (AccionRegistry)

Contrato y propósito

- Servicio: `Motor/Servicios/AccionRegistry.cs` centraliza el registro de eventos del juego para impulsar desbloqueos de habilidades definidas en datos.
- Datos: las habilidades (`Habilidades/HabilidadLoader.cs`) pueden declarar `Condiciones[]` con campos `Tipo` y `Accion`. Cualquier condición que tenga `Accion` se trata como contador accionable; `Tipo` se usa para hints ("nivel", "mision", etc.).

API mínima

- `bool RegistrarAccion(string accionId, Personaje pj, object? contexto = null)`
  - Suma +1 al progreso de todas las habilidades no aprendidas que refieran `accionId` en sus condiciones.
  - Si todas las condiciones AND (atributos, nivel/misión básicas y contadores de acción) se cumplen, crea el `HabilidadProgreso` vía `HabilidadCatalogService.AProgreso` y llama `pj.AprenderHabilidad(...)`.
  - Retorna true si alguna habilidad se desbloqueó en la llamada.
- `int GetProgreso(Personaje pj, string habilidadId, string accionId)`
  - Lee el contador acumulado.

Persistencia

- `Personaje.ProgresoAccionesPorHabilidad: Dictionary<string, Dictionary<string,int>>` con la forma `{ habilidadId: { accionId: cantidad } }`.
- Guardado/Lectura en `GuardadoService` a `PjDatos/progreso_acciones.json` (compatibilidad hacia atrás asegurada).

Hooks iniciales (MVP)

- Combate: al atacar básico y al usar habilidades mapeadas a `ataque_fisico` se registran `Golpear` y `CorrerGolpear`.
- Mundo: al explorar sector se registra `ExplorarSector`.
- Recolección: al recolectar con éxito se registra `RecolectarMaterial`.
- NPC: en el menú de NPCs, la acción "Observar" registra `ObservarNPC` (no afecta misiones/comercio).
- Crafteo: al craftear con éxito se registra `CraftearObjeto`.

Notas de diseño

- Progresión lenta: los contadores suelen requerir cantidades moderadas/altas; el sistema no otorga avances por acciones desconocidas.
- Compatibilidad: se respetan condiciones básicas (nivel/misión) y atributos necesarios antes del desbloqueo.
- Telemetría: se pueden activar hints sutiles con `AvisosAventura` al aprender una habilidad; el ruido de consola se mantiene bajo.

### 6.1 Extensión (Diseño): Combate por Puntos de Acción (PA) Fase 1

Objetivo: permitir múltiples acciones cortas en un turno (ej. movimiento + ataque + usar poción) sin acelerar exponencialmente la progresión.

Componentes:
- Flag `CombatConfig.ModoAcciones` para activar la lógica PA (OFF → loop legacy intacto).
- `ActionPointService` (existente) calcula PA inicial por turno: se almacena localmente como `paRestantes`.
- Extensión mínima de `IAccionCombate`: propiedad opcional `CostoPA` (default=1). Acciones actuales retornan 1 vía wrapper si no implementan.
- Bucle interno del turno jugador: mientras `paRestantes > 0` mostrar menú reducido (acciones disponibles + opción terminar turno).

Reglas Fase 1:
- Todos los costes = 1. (Futuro: ataque pesado 2, defensa 1, moverse 1, habilidad compleja 2…)
- Enemigos mantienen 1 acción estándar por turno para estabilidad inicial.
- Regeneración de maná y avance de efectos/cooldowns ocurren al cerrar el bloque PA, no por cada acción.
- Fallo por cooldown / recursos NO consume PA.

Flujo (pseudocódigo):

1. if (!cfg.ModoAcciones) → comportamiento actual.
2. pa = ComputePA(jugador,cfg).
3. while (pa > 0 && jugador.EstaVivo && enemigosVivos): mostrar menú acciones.
4. Ejecutar acción válida → pa -= CostoPA; aplicar cooldown/recursos.
5. Opción “0” termina turno anticipadamente.

### 6.2 Flags y parámetros experimentales de combate (tabla de referencia)

| Flag / Param | Tipo | Scope | Descripción | Estado | Plan retiro |
|--------------|------|-------|-------------|--------|-------------|
| `--damage-shadow` | CLI | Runtime | Ejecuta nuevo DamagePipeline en paralelo (no afecta gameplay) y registra diferencias. | Estable (shadow) | Cuando `--damage-live` alcance drift <±3% en 3 sesiones.
| `--damage-live` | CLI | Runtime | Reemplaza cálculo legacy por pipeline nuevo (sin shadow). | Experimental | Al confirmar estabilidad (<±3%) retirar legacy.
| `--precision-hit` | CLI/Toggle | Combate | Activa chequeo de precisión para ataques físicos. | Opcional | Integrar en balance base tras validación caps.
| `--penetracion` | CLI/Toggle | Combate | Aplica penetración de defensa previa a mitigaciones. | Parcial | Se vuelve siempre ON tras tuning final.
| `--combat-verbose` | CLI/Toggle | UI | Mensajes explicativos de cálculo de daño. | Parcial | Integrar nivel detalle configurable.
| `--shadow-benchmark` | CLI | QA | Corre benchmark sintético comparando legacy vs pipeline. | QA | Retirar tras retirar legacy.
| `--shadow-sweep` | CLI | QA | Recorre combinaciones CritScaling/FactorPenCrit. | QA | Igual que benchmark.
| `--test-rareza-meta` | CLI | QA | Ejecuta validaciones de rarezas (precio/fallback). | QA | Mantener como smoke-data.

Parámetros dinámicos (`CombatConfig` / `CombatBalanceConfig`):

| Campo | Origen | Default | Efecto | Notas |
|-------|--------|---------|--------|-------|
| `CritScalingFactor` | CombatConfig | 0.65 | Fracción aplicada sobre la parte extra del crítico ((Mult-1)*F). | Reduce volatilidad del daño crítico. |
| `ReducePenetracionEnCritico` | CombatConfig | true | Si ON, aplica `FactorPenetracionCritico` a penetración antes de recalcular defensa. | Evita builds explosivas. |
| `FactorPenetracionCritico` | CombatConfig | 0.80 | Multiplicador sobre penetración en golpes críticos. | Ajustado vía sweep. |
| `UseCritDiminishingReturns` | CombatConfig | true | Activa curva DR sobre CritChance. | Usa fórmula en `CombatBalanceConfig.CritChanceWithDR`. |
| `CritChanceHardCap` | CombatConfig | 0.60 | Cap duro para DR (si DR activo). | Distinto al cap global de progression si se define. |
| `CritDiminishingK` | CombatConfig | 50 | Factor K para curva DR. | Mayor K = curva más lenta. |
| `ModoAcciones` | CombatConfig | false | Activa loop de PA. | Fase 1 aún no implementada en loop. |
| `CritMultiplier` | CombatConfig | 1.35 | Multiplicador base crítico (antes de scaling factor). | Puede ajustarse tras tuning. |
| `PrecisionMax` | progression.json (StatsCaps) | 0.95 | Clamp de precisión. | Cargado por `CombatBalanceConfig`. |
| `CritChanceMax` | progression.json (StatsCaps) | 0.50 | Clamp de crit chance pre DR. | Puede ser < HardCap DR. |
| `CritMultMin/Max` | progression.json (StatsCaps) | 1.25/1.75 | Rango permitido para crit mult derivado. | Se aplica sobre cálculo base. |
| `PenetracionMax` | progression.json (StatsCaps) | 0.25 | Clamp de penetración base. | Penetración crítica se calcula tras clamp. |

Notas:

- El pipeline nuevo aplica pasos en orden fijo documentado; cualquier ajuste debe reflejarse en esta tabla y la Bitácora.
- DR de crítico sólo se ejecuta cuando la feature flag correspondiente está activa.

### 6.3 Alias de rareza (transición Normal / Comun)

Contexto: algunos datos históricos emplean "Comun" mientras las configuraciones recientes y documentación usan "Normal" como rareza base. El sistema actual es case-insensitive y tolera ambas, pero para evitar divergencias:

Política temporal:

- Considerar "Comun" y "Normal" alias equivalentes hasta migración definitiva de data.
- Nuevos JSON deben emplear la forma estándar: `"Normal"` (o minúscula `normal` según convención general) en `rareza_pesos.json` y definiciones de ítems.
- Validadores registrarán advertencia si aparece "Comun" tras la migración.

Motivación: reducir ruido de balance y evitar multiplicación de entries de rareza en configuraciones.

6. Tras salir: procesar efectos, regen maná, turno enemigos.

Testing previsto:

- Caso feliz: PA=3 → 3 ataques físicos con seed determinista.
- Edge: acción en cooldown repetida no reduce PA y permite intentar otra.
- Flag OFF: snapshot daño total tras N turnos igual al legacy.

Fases posteriores (referencia):

- F2: iniciativa por Velocidad, costes variables, acciones defensivas y posicionamiento.
- F3: integrar Stamina/Poise y efectos de interrupción.

Impacto esperado: mayor expresividad táctica, micro‑decisiones por turno y base para estados avanzados sin reescribir pipeline de daño.

### 6.2 Diseño Técnico Detallado (Pre‑Implementación) — Sistema de Acciones Encadenadas y Pipeline de Daño Unificado (2025-10-01)

Esta sección fija el contrato técnico completo previo a codificar el loop PA, asegurando trazabilidad y minimizando ambigüedades. Todo lo descrito aquí es DATA / API target; el código legacy sigue activo hasta integrar gradualmente detrás de `CombatConfig.ModoAcciones`.

#### 6.2.1 Resumen Ejecutivo

Cada combatiente recibe PA (1..PAMax) por turno según atributos (Agilidad, Destreza, Nivel, equipo y efectos). Consume PA ejecutando acciones encadenadas (ataque, moverse, usar poción, observar, defensivas, sociales). El daño usa pipeline determinista (orden fijo) y admite reacciones inmediatas si hay slots de reacción disponibles. Todas las acciones y sus requisitos/costes se describen en JSON extendido; la IA podrá razonar solo con metadatos (sin hardcode de reglas específicas).

#### 6.2.2 Definiciones Básicas

- PA: puntos enteros gastables en un mini‑turno.
- Slot de Reacción: capacidad discreta de disparar una acción reactiva fuera del flujo secuencial (por defecto 1; escalable con atributos).
- Acción Macro: entrada que expande en varias acciones base (p.ej. `CorrerGolpear` → `[Correr, AtaqueFisico]`).
- Acciones Pasivas: evaluadas por triggers; no consumen PA directamente.
- Distancia Abstracta: {cuerpo, corto, medio, largo}; acciones de movimiento modifican la distancia actual actor↔objetivo.

#### 6.2.3 Cálculo de PA (Fórmula Implementable)

PA = clamp(BasePA + floor(Agi / AgilityDivisor) + floor(Dex / DexterityDivisor) + floor(Nivel / LevelDivisor) + Sum(BonusEquipo) + Sum(BuffsPA) - Sum(DebuffsPA), PAMin, PAMax)

Defaults actuales (`CombatConfig`): BasePA=2, PAMax=6, AgilityDivisor=30, DexterityDivisor=40, LevelDivisor=10, PAMin=1.
Ejemplo A (básico): Agi=20 Dex=15 Nivel=5 ⇒ 2 +0+0+0 =2.
Ejemplo B (rápido): Agi=70 Dex=55 Nivel=18 ⇒ 2 +2+1+1 =6 (tope).

Extensiones futuras: BonusEquipo (campo `BonusPA` en objetos), Buffs/Debuffs (efectos con interfaz `IEfectoPA`), gating de Stamina/Poise (no afecta PA base, pero limitará acciones pesadas repetidas).

#### 6.2.4 Pipeline de Daño (Orden Inmutable)

1) BaseDamage (tipo Físico/Mágico/Elemental)  
2) Hit / Evasión (si falla: daño=0, FueEvadido=true, fin)  
3) Penetración (DEF * (1-PEN))  
4) Resta de Defensa (min 1 si impacta)  
5) Mitigación porcentual (afterDef * (1-MIT))  
6) Crítico (afterMit * CritMultiplierEscalado)  
7) Vulnerabilidades/Elementos (afterCrit * VulnFactor)  
8) Redondeo (AwayFromZero) + Mínimo (≥1 si impactó)  

Fórmulas clave:  
defensaEfectiva = max(0, DEF *(1 - PEN))  
afterDef = max(1, DB - defensaEfectiva)  
afterMit = afterDef* (1 - MIT)  
final = round(afterMit *(isCrit ? CritMultEscalado : 1)* VulnFactor)  
final = max(1, final)

CritMultEscalado = 1 + (CritMultBase - 1) *CritScalingFactor (balance fino).  
Penetración en crítico puede reducirse: PENcrit = PEN* FactorPenetracionCritico (si `ReducePenetracionEnCritico`).

#### 6.2.5 Precisión / Evasión / Crítico

HitChance = clamp(BasePrecision + PrecisionStats - EvasionObjetivo, MinHit, 1.0).  
BasePrecision recomendada 0.90; MinHit 0.05.  
CritChance = clamp(CritStats + CritBuffs, 0, CritCap).  
Curva DR opcional: chanceDR = stat / (stat + K) con cap superior.

#### 6.2.6 Penetración y Mitigación

PEN sumada de fuentes y clamp [0, PenetracionMax]. Aplica antes de resta de defensa. MIT aplica después de resta y antes de crítico. Vulnerabilidades multiplican después del crítico para consistencia (crítico amplifica la porción mitigada base y luego se aplica la susceptibilidad).

#### 6.2.7 Reacciones Inmediatas

ReactionSlots = 1 + floor((Destreza + Agilidad)/100).  
Uso: durante ejecución de una acción que marque ventana de interrupción (`ventanaInterrupcion.frames > 0`), un defensor puede disparar acción reactiva (ej. `ContraataqueRapido`) si tiene slot libre. Fase 1: modelado de slots; la ejecución real se activará en Fase 2.

#### 6.2.8 Esquema Extendido `acciones_catalogo.json`

Campos nuevos (todos opcionales para compatibilidad):
```
{
  "Id": "Correr",
  "Descripcion": "Moverse rápidamente.",
  "costePA": 1,
  "tipoAccion": "movimiento",
  "rangoDistancia": ["medio","largo"],
  "cambiaDistanciaA": "corto",
  "requisitos": { "estadosActor": ["Enfocado"], "distanciaActual": ["medio","largo"] },
  "estadosAplica": { "self": ["Impulso"], "objetivo": [] },
  "macros": ["AtaqueFisico"],
  "reacciones": [ { "id": "Retroceder", "probabilidad": 0.5 } ],
  "personalidadPesos": { "agresivo": 0.4, "defensivo": 0.3, "astuto": 0.3 },
  "pasiva": false,
  "interruptible": true,
  "ventanaInterrupcion": { "frames": 1, "reaccionSugerida": ["ContraataqueRapido"] },
  "moralImpacto": { "deltaObjetivo": -2, "siFalla": 1 },
  "meta": { "descripcion": "Acorta distancia 1 paso" }
}
```

Interpretación: Campos ausentes = defaults neutros (p.ej. costePA=1, cualquier distancia, sin reacciones, sin cambios de estado, no pasiva).

#### 6.2.9 Pseudocódigo de Referencia (Consolidado)

ComputePA, DamageCalculator y bucle PA se documentaron con ejemplos numéricos en §6.2.3/6.2.4. Bucle PA final:

```
pa = ComputePA(pj,cfg)
while (pa > 0 && enemigosVivos) {
  MostrarAccionesFiltradas(pa, estados, distancia)
  accion = Seleccionar()
  if (!Valida(accion)) continue
  if (accion.EsMacro) Expandir(accion)
  EjecutarAccion(accion)
  if (accion.ConsumePA) pa -= accion.CostePA
  ProcesarReaccionesInmediatas()
}
AvanzarCooldowns(pj); RegenerarMana(pj); TickEfectos()
```

#### 6.2.10 Tests Sugeridos (Resumen Ejecutable)

1. PA_Calculo_BasicoYRapido (valores de ejemplo).  
2. Daño_Pipeline_Secuencia (verifica pasos intermedios con seed).  
3. Critico_Escalado_Factor (comprueba CritScalingFactor).  
4. Macro_Expansion (CorrerGolpear produce dos acciones).  
5. Reaccion_SlotConsumido (placeholder futuro).  
6. Accion_Coste_NoConsumeSiCooldown (fail gating).  
7. Distancia_Transicion (Correr modifica distancia).  
8. Fallback_CamposAusentes (acciones sin costePA usan 1).  
9. Mitigacion_Orden (Penetración antes de defensa; MIT antes de crítico).  
10. Vulnerabilidad_PostCritico (aplicación final).  

#### 6.2.11 Balance Inicial (Parámetros Recomendados)

BasePA=2 PAMax=6 CritMultiplierBase=1.5 CritScalingFactor=0.65 CritCap=0.50 PenetracionMax=0.9 MinHit=0.05. Distancias: default inicial = "corto". Costes PA Fase 1: ataque=2, poción=2, moverse=1, observar=1, habilidad ligera=2, habilidad pesada=3 (placeholder; se materializará al introducir tabla coste). Reaccionar (contraataque rápido)=1 (PA reacción separado en fases futuras o utiliza slot).

#### 6.2.12 Riesgos y Mitigaciones

- Complejidad JSON → Validadores y defaults; warnings no fatales.
- Explosión combinatoria IA → Iniciar con pesos estáticos personalidad + random sesgado.
- Narrativa ruidosa → Log condensado por mini‑turno antes de imprimir.
- Desbalance crítico/penetración → Benchmarks (shadow) ya instalados; repetir tras integrar PA.

#### 6.2.13 Estrategia de Integración Incremental

1) Implementar soporte `CostoPA` mínimo y loop PA solo jugador (flag).  
2) Añadir diccionario temporal de costes (sin tocar JSON aún).  
3) Introducir macro expansión simple.  
4) Agregar tracking de distancia (enum + campo en contexto de combate).  
5) Activar reacciones (placeholder: contador slot).  
6) Migrar costes al JSON extendido.  
7) IA multi‑acción y pesos de personalidad.

#### 6.2.14 Criterios de “Listo para Fase 2”

- Loop PA ON produce mismos resultados de daño promedio (±5%) que legacy en benchmark de 100 turnos (seed fija).  
- Test suite §6.2.10 en verde.  
- Documentación sincronizada (Bitácora + Roadmap).  
- Flag OFF: comportamiento idéntico al anterior (prueba regresión).  

Última actualización: 2025-10-01.

## 6. Recolección y mundo

- Biomas con nodos (rareza, producción min/max, cooldowns).
- Encuentros aleatorios (Chance/Prioridad/Cooldown) persistidos.
- Exploración alimenta progresión y rutas.

### 5.1 Coste de Energía (EnergiaService)

Archivo de configuración: [`DatosJuego/energia.json`](../DatosJuego/energia.json):

- Costo = `Base_tipo · (1 + ModHerramienta + ModBioma + ModAtributo + ModClase)`
- Reducción por atributo relevante si supera `UmbralAtributo` (25 por defecto) hasta `FactorReduccionAtributo` (0.4) con tope 5× umbral.
- `ModClase`: suma bonificadores de clases (`Energia.ModClase`, `Energia.ModAccion.<Tipo>`).
- Clamps: `CostoMinimo`/`CostoMaximo` (3/25).
- Energía: máx 100; +1 cada 10 min; posada recupera % decreciente por descanso en el día.

## 7. Objetos, inventario y comercio

Última actualización: 2025-09-22

- Tipos: armas, armaduras, pociones, materiales; gestores en migración a repos JSON.
- Inventario: `IInventariable`/`IUsable`; consumo en combate vía `IAccionCombate`.
- Tienda: precios afectan reputación; reglas por facción.

## 8. Misiones y encuentros

- Misiones: requisitos/recompensas (plan a `IRequisito`/`IRecompensa`).
- `EncuentrosService`: gating por kills, hora, chance, prioridad, cooldown persistente.

### 7.1 Encuentros (detalles exactos)

Fuente: [`Motor/Servicios/EncuentrosService.cs`](../Motor/Servicios/EncuentrosService.cs) + [`DatosJuego/eventos/encuentros.json`](../DatosJuego/eventos/encuentros.json).

- Filtros: `MinNivel`, `MinKills`, ventana `HoraMin/HoraMax` (gestiona medianoche), `CooldownMinutos` con clave `bioma|tipo|param`.
- Entradas con `Chance`: primero RNG < Chance; desempate por `Prioridad` y luego `Peso`.
- Fallback ponderado: selección por `Peso` entre entradas sin `Chance` post-filtros.
- Mods por atributos (`CalcularModificador`):
  - Botín/Materiales: + hasta 50% con Percepción+Suerte.
  - NPC/Eventos/Mazmorras raras: + hasta 25% con Suerte.
  - Combates comunes/bioma: + hasta 30% con Agilidad+Destreza.
  - MiniJefe: requiere `MinKills`; bonus por kills extra y Suerte (máx +50%).
- Cooldowns consultables/limpiables; expone estado con minutos restantes.

## 9. Supervivencia

- Config: [`DatosJuego/config/supervivencia.json`](../DatosJuego/config/supervivencia.json); servicio [`SupervivenciaService`](../Motor/Servicios/SupervivenciaService.cs) y runtime asociado.
- Penalizaciones actuales: `FactorEvasion` (jugador) y `FactorRegen` (maná); `FactorPrecision` listo para el paso 1 del pipeline de combate.

### 8.1 Config y runtime

- Tasas: `HambrePorHora`, `SedPorHora`, `FatigaPorHora`, `TempRecuperacionPorHora`.
- Multiplicadores por contexto: `Explorar`, `Viajar`, `Entrenar`, `Combate`, `Descanso`.
- Umbrales: `OK/ADVERTENCIA/CRÍTICO` para H/S/F; temperatura con `Frio/Calor` (advertencia/crítico) y estados (FRÍO, CALOR, HIPOTERMIA, GOLPE DE CALOR).
- Reglas por bioma: `TempDia`, `TempNoche`, `SedMultiplier`, `HambreMultiplier`, `FatigaMultiplier`.
- Penalizaciones por umbral: factores `Precision`, `Evasion`, `ManaRegen` y `ReduccionAtributos` (mapa atributo→% negativo).
- Runtime: por minuto, incrementa H/S/F con multiplicadores; ajusta `TempActual`; emite eventos al cruzar umbrales.
- Integraciones: `ActionRulesService` reduce regen de maná; `Personaje.IntentarEvadir` aplica `FactorEvasion`.

## 10. UI y presentación

- `IUserInterface`: desacopla vista; implementaciones de consola y silenciosa (tests).
- `UIStyle`: estilo de encabezados y etiquetas de reputación/supervivencia.

### 9.1 Estado del personaje (layout actual)

- Implementación: `Motor/EstadoPersonajePrinter.cs` usa `UIStyle` para renderizar un layout profesional y compacto, con opción de modo detallado.
- Secciones:
  - Resumen superior: `Nombre — Clase • Nivel • Título` y tiempo de mundo.
  - Barras: Vida/Maná/Energía con porcentaje y segmentos; barra de XP con “faltante” al siguiente nivel.
  - Atributos: compactos en línea, mostrando bonos agregados por equipo/clases (por ejemplo: `FUE 10 (+2)`).
  - Estadísticas clave: Defensa/Precisión/Crítico/Penetración/Velocidad.
  - Supervivencia: indicador compacto (Hambre/Sed/Fatiga/Temperatura) con etiquetas por umbral (OK/ADVERTENCIA/CRÍTICO).
  - Modo detallado (opcional): sección "Equipo" con slots (Arma, Casco, Armadura, Pantalón, Zapatos, Collar, Cinturón, Accesorio 1/2). Para cada pieza muestra nombre y stats clave (Rareza/Perfección; y en armas, Daño Físico/Mágico). Se activa llamando `EstadoPersonajePrinter.MostrarEstadoPersonaje(pj, true)` o desde el `Menú Fijo` (opción "Estado (detallado)").
- Decisión de diseño: por defecto, la vista compacta evita listados largos de equipo para priorizar legibilidad. El detalle de equipo está disponible bajo demanda a través del modo detallado o el Inventario.

### 9.3 Clases dinámicas: flujo en Admin

- Carga de definiciones: `MenuAdmin` fuerza `ClaseDinamicaService.Cargar()` antes de listar/forzar para garantizar que el campo interno `defs` esté poblado, evitando listas vacías.
- Forzar clase: al seleccionar una clase se aplican bonos iniciales y se reevalúan cadenas; si ya estaba desbloqueada, se ofrecen dos opciones:
  - Retomar como ACTIVA (no se suman bonos nuevamente). Esto sólo actualiza `pj.Clase` y recalcula estadísticas preservando el ratio de maná actual.
  - Reaplicar bonos iniciales (acumulativo) con confirmación explícita. Tras aplicar, opcionalmente se puede marcar como ACTIVA. En ambos casos, se recalculan estadísticas conservando el porcentaje de maná.
- Presentación: el listado de “Forzar clase” muestra el estado junto a cada nombre: `[ACTIVA]`, `[DESBLOQUEADA]`, `[DISPONIBLE]` o `[BLOQUEADA]` (esta última con motivos en el listado de diagnóstico).

Nota operativa (clase activa vs desbloqueo):

- Desbloquear una clase aplica sus bonos iniciales (acumulativos) y añade la clase al conjunto `ClasesDesbloqueadas`.
- La “clase activa” es una etiqueta operativa (`pj.Clase.Nombre`) que puede alternarse sin rebonificar. Usos actuales: gating ligero de UI y futuras reglas de coste/energía. Se puede cambiar desde MenuAdmin opción 21.
- UX de carga: si un guardado trae clases desbloqueadas pero `Clase == null`, el juego auto-activa la primera por orden alfabético y recalcula estadísticas preservando el ratio de maná. No se aplican bonos adicionales.

### 9.2 Gating de menús por sector (Ciudad vs Fuera de Ciudad)

- Lógica en `Motor/Juego.cs` (`MostrarMenuPorUbicacion`): el menú de ciudad se muestra solo si `SectorData.Tipo == "Ciudad"` y además `EsCentroCiudad` o `CiudadPrincipal` son verdaderos. Para cualquier otra parte de una ciudad (`ParteCiudad`), se usa el menú de “Fuera de Ciudad”.
- Justificación: evita mostrar opciones de ciudad completa en entradas/periferias; alinea UX con exploración por zonas.
- Soporte de datos: `PjDatos/SectorData.cs` define `Tipo` con valor por defecto `"Ruta"`, previniendo clasificaciones incorrectas cuando el JSON omite el campo.

## 11. Datos, validación y guardado

- `PathProvider` centraliza rutas.
- `DataValidatorService`: valida mapa, misiones, NPCs, enemigos; objetos pendiente.
- `GuardadoService`: persiste drops únicos y cooldowns de encuentros; sustituye I/O ad-hoc.

## 12. Testing y determinismo

- xUnit; `RandomService.SetSeed` para reproducibilidad.
- `SilentUserInterface` para evitar bloqueos por input.
- Proveedores inyectables (hora, paths) para desacoplar entorno.

## 13. Migración a Unity

- Mantener dominio puro (independiente de consola/UI).
- Adaptadores de UI/Logger/Input en capa presentación.
- Convertir JSON a ScriptableObjects; usar adapters para `IUserInterface`.

## 14. Problemas conocidos y edge cases

- Doble evasión: consolidar en resolver.
- Contratos JSON: inconsistencias históricas en objetos; mitigado por validadores y herramientas.
- Balance conservador: evitar “power spikes”; progresión intencionalmente lenta.

## 15. Ejemplos prácticos

### 14.1 Secuencia de ataque físico (actual)

1) Usuario elige “Ataque Físico”.
2) `AtaqueFisico` → `DamageResolver.ResolverAtaqueFisico`.
3) `AtacarFisico` calcula daño y chequea evasión del objetivo; 0 si evade.
4) `DamageResolver` marca `FueEvadido` si daño==0, evalúa posible `FueCritico`, compone mensajes.
5) `CombatePorTurnos` muestra y procesa efectos.

### 14.2 Fórmula de impacto (futura)

$p_{hit} = clamp(0.35 + Precision - 1.0·Evasion,\ 0.20,\ 0.95)$; hit si RNG < $p_{hit}$; si miss: `DanioReal=0`, `FueEvadido=true`.

### 14.3 Progresión lenta: ejemplo

Jugador con baja `Precision` vs lobo ágil: esperar más fallos; estrategia: entrenar Agilidad/Percepción o equipar bonus de Precision antes de adentrarse.

## 16. Apéndice de contratos (interfaces y DTOs)

Resumen de firmas públicas previstas (se omiten namespaces para brevedad):

```csharp
public interface ICombatiente {
          int Vida { get; }
          int VidaMaxima { get; }
          int Mana { get; }
          int ManaMaximo { get; }
          double DefensaFisica { get; }
          double DefensaMagica { get; }
          int AtacarFisico(ICombatiente objetivo);
          int AtacarMagico(ICombatiente objetivo);
          void RecibirDanioFisico(int danio);
          void RecibirDanioMagico(int danio);
}

public interface IEvadible {
          bool IntentarEvadir(bool esMagico);
}

public interface IAccionCombate {
          ResultadoAccion Ejecutar(CombateContext ctx);
          int CooldownTurnos { get; }
          int CostoMana { get; }
}

public sealed class ResultadoAccion {
          public string Mensaje { get; set; }
          public int DanioReal { get; set; }
          public bool FueCritico { get; set; }
          public bool FueEvadido { get; set; }
          public bool ObjetivoDerrotado { get; set; }
}
```

Nota: las firmas exactas pueden variar en el código; este apéndice busca fijar la intención y el contrato lógico que guía las pruebas y la migración a Unity.

---

Anexo vivo: cualquier cambio en código o balance debe anotarse aquí y en `Roadmap.md` (Bitácora + sección relevante) para mantener sincronización entre equipos/PCs.
