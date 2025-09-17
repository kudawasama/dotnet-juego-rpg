# MiJuegoRPG — Arquitectura y Funcionamiento (Estudio Detallado)

> Objetivo: documentar en profundidad la estructura, el flujo y las reglas del juego para facilitar mantenimiento, onboarding y futura migración a Unity. Este documento sirve como guía viva y complementa `Roadmap.md` y `progression_config.md`.

Documentos relacionados

- Roadmap (plan y estado): `./Roadmap.md`
- Bitácora (historial): `./Bitacora.md`
- Config de progresión: `./progression_config.md`

Tabla de contenidos

1. Visión general del sistema
1. Núcleo del dominio
1. Progresión y atributos
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

---

## 1. Visión general del sistema

El proyecto está organizado por capas y módulos, con un enfoque data-driven. Las piezas principales son:

- Dominio (lógica del juego): Personaje, Enemigos, Combate, Progresión, Recolección.
- Servicios: RandomService, ProgressionService, ReputacionService, EnergiaService, GuardadoService, etc.
- UI: IUserInterface y adaptadores (consola/silencioso), UIStyle para consistencia visual.
- Datos: JSONs en `DatosJuego/` y modelos en `PjDatos/`.
- Herramientas/QA: validadores, generadores y reparadores.

Diagrama conceptual simplificado:
Jugador/Enemigo -> Acciones -> DamageResolver -> ResultadoAccion -> UI
            ^                                     |
            |                                     v
       ProgressionService <--- Eventos ---- GuardadoService

Metas clave:

- Progresión lenta y desafiante (balance conservador).
- Modularidad y futura migración a Unity.
- Determinismo en pruebas (RandomService.SetSeed).

## 2. Núcleo del dominio

Entidades principales:

- Personaje: atributos, estadísticas derivadas, inventario, reputaciones, clases dinámicas; implementa ICombatiente e IEvadible.
- Enemigo: base de comportamiento similar a Personaje pero con mitigaciones, inmunidades y drops.
- Acciones de combate: AtaqueFísico, AtaqueMágico, UsarPoción, AplicarEfectos; exponen `IAccionCombate.Ejecutar` y devuelven `ResultadoAccion`.
- Efectos: `IEfecto` con ticks por turno y expiración (veneno implementado).

Interfaces clave (contratos):

- ICombatiente: AtacarFisico/Magico, RecibirDanioFisico/Magico, Vida/VidaMaxima/Defensas.
- IEvadible: IntentarEvadir(bool esMagico).
- IAccionCombate: Ejecutar, CooldownTurnos, costes.

## 3. Progresión y atributos

Configurable vía `progression_config.md`/JSON. Atributos base (Fuerza, Inteligencia, etc.) y estadísticas derivadas (Ataque, PoderMagico, DefensaMagica, Precision, CritChance, CritMult, Penetracion).

Fórmula general de experiencia por atributo (ejemplo orientativo):

- Exp otorgada = Base + Factor * contexto (recolección/entrenamiento/exploración), afectada por caps y factor mínimo.
- Subida de nivel por atributo: costo creciente no lineal, ver `progression_config.md`.

Ejemplo (pseudo):

- EXP(Fuerza) += max(minExp, f(recolección))
- Nivel(Fuerza) up si EXP >= costo(nivelActual)

### 3.1 Atributos base (del código)

- Fuerza, Destreza, Vitalidad, Agilidad, Suerte, Defensa, Resistencia, Sabiduría, Inteligencia, Fe, Percepcion, Persuasion, Liderazgo, Carisma, Voluntad, Oscuridad.

### 3.2 Estadísticas derivadas (fórmulas actuales)

Las siguientes relaciones provienen del constructor de `Estadisticas(AtributosBase)` y están sujetas a tuning conservador:

