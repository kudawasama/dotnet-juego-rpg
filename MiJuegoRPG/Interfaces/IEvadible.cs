namespace MiJuegoRPG.Interfaces
{
    /// <summary>
    /// Contrato para entidades que pueden intentar evadir un ataque entrante.
    /// </summary>
    public interface IEvadible
    {
        /// <summary>
        /// Devuelve true si el ataque es evadido (sin daño).
        /// </summary>
        /// <param name="esAtaqueMagico">true si el ataque es mágico; false si es físico.</param>
        /// <returns></returns>
        bool IntentarEvadir(bool esAtaqueMagico);
    }
}
