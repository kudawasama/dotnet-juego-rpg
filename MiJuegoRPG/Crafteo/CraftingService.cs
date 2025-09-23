using System;
using System.Collections.Generic;
using MiJuegoRPG.Objetos;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Crafteo
{
    public class RecetaCrafteo
    {
        public required string Nombre { get; set; }
        public List<(string nombreMaterial, int cantidad)> Materiales { get; set; } = new();
        public required Objeto ObjetoResultado { get; set; }
    }

    

    public class CraftingService
    {
        private readonly List<RecetaCrafteo> _recetas = new();

        public void AgregarReceta(RecetaCrafteo receta)
        {
            _recetas.Add(receta);
        }

        public IEnumerable<RecetaCrafteo> ObtenerRecetasDisponibles()
        {
            return _recetas;
        }

        public bool PuedeCraftear(Personaje.Personaje jugador, RecetaCrafteo receta)
        {
            foreach (var (nombre, cantidad) in receta.Materiales)
            {
                if (jugador.Inventario.ContarMaterial(nombre) < cantidad)
                    return false;
            }
            return true;
        }

        public bool Craftear(Personaje.Personaje jugador, RecetaCrafteo receta, out string mensaje)
        {
            if (!PuedeCraftear(jugador, receta))
            {
                mensaje = "No tienes los materiales necesarios.";
                return false;
            }
            foreach (var (nombre, cantidad) in receta.Materiales)
            {
                jugador.Inventario.ConsumirMaterial(nombre, cantidad);
            }
            jugador.Inventario.AgregarObjeto(receta.ObjetoResultado);
            mensaje = $"¡Has creado {receta.ObjetoResultado.Nombre}!";
            // Hook de acciones: registrar crafteo exitoso
            try { if (jugador != null) MiJuegoRPG.Motor.Servicios.AccionRegistry.Instancia.RegistrarAccion("CraftearObjeto", jugador); } catch { }
            return true;
        }
    }
}