- Salud: Salud = 10·Vitalidad
- Maná: suma ponderada de múltiples atributos (Int, Sab, Fe, Vol, Carisma, Liderazgo, Vitalidad, Fuerza, Destreza, Agilidad, Suerte, Defensa, Resistencia, Percepcion, Persuasion)
- Energía: Energia = 10·Agilidad
- Ataque (físico base): Ataque = 0.01·(Fuerza + Destreza)
- Defensa Física: DefensaFisica = 0.01·(Defensa + Vitalidad)
- Poder Mágico: PoderMagico = 0.01·(Inteligencia + Sabiduría)
- Defensa Mágica: DefensaMagica = 0.01·(Resistencia + Sabiduría)
- Regeneración de Maná: RegeneracionMana = 0.01·Inteligencia
- Evasión: Evasion = 0.01·(Agilidad + Suerte)
- Crítico (alias CritChance): Critico = 0.01·(Destreza + Suerte)
- Precisión: Precision = clamp(0.01·Destreza + 0.005·Percepcion, 0, 0.95)
- Mult. crítico: CritMult = clamp(1.5 + 0.001·Sabiduría, -, 2.0)
- Penetración: Penetracion = clamp(0.002·Destreza, 0, 0.2)

Notas:

- El jugador tiene un cap de evasión efectiva en `IntentarEvadir` de 0.5 (antes de RNG), con penalización 0.8 para hechizos.
- Estas estadísticas se verán afectadas por equipo vía `ObtenerBonificadorAtributo/Estadistica` y por estados de supervivencia (ver sección 8).

## 4. Combate (pipeline y estados)

Estado actual:

- `DamageResolver` centraliza metadatos: registra crítico (`FueCritico`) y evasión (`FueEvadido`) y construye mensajería de acción sin alterar aún las fórmulas.
- Chequeos de evasión existen en `AtacarFisico/Magico` (combatientes) y al recibir daño en `Personaje`.
- `CombatePorTurnos` orquesta turno, selección de acciones, aplicación de efectos y UI.

Plan de pipeline (orden propuesto):

1) Hit/Evasión: Precision atacante vs Evasion objetivo (penalizable por Supervivencia).
2) Crítico: chance y multiplicador, caps/floors conservadores.
3) Defensa/Penetración: reducir defensa por penetración antes de mitigar.
4) Mitigaciones del objetivo: físicas/mágicas por porcentaje.
5) Resistencias especiales (elementales, inmunidades) y debilidades (vulnerabilidades):
     - ResistenciasElementales { tipo: 0..0.9 } se aplican como mitigación adicional post-defensa.
     - VulnerabilidadesElementales { tipo: 1.0..1.5 } se aplican como multiplicador post-mitigación. Implementado inicialmente para el canal genérico "magia".
6) Aplicación del daño y efectos OnHit/OnKill.
7) Registro en `ResultadoAccion` y UI.

Ecuación base (futura) para probabilidad de impacto:

- p_hit = clamp(0.35 + Precision_att - k * Evasion_obj, 0.20, 0.95), k ∈ [1.0, 1.2]
- Aplicar multiplicador de Supervivencia: p_hit *= FactorPrecision(hambre, sed, fatiga)

Crítico (futuro, conservador):

- Si RNG < CritChance: daño *= CritMult; cap CritChance <= 0.5; CritMult ∈ [1.25, 1.75].

Edge cases relevantes:

- Daño mínimo = 1 (si impacta y tras mitigaciones > 0), salvo inmunidad.
- Evasión duplicada (atacante y objetivo): resolver una sola vez en el pipeline.
- Overkill: clamp vida a 0; marcar `ObjetivoDerrotado`.

Estado actual (del código):

- `Personaje.AtacarFisico/Magico` realiza un chequeo de evasión en el objetivo (si implementa `IEvadible`), y retorna 0 si evade.
- `Personaje.RecibirDanioFisico/Magico` también realiza un chequeo de evasión adicional; estamos migrando a una única decisión centralizada en el resolver.
- `DamageResolver` anota `FueEvadido` si el daño resultó 0 y añade mensaje; anota `FueCritico` según `Estadisticas.Critico` (mientras se consolida el pipeline).
- Caps visibles: `Precision <= 0.95`, `CritMult <= 2.0`, `Penetracion <= 0.2`, `Evasion jugador <= 0.5`.

## 5. Recolección y mundo

- Tablas por bioma; nodos con rareza y producción min/max. Cooldowns por nodo.
- Encuentros aleatorios con Chance/Prioridad y Cooldown por evento (persistidos).
- Progresión por exploración integrada en rutas.

### 5.1 Coste de Energía (del servicio EnergiaService)

