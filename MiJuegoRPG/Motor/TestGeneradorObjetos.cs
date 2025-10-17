using System;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Motor
{
    public static class TestGeneradorObjetos
    {
        public static void Probar()
        {
            // Carga automática: intenta leer per-item bajo DatosJuego/Equipo/<tipo>/**.json
            // y si no existen carpetas, cae a los JSON agregados por tipo.
            GeneradorObjetos.CargarEquipoAuto();

            Console.WriteLine("--- Prueba de generación de objetos aleatorios ---");
            var arma = GeneradorObjetos.GenerarArmaAleatoria(1);
            Console.WriteLine($"Arma: {arma.Nombre}, Daño: {arma.DañoFisico}/{arma.DañoMagico}, Rareza: {arma.Rareza}, Perfección: {arma.Perfeccion}");

            var armadura = GeneradorObjetos.GenerarArmaduraAleatoria(1);
            Console.WriteLine($"Armadura: {armadura.Nombre}, Defensa: {armadura.Defensa}, Rareza: {armadura.Rareza}, Perfección: {armadura.Perfeccion}");

            var casco = GeneradorObjetos.GenerarCascoAleatorio(1);
            Console.WriteLine($"Casco: {casco.Nombre}, Defensa: {casco.Defensa}, Rareza: {casco.Rareza}, Perfección: {casco.Perfeccion}");

            var accesorio = GeneradorObjetos.GenerarAccesorioAleatorio(1);
            Console.WriteLine($"Accesorio: {accesorio.Nombre}, Ataque: {accesorio.BonificacionAtaque}, Defensa: {accesorio.BonificacionDefensa}, Rareza: {accesorio.Rareza}, Perfección: {accesorio.Perfeccion}");

            var botas = GeneradorObjetos.GenerarBotasAleatorias(1);
            Console.WriteLine($"Botas: {botas.Nombre}, Defensa: {botas.Defensa}, Rareza: {botas.Rareza}, Perfección: {botas.Perfeccion}");

            var cinturon = GeneradorObjetos.GenerarCinturonAleatorio(1);
            Console.WriteLine($"Cinturón: {cinturon.Nombre}, Carga: {cinturon.BonificacionCarga}, Rareza: {cinturon.Rareza}, Perfección: {cinturon.Perfeccion}");

            var collar = GeneradorObjetos.GenerarCollarAleatorio(1);
            Console.WriteLine($"Collar: {collar.Nombre}, Defensa: {collar.BonificacionDefensa}, Energía: {collar.BonificacionEnergia}, Rareza: {collar.Rareza}, Perfección: {collar.Perfeccion}");

            var pantalon = GeneradorObjetos.GenerarPantalonAleatorio(1);
            Console.WriteLine($"Pantalón: {pantalon.Nombre}, Defensa: {pantalon.Defensa}, Rareza: {pantalon.Rareza}, Perfección: {pantalon.Perfeccion}");
        }
    }
}
