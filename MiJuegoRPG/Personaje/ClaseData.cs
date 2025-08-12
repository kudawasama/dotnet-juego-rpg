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
    }
}
