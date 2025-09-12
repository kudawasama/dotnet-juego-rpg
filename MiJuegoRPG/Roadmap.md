PLAN DE REFACTORIZACIÓN Y PROGRESO (log incremental)
=================================================

Estado de avance (resumen): 29/179 Hecho · 3/179 Parcial · 147/179 Pendiente
███████████░░░░░░ 16% completado (estimado por ítems del roadmap)

Formato columnas: [ID] Estado | Área | Descripción breve | Próxima acción
Estados posibles: Pendiente, En curso, Parcial, Hecho, Bloqueado

Legend inicial: Solo la 1.x se empieza ahora para evitar cambios masivos de golpe.

1. FUNDAMENTOS (Infra / Enumeraciones / Servicios base)
------------------------------------------------------
[1.1] Hecho | Organización | Crear este archivo de tracking | Archivo creado y actualizado periódicamente
[1.2] Hecho | Enumeraciones | Definir enums: Atributo, TipoRecoleccion, OrigenExp | En uso en ProgressionService y RecoleccionService
[1.3] Hecho | Servicio | Crear ProgressionService (sólido) con método AplicarExpAtributo | Recolección + entrenamiento + exploración centralizados (tests pendientes 9.x)
[1.4] Hecho | Servicio | RandomService centralizado (inyectable) | Reemplazados todos los usos de new Random() en dominio
[1.5] Hecho | Limpieza | Sustituir strings mágicos de recolección por enum | Menú y acción usan TipoRecoleccion
[1.6] Hecho | Guardado | Completar GuardadoService (ya scaffold) | Integrado en Juego y Menús, reemplaza GestorArchivos

2. EVENTOS Y DESACOPLAMIENTO
----------------------------
[2.1] Hecho | Infra | EventBus simple (pub/sub en memoria) | EventBus.cs + integración ProgressionService
[2.2] Hecho | Progresión | Emitir eventos en subidas de nivel / atributo / misión | Atributos, nivel y misión integrados
[2.3] Parcial | UI | Sustituir Console directa por IUserInterface | Interfaz creada + ConsoleUserInterface + SilentUserInterface para pruebas; InputService delega a Juego.Ui para lectura/pausa. Juego permite inyectar UI vía UiFactory. Progreso: Juego, menús modulares, MenusJuego y Program migrados; Servicios migrados: RecoleccionService, EnergiaService, EstadoPersonajePrinter y MotorMisiones. Pendiente: limpiar Console.* en dominio (Personaje, Inventario, CombatePorTurnos, Objetos y Gestores) y herramientas.

3. PROGRESIÓN Y ATRIBUTOS
-------------------------
[3.1] Hecho | Dominio | Unificar experiencia de atributos en estructura (ExpAtributo) | Implementado ExpAtributo
[3.2] Hecho | Dominio | Migrar Personaje a diccionario <Atributo, ExpAtributo> | Personaje.ExperienciaAtributos + migración legacy
[3.3] Hecho | Balance | Parametrizar fórmula en ProgressionConfig (JSON) | progression.json actualizado con escalados y factorMinExp + documentación añadida

4. RECOLECCIÓN Y MUNDO
----------------------
[4.1] Hecho | Servicio | RecoleccionService (mover RealizarAccionRecoleccion + MostrarMenuRecoleccion) | Menú y ejecución centralizados
[4.2] Parcial | Data | Añadir tiempos de respawn y rarezas a nodos | Cooldown multisector implementado (JSON por sector); falta balance rareza y limpiar crecimiento futuro.
[4.3] Hecho | Energía | Integrar coste dinámico según herramienta y bioma | energia.json + cálculo en EnergiaService (modificadores por tipo/herramienta/bioma/atributos/clase)
[4.4] Hecho | UX | Menú híbrido con filtros + búsqueda + cooldown + fallo | Implementado en RecoleccionService

5. COMBATE
-----------
[5.1] Pendiente | Dominio | Definir IAccionCombate + ResultadoAccion | Base
[5.2] Pendiente | Dominio | Refactor CombatePorTurnos a cola de acciones | Tras 5.1
[5.3] Pendiente | Estados | Implementar IEfecto (veneno, sangrado, buff) | Tras 5.2
[5.4] Pendiente | Balance | Escalado por velocidad (orden dinámico) | Tras 5.2

