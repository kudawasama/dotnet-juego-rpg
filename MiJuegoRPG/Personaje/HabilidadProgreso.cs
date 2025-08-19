using System.Collections.Generic;

namespace MiJuegoRPG.Personaje
{
    public class HabilidadProgreso
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty; 
        public int Exp { get; set; } = 0;
        public List<EvolucionHabilidad> Evoluciones { get; set; } = new List<EvolucionHabilidad>();
        public List<string> EvolucionesDesbloqueadas { get; set; } = new List<string>();
    // Nivel calculado: cada 10 exp = 1 nivel (ajusta la fórmula si lo deseas)
    public int Nivel => (Exp / 10) + 1;
    }

    public class EvolucionHabilidad
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Beneficio { get; set; } = string.Empty;
        public List<CondicionEvolucion> Condiciones { get; set; } = new List<CondicionEvolucion>();
    }

    public class CondicionEvolucion
    {
        public string Tipo { get; set; } = string.Empty; // Ej: "NvHabilidad", "NvJugador", "Ataque"
        public double Cantidad { get; set; } = 0;
    }
}
