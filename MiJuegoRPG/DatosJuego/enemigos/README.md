# Catálogo de Enemigos (Data-Driven)

Puedes organizar enemigos en esta carpeta de la forma que prefieras. El juego carga todos los `*.json` recursivamente, por lo que cualquier subcarpeta será leída automáticamente.

 Organización recomendada (principal):

- por_bioma/
  - bosque/
  - montana/
  - pantano/

Estructura detallada sugerida por bioma/nivel/categoría:

- por_bioma/
  - bosque/
    - nivel_1_3/
      - normal/
      - elite/
      - jefe/
      - campo/
      - legendario/
      - unico/
      - mundial/
  - nivel_4_6/ (etc.)

Alternativas opcionales (si te sirven para edición interna, pero no dupliques enemigos):

- por_familia/
  - no_muerto/
  - bestia/
  - elemental/
- por_nivel/
  - nivel_1/
  - nivel_2_3/
  - nivel_4_6/

Reglas:

- Un archivo puede contener un único objeto `EnemigoData` o una lista `EnemigoData[]`.
- Campos admitidos extra: `Inmunidades`, `MitigacionFisicaPorcentaje`, `MitigacionMagicaPorcentaje`, `Tags`, `Id`.
- Si `Familia` es `NoMuerto` y no defines `Inmunidades.veneno`, el motor lo aplica por defecto (juego difícil y lógico con la fantasía).
- Evita duplicar el mismo enemigo en varias rutas; usa `Tags` para clasificaciones cruzadas (p.ej., `"bosque"`, `"no-muerto"`).

Plantilla JSON recomendada (mínimos y campos extendidos):

```json
{
  "Nombre": "Lobo del Bosque",
  "VidaBase": 40,
  "AtaqueBase": 10,
  "DefensaBase": 4,
  "DefensaMagicaBase": 2,
  "Nivel": 2,
  "ExperienciaRecompensa": 8,
  "OroRecompensa": 4,
  "Familia": "Bestia",
  "Rareza": "Comun",
  "Categoria": "Normal",
  "ArmaNombre": "Mordisco",
  "MitigacionFisicaPorcentaje": 0.08,
  "MitigacionMagicaPorcentaje": 0.04,
  "Tags": ["lobo", "bosque", "bestia"],
  "ResistenciasElementales": { "hielo": 0.10 },
  "DanioElementalBase": { "sangrado": 2 },
  "EquipoInicial": { "Arma": "Mordisco" },
  "EvasionFisica": 0.20,
  "EvasionMagica": 0.10,
  "SpawnWeight": 5,
  "Drops": [
    { "Tipo": "Material", "Nombre": "Colmillo de Lobo", "Rareza": "Normal", "Chance": 0.15, "CantidadMin": 1, "CantidadMax": 2 }
  ]
}
```

Notas de diseño (alineadas al proyecto):

- Progresión lenta: mantener recompensas bajas y estadísticas ajustadas; elites/jefes deben ser un reto significativo.
- Mundos/biomas: los archivos por categoría ayudan a balancear distribuciones y pesos de aparición.

Consejos de balance para este proyecto:

- Mantén `Mitigacion*` moderadas (0.05..0.30) salvo elites/jefes.
- Evita dar muchas inmunidades a la vez, salvo arquetipos concretos.
- Alinea `Nivel` y recompensas con `DatosJuego/progression_config.md` para sostener progresión lenta.
