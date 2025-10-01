using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MiJuegoRPG.Motor.Servicios.Repos
{
    /// <summary>
    /// Repositorio JSON genérico (lista raíz u objeto único). Requiere que T tenga al menos un campo identificador accesible.
    /// El resolutor de Id se pasa por delegado para no imponer convención rígida.
    /// </summary>
    public class JsonRepository<T> : IRepository<T> where T : class
    {
        private readonly string _ruta;
        private readonly Func<T,string> _idSelector;
        private readonly bool _allowObjectRoot;
        private readonly JsonSerializerOptions _opts;
        private Dictionary<string,T>? _cache;
        private DateTime _lastLoad;

        public JsonRepository(string ruta, Func<T,string> idSelector, bool allowObjectRoot = false, JsonSerializerOptions? opts = null)
        {
            _ruta = ruta;
            _idSelector = idSelector;
            _allowObjectRoot = allowObjectRoot;
            _opts = opts ?? new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private void EnsureLoaded()
        {
            if (_cache != null) return;
            _cache = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (!File.Exists(_ruta)) return; // vacío tolerante
                using var doc = JsonDocument.Parse(File.ReadAllText(_ruta));
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in doc.RootElement.EnumerateArray())
                    {
                        try
                        {
                            var ent = JsonSerializer.Deserialize<T>(el.GetRawText(), _opts);
                            if (ent == null) continue;
                            var id = _idSelector(ent);
                            if (string.IsNullOrWhiteSpace(id)) continue;
                            if (!_cache.ContainsKey(id)) _cache[id] = ent;
                        }
                        catch { /* ignorar elemento corrupto */ }
                    }
                }
                else if (_allowObjectRoot && doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    var ent = JsonSerializer.Deserialize<T>(doc.RootElement.GetRawText(), _opts);
                    if (ent != null)
                    {
                        var id = _idSelector(ent);
                        if (!string.IsNullOrWhiteSpace(id) && !_cache.ContainsKey(id)) _cache[id] = ent;
                    }
                }
            }
            catch { /* silencio controlado */ }
            _lastLoad = DateTime.UtcNow;
        }

        public IReadOnlyCollection<T> GetAll()
        {
            EnsureLoaded();
            return _cache!.Values;
        }

        public T? GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            EnsureLoaded();
            return _cache!.TryGetValue(id, out var v) ? v : null;
        }

        public bool TryGet(string id, out T? entity)
        {
            entity = GetById(id);
            return entity != null;
        }

        public void Invalidate()
        {
            _cache = null;
        }

        public void SaveAll(IEnumerable<T> entities)
        {
            // Persistencia básica: serializa como lista
            try
            {
                var list = new List<T>(entities);
                var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_ruta, json);
                _cache = null; // forzar recarga futura
            }
            catch (Exception)
            {
                throw; // Escalar; capas superiores decidirán degradar
            }
        }
    }
}
