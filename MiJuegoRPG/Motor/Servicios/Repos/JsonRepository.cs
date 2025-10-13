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
    public class JsonRepository<T> : IRepository<T>
        where T : class
    {
        private readonly string ruta;
        private readonly Func<T, string> idSelector;
        private readonly bool allowObjectRoot;
        private readonly JsonSerializerOptions opts;
        private Dictionary<string, T>? cache;
        private DateTime lastLoad;

        public JsonRepository(string ruta, Func<T, string> idSelector, bool allowObjectRoot = false, JsonSerializerOptions? opts = null)
        {
            this.ruta = ruta;
            this.idSelector = idSelector;
            this.allowObjectRoot = allowObjectRoot;
            this.opts = opts ?? new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private void EnsureLoaded()
        {
            if (cache != null)
                return;
            cache = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (!File.Exists(ruta))
                    return; // vacío tolerante
                using var doc = JsonDocument.Parse(File.ReadAllText(ruta));
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in doc.RootElement.EnumerateArray())
                    {
                        try
                        {
                            var ent = JsonSerializer.Deserialize<T>(el.GetRawText(), opts);
                            if (ent == null)
                                continue;
                            var id = idSelector(ent);
                            if (string.IsNullOrWhiteSpace(id))
                                continue;
                            if (!cache.ContainsKey(id))
                                cache[id] = ent;
                        }
                        catch { /* ignorar elemento corrupto */ }
                    }
                }
                else if (allowObjectRoot && doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    var ent = JsonSerializer.Deserialize<T>(doc.RootElement.GetRawText(), opts);
                    if (ent != null)
                    {
                        var id = idSelector(ent);
                        if (!string.IsNullOrWhiteSpace(id) && !cache.ContainsKey(id))
                            cache[id] = ent;
                    }
                }
            }
            catch { /* silencio controlado */ }
            lastLoad = DateTime.UtcNow;
        }

        public IReadOnlyCollection<T> GetAll()
        {
            EnsureLoaded();
            return cache!.Values;
        }

        public T? GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            EnsureLoaded();
            return cache!.TryGetValue(id, out var v) ? v : null;
        }

        public bool TryGet(string id, out T? entity)
        {
            entity = GetById(id);
            return entity != null;
        }

        public void Invalidate()
        {
            cache = null;
        }

        public void SaveAll(IEnumerable<T> entities)
        {
            // Persistencia básica: serializa como lista
            try
            {
                var list = new List<T>(entities);
                var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ruta, json);
                cache = null; // forzar recarga futura
            }
            catch (Exception)
            {
                throw; // Escalar; capas superiores decidirán degradar
            }
        }
    }
}