Archivo de config: `DatosJuego/energia.json`.

- Cálculo general: Costo = Base_tipo · (1 + ModHerramienta + ModBioma + ModAtributo + ModClase)
- Reducción por atributo relevante: si el atributo mapeado para el tipo supera `UmbralAtributo` (default 25), aplica reducción lineal hasta `FactorReduccionAtributo` (default 0.4) con máximo en exceso 5× umbral.
- ModClase: suma de modificadores por nombre de clase y por bonificadores dinámicos de clases emergentes (`Energia.ModClase`, `Energia.ModAccion.<Tipo>`).
- Clamps: `CostoMinimo`/`CostoMaximo` (defaults 3/25).
- Inicialización: energía máxima por defecto 100; recuperación pasiva +1 cada 10 min; descanso en posada recupera un porcentaje decreciente según descansos diarios.

## 6. Objetos, inventario y comercio

- Objetos: armas, armaduras, pociones, materiales; gestión por Gestores (migrando a repos JSON).
- Inventario con `IInventariable`/`IUsable`; consumo en combate vía `IAccionCombate`.
- Tienda: precios afectan reputación (y viceversa), políticas por facción.

## 7. Misiones y encuentros

- Misiones: requisitos y recompensas (plan de refactor a IRequisito/IRecompensa).
- EncuentrosService: gating por kills, hora, chance, prioridad, cooldown persistente.

### 7.1 Encuentros (detalles exactos)

Fuente: `Motor/Servicios/EncuentrosService.cs` y `DatosJuego/eventos/encuentros.json`.

- Filtros previos: `MinNivel`, `MinKills` (usa contadores reales del jugador), ventana horaria `HoraMin/HoraMax` (maneja cruce de medianoche), y cooldown por entrada (`CooldownMinutos`) con persistencia por clave `bioma|tipo|param`.
- Entradas con `Chance` se evalúan primero (RNG < Chance), desempate por `Prioridad` y luego `Peso`.
- Fallback ponderado: selección por peso entre entradas sin `Chance` (luego de filtros).
- Modificadores por atributos (método `CalcularModificador`):

- Botín/Materiales: + hasta 50% con Percepción+Suerte.
- NPC/Eventos/Mazmorras raras: + hasta 25% con Suerte.
- Combates comunes/bioma: + hasta 30% con Agilidad+Destreza.
- MiniJefe: requiere `MinKills`; bonus adicional por kills extra y Suerte (máx +50%).
- Cooldowns: pueden consultarse y limpiarse; el servicio expone un estado con minutos restantes por clave.

## 8. Supervivencia

- Config en `DatosJuego/config/supervivencia.json` y servicio `SupervivenciaService`.
- Penalizaciones aplicadas: `FactorEvasion` (jugador) y `FactorRegen` (maná); `FactorPrecision` listo para usarse cuando el pipeline integre hit.

### 8.1 Estructura de configuración y runtime

- Tasas: `HambrePorHora`, `SedPorHora`, `FatigaPorHora`, `TempRecuperacionPorHora`.
- Multiplicadores por contexto: `Explorar`, `Viajar`, `Entrenar`, `Combate`, `Descanso` (claves libres). Cada uno ajusta H/S/F.
- Umbrales: etiquetas `OK/ADVERTENCIA/CRÍTICO` por H/S/F; temperatura con `Frio/Calor` (advertencia y crítico) y estados derivados (FRÍO, CALOR, HIPOTERMIA, GOLPE DE CALOR).
- Reglas por bioma: `TempDia`, `TempNoche`, `SedMultiplier`, `HambreMultiplier`, `FatigaMultiplier`.
- Penalizaciones por umbral (sección `Penalizaciones`):

- Niveles `Advertencia` y `Critico` con campos como `Precision`, `Evasion`, `ManaRegen` (factores sumados a 1.0) y `ReduccionAtributos` (mapa atributo→porcentaje negativo).
- Runtime (`SupervivenciaRuntimeService.ApplyTick`): incrementa H/S/F por minuto simulado con multiplicadores; ajusta `TempActual` hacia el objetivo por bioma; publica eventos ante cruces de umbral.
- Integraciones actuales: `ActionRulesService` penaliza regeneración de maná; `Personaje.IntentarEvadir` aplica `FactorEvasion`. Listo `FactorPrecision` para el paso de Hit del pipeline.

