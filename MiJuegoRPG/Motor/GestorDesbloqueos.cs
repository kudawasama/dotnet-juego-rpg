using System;
using System.Collections.Generic;
using MiJuegoRPG.Personaje;

namespace MiJuegoRPG.Motor
{
    public class Desbloqueable
    {
        public string Id { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // "sector", "clase", "mision", etc.
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public List<string> Requisitos { get; set; } = new List<string>();
    }

    public static class GestorDesbloqueos
    {
        // Lista global de desbloqueables (puedes cargarla desde JSON o definirla aquí)
        public static List<Desbloqueable> ListaDesbloqueables = new List<Desbloqueable>
        {
            new Desbloqueable {
                Id = "sector_bosque_encantado",
                Tipo = "sector",
                Nombre = "Bosque Encantado",
                Descripcion = "Ahora puedes explorar nuevas áreas y encontrar desafíos únicos.",
                Requisitos = new List<string> { "nivel:5" }
            },
            new Desbloqueable {
                Id = "clase_guerrero_legendario",
                Tipo = "clase",
                Nombre = "Guerrero Legendario",
                Descripcion = "¡Has descubierto una clase secreta con habilidades únicas!",
                Requisitos = new List<string> { "fuerza:20", "misioncompletada:BAI-INI-001" }
            }
            // Agrega más desbloqueables aquí
        };

        // Llama a esto tras cada acción relevante
        public static void VerificarDesbloqueos(Personaje.Personaje pj)
        {
            if (pj.DesbloqueosNotificados == null)
                pj.DesbloqueosNotificados = new HashSet<string>();
            foreach (var d in ListaDesbloqueables)
            {
                if (!pj.DesbloqueosNotificados.Contains(d.Id))
                {
                    bool cumple = true;
                    foreach (var req in d.Requisitos)
                    {
                        var partes = req.Split(':');
                        string clave = partes[0].Trim();
                        object? valor = null;
                        if (partes.Length > 1)
                        {
                            valor = partes[1].Trim();
                            if (int.TryParse(valor.ToString(), out int valorInt))
                                valor = valorInt;
                        }
                        if (valor == null)
                        {
                            cumple = pj.Inventario.NuevosObjetos.Any(o => o.Objeto.Nombre.Contains(clave, StringComparison.OrdinalIgnoreCase));
                        }
                        else
                        {
                            cumple = pj.CumpleRequisito(clave, valor);
                        }
                        if (!cumple) break;
                    }
                    if (cumple)
                    {
                        AvisosAventura.MostrarAviso(d.Tipo, d.Nombre, d.Descripcion);
                        pj.DesbloqueosNotificados.Add(d.Id);
                    }
                }
            }
        }
    }
}
