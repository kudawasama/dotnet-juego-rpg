namespace MiJuegoRPG.PjDatos
{
    public class PenalizacionesConfig
    {
        public PenalizacionNivel Advertencia { get; set; } = new PenalizacionNivel();
        public PenalizacionNivel Critico { get; set; } = new PenalizacionNivel();
    }
}