6. MISIONES Y REQUISITOS
------------------------
[6.1] Pendiente | Dominio | Reemplazar strings requisitos por IRequisito | Base
[6.2] Pendiente | Dominio | Reemplazar recompensas por IRecompensa | Tras 6.1
[6.3] Pendiente | Flujo | Cadena de misiones con grafo (prerequisitos) | Tras 6.1

7. REPOSITORIOS / DATA
----------------------
[7.1] Pendiente | Infra | IRepository<T> genérico JSON | Base
[7.2] Pendiente | Infra | Repos específico Misiones / Enemigos / Objetos | Tras 7.1
[7.3] Pendiente | Cache | Carga diferida + invalidación | Tras 7.2

8. UI / PRESENTACIÓN
--------------------
[8.1] Hecho | Abstracción | IUserInterface (WriteLine, ReadOption, Confirm) | Interfaz creada + adaptadores: ConsoleUserInterface y SilentUserInterface (para tests); InputService usa la UI para leer opciones/números y pausar. Añadido InputService.TestMode para evitar bloqueos en tests. Juego expone UiFactory para inyección. Logger central agregado y enlazado a la UI. Migradas salidas principales en Juego (menú, viaje, recolección, mazmorra, rutas) y GeneradorEnemigos. Menús migrados: MenuCiudad, MenuFueraCiudad, MenuRecoleccion, MenuFijo, MenuAdmin, MenuEntreCombate, MenusJuego y Program.cs. Pendiente: unificar colores/estilo.
[8.2] Pendiente | Menús | Refactor menús a comandos (Command Pattern) | Tras 8.1
[8.3] Parcial | Estilo | Colores y layout unificados | Etiquetas de reputación colorizadas en ciudad/tienda/NPC/misiones; Recolección (híbrida), Energía, Estado del Personaje y Misiones ya usan la UI unificada. Siguiente: CombatePorTurnos, Inventario detallado y Gestores de objetos. 
[8.4] Pendiente | Theming | Servicio de estilo (UIStyleService) con paleta y helpers (títulos, listas, etiquetas) | Facilita unificación visual y futura migración a UI de Unity

9. TESTING
----------
[9.1] Hecho | Infra | Crear proyecto tests xUnit | Proyecto MiJuegoRPG.Tests creado (xUnit) y referenciado en la solución
[9.2] Hecho | Test | Mapa.MoverseA casos | Tres casos cubiertos: inicialización (CiudadPrincipal), adyacencias y movimiento válido/ inválido + descubrimiento
[9.3] Hecho | Test | GeneradorEnemigos nivel y drops | Tests deterministas con RandomService.SetSeed y filtro por nivel; E/S aislada a %TEMP% y opción DesactivarPersistenciaDrops para evitar escribir JSONs reales.
[9.4] Hecho | Test | ProgressionService fórmula | Explorar (Percepción+Agilidad), Entrenamiento con subida y Recolección por tipo
[9.5] Hecho | Test | Recolección energía y requisitos | Cooldown por nodo: aplicar y limpiar al entrar sector (persistencia multisector)

10. LIMPIEZA / QUALITY
----------------------
[10.1] Hecho | Rutas | Centralizar rutas en PathProvider | Servicio PathProvider agregado; refactors en Juego, ProgressionService, EnergiaService, ReputacionService, ReputacionPoliticas, ShopService, MenusJuego, MotorMisiones, GestorArmas, GestorPociones, GestorMateriales, GuardadoService, CreadorPersonaje, TestGeneradorObjetos
[10.2] Hecho | Random | Sustituir usos dispersos | RecoleccionService y BiomaRecoleccion usan RandomService; agregado SetSeed(int) para tests deterministas
[10.3] Pendiente | Nombres | Uniformar nombres archivos (GeneradorObjetos vs GeneradorDeObjetos) | Revisión
[10.4] Pendiente | Comentarios | Podar comentarios redundantes | Continuo
[10.5] Pendiente | Documentación | README arquitectura modular | Final intermedio
[10.6] Pendiente | Validación Data | Validador JSON referencial (IDs de mapa, facciones, misiones, objetos) + pruebas | Evitar roturas por referencias inexistentes
[10.7] Pendiente | Higiene Git | Decidir si versionar juego.db; si no, añadir a .gitignore y documentar | Mantener repo limpio entre equipos

11. CLASES DINÁMICAS / PROGRESIÓN AVANZADA
------------------------------------------
[11.1] Hecho | Atributo Extra | Agregar 'Oscuridad' a AtributosBase | Disponibles requisitos y clases oscuras futuras
[11.2] Hecho | Evaluación Requisitos | ClaseDinamicaService: nivel, clasesPrevias, clasesAlguna, exclusiones, atributos, estadísticas, actividad, reputación, misiones múltiple/única, objeto único | Lógica centralizada CumpleHardRequirements
[11.3] Hecho | Bonos Iniciales | Aplicar AtributosGanados al desbloquear clase (incluye Oscuridad) | Método AplicarBonosAtributoInicial
[11.4] Hecho | Desbloqueo Emergente | Score parcial (PesoEmergenteMin) | Dataset aún no lo usa (seguir monitoreo)
[11.5] Hecho | Reputación Facción | Campo ReputacionFaccionMin en ClaseData + check | Evaluado en ClaseDinamicaService
[11.6] Pendiente | Bonificadores Globales | Servicio unificador (XP.*, Drop.*, Energia.*) | Diseñar BonosGlobalesService
[11.7] Hecho | Clamp Atributos | Evitar negativos / límites soft-hard | Aplicado en bonos de clase y menú admin

12. REPUTACIÓN
--------------
[12.1] Hecho | Persistencia | Reputacion global y por facción en Personaje | Campos Reputacion / ReputacionesFaccion
[12.2] Hecho | Servicio | ReputacionService (modificar global/facción + reevaluar clases) | Integrado en Juego
[12.3] Hecho | Umbrales | reputacion_umbrales.json + eventos y avisos | ReputacionService publica EventoReputacionUmbral*
[12.4] Hecho | Alineación Negativa | Feedback visual y gating por reputación negativa | Etiquetas compactas colorizadas + gating en NPC y tienda alineado a bandas; políticas en JSON
[12.5] Pendiente | Métricas | Tracking de cambios reputación para balance | Requiere logger/telemetría ligera
[12.6] Hecho | Tienda ↔ Reputación | Ganancia por compra (+1/100 oro) y venta (+1/200 oro); descuentos por rep global/facción | Lógica centralizada en ShopService (GetPrecioCompra/Venta, PuedeAtender); MenusJuego solo UI; facciones_ubicacion.json data-driven (fallback activo); unificación a IDs de mapa en curso

14. MIGRACIÓN / INTEGRACIÓN UNITY
---------------------------------
[14.1] Pendiente | Abstracciones | Separar estrictamente dominio (lógica) de presentación (UI) | Facilitar portar a Unity sin reescribir núcleo
[14.2] Pendiente | Carga Datos | Diseñar conversor JSON → ScriptableObjects (plan de tool) | Pipeline de datos para Unity
[14.3] Pendiente | Servicios | Adaptadores de IUserInterface/Logger a Unity UI/Console | Reuso de menús y mensajes
[14.4] Pendiente | Tiempo/Juego | Servicio de tiempo (tick/update) desacoplado de Console loop | Integración con game loop de Unity
[14.5] Pendiente | Input | Adaptar InputService a sistemas de input (teclado/control) | Capa de entrada unificada

13. ADMIN / HERRAMIENTAS QA
---------------------------
[13.1] Hecho | Menú Admin | Separado del menú principal (opción 5) | Aísla flujos de jugador
[13.2] Hecho | Ajustes Directos | TP, reputación global/facción, verbose reputación, nivel +/- | MenuAdmin opciones 1–6
[13.3] Hecho | Atributos | Modificar atributo individual con recálculo y reevaluación clases | Opción 7
[13.4] Hecho | Diagnóstico | Listar clases (motivos bloqueo), atributos+stats, habilidades, inventario, resumen integral | Opciones 8–12
[13.5] Hecho | Forzar Clase | Desbloqueo manual (override) con aplicación de bonos y reevaluación | Opción 13 en MenuAdmin
[13.6] Hecho | Export Snapshot | Guardar resumen integral a archivo (logs/admin) | Opción 14 en MenuAdmin
[13.7] Hecho | Batch Atributos | Parser múltiple (fuerza+5,int+3) | Opción 7 soporta entrada batch
[13.8] Pendiente | Seguridad | Flag para ocultar menú admin en build release | Config build / preprocesador

