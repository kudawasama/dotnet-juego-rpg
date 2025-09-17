# MiJuegoRPG — Arquitectura y Funcionamiento (Estudio Detallado)

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

Organización por capas con enfoque data-driven. Piezas principales (enlaces a implementación real):

- Dominio (lógica del juego): Personaje, Enemigos, Combate, Progresión, Recolección.
- Servicios: [`RandomService`](../Motor/Servicios/RandomService.cs), [`ProgressionService`](../Motor/Servicios/ProgressionService.cs), `ReputacionService` (en preparación), [`EnergiaService`](../Motor/EnergiaService.cs), `GuardadoService` (módulo actual), [`SupervivenciaService`](../Motor/Servicios/SupervivenciaService.cs).
- UI: [`IUserInterface`](../Interfaces/IUserInterface.cs) y adaptadores (consola/silencioso, p. ej. [`ConsoleUserInterface`](../Motor/Servicios/ConsoleUserInterface.cs)); `UIStyle` para consistencia visual.
- Datos: JSONs en `MiJuegoRPG/DatosJuego/` y modelos persistidos en `PjDatos/`.
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
- Precision = `clamp(0.01·Destreza + 0.005·Percepcion, 0, 0.95)`
- CritMult = `clamp(1.5 + 0.001·Sabiduría, -, 2.0)`
- Penetracion = `clamp(0.002·Destreza, 0, 0.2)`

Notas: cap de evasión efectiva en `IntentarEvadir` = 0.5 (previo a RNG), penalización 0.8 para hechizos. Equipo y supervivencia modifican estas cifras.

## 4. Combate (pipeline y estados)

Estado actual:

- [`DamageResolver`](../Motor/Servicios/DamageResolver.cs) registra `FueCritico`/`FueEvadido` y compone mensajes.
- Evasión se chequea en `Atacar*` y en `RecibirDanio*` (en transición a resolver único).
- [`CombatePorTurnos`](../Motor/CombatePorTurnos.cs) orquesta turnos, acciones, efectos y UI.

Orden de pipeline propuesto (futuro inmediato):

1) Hit/Evasión: $p_{hit} = clamp(0.35 + Precision_{att} - k·Evasion_{obj},\ 0.20,\ 0.95)$, con $k \in [1.0, 1.2]$.
      - Aplicar factor de Supervivencia: $p_{hit} *= FactorPrecision$.
2) Crítico: si RNG < `CritChance`, multiplicar por `CritMult`; caps: `CritChance ≤ 0.5`, `CritMult ∈ [1.25, 1.75]`.
3) Defensa/Penetración: reducir defensa por `Penetracion` y mitigar.
4) Mitigaciones del objetivo: físicas/mágicas.
5) Elementales: resistencias (0..0.9) y vulnerabilidades (1.0..1.5) por canal (`magia` hoy).
6) Aplicar daño y efectos OnHit/OnKill.
7) Registrar en `ResultadoAccion` y presentar en UI.

Edge cases y decisiones:

- Daño mínimo = 1 si impacta y tras mitigaciones queda > 0 (salvo inmunidades explícitas).
- Evasión duplicada: consolidar en un solo chequeo en el resolver (evitar doble miss).
- Overkill: clamp vida a 0; marcar `ObjetivoDerrotado`.

## 5. Recolección y mundo

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

## 6. Objetos, inventario y comercio

- Tipos: armas, armaduras, pociones, materiales; gestores en migración a repos JSON.
- Inventario: `IInventariable`/`IUsable`; consumo en combate vía `IAccionCombate`.
- Tienda: precios afectan reputación; reglas por facción.

## 7. Misiones y encuentros

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

## 8. Supervivencia

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

## 9. UI y presentación

- `IUserInterface`: desacopla vista; implementaciones de consola y silenciosa (tests).
- `UIStyle`: estilo de encabezados y etiquetas de reputación/supervivencia.

## 10. Datos, validación y guardado

- `PathProvider` centraliza rutas.
- `DataValidatorService`: valida mapa, misiones, NPCs, enemigos; objetos pendiente.
- `GuardadoService`: persiste drops únicos y cooldowns de encuentros; sustituye I/O ad-hoc.

## 11. Testing y determinismo

- xUnit; `RandomService.SetSeed` para reproducibilidad.
- `SilentUserInterface` para evitar bloqueos por input.
- Proveedores inyectables (hora, paths) para desacoplar entorno.

## 12. Migración a Unity

- Mantener dominio puro (independiente de consola/UI).
- Adaptadores de UI/Logger/Input en capa presentación.
- Convertir JSON a ScriptableObjects; usar adapters para `IUserInterface`.

## 13. Problemas conocidos y edge cases

- Doble evasión: consolidar en resolver.
- Contratos JSON: inconsistencias históricas en objetos; mitigado por validadores y herramientas.
- Balance conservador: evitar “power spikes”; progresión intencionalmente lenta.

## 14. Ejemplos prácticos

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

## 15. Apéndice de contratos (interfaces y DTOs)

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
