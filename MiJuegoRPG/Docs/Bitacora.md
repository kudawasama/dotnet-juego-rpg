# Bitácora de desarrollo

## 2025-09-17 — Expansión documental detallada

- `progression_config.md`: se añadieron fórmulas en KaTeX, ejemplos numéricos paso a paso, orden de aplicación (clamps) y contrato JSON sugerido con defaults. Se incluyeron pruebas recomendadas y guías de tuning.
- `Arquitectura_y_Funcionamiento.md`: se profundizó en contratos (interfaces/DTOs), pipeline de combate con orden exacto y límites, referencias a `Flujo.txt`, y apéndice de firmas públicas. Objetivo: facilitar onboarding y migración a Unity.
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
