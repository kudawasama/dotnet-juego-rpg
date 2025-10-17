# Flujo de Menús y Juego (Migrado desde Flujo.txt)

_Este archivo reemplaza al antiguo `Flujo.txt` y define el flujo principal de navegación y estados del juego. Las secciones incluyen anclas usadas por `Arquitectura_y_Funcionamiento.md`._

---

## INICIO DEL JUEGO (Program.cs)

Ancla: inicio-del-juego-programcs

Secuencia inicial:

1. Procesamiento de flags CLI tempranos (ayuda, benchmarks, validadores).
2. Carga de configuraciones tolerantes (CombatConfig, Progression, RarezaConfig).
3. Inicialización de servicios base (Logger, PathProvider, RandomService).
4. Presentación del Menú Principal.

---

## MENÚ PRINCIPAL DEL JUEGO (Juego.Iniciar)

Ancla: menú-principal-del-juego-juegoiniciar

Opciones típicas:

- Nueva partida
- Continuar
- Opciones (flags, verbosidad, validadores de datos)
- Salir

---

## MENÚ DE CIUDAD (MenuCiudad)

Ancla: menú-de-ciudad-menuciudad

Opciones:

- Visitar tiendas (comercio)
- Entrenar (entrenamiento atributos)
- Misiones y NPC
- Descansar (regeneración fuera de combate)

---

## MENÚ FUERA DE CIUDAD (MenuFueraCiudad)

Ancla: menú-fuera-de-ciudad-menufueraciudad

Opciones:

- Explorar sector
- Recolectar (minar, talar, pescar)
- Viajar a otro sector
- Entrar en combate (si hay encuentro)

---

## MENÚ DE MISIONES Y NPC (Menus.Juego.MostrarMenuMisionesNpc)

Ancla: menú-de-misiones-y-npc-menusjuegomostrarmenumisionesnpc

Flujo:

1. Listar misiones disponibles/activas/completadas
2. Ver detalles y requisitos
3. Aceptar/Entregar misión

---

## MENÚ DE RUTAS (Juego.MostrarMenuRutas)

Ancla: menú-de-rutas-juegomostrarmenurutas

Permite seleccionar destinos y muestra requisitos y riesgos estimados.

---

## MENÚ DE COMBATE (Base actual)

Ancla: menú-de-combate-base-actual

Fase actual (post-rollback):

- Resolución de ataques con DamageResolver mínimo (estable)
- Flags opcionales: precisión, penetración, verbose
- Regeneración al cierre de turno

---

## MENÚ ENTRE COMBATES (MenuEntreCombate)

Ancla: menú-entre-combates-menuentrecombate

Opciones:

- Usar pociones
- Revisar equipo
- Aprender/Asignar habilidades

---

## MENÚ FIJO (Accesible desde ciudad/fuera/combate)

Ancla: menú-fijo-accesible-desde-ciudadfueracombate

Incluye accesos:

- Inventario
- Habilidades
- Estadísticas
- Guardar/Cargar (cuando corresponda)

---

```text
Notas:
- Esta estructura es un esqueleto para documentar navegación; no fuerza implementación.
- Los nombres entre paréntesis refieren a métodos actuales en código cuando existen.
```
