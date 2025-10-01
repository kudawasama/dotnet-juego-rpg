# Resumen de Datos del Juego (Snapshot 2025-10-01)

Este documento consolida en un único lugar la información operativa de alto nivel solicitada: enemigos base, misiones definidas y organización de materiales. Sirve como índice rápido; la lógica, fórmulas y contratos siguen en `Arquitectura_y_Funcionamiento.md`.

> Nota: Este snapshot no reemplaza a los JSON; es una vista de referencia para comprender alcance y progresión. Para edición/balance usar siempre los archivos fuente en `DatosJuego/`.

## 1. Enemigos (enemigos.json)

Total listados: 16

| Nombre | Nivel | Vida | Ataque | Defensa | Rareza | Categoría | Familia | Recompensa EXP | Oro |
|--------|-------|------|--------|---------|--------|-----------|---------|----------------|-----|
| Goblin | 1 | 50 | 10 | 5 | Comun | Normal | Humanoide | 5 | 5 |
| Rata | 1 | 30 | 5 | 2 | Comun | Normal | Bestia | 2 | 1 |
| Zombi | 1 | 55 | 11 | 4 | Comun | Normal | NoMuerto | 6 | 4 |
| Slime | 1 | 40 | 8 | 2 | Comun | Normal | Bestia | 3 | 2 |
| Murciélago | 2 | 35 | 7 | 3 | Comun | Normal | Bestia | 4 | 3 |
| Rata Gigante | 2 | 45 | 9 | 4 | Comun | Normal | Bestia | 5 | 4 |
| Gran Goblin | 3 | 80 | 15 | 8 | Raro | Normal | Humanoide | 15 | 10 |
| Esqueleto | 3 | 60 | 12 | 6 | Comun | Normal | NoMuerto | 10 | 7 |
| Lobo | 4 | 70 | 14 | 7 | Comun | Normal | Bestia | 12 | 8 |
| Bandido | 4 | 65 | 13 | 6 | Comun | Normal | Humanoide | 13 | 12 |
| Golem de Fuego | 5 | 150 | 25 | 20 | Epico | Elite | Elemental | 50 | 30 |
| Orco | 6 | 120 | 22 | 12 | Raro | Normal | Humanoide | 30 | 18 |
| Esqueleto Mago | 7 | 80 | 28 | 8 | Raro | Elite | NoMuerto | 35 | 20 |
| Troll | 8 | 200 | 35 | 18 | Epico | Elite | Humanoide | 60 | 40 |
| Dragón Bebé | 10 | 180 | 40 | 22 | Legendario | Jefe | Dragon | 100 | 80 |

Observaciones:
- Curva de Vida/Daño aumenta de forma escalonada; jefes y élites presentan saltos (checkpoints de progresión).
- Rarezas no determinan directamente el loot aquí; la escalada se apoya en EXP/Oro.
- Espacios para futuros biomas: añadir variantes elementales y categorías específicas.

## 2. Misiones (misiones/*.json)

Total listadas: 24

Líneas principales y progresión (ejemplos):
  
- Herrero Bairan: BAI-HER-001 → BAI-HER-002 → BAI-HER-003 (progresión de forja, defensa de ciudad).
- Recolección Bairan: Cadena de misiones de gather (`BAI-REC-*` + especial `BAI-ESP-*` desbloqueada por requisitos compuestos).
- Inicio Bairan: BAI-INI-001 (entrega armas iniciales al alcanzar nivel 2).
- Herrero Winston / Cocina Zircon / Magia Aethel / Guardia Morholt / Agricultura Laito / Minería Rok / Comercio Raven: líneas temáticas independientes con recompensas de acceso o mejora de stats específicos.

Distribución de recompensas (tendencias):
  
