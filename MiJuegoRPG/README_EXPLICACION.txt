# Explicación y Ejemplo de Sistema RPG Versátil

## 1. Mapa y Exploración
- El juego usa un "Mapa Imaginario" donde cada ubicación es un nodo (ciudad, ruta, mazmorra).
- El menú muestra las opciones disponibles según lo desbloqueado.
- Ejemplo de menú:

=== Ciudad de Albor ===
1. Tienda
2. Escuela de Entrenamiento
3. Explorar sector
4. Descansar en posada
5. Salir de la ciudad

Al explorar sector, pueden ocurrir eventos:
- Encuentro con enemigo
- Descubrir objeto
- Encontrar mazmorra
- Encontrar NPC con misión
- Evento especial (tormenta, robo, emboscada)

Salir de la ciudad muestra rutas:
1. Camino al Bosque Oscuro
2. Camino al Río Plateado (requiere barco)
3. Camino a Ciudad Bruma

Las rutas pueden tener requisitos y cambiar de estado (segura/insegura).

## 2. Progresión por Actividad
- No hay clase inicial, todos empiezan como Aventurero.
- Los atributos suben por actividad:
  - Fuerza: entrenar, combatir, cargar objetos
  - Destreza: usar armas ligeras, esquivar, explorar
  - Inteligencia: estudiar, usar magia, resolver acertijos
  - Resistencia: viajar, recibir daño, trabajar
- Cada actividad suma XP al atributo relacionado.
- Ejemplo: Al combatir, ganas XP en Fuerza y Resistencia.
- Cuando un atributo llega a cierto valor, se desbloquean opciones de clase/título:
  - STR >= 20 + END >= 15 → Guerrero
  - INT >= 20 + DEX >= 10 → Mago de Batalla
  - DEX >= 20 + END >= 15 → Explorador

## 3. Profesiones y Especializaciones
- Puedes entrenar profesiones (herrero, herbolaria, domador, alquimista, cazador, explorador).
- Al llegar a 100 puntos en una profesión, te especializas y obtienes beneficios únicos.

## 4. Misiones de NPC y Eventos
- Los NPC pueden ofrecer misiones que desbloquean rutas, tiendas o eventos.
- Ejemplo de misión:
  - "Ayuda al herrero a encontrar mineral raro."
  - Recompensa: acceso a la tienda de armas especiales.
- Eventos globales (invasiones, ferias, sequías) pueden cambiar el mapa y la economía.

## 5. Economía Dinámica
- Los precios de objetos pueden variar según la ciudad y los eventos globales.
- Ejemplo: En Ciudad Bruma, las pociones mágicas son más baratas durante la feria.

## 6. Mazmorras y Cambios Dinámicos
- Mazmorras pueden cambiar tras derrotar al jefe (convertirse en base, tienda, etc.).
- El estado del mundo se guarda y afecta futuras exploraciones.

## 7. Guardado y Organización
- Todos los datos se guardan en la carpeta `PjDatos/` y subcarpetas.
- Nunca se guardan datos en `bin` ni `obj`.
- El sistema es expansible y modular.

---

## Ejemplo de Progresión por Actividad (C#)
```csharp
// Al combatir:
jugador.Entrenar("fuerza");
jugador.Entrenar("resistencia");
// Al estudiar:
jugador.Entrenar("inteligencia");
// Al explorar:
jugador.Entrenar("destreza");
```

## Ejemplo de Menú de Ubicación
```csharp
Console.WriteLine($"=== {ubicacionActual.Nombre} ===");
Console.WriteLine(ubicacionActual.Descripcion);
foreach (var evento in ubicacionActual.EventosPosibles)
{
    Console.WriteLine(evento);
}
```

## Ejemplo de Misión
```csharp
var mision = new Mision {
    Nombre = "Mineral Raro",
    Descripcion = "Ayuda al herrero a encontrar mineral raro en el Bosque Oscuro.",
    Estado = "No iniciada",
    Requisitos = new List<string> { "Bosque Oscuro desbloqueado" },
    Recompensas = new List<string> { "Acceso a tienda especial" },
    UbicacionNPC = "Ciudad de Albor",
    Destino = "Bosque Oscuro",
    DesbloqueaRuta = true,
    RutaDesbloqueada = "Camino a la Mina"
};
```

# Ejemplo de integración de misiones, economía y eventos

## Ejemplo de integración de misiones en el bucle principal:
```csharp
// En el menú principal del juego
if (ubicacionActual.EventosPosibles.Contains("NPC con misión"))
{
    NPC npc = new NPC { Nombre = "Herrero", Ubicacion = ubicacionActual.Nombre };
    Mision mision = new Mision {
        Nombre = "Mineral raro para el herrero",
        Descripcion = "Encuentra el mineral en la mazmorra cercana.",
        Recompensas = new List<string> { "Acceso a tienda especial" },
        UbicacionNPC = ubicacionActual.Nombre,
        Destino = "Mazmorra de la Cueva",
        DesbloqueaRuta = true,
        RutaDesbloqueada = "Camino a la Cueva"
    };
    npc.Misiones.Add(mision);
    // Mostrar misión al jugador y permitir aceptarla
}
```

## Ejemplo de economía dinámica:
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

## Ejemplo de evento global que afecta el mundo:
```csharp
if (eventoGlobal == "Invasión goblin")
{
    foreach (var ubicacion in estadoMundo.Ubicaciones)
    {
        if (ubicacion.Tipo == "Ciudad")
            ubicacion.EventosPosibles.Add("Defender ciudad");
    }
}
```

---

Este sistema te permite expandir el juego fácilmente, agregar nuevas ubicaciones, profesiones, misiones y eventos sin modificar la estructura base.