15. OBJETOS / CRAFTEO / DROPS
-----------------------------
[15.1] Pendiente | Data | Esquema común de objetos/materiales (JSON) + repositorios | Consolidar GestorArmas/Materiales/Pociones bajo repos JSON; IDs únicos, Rareza, NivelRequerido, BonosAtributo/Stats, DurabilidadBase (opcional). Integrar con PathProvider y validar con 10.6
[15.2] Pendiente | Drops Enemigos | Tablas de botín por enemigo (base) + modificadores por sector/bioma/dificultad | Extender enemigos.json con secciones de drop o crear loot/enemigos_*.json; soportar DropRate, CantidadMin/Max, UniqueOnce; usar RandomService. Diseñar clamps anti-farming
[15.3] Pendiente | Drops Mapa | Tablas de botín por sector (cofres/eventos ambientales) | Archivo loot/sectores.json; gating por reputación/llaves/misiones; sincronizar con IDs de sector
[15.4] Pendiente | Crafteo | Sistema de recetas (recetas.json) + blueprints desbloqueables | Requisitos por atributos/habilidad/misiones; coste de energía/tiempo; chance de fallo; calidad resultante; estaciones de trabajo por ciudad/ubicación
[15.5] Pendiente | Desmontaje | Desmontar objetos para recuperar materiales | Rendimiento según skill y estado del objeto; pérdida parcial en fallos; economía anti-exploit
[15.6] Pendiente | Durabilidad/Repair | Degradación y reparación con materiales | Integrar con EnergiaService; estaciones de reparación; opcional pero recomendado para progresión lenta
[15.7] Pendiente | Balance | Rareza, caps y cooldowns | Límites por nodo/sector, cooldown por crafteo avanzado, protección contra rachas RNG (bad luck protection)
[15.8] Pendiente | Economía | Integración con ShopService | Precios dinámicos de materiales/crafteados según reputación y facción; stock rotativo por ciudad
[15.9] Pendiente | Testing | Determinismo y contratos | Tests de drop tables y crafteo con RandomService.SetSeed; validación de contratos JSON (10.6)
[15.10] Pendiente | Telemetría | Métricas de crafting/drops | Tasas de éxito, consumo de materiales, progresión de skill de artesanía para balance futuro

16. ESTADO POR ARCHIVO / MÓDULO (inventario actual)  
----------------------------------------------------
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

- Combate (Pendiente):  
  - Pendiente: Motor/CombatePorTurnos.cs, Motor/MotorCombate.cs  
  - Pendiente/Parcial: Habilidades/{AtaqueFisico, AtaqueMagico, Hechizo, Habilidad, GestorHabilidades, HabilidadLoader} (faltan estados/orden por Velocidad y UI)

- Inventario y Personaje:  
  - Dominio Hecho: Personaje/{Personaje, AtributosBase, ExpAtributo, Estadisticas, Clase, ClaseData, HabilidadProgreso, FuenteBonificador}  
  - UI/Flujos Pendiente: Motor/MotorInventario.cs, Personaje/Inventario.cs (migrar a UI + mensajes consistentes)

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

17. HABILIDADES Y MAESTRÍAS
---------------------------
[17.1] Pendiente | Progresión por uso | Subir skill por tipo de arma/armadura/habilidad; bonifica precisión/daño/defensa | Integrar con ProgressionService y RandomService
[17.2] Pendiente | Árboles por arquetipo | Guerrero/Explorador/Mago con sinergias por atributos | Data JSON y evaluador de requisitos
[17.3] Pendiente | Costes y recursos | Mana/Concentración y cooldowns; recuperación y pociones | Hook a EnergiaService/recursos
[17.4] Pendiente | Gating | Requisitos por nivel de skill/atributo/clase/reputación | Validación en uso de skills

18. ITEMIZACIÓN AVANZADA
------------------------
[18.1] Pendiente | Afijos | Prefijos/Sufijos con rangos y rareza | Generador ponderado; validación de compatibilidades
[18.2] Pendiente | Únicos/Sets | Objetos con propiedades fijas y bonos por set | Data-driven; bonus 2/3/4 piezas
[18.3] Pendiente | Sockets/Gemas | Inserción/extracción con coste y riesgo | Interacción con crafteo y durabilidad
[18.4] Pendiente | Calidad | Calidad del ítem que escala stats y precio | Afecta reparación y drop rate
[18.5] Pendiente | Forja/Mejora | Mejora con probabilidad de fallo/retroceso | Integración con CraftingService

