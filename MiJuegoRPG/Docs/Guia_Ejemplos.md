# Guía de ejemplos (para principiantes)

Esta guía reúne ejemplos prácticos de uso del juego para aprender más rápido las mecánicas.

## Menú y navegación

Ejemplo de menú de ciudad y opciones disponibles:

```
=== Ciudad Principal ===
1. Tienda
2. Escuela de Entrenamiento
3. Explorar sector
4. Descansar en posada
5. Salir de la ciudad
```

Al explorar sector pueden ocurrir eventos como: combate, objetos, mazmorras, NPC con misión o eventos especiales.

## Progresión por actividad (C#)

```csharp
// Al combatir
jugador.Entrenar("fuerza");
jugador.Entrenar("resistencia");
// Al estudiar
jugador.Entrenar("inteligencia");
// Al explorar
jugador.Entrenar("destreza");
```

## Misiones (C#)

```csharp
var mision = new Mision {
    Nombre = "Mineral Raro",
    Descripcion = "Ayuda al herrero a encontrar mineral raro en el Bosque.",
    Estado = "No iniciada",
    Requisitos = new List<string> { "Bosque desbloqueado" },
    Recompensas = new List<string> { "Acceso a tienda especial" },
    UbicacionNPC = "Ciudad Principal",
    Destino = "Bosque",
    DesbloqueaRuta = true,
    RutaDesbloqueada = "Camino a la Mina"
};
```

## Economía dinámica (C#)

```csharp
// Al comprar en tienda
if (eventoGlobal == "Feria mágica")
{
    precioPocionMagica = precioBase * 0.7;
}
else
{
    precioPocionMagica = precioBase;
}
```

## Eventos globales (C#)

```csharp
if (eventoGlobal == "Invasión goblin")
{
    foreach (var ubicacion in estadoMundo.Ubicaciones)
    {
        if (ubicacion.Tipo == "Ciudad")
        {
            ubicacion.EventosPosibles.Add("Defender ciudad");
        }
    }
}
```

## Consejos rápidos

- Usa el menú Admin para probar sistemas rápidamente.
- Activa el logger con `--log-level=debug` cuando quieras ver más detalle.
- Para datos de prueba, usa validadores y herramientas de QA desde CLI.

## Menú Admin y atajos útiles

- Opción 21: Cambiar clase ACTIVA (no rebonifica). Útil para probar builds sin alterar bonos.
- Opción 22: Dar objeto/equipo/material por nombre. Acepta `tipo:nombre` o solo nombre; muestra coincidencias y permite equipar.
- Atajo GM: escribe `gm:set` para recibir y equipar el set completo GM (útil para QA extremo).

## Habilidades por archivo y uso en combate

- Organización recomendada: un archivo por habilidad bajo `DatosJuego/habilidades/<tema>/` (ej.: `Hab_Fisicas/GolpeFuerte.json`).
- El juego mapea habilidades aprendidas a acciones de combate automáticamente (`HabilidadAccionMapper`).
- En combate, usa la opción "Habilidad" para ver las que son usables con su coste/CD; al usarlas, ganan EXP y pueden evolucionar si cumplen condiciones.

## Acciones de Mundo (Energía + Tiempo)

Fuera de combate, ciertas acciones consumen Energía y Tiempo y pueden estar condicionadas por políticas de la zona (ciudad, parte de ciudad, ruta).

Ejemplo 1: Intentar robar en Ciudad (bloqueado por política)

- Acción: robar_intento
- Zona: Ciudad (centro)
- Resultado esperado: Política lo marca como no permitido; no consume recursos; muestra aviso y, si existiera configuración de “riesgo” en ese sector, podría disparar consecuencias si el diseño lo permite. Por defecto: bloqueado sin efectos.

Ejemplo 2: Intentar robar en Ruta (permitido, con riesgo)

- Acción: robar_intento
- Zona: Ruta
- Costes: Energía 8; Tiempo 3 min
- Resultado: Se evalúa detección (ej. 25%). Si es detectado, aplica consecuencias del delito (reputación con guardia -5, posible multa); si no, éxito silencioso con posibles recompensas o progresión.

Notas rápidas

- Las políticas por zona gobiernan si una acción está permitida, si es arriesgada y cuáles son sus consecuencias por defecto.
- Los catálogos y políticas propuestos están documentados en Docs/Resumen_Datos.md (secciones 28–30) y el flujo/contratos en Docs/Arquitectura_y_Funcionamiento.md.

Última actualización: 2025-10-15
