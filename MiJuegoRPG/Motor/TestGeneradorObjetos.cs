using System;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Motor
{
    public static class TestGeneradorObjetos
    {
        public static void Probar()
        {
            // Cargar archivos JSON (ajusta las rutas si es necesario)
            GeneradorObjetos.CargarArmas("PjDatos/Equipo/armas.json");
            GeneradorObjetos.CargarArmaduras("PjDatos/Equipo/Armaduras.json");
            GeneradorObjetos.CargarAccesorios("PjDatos/Equipo/Accesorios.json");
            GeneradorObjetos.CargarBotas("PjDatos/Equipo/Botas.json");
            GeneradorObjetos.CargarCinturones("PjDatos/Equipo/Cinturones.json");
            GeneradorObjetos.CargarCollares("PjDatos/Equipo/Collares.json");
            GeneradorObjetos.CargarPantalones("PjDatos/Equipo/Pantalones.json");

            Console.WriteLine("--- Prueba de generación de objetos aleatorios ---");
            var arma = GeneradorObjetos.GenerarArmaAleatoria(1);
            Console.WriteLine($"Arma: {arma.Nombre}, Daño: {arma.Daño}, Rareza: {arma.Rareza}, Perfección: {arma.Perfeccion}");

            var armadura = GeneradorObjetos.GenerarArmaduraAleatoria(1);
            Console.WriteLine($"Armadura: {armadura.Nombre}, Defensa: {armadura.Defensa}, Rareza: {armadura.Rareza}, Perfección: {armadura.Perfeccion}");

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