19. ECONOMÍA Y SINKS
--------------------
[19.1] Pendiente | Costes de viaje | Oro/energía por rutas largas/peligrosas | Balance con reputación/facción
[19.2] Pendiente | Entrenamiento avanzado | Tarifas en entrenadores por rango | Requiere reputación/licencias
[19.3] Pendiente | Reparación y mantenimiento | Tasas crecientes según nivel/calidad | Vinculado a 15.6
[19.4] Pendiente | Impuestos/Peajes | Zonas con peaje o tasa de comercio | Afecta ShopService
[19.5] Pendiente | Licencias/Gremios | Acceso a crafteo avanzado/áreas | Gating de sistemas
[19.6] Pendiente | Stock rotativo/eventos | Escasez/bonanza por ciudad/facción | Data eventos económicos

20. MUNDO DINÁMICO Y EXPLORACIÓN
---------------------------------
[20.1] Pendiente | Encuentros | Tablas por sector/bioma con hora/clima | Integración con GeneradorEnemigos
[20.2] Pendiente | Trampas/llaves | Cerraduras, llaves y trampas con detección | Usa Percepción/Agilidad
[20.3] Pendiente | Eventos ambientales | Cofres, santuarios, anomalías con cooldown | Ver 15.3 loot por sector

21. MISIONES CON CONSECUENCIAS
------------------------------
[21.1] Pendiente | Grafo con ramas | Rutas exclusivas por facción/decisiones | Impacta reputación y tiendas
[21.2] Pendiente | Recompensas significativas | Blueprints/llaves/accesos en lugar de solo oro/XP | Data y gating
[21.3] Pendiente | Persistencia de impacto | Cambios en NPC/stock/hostilidad | Servicio de mundo persistente

22. LOGROS Y RETOS
------------------
[22.1] Pendiente | Sistema de logros | Tracking de hitos y retos | Export/telemetría opcional
[22.2] Pendiente | Retos (ironman) | Muerte permanente/no usar X/tiempo límite | Flags de partida
[22.3] Pendiente | Recompensas leves | Cosméticos/QoL leve para no romper balance | Evitar pay-to-win

23. GUARDADO VERSIONADO Y MIGRACIONES
-------------------------------------
[23.1] Pendiente | Versionado | Save y datasets con versión | Comparador al cargar
[23.2] Pendiente | Migradores | Pasos entre versiones | Backups automáticos
[23.3] Pendiente | Compresión/Rotación | Archivos compactos y rotación de backups | Configurable

24. LOCALIZACIÓN (i18n)
-----------------------
[24.1] Pendiente | Desacoplar textos | Recursos por clave | Sustituir literales gradualmente
[24.2] Pendiente | Idiomas | Plantillas ES/EN | Selección de idioma en opciones
[24.3] Pendiente | Longitudes UI | Revisar cortes/colores por idioma | Con UIStyleService

25. PERFORMANCE Y CACHING
-------------------------
[25.1] Pendiente | Índices repos | Búsquedas por ID y nombre | Repositorios 7.x
[25.2] Pendiente | Lazy/Caché | Carga diferida e invalidación | Calentamiento en menús
[25.3] Pendiente | Reducir E/S | Minimizar lecturas de JSON | Batch y snapshots
[25.4] Pendiente | Pooling | Reutilizar entidades temporales (enemigos/efectos) | Combate y generación

26. ACCESIBILIDAD Y QoL
-----------------------
[26.1] Pendiente | Paleta accesible | Colorblind-safe en UIStyleService | Pruebas visuales
[26.2] Pendiente | Verbosidad | Niveles de detalle en UI/Logger | Configurable por jugador
[26.3] Pendiente | Confirmaciones | Acciones destructivas requieren confirmación | Consistente en menús

ESTADO ACTUAL (snapshot):
- Fundamentos base completos (1.1–1.6). GuardadoService reemplaza llamadas directas a GestorArchivos en Juego y Menús.
- ProgressionService extendido: recolección, entrenamiento y micro EXP de exploración integrada en movimiento (MostrarMenuRutas).
- Clave ExpBaseExploracion añadida a progression.json para balance.
- Personaje migrado a ExperienciaAtributos (3.1, 3.2) con campos legacy ignorados y migración automática.
- Mapas: selección inicial por CiudadPrincipal funcionando.
- Menú de rutas aplica experiencia de exploración correctamente.
- Sistema de clases dinámicas completo (11.2) con reputación integrada (12.1/12.2).
- Menú Admin implementado (13.1–13.4) facilita balance y QA.

