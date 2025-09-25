### Rarezas y probabilidades de aparición

El sistema de equipo permite definir rarezas adicionales (por ejemplo, `Epica`) siempre que:

- Se agregue el valor al enum `Rareza` en el código.
- Se incluya en los archivos de configuración (`rareza_pesos.json`, `rareza_perfeccion.json`).
- Se use de forma consistente en los datos de objetos.

Las probabilidades de aparición pueden ser decimales (por ejemplo, `"Ornamentada": 0.1`), lo que permite controlar la rareza de aparición de forma precisa.

Validado en build y pruebas (2025-09-24).
Última actualización: 2025-09-24
### Convención de rarezas para equipo (armas.json)

Todos los valores de rareza en los archivos de equipo deben coincidir exactamente con el enum `Rareza` definido en `EnumsObjetos.cs`:

```
public enum Rareza { Rota, Pobre, Normal, Superior, Rara, Legendaria, Ornamentada }
```

Mapeo aplicado (2025-09-24):

- `Comun` → `Normal`
- `PocoComun` → `Superior`
- `Raro` → `Rara`
- `Epico` → `Ornamentada`
- `Legendario`/`Legendaria` → `Legendaria`

Esto es obligatorio para evitar errores de deserialización y garantizar la integridad de datos en combate y generación de enemigos.
Última actualización: 2025-09-24
#### 2025-09-23: Robustez en menú admin (clases)

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

## Validación de enums en archivos de datos

Todos los archivos JSON que contienen campos de tipo enum (por ejemplo, `Rareza` en materiales, armas, objetos) deben usar exactamente los mismos valores definidos en los enums de C#.

**Ejemplo:**

- Correcto: `"Rareza": "Comun"`
- Incorrecto: `"Rareza": "Normal"` (no existe en el enum)

Se recomienda validar los datos antes de cargar y mantener tests que verifiquen la correspondencia entre los datos y los enums del código.

Última actualización: 2025-09-23

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
1. Recolección y mundo
1. Objetos, inventario y comercio
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

Todos los materiales referenciados como drops de enemigos cuentan ahora con archivos `.json` individuales en la subcarpeta correspondiente (ejemplo: `Mat_Cocina`).

Esto permite:

- Integración directa con el sistema de crafteo, cocina y progresión.
- Validación y QA automáticos desde el loader, evitando referencias huérfanas.
- Escalabilidad para añadir nuevos materiales, recetas y drops sin modificar el código fuente.
- Trazabilidad y documentación completa de cada material, su rareza, origen y usos.

Esta modularidad es clave para la futura migración a Unity y para mantener la progresión lenta y desafiante definida en `progression_config.md`.

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
