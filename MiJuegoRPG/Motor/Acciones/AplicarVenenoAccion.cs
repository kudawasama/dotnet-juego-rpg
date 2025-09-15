using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor.Acciones
{
    public class AplicarVenenoAccion : IAccionCombate
    {
        public string Nombre => "Aplicar Veneno";
        public int CostoMana => 3;
        public int CooldownTurnos => 3;

        private readonly int _danioPorTurno;
        private readonly int _duracion;

        public AplicarVenenoAccion(int danioPorTurno = 4, int duracion = 3)
        {
            _danioPorTurno = danioPorTurno;
            _duracion = duracion;
        }

        public ResultadoAccion Ejecutar(ICombatiente ejecutor, ICombatiente objetivo)
        {
            var res = new ResultadoAccion
            {
                NombreAccion = Nombre,
                Ejecutor = ejecutor,
                Objetivo = objetivo
            };
            // Chequear inmunidad básica a veneno (no-muertos, etc.)
            bool inmune = false;
            if (objetivo is MiJuegoRPG.Enemigos.Enemigo ene)
            {
                // Inmunidad declarativa por diccionario o por Tag/Nombre heurístico
                try
                {
                    if (ene.Inmunidades != null && ene.Inmunidades.TryGetValue("veneno", out var val) && val)
                        inmune = true;
                }
                catch { }
                if (!inmune)
                {
                    var tag = (ene.Tag ?? ene.Nombre ?? string.Empty).ToLowerInvariant();
                    if (tag.Contains("zombi") || tag.Contains("zombie") || tag.Contains("esqueleto") || tag.Contains("undead") || tag.Contains("no-muerto"))
                        inmune = true;
                }
            }

            if (inmune)
            {
                res.Mensajes.Add($"{objetivo.Nombre} es inmune al Veneno.");
                return res;
            }

            var efecto = new EfectoVeneno(_danioPorTurno, _duracion, magico: true);
            res.EfectosAplicados.Add(efecto);
            res.Mensajes.Add($"{ejecutor.Nombre} aplica Veneno a {objetivo.Nombre} por {_duracion} turnos.");
            return res;
        }
    }
}
