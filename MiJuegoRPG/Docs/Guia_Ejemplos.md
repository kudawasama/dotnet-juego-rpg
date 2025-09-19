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
