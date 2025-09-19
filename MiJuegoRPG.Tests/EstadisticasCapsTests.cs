using MiJuegoRPG.Personaje;
using MiJuegoRPG.Motor.Servicios;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class EstadisticasCapsTests
    {
        private static AtributosBase CrearAtributosSuperAltos()
        {
            return new AtributosBase
            {
                Destreza = 999,
                Percepcion = 999,
                Sabiduría = 999,
                Suerte = 999,
                Inteligencia = 50,
                Fuerza = 50,
                Vitalidad = 50,
                Defensa = 50,
                Resistencia = 50,
                Agilidad = 50,
                Fe = 0,
                Voluntad = 0,
                Carisma = 0,
                Liderazgo = 0,
                Persuasion = 0
            };
        }

        [Fact]
        public void Estadisticas_RespetaCaps_PorDefecto()
        {
            // Asegurar carga de caps por defecto (sin necesidad de StatsCaps en JSON)
            CombatBalanceConfig.EnsureLoaded();

            var a = CrearAtributosSuperAltos();
            var e = new Estadisticas(a);

            Assert.InRange(e.Precision, 0.0, CombatBalanceConfig.PrecisionMax + 1e-9);
            Assert.InRange(e.CritChance, 0.0, CombatBalanceConfig.CritChanceMax + 1e-9);
            Assert.InRange(e.CritMult, CombatBalanceConfig.CritMultMin - 1e-9, CombatBalanceConfig.CritMultMax + 1e-9);
            Assert.InRange(e.Penetracion, 0.0, CombatBalanceConfig.PenetracionMax + 1e-9);
        }
    }
}
