Estructura de datos de Equipo (per-item)

Esta carpeta soporta dos modos de carga:

- Archivos agregados por tipo (compatibilidad): `armas.json`, `Armaduras.json`, `Accesorios.json`, etc.
- Modo recomendado (per-item): subcarpetas por tipo con uno o varios JSON por ítem o lista.

Subcarpetas sugeridas:

- armas/
- armaduras/
- accesorios/
- botas/
- cascos/
- cinturones/
- collares/
- pantalones/

Formato JSON por ítem (ejemplos):

Arma (`armas/espada_corta.json`):

{
  "Nombre": "Espada Corta",
  "Daño": 6,
  "NivelRequerido": 1,
  "Valor": 12,
  "Tipo": "UnaMano",
  "Rareza": "Normal",
  "Perfeccion": 50
}

Armadura (`armaduras/cota_cuero.json`):

{
  "Nombre": "Cota de Cuero",
  "Defensa": 4,
  "Nivel": 1,
  "TipoObjeto": "Armadura",
  "Rareza": "Pobre",
  "Perfeccion": 45
}

Casco (`cascos/casco_cuero.json`):

{
  "Nombre": "Casco de Cuero",
  "Defensa": 2,
  "Nivel": 1,
  "TipoObjeto": "Cabeza",
  "Rareza": "Pobre",
  "Perfeccion": 48
}

Notas:

- Los archivos pueden contener un objeto único o una lista de objetos del mismo tipo.
- Los campos `Rareza` aceptan valores del enum `Objetos.Rareza`: Rota, Pobre, Normal, Superior, Rara, Legendaria, Ornamentada. El valor "Comun" será interpretado como `Normal` por compatibilidad.
- La perfección ajusta los valores finales (daño/defensa) con la fórmula actual `valor * (Perfeccion / 50.0)`.

Selección ponderada por rareza:

- Por defecto, el generador usa pesos conservadores para favorecer ítems comunes: Rota=50, Pobre=35, Normal=20, Superior=7, Rara=3, Legendaria=1, Ornamentada=1.
- Puedes desactivar esta política en runtime ajustando `GeneradorObjetos.UsaSeleccionPonderadaRareza = false`.

Configuración de pesos de rareza (rareza_pesos.json):

- Para ajustar la probabilidad relativa de aparición por rareza sin recompilar, crea `DatosJuego/config/rareza_pesos.json`.
- Compatibilidad: si no existe ahí, se intenta `DatosJuego/Equipo/rareza_pesos.json`.
- Formatos soportados:

  Objeto

  {
    "Rota": 50,
    "Pobre": 35,
    "Normal": 20,
    "Superior": 7,
    "Rara": 3,
    "Legendaria": 1,
    "Ornamentada": 1
  }

  Lista de entradas

  [
    { "Nombre": "Rota", "Peso": 50 },
    { "Nombre": "Pobre", "Peso": 35 },
    { "Nombre": "Normal", "Peso": 20 },
    { "Nombre": "Superior", "Peso": 7 },
    { "Nombre": "Rara", "Peso": 3 },
    { "Nombre": "Legendaria", "Peso": 1 },
    { "Nombre": "Ornamentada", "Peso": 1 }
  ]

Notas:

- Claves toleran alias/acentos: "Comun"/"Común" → Normal; "Raro" → Rara.
- Si el archivo no existe o es inválido, se usan los defaults conservadores mostrados arriba.
- La selección ponderada se aplica a todos los tipos de equipo (armas, armaduras, cascos, botas, cinturones, collares y pantalones).

Base de Perfección (Normal=50%):

- La perfección escala los valores del equipo con la fórmula `valorFinal = round(valorBase * (Perfeccion / 50.0))`.
- Interpretación: la rareza Normal fija `Perfeccion=50` como base (100% del valor base); rarezas inferiores bajan el rango, superiores lo elevan.

Rangos de Perfección por Rareza (rareza_perfeccion.json):

- Define los rangos permitidos de Perfección para cada rareza en `DatosJuego/config/rareza_perfeccion.json` (preferido). Fallback: `DatosJuego/Equipo/rareza_perfeccion.json`.
- Formatos soportados:
  - Objeto con arrays: `{ "Rota": [10,20], "Pobre": [20,49], ... }`
  - Objeto con Min/Max: `{ "Rota": {"Min":10, "Max":20}, ... }`
  - Lista de entradas: `[ { "Nombre": "Rota", "Min": 10, "Max": 20 }, ... ]`
- Si no existe, se usan los defaults conservadores: Rota(10–20), Pobre(20–49), Normal(50), Superior(51–60), Rara(61–75), Legendaria(75–89), Ornamentada(90–100).

Esquema extendido “Arma v2” (opcional y compatible):

- Soporta definir rangos de Nivel (`NivelMin`/`NivelMax`), rarezas permitidas (`RarezasPermitidasCsv`), perfección en rango (`PerfeccionMin`/`PerfeccionMax`), daño en rango (`DañoMin`/`DañoMax`) y/o por canales (`DañoFisico`/`DañoMagico`), y metadatos ricos (crítico, penetración, precisión, velocidad, bonificadores, efectos, requisitos, economía, peso/durabilidad, tags, descripción).
- Si estos campos no están, el sistema mantiene el comportamiento anterior (compatibilidad total).

Ejemplo:

{
  "Nombre": "Espada de Hierro",
  "Daño": 12,
  "DañoMin": 10,
  "DañoMax": 14,
  "NivelRequerido": 2,
  "NivelMin": 2,
  "NivelMax": 4,
  "Tipo": "UnaMano",
  "Rareza": "Normal",
  "RarezasPermitidasCsv": "Pobre, Normal, Superior",
  "Perfeccion": 50,
  "PerfeccionMin": 45,
  "PerfeccionMax": 65,
  "CriticoProbabilidad": 0.07,
  "CriticoMultiplicador": 1.6,
  "Penetracion": 0.05,
  "Precision": 0.02,
  "VelocidadAtaque": 1.0,
  "BonificadoresAtributos": { "Fuerza": 1 },
  "BonificadoresEstadisticas": { "Defensa": 0 },
  "Efectos": [
    { "Tipo": "OnHit", "Nombre": "Sangrado", "Probabilidad": 0.05, "Potencia": 1, "DuracionTurnos": 2 }
  ],
  "HabilidadesOtorgadas": [],
  "Requisitos": { "Fuerza": 4, "Nivel": 2 },
  "ValorVenta": 5,
  "Peso": 2.4,
  "Durabilidad": 60,
  "Descripcion": "Espada de hierro forjada; fiable y equilibrada.",
  "Tags": ["espada", "hierro", "una_mano"]
}

Notas de estandarización:

- Puedes dejar `Rareza` en "Normal" y especificar `RarezasPermitidasCsv` para que la instancia se elija ponderada dentro de ese subconjunto.
- Usa rangos de `DañoMin/Max` y `NivelMin/Max` para variedad controlada; el escalado por `Perfeccion` se aplica después.