- ExpNivel: micro-incrementos (0.004–0.03) salvo misión de inicio (0.5) para acelerar arranque controlado.
- ExpAtributos: orientadas a temática (Herrero → Fuerza/Resistencia; Cocina → Resistencia/Suerte; Magia → Inteligencia/Sabiduría; Comercio → Carisma/Inteligencia).
- Recompensas cualitativas: acceso a tienda especial, expansión inventario, objetos legendarios, permisos y tiendas secretas (gate de progreso horizontal).

Ejemplo de gating compuesto: `BAI-ESP-001` requiere completar BAI-REC-002 y BAI-REC-003 (multi-linea), encadenándose a un trato secreto (economía oculta) y gating de acceso.

## 3. Materiales (estructura de carpetas)

Raíz: `DatosJuego/Materiales/` → subcarpetas por especialidad: `Mat_Herrero`, `Mat_Herbolario`, `Mat_Sastre`, `Mat_Curtidor`, `Mat_Ingeniero`, `Mat_Mistico`, (y otras similares si se añaden). Más de 200 definiciones individuales.

Ejemplo (Herrero): `Mineral_de_Hierro.json`
```json
{ "id": "mineral_de_hierro", "nombre": "Mineral de Hierro", "rareza": "comun", "especialidad": "herrero" }
```

Patrones:
  
- Nombres normalizados snake-case en `id`, nombres legibles en `nombre`.
- `rareza` en minúsculas (conversión tolerante en runtime).
- `especialidad` alinea con la carpeta padre (validación semántica simple posible).

Uso previsto:
  
- Crafteo futuro (recetas hacen referencia por `id`).
- Misiones (strings en requisitos; migración recomendada a identificadores formales con cantidad para evitar ambigüedades: propuesto `{ "Item": "mineral_de_hierro", "Cantidad": 5 }`).
- Comercio y economía (valores base ajustables por rareza y demanda futura).

## 4. Próximos Pasos de Documentación

1. Incorporar tabla de efectos/estados cuando se amplíe más allá de veneno.
2. Añadir matriz Distancia x Acción al integrar sistema de posicionamiento.
3. Migrar requisitos de misiones a estructura tipada (evitar strings libres como `"nivel:2"`).
4. Generar script de validación que consolide inconsistencias (rareza vs carpeta, ids duplicados, requisitos inexistentes).

## 5. Referencias Cruzadas

- Fórmulas, pipeline y PA: ver `Arquitectura_y_Funcionamiento.md` (§3, §5, §6.1, §6.2).
- Balance y caps: `progression_config.md`.
- Evolución de sistemas: `Bitacora.md` (entradas por fecha).

Última actualización snapshot: 2025-10-01.

---

## 6. NPCs (npcs/npc.json)

Total NPCs listados: 9

Roles clave: Mercader, Herrero, Cocinero, Maga, Guardia, Agricultora, Minero, Comerciante, Guía inicial.

Uso en sistemas:
  
- Gating de misiones (referencia Id misión en `Misiones[]`).
- Comercio especializado (futuro: catálogos dinámicos por reputación).
- Entrada de tutorial (Guía: entrega armas iniciales + misiones de inicio/comercio).

Riesgos detectados: Falta campo formal para reputación/facción (actual implícito). Recomendado añadir `Faccion` y `ReputacionMin` para escalado futuro.

## 7. Clases (clases.json)

Total definidas: 28 (profesiones, físicas, mágicas, mixtas, divinas, únicas).

Estructura clave por entrada:
  
- Nombre, Categoria, Rareza (escala subjetiva de desbloqueo), Oculta, ClasesPrevias, NivelMinimo, AtributosRequeridos, EstadisticasRequeridas, MisionesRequeridas, ReputacionMinima, AtributosGanados (+ posibles penalizadores negativos), MisionUnica/ObjetoUnico opcionales.

Observaciones:
  
- Curva de progresión jerárquica (Aprendiz → Clase básica → Especialización Rara/Epica → Legendaria/Divina/Única).
- Uso de atributos negativos para compensar clases avanzadas; mantener consistencia (evitar invertir estilo de juego del jugador de forma irreversible).
- Faltan identificadores estables (usar slug derivado de nombre para internacionalización futura).

