using System;
using System.Collections.Generic;
using MiJuegoRPG.Objetos;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Crafteo
{
    // SA1402: RecetaCrafteo se movió a RecetaCrafteo.cs para cumplir con SA1402 (un tipo por archivo)

    public class CraftingService
    {
        private readonly List<RecetaCrafteo> recetas = new();

        public void AgregarReceta(RecetaCrafteo receta)
        {
            recetas.Add(receta);
        }

        public IEnumerable<RecetaCrafteo> ObtenerRecetasDisponibles()
        {
            return recetas;
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
            try
            {
                if (jugador != null)
                    MiJuegoRPG.Motor.Servicios.AccionRegistry.Instancia.RegistrarAccion("CraftearObjeto", jugador);
            }
            catch { }
            return true;
        }
    }
}