## 9. UI y presentación

- `IUserInterface` desacopla UI; `ConsoleUserInterface` y `SilentUserInterface` (tests).
- `UIStyle`: encabezados/subtítulos, etiquetas de reputación y estado de supervivencia.

## 10. Datos, validación y guardado

- `PathProvider` centraliza rutas.
- Validación progresiva con `DataValidatorService` (mapa, misiones, NPCs, enemigos); pendiente objetos.
- Guardado: `GuardadoService` reemplaza I/O directa; persistencia de drops únicos y cooldowns de encuentros.

## 11. Testing y determinismo

- xUnit; `RandomService.SetSeed` para reproducibilidad.
- `SilentUserInterface` evita bloqueos.
- Proveedores inyectables (hora, paths) para aislar entorno.

## 12. Migración a Unity

- Mantener dominio puro (sin dependencias de consola/UI).
- Adaptadores de UI/Logger/Input en capa presentación.
- Conversión de JSON a ScriptableObjects como pipeline de datos.

## 13. Problemas conocidos y edge cases

- Doble evasión (ataque y recepción): en transición; se anotará una sola vez en el pipeline.
- Contratos JSON: inconsistencias históricas en objetos; mitigado por validadores y herramientas de reparación.
- Balance inicial conservador y desafiante: evitar saltos de poder; progresión lenta intencional.

## 14. Ejemplos prácticos

### 14.1. Secuencia de ataque físico (estado actual)

1) Usuario elige “Ataque Físico”.
2) `AtaqueFisicoAccion` → `DamageResolver.ResolverAtaqueFisico`.
3) `AtacarFisico` del ejecutor calcula daño y aplica evasión del objetivo; retorna daño (0 si evadido).
4) `DamageResolver` anota `FueEvadido` si daño==0, posible `FueCritico`, y construye mensajes.
5) `CombatePorTurnos` muestra resultado y procesa efectos.

### 14.2. Fórmula propuesta de impacto (futura)

- p_hit = clamp(0.35 + Precision_att - 1.0 * Evasion_obj, 0.20, 0.95)
- hit si RNG < p_hit; si miss: `DanioReal=0`, `FueEvadido=true`, mensaje “falló”.

### 14.3. Escenario de progresión lenta

- Un jugador con baja Precision contra un lobo ágil: espera más fallos; la mejor estrategia es entrenar Agilidad/Percepción o equipar bonus de Precision antes de adentrarse en zonas peligrosas.

---

Anexo vivo: este documento refleja el estado actual y el plan inmediato; cualquier cambio en código o balance debe anotarse aquí y en `Roadmap.md` (Bitácora + sección relevante) para mantener sincronización entre equipos/PCs.

### 15. Clases dinámicas (resumen)

Fuente: `Motor/Servicios/ClaseDinamicaService.cs` + `DatosJuego/clases_dinamicas.json`.

- Evaluación centralizada de requisitos “duros”: clases previas/tier, exclusiones, nivel mínimo, atributos/estadísticas mínimas, actividad (contadores), reputación global y por facción, misiones (múltiples/única), objetos únicos.
- Desbloqueo emergente opcional por score parcial (`PesoEmergenteMin`) basado en porcentaje de cumplimiento de requisitos suaves (actividad/atributos).
- Al desbloquear: aplicación automática de `AtributosGanados` con clamp a ≥0.
- Bonificadores por clase: mapa libre (`Bonificadores`) con claves como `Energia.ModAccion.Minar`, `Atributo.Fuerza`, `XP.Combate.Mult`, etc., consultables por `EnergiaService` y otros.
- Dataset actual incluye ramas de Oficio (Minero/Leñador/Pescador/Mercader/Herrero/Alquimista/Recolector), Combate (Aprendiz/Guerrero/Berserker), Magia (Aprendiz/Mago/Hechicero/Clérigo/Nigromante/Archimago), Híbridas (Druida/Paladín/Espadachín Mágico), y Divinas (Avatar Divino/Templario Sagrado). Requisitos exigentes y progresión lenta alineados al diseño del juego.
