using System.Collections.Generic;

namespace MiJuegoRPG.Motor.Servicios.Repos
{
    /// <summary>
    /// Contrato mínimo para repositorios de lectura/escritura basados en JSON.
    /// Diseñado para ser simple y testable. No expone IQueryable para evitar acoplamientos.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        IReadOnlyCollection<T> GetAll();
        T? GetById(string id);
        bool TryGet(string id, out T? entity);
        void Invalidate();
        /// <summary>
        /// Guarda (sobrescribe) todas las entidades si el repositorio es persistente (opcional).
        /// Implementaciones read-only pueden lanzar NotSupportedException.
        /// </summary>
        void SaveAll(IEnumerable<T> entities);
    }
}