## 8. Acciones (acciones/acciones_catalogo.json)

Total acciones catalogadas: ~80 (contabilizadas por IDs presentes).

Clasificación sugerida (no en datos aún):
  
- Combate: AtaqueFisico, AtaqueMagico, BloquearAtaque, CurarAliado, AturdirEnemigo, DefenderAliado, HuirCombate.
- Movimiento/Mixto: Correr, CorrerGolpear, ViajarSector.
- Progresión/Habilidad: SubirNivel, AprenderHabilidad.
- Interacción NPC: DialogarNPC, ObservarNPC, IntercambiarObjeto, AceptarMision, RechazarMision, RobarIntento.
- Recolección/Crafteo: RecolectarMaterial, Minar, TalarArbol, Pescar, CraftearObjeto, DesmontarObjeto, Forjar, Cocinar, Hornear, Fermentar, Destilar.
- Gestión Equipo: EquiparObjeto, DesequiparObjeto, MejorarObjeto, RepararObjeto.
- Metajuego/Estados: Descansar, Meditar, DetectarEnergia, CanalizarMana.

Próxima ampliación de esquema: `CostoPA`, `Tipo` (combate/mundo/social), `Tags` (movimiento, daño, soporte), `CooldownBase`, `Requisitos` (atributo, clase, misión), `RecompensasProgreso` (para desbloqueos ocultos).

## 9. Armas (Equipo/armas.json)

Entradas (parcial >100, grep mostró primeras 20). Campos observados: Nombre, DañoFisico/DañoMagico/Defensa, NivelRequerido, Rareza heterogénea (usa valores legacy: Normal, Superior, Rara, Ornamentada, Legendaria). Hay Perfeccion >100 (indica generador previo sin clamp). BonificadorAtributos en algunas.

Pendiente: Migrar rarezas textual a nuevo sistema unificado (minúscula) y definir rango `Perfeccion` válido (0–100 o permitir >100 como overquality con curva decreciente?).

## 10. Set GM (Equipo/sets/GM.json)

Set demostración extremo para pruebas: umbrales 2/4/6 piezas → +Defensa, +Ataque, +Mana/Energia + habilidad temporal `descarga_arcana`.
Sirve para validar: habilidades otorgadas temporales, recalculo de estadísticas, limpieza al perder piezas.

## 11. Habilidades (habilidades/**/*)

Archivos detectados: 30 (carpetas por tipo + mapper + test). Archivo raíz `habilidades.json` vacío (posible placeholder; considerar eliminar o documentar). `habilidades_mapper_demo.json` vincula habilidades a AccionId.

Modelos: per-file (ej. `Hab_Fisicas/GolpeFuerte.json`) y agregados históricos (`Hab_Fisicas.json`) coexistiendo. Loader tolera ambos; plan de limpieza: retirar agregados tras confirmar paridad.

## 12. Mapa (mapa.txt)

Dimensiones: 55 x 55 (0..54 en filas y columnas) → 3025 celdas. Cada celda identificada `fila_col`. No hay metadatos (bioma, visitado, elevación, tipo de nodo) en este archivo.

Próxima evolución: archivo `mapa_sectorizado.txt` (no encontrado) o JSON con propiedades: Bioma, Dificultad, Recursos, Eventos, VisitadoInicial, Conectividad, PeligroBase.

## 13. Materiales (Materiales/**/*)

Muestra de búsqueda indica >400 archivos (duplicados o variaciones por especialidad). Algunas rutas repetidas (Mat_Cocina vs Mat_Cazador con mismo recurso funcional). Necesario normalizar taxonomía:
  
- Especialidades base esperadas: herrero, herbolario, sastre, curtidor, ingeniero, alquimista, cazador, cocina (sinonimizar ‘cocinero’).
- Duplicaciones: items como Escama_Ignea presentes en múltiples especialidades (decidir si multi-uso o duplicado accidental).

