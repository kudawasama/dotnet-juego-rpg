using System.Collections.Generic;

namespace MiJuegoRPG.Personaje
{
    public class ClaseData
    {
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Rareza { get; set; } = string.Empty;
    public bool Oculta { get; set; } = false;
    public List<string> ClasesPrevias { get; set; } = new List<string>();
    public int NivelMinimo { get; set; } = 0;
    public Dictionary<string, int> AtributosRequeridos { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> EstadisticasRequeridas { get; set; } = new Dictionary<string, int>();
    public List<string> MisionesRequeridas { get; set; } = new List<string>();
    public int ReputacionMinima { get; set; } = 0;
    public Dictionary<string, int> AtributosGanados { get; set; } = new Dictionary<string, int>();
    public string MisionUnica { get; set; } = string.Empty;
    public string ObjetoUnico { get; set; } = string.Empty;
    // Nuevos campos para sistema dinámico
    public Dictionary<string,int> ActividadRequerida { get; set; } = new(); // clave -> mínimo (ej: Recoleccion.Minar)
    public Dictionary<string,int> HabilidadesNivelRequerido { get; set; } = new(); // id habilidad -> nivel mínimo
    public List<string> ClasesAlguna { get; set; } = new(); // Al menos una requerida
    public List<string> ClasesExcluidas { get; set; } = new(); // Si posee alguna, bloquea
    public Dictionary<string,double> Bonificadores { get; set; } = new(); // Atributo.Fuerza, Energia.ModAccion.Minar, etc.
    public int Tier { get; set; } = 1; // Escalón o profundidad
    public string Rama { get; set; } = string.Empty; // Grupo temático (Oficio, Hibrida, Combate)
    public double PesoEmergenteMin { get; set; } = 0; // >0 habilita modo emergente por score
    // Reputación mínima por facción: clave = nombre facción, valor = reputación mínima requerida
    public Dictionary<string,int> ReputacionFaccionMin { get; set; } = new();
    }
}
