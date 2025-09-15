# Catálogo de Enemigos (Data-Driven)

Puedes organizar enemigos en esta carpeta de la forma que prefieras. El juego carga todos los `*.json` recursivamente, por lo que cualquier subcarpeta será leída automáticamente.

 Organización recomendada (principal):

- por_bioma/
  - bosque/
  - montana/
  - pantano/
 
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

Consejos de balance para este proyecto:

- Mantén `Mitigacion*` moderadas (0.05..0.30) salvo elites/jefes.
- Evita dar muchas inmunidades a la vez, salvo arquetipos concretos.
- Alinea `Nivel` y recompensas con `DatosJuego/progression_config.md` para sostener progresión lenta.