Acción recomendada: generar índice maestro (script) que cubra: `id`, especialidades, rareza, orígenes (enemigos drop), usos (recetas). A partir de allí validar duplicados.

## 14. Validaciones y Consistencias Recomendadas

Checklist inicial para un `DataValidatorService` extendido:
  
1. Rarezas: normalizar a minúsculas; advertir valores fuera de catálogo configurado.
2. Armas: `Perfeccion` fuera de rango definido → log (no abortar).
3. Clases: nombres duplicados / faltan slugs → sugerir slug.
4. Acciones: IDs duplicados (actualmente no hay, validar al cargar).
5. Misiones: `SiguienteMisionId` que no existe → advertencia.
6. Misiones: requisitos tipo `"nivel:2"`, `"oro:300"` → migrar a `{ "tipo": "nivel", "valor": 2 }` / `{ "tipo": "oro", "valor": 300 }`.
7. NPC: `Misiones[]` que no existen en catálogo.
8. Materiales: ids repetidos en especialidades distintas con contenido idéntico → fusionar o marcar multiuso.
9. Sets: thresholds ordenados ascendente y sin solapamiento de piezas.
10. Habilidades: conflicto entre archivos agregados y per-file (preferir per-file; marcar duplicados en log).

## 15. Próximas Acciones de Datos (Prioridad sugerida)

1. Script índice materiales + detección duplicados (alto impacto balance).
2. Migrar misiones a requisitos tipados (reduce parsing frágil).
3. Añadir esquema extendido acciones (PA, tags) y validarlo.
4. Normalizar rarezas armas y clamp perfección si procede.
5. Introducir metadatos de mapa (bioma, dificultad) para integrar spawn y progresión.
6. Generar slugs de clase y acción para internacionalización futura.

---

## 16. Biomas (biomas.json)

Biomas definidos (muestra): Bosque Encantado, Bosque, Montaña, Lago, Desierto, Cueva, CuevaProfunda, Cañon, Océano, Océano Lejano, Campo (y continúa en archivo). Cada bioma:
- NodosComunes / NodosRaros: lista de nodos con Materiales (Nombre, Cantidad), cooldown y producción min/max.
- Campos opcionales: Tipo (Talar/Minar), Requiere (herramienta), Rareza nodal.

Observaciones:
- Duplicidad de ciertos nodos (Piedra Brillante/Cristales) entre Cueva y CuevaProfunda; considerar escalado de cantidades o rareza distinta.
- Normalizar acentos / nombres para slug (`BosqueEncantado`, `Cueva_Profunda`).
- Integrar peso de aparición para spawn procedural (aún no existe campo `Peso`).

## 17. Progresión (progression.json)

Claves:
- Exp base (Recolección/Entrenamiento/Exploración) y escalados >1 para costo incremental.
- Índices de atributos (Fuerza 3.0, Inteligencia 8.0, etc.) sugieren coste relativo para subirlos.
- Regen de Maná en combate y fuera con base + factor + límite por tick/turno.

Acciones recomendadas: Documentar fórmula concreta de subida de atributo usando `Indices`; centralizar en `Arquitectura` si no está.

## 18. Energía (energia.json)

Define coste base por tipo (`Recolectar`, `Minar`, `Talar`), modificadores por herramienta (`Pico` -0.15), bioma (Bosque +0.05), clase (Minero -0.20), reducción por atributo relevante sobre umbral (25) con factor 0.4 y límites [3,25].

Contrato propuesto fórmula:
`costo = Clamp( (BaseTipo * (1 + ModBioma) * (1 + ModHerramienta) * (1 + ModClase)) * ReduccionAtributo , CostoMinimo, CostoMaximo)` donde `ReduccionAtributo = (atributo < Umbral) ? 1 : (1 - FactorReduccionAtributo)` (pendiente validar exactitud en código runtime).

## 19. Reputación (reputacion_umbrales.json)

