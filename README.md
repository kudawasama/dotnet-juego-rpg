# Mi Juego RPG

Un juego de rol clásico desarrollado en .NET con arquitectura modular y diseño extensible.

## 🎮 Características

- **Sistema de Personajes**: Creación y progresión por actividad
- **Combate por Turnos**: Sistema táctico con diferentes tipos de enemigos
- **Inventario Dinámico**: Objetos con rareza, perfección y propiedades especiales
- **Exploración**: Mundo imaginario con ciudades, rutas y mazmorras
- **Sistema de Guardado**: Persistencia de progreso en archivos JSON
- **Arquitectura Modular**: Código organizado y fácilmente extensible

## 🛠️ Tecnologías

- **.NET 8.0**: Framework principal
- **C#**: Lenguaje de programación
- **JSON**: Serialización de datos y configuración
- **SQLite**: Base de datos (Microsoft.Data.Sqlite)

## 📁 Estructura del Proyecto

```
MiJuegoRPG/
├── Motor/                  # Engine principal del juego
│   ├── Menus/             # Sistemas de menús
│   ├── Acciones/          # Acciones del jugador
│   └── Servicios/         # Lógica de negocio
├── Personaje/             # Sistema de personajes
├── Enemigos/              # Sistema de combate y enemigos
├── Objetos/               # Items y equipamiento
├── Interfaces/            # Contratos y abstracciones
├── DatosJuego/           # Configuración JSON
├── PjDatos/              # Datos de personajes guardados
└── Program.cs            # Punto de entrada
```

## 🚀 Cómo Ejecutar

1. **Prerrequisitos**: .NET 8.0 SDK
2. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/kudawasama/dotnet-juego-rpg.git
   cd dotnet-juego-rpg
   ```
3. **Ejecutar el juego**:
   ```bash
   cd MiJuegoRPG
   dotnet run
   ```

## 🎲 Cómo Jugar

1. **Crear Personaje**: Elige nombre y comienza como Aventurero
2. **Explorar**: Descubre nuevas ubicaciones y eventos aleatorios
3. **Combatir**: Enfrenta enemigos en combate por turnos
4. **Progresar**: Los atributos mejoran según las actividades realizadas
5. **Gestionar Inventario**: Recolecta y equipate objetos
6. **Guardar Progreso**: Guarda tu personaje para continuar después

## 🏗️ Arquitectura

### Principios de Diseño
- **Modularidad**: Cada sistema está separado en su propio namespace
- **Extensibilidad**: Fácil agregar nuevos tipos de objetos, enemigos o mecánicas
- **Separación de Responsabilidades**: Motor, datos y lógica claramente separados
- **Configuración Externa**: Datos del juego en archivos JSON modificables

### Patrones Implementados
- **Strategy Pattern**: Para diferentes tipos de objetos y comportamientos
- **Factory Pattern**: Generación de objetos y enemigos aleatorios
- **Observer Pattern**: Sistema de eventos del juego
- **Command Pattern**: Sistema de acciones del jugador

## 📄 Documentación Adicional

- [Diseño del Juego](GAME_DESIGN.md) - Mecánicas y sistema de juego detallado
- [Roadmap](ROADMAP.md) - Funcionalidades planeadas y prioridades

## 🤝 Contribuir

1. Fork el proyecto
2. Crea una rama para tu funcionalidad (`git checkout -b feature/nueva-funcionalidad`)
3. Commit tus cambios (`git commit -m 'Agregar nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request

## 📋 Buenas Prácticas

- **Código Limpio**: Nombres descriptivos y métodos pequeños
- **Testing**: Cada nueva funcionalidad debe incluir pruebas
- **Documentación**: Comentar código complejo y actualizar documentación
- **Git**: Commits atómicos con mensajes descriptivos

## 🐛 Reportar Problemas

Si encuentras un bug o tienes una sugerencia, por favor [abre un issue](https://github.com/kudawasama/dotnet-juego-rpg/issues).

## 📜 Licencia

Este proyecto está bajo la licencia MIT. Ver [LICENSE](LICENSE) para más detalles.

---

**¡Disfruta el juego y contribuye a hacerlo aún mejor!** 🎮✨