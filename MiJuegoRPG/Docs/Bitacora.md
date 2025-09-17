# Bitácora de desarrollo

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