Define umbrales globales [-100,-50,0,50,100] y por facción (Guardia de Bairan, Gremio de Ladrones) con mensajes por tramo.

Faltantes:
- No hay escalado numérico de efectos (descuentos, agresividad); sólo flavor text.
- Recomendado: añadir `Efectos` (ej. `{ "DescuentoComercio": 0.05 }`) por umbral.

## 20. Config Combate (config/combat_config.json)

Parámetros ya descritos en secciones previas (PA, crítico, penetración, flags pipeline). Confirmado `ModoAcciones=false` todavía.

## 21. Rarezas Config (rareza_pesos / rareza_perfeccion)

Pesos: Rota 20, Pobre 35, Normal 50, Superior 10, Rara 3, Epica 2, Legendaria 1, Ornamentada 0.1 (suma > 121.1 → se normaliza en carga). Perfección por rareza escalonada en bloques no uniformes.

Pendientes:
- Añadir documentación para derivar `Probabilidad = peso / Σ` y validación de monotonía de rangos (sin superposiciones).
- Considerar rareza “Unica” vs. “Legendaria/Ornamentada” si aparece en otros datasets (clases la usan, falta en pesos).

## 22. Crafteo (crafteo/recetas.json + Recetas<Especialidad>.json)

`recetas.json` vacío (placeholder). Archivos específicos por especialidad con formato aún no revisado (no cargado aquí). Acciones:
1. Consolidar esquema común (Inputs[], Output, Tiempo, Herramienta, Estacion, Experiencia).
2. Validar que cada material referenciado exista.
3. Integrar coste de energía usando `energia.json`.

## 23. Eventos (eventos/eventos.json)

Listado de 10 eventos (Festival, Invasión, Ruinas, Mercado Nocturno, Duelo, Tesoro, Rescate, Meteorito, Fiesta Cosecha, Portal). Campos: Id, Tipo, Categoria, Rareza, Unico, Mision (opcional).

Observaciones:
- Rarezas usan acentos inconsistentes ("Épico" vs `Epica` en otras partes) → normalizar.
- Falta ventana temporal / condiciones de aparición (hora, bioma, reputación mínima).

## 24. Pociones (pociones/pociones.json)

Archivo con duplicado de “Poción Pequeña”. Requiere desduplicación e inclusión de identificador (`Id`). Campos mínimos actuales: Nombre, Curacion, Rareza, Categoria.

Recomendado:
- Añadir `Cooldown`, `StackMax`, `CostoEconomiaBase` y `EfectosSecundarios`.
- Normalizar rarezas a minúsculas y catálogo central.

## 25. Clases Dinámicas (clases/*/*.json)

Subcarpetas `basicas`, `avanzadas`, `especiales` con pares `<nombre>.json` y `<nombre>_dinamico.json`. Objetivo: override condicional (misiones, reputación) sin editar base.

Pendiente establecer regla de fusión:
1. Cargar base.
2. Si existe `_dinamico`, aplicar diff (sólo campos presentes) conservando ausentes.
3. Registrar en log campos sobreescritos.

## 26. Mapa Sectorizado (mapa/)

Existe estructura generador (`GeneradorSectores.cs`), subcarpetas biomas y sectores. Integrar con biomas.json para spawn nodos. Añadir a futuro: densidad nodos, semillas de loot, nivel sugerido, fauna principal.

## 27. Consolidación de Pendientes Globales

Nueva lista complementaria a la sección 15 (no duplicar, profundiza):
  
- Definir catálogo maestro de rarezas usado por TODAS las categorías (objetos, clases, eventos, pociones).
- Migrar nombres con acentos a formato slug consistente para referencias cruzadas.
- Script de detección de duplicados (pociones, materiales multi-carpeta, habilidades agregadas vs per-file).
- Fusión base/dinámico de clases (algoritmo documentado).
- Incorporar efectos cuantitativos de reputación y eventos.
- Esquema recetas crafteo + validador de insumos.
- Acciones: Añadir campos PA y coste a acciones de combate.

---