- Testing: proyecto xUnit agregado (MiJuegoRPG.Tests). Pruebas de Mapa + ProgressionService + Recolección (cooldown multisector) + GeneradorEnemigos pasando (12/12). E/S de objetos aislada en tests a %TEMP% y limpieza automática. Logger permite reducir ruido en pruebas. Determinismo reforzado: SetSeed se aplica tras inicializar Juego y se desactivó la paralelización de xUnit (CollectionBehavior). Build OK tras migrar menús y Program a UI/Logger. Última corrida post-migración de servicios (Recolección/Energía/Estado/Misiones): 12/12 PASS.
- UI: Base de IUserInterface implementada con ConsoleUserInterface; InputService ya delega lectura/pausas a Juego.Ui y soporta TestMode (evita bloqueos en tests).
- Random: centralización completa y SetSeed disponible para determinismo en pruebas.

- Reputación: bandas y colores parametrizados (DatosJuego/config/reputacion_bandas.json).
- Políticas de bloqueo centralizadas (NPC/Tienda) en servicio `ReputacionPoliticas` con config en `DatosJuego/config/reputacion_politicas.json`.
- Menús muestran etiquetas compactas de reputación con color y valor numérico en Ciudad, Tienda, NPCs y Misiones.
- Gating por reputación negativa activo y alineado a bandas en NPC y Tienda.
- Normalización de ubicaciones a IDs de sector aplicada en menús y tienda (compatibilidad con nombres durante migración de datos).
 - Rutas centralizadas: nuevo `PathProvider` define carpetas canónicas de DatosJuego/PjDatos; eliminado código ad-hoc de combinaciones de rutas en servicios clave.

MÉTRICAS / OBS (para futura instrumentación ligera):
- Clases desbloqueadas por sesión y top motivos bloqueo.
- Frecuencia ajustes reputación (detección abuso admin).
- Atributos manualmente más alterados (apoyo tuning progresión).

PRÓXIMOS PASOS SUGERIDOS (reordenados tras avances):

1) (4.2) Extender nodos: campos JSON sugeridos: Rareza (Comun/Raro/Épico), ProduccionMin/ProduccionMax, CooldownBase.
	- Persistir cooldown opcional: guardar timestamp último uso en guardado rápido (marcar diseño).
2) (2.3/8.3) Completar migración UI: CombatePorTurnos, Inventario/Personaje, Gestores (Armas/Pociones/Materiales), GuardadoService interactivo; introducir UIStyleService.
3) (7.1) IRepository<T> base (LoadAll, SaveAll, GetById) + implementación JSON simple (sin cache) para Misiones como piloto.
4) (4.2) Balance recolección: aplicar Rareza en nodos/materiales y rango ProducciónMin/Max de forma consistente; telemetría mínima de éxito/fallo.
5) (10.6) Validador JSON: verificar referenciales (IDs de mapa, facciones, misiones, objetos) + test de contrato por archivo.
6) (12.5) Métricas reputación: contador de eventos de reputación y cambios por facción; export opcional a CSV/JSON.
7) (Datos) Completar unificación a IDs de mapa en npc.json y facciones_ubicacion.json; mantener compatibilidad durante migración.
8) (14.x) Preparación Unity: esqueleto de adaptadores (UI/Logger/Input) y script de conversión JSON→SO (diseño).
9) (15.1–15.3) Iniciar base de objetos/drops: definir esquema JSON y repos; piloto con 1 enemigo y 1 sector; tests de contrato.
10) (15.4) Esqueleto de crafteo: recetas mínimas + consumo de energía + fallo/éxito; UI básica integrada.
11) (15.5–15.6) Dismantle y reparación: flujo y balance inicial; logs para telemetría (15.10).

NOTAS RIESGO / DEPENDENCIAS:
- Persistir cooldown requiere definir formato (epoch segundos o ISO8601) y limpiar cooldowns expirados al cargar.
- IUserInterface debe entrar antes de colorear UI (8.3) para evitar rehacer cambios.
- Repositorios: migrar uno (Misiones) antes de aplicar a Enemigos/Objetos para validar patrón.

— Fin snapshot actualizado —
