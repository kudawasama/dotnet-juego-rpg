using System;
using System.Linq;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor.Acciones
{
    public class UsarPocionAccion : IAccionCombate
    {
        public string Nombre => "Usar Poción";
        public int CostoMana => 0;

        private readonly int _indiceInventario; // índice de la poción en una lista prefiltrada (externa)

        public UsarPocionAccion(int indiceInventario)
        {
            _indiceInventario = indiceInventario;
        }

        public ResultadoAccion Ejecutar(ICombatiente ejecutor, ICombatiente objetivo)
        {
            var res = new ResultadoAccion
            {
                NombreAccion = Nombre,
                Ejecutor = ejecutor,
                Objetivo = objetivo,
                DanioBase = 0,
                DanioReal = 0,
                EsMagico = false,
            };
            if (ejecutor is not MiJuegoRPG.Personaje.Personaje pj)
            {
                res.Mensajes.Add("No puedes usar pociones con este combatiente.");
                return res;
            }
            if (pj.Vida >= pj.VidaMaxima)
            {
                res.Mensajes.Add("Tu vida ya está al máximo.");
                return res;
            }
            // Filtra las pociones disponibles nuevamente para evitar desalineaciones
            var pociones = pj.Inventario.NuevosObjetos
                .Where(o => o.Objeto is MiJuegoRPG.Objetos.Pocion && o.Cantidad > 0)
                .ToList();
            if (pociones.Count == 0 || _indiceInventario < 0 || _indiceInventario >= pociones.Count)
            {
                res.Mensajes.Add("No tienes pociones disponibles.");
                return res;
            }
            var entry = pociones[_indiceInventario];
            var pocion = (MiJuegoRPG.Objetos.Pocion)entry.Objeto;
            int vidaAntes = pj.Vida;
            int curacion = Math.Max(0, Math.Min(pocion.Curacion, pj.VidaMaxima - pj.Vida));
            pj.Vida += curacion;
            entry.Cantidad--;
            if (entry.Cantidad <= 0)
            {
                pj.Inventario.NuevosObjetos.Remove(entry);
            }
            res.Mensajes.Add($"Usaste {pocion.Nombre}: {vidaAntes} → {pj.Vida} HP.");
            return res;
        }
    }
}
