using System;
using System.Collections.Generic;

namespace MiJuegoRPG.Motor
{
    public class Sector
    {
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }
        public bool Descubierto { get; set; } = false;
        public List<string> Eventos { get; set; } = new List<string>();
        public List<string> EnemigosPosibles { get; set; } = new List<string>();
        public List<string> ObjetosPosibles { get; set; } = new List<string>();
    }

    public class Mapa
    {
        public string CiudadActual { get; set; }
        public List<Sector> Sectores { get; set; } = new List<Sector>();

        public Mapa(string ciudad)
        {
            CiudadActual = ciudad;
            InicializarSectores();
        }

        private void InicializarSectores()
        {
            Sectores.Add(new Sector {
                Nombre = "Plaza Central",
                Descripcion = "El corazón de la ciudad, lleno de vida, rumores y NPCs.",
                Eventos = new List<string> { "EventoSocial", "Rumores", "HablarNPC", "Descansar" }
            });
            Sectores.Add(new Sector {
                Nombre = "Tienda",
                Descripcion = "Compra y vende armas, pociones y materiales.",
                Eventos = new List<string> { "Comprar", "Vender", "Negociar", "BuscarOferta" }
            });
            Sectores.Add(new Sector {
                Nombre = "Escuela de Entrenamiento",
                Descripcion = "Mejora tus atributos realizando actividades físicas y mentales.",
                Eventos = new List<string> { "EntrenarFuerza", "EntrenarMagia", "EntrenarAgilidad", "EntrenarInteligencia", "EntrenarResistencia" }
            });
            Sectores.Add(new Sector {
                Nombre = "Biblioteca",
                Descripcion = "Estudia para mejorar tu inteligencia y aprender habilidades especiales.",
                Eventos = new List<string> { "Estudiar", "LeerLibro", "DescubrirSecreto" }
            });
            Sectores.Add(new Sector {
                Nombre = "Bosque Exterior",
                Descripcion = "Un bosque peligroso, ideal para aventuras y recolección de materiales.",
                EnemigosPosibles = new List<string> { "Goblin", "Slime", "Lobo", "Bandido" },
                ObjetosPosibles = new List<string> { "Material", "Pocion", "Arma" },
                Eventos = new List<string> { "Explorar", "EncontrarMazmorra", "RecolectarMaterial" }
            });
            Sectores.Add(new Sector {
                Nombre = "Mazmorra Abandonada",
                Descripcion = "Un reto para aventureros, con enemigos fuertes y tesoros.",
                EnemigosPosibles = new List<string> { "Esqueleto", "Golem de Fuego", "Orco" },
                ObjetosPosibles = new List<string> { "Arma", "Material", "Pocion" },
                Eventos = new List<string> { "Explorar", "DerrotarJefe", "EncontrarTesoro" }
            });
            Sectores.Add(new Sector {
                Nombre = "Camino a la siguiente ciudad",
                Descripcion = "Solo puedes avanzar si cumples ciertos requisitos o derrotas al jefe local.",
                Eventos = new List<string> { "DesbloquearRuta", "DesafiarJefe" }
            });
        }

        public void MostrarMapa()
        {
            Console.WriteLine($"Mapa de {CiudadActual}:");
            for (int i = 0; i < Sectores.Count; i++)
            {
                var s = Sectores[i];
                Console.WriteLine($"{i + 1}. {s.Nombre} - {(s.Descubierto ? "Descubierto" : "Sin explorar")}");
            }
        }
    }
}
