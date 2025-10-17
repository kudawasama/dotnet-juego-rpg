using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Motor.Servicios.Repos
{
    /// <summary>
    /// Repositorio específico de Material.
    /// Fuente de verdad jerárquica:
    ///   1) Base: DatosJuego/Materiales/** (archivos .json individuales o listas)
    ///   2) Overlay opcional: PjDatos/materiales.json (lista) que sobreescribe por Nombre
    /// Reglas:
    ///   - Nombre (case-insensitive) único. Base: primer archivo gana, overlay reemplaza.
    ///   - Campos tolerantes (PropertyNameCaseInsensitive=true). Alias: nombre/nombre, rareza/rareza, categoria/categoria, especialidad -> Categoria.
    ///   - Rareza normalizada a convención interna (Comun, Superior, Rara, Epica, Legendaria, etc.).
    ///   - Fallos por archivo no rompen la carga completa (log Warn y continúa).
    /// </summary>
    public class MaterialRepository
    {
        private readonly Dictionary<string, MaterialJson> cache = new(StringComparer.OrdinalIgnoreCase);
        private bool loaded;

        public MaterialRepository()
        {
            // Carga diferida (lazy). No se hace nada en ctor.
        }

        private void EnsureLoaded()
        {
            if (loaded)
                return;
            loaded = true; // Evitar reentrancia
            try
            {
                CargarBaseJerarquica();
                AplicarOverlay();
            }
            catch (Exception ex)
            {
                Logger.Warn($"[MaterialRepository] Error carga inicial: {ex.Message}");
            }
        }

        private void CargarBaseJerarquica()
        {
            var dir = PathProvider.MaterialesDir();
            if (!Directory.Exists(dir))
                return; // silencioso
            foreach (var file in Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    if (string.IsNullOrWhiteSpace(json))
                        continue;
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        var lista = TryDeserializeLista(doc.RootElement, file);
                        if (lista != null)
                            AgregarListaSiNoExiste(lista, origen: "base");
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        var uno = TryDeserializeObjeto(doc.RootElement, file);
                        if (uno != null)
                            AgregarSiNoExiste(uno, origen: "base");
                    }
                }
                catch (Exception exFile)
                {
                    Logger.Warn($"[MaterialRepository] Archivo '{file}' ignorado: {exFile.Message}");
                }
            }
        }

        private void AplicarOverlay()
        {
            var rutaOverlay = PathProvider.PjDatosPath("materiales.json");
            if (!File.Exists(rutaOverlay))
                return;
            try
            {
                var json = File.ReadAllText(rutaOverlay);
                if (string.IsNullOrWhiteSpace(json))
                    return;
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var lista = JsonSerializer.Deserialize<List<MaterialOverlayDto>>(json, opts);
                if (lista == null)
                    return;
                foreach (var dto in lista)
                {
                    if (string.IsNullOrWhiteSpace(dto.Nombre))
                        continue;
                    var mat = new MaterialJson
                    {
                        Nombre = dto.Nombre!.Trim(),
                        Rareza = RarezaNormalizer.Normalizar(dto.Rareza),
                        Categoria = string.IsNullOrWhiteSpace(dto.Categoria) ? (dto.Especialidad ?? "Material") : dto.Categoria!
                    };
                    cache[mat.Nombre] = mat; // overlay reemplaza
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"[MaterialRepository] Overlay ignorado: {ex.Message}");
            }
        }

        private class MaterialOverlayDto
        {
            public string? Nombre
            {
                get; set;
            }
            public string? Rareza
            {
                get; set;
            }
            public string? Categoria
            {
                get; set;
            }
            public string? Especialidad
            {
                get; set;
            }
        }

        private List<MaterialJson>? TryDeserializeLista(JsonElement arrayRoot, string file)
        {
            var resultado = new List<MaterialJson>();
            foreach (var elem in arrayRoot.EnumerateArray())
            {
                if (elem.ValueKind != JsonValueKind.Object)
                    continue;
                var m = TryDeserializeObjeto(elem, file);
                if (m != null)
                    resultado.Add(m);
            }
            return resultado;
        }

        private MaterialJson? TryDeserializeObjeto(JsonElement obj, string file)
        {
            try
            {
                string? nombre = LeerString(obj, "nombre") ?? LeerString(obj, "Nombre");
                if (string.IsNullOrWhiteSpace(nombre))
                    return null;
                string? rareza = LeerString(obj, "rareza") ?? LeerString(obj, "Rareza");
                string? categoria = LeerString(obj, "categoria") ?? LeerString(obj, "Categoria") ?? LeerString(obj, "especialidad") ?? LeerString(obj, "Especialidad");
                if (string.IsNullOrWhiteSpace(categoria))
                    categoria = "Material";
                var mj = new MaterialJson
                {
                    Nombre = nombre.Trim(),
                    Rareza = RarezaNormalizer.Normalizar(rareza),
                    Categoria = categoria.Trim()
                };
                return mj;
            }
            catch (Exception ex)
            {
                Logger.Warn($"[MaterialRepository] Objeto en '{file}' inválido: {ex.Message}");
                return null;
            }
        }

        private static string? LeerString(JsonElement obj, string prop)
        {
            if (obj.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String)
                return v.GetString();
            return null;
        }

        private void AgregarListaSiNoExiste(IEnumerable<MaterialJson> lista, string origen)
        {
            foreach (var m in lista)
                AgregarSiNoExiste(m, origen);
        }

        private void AgregarSiNoExiste(MaterialJson m, string origen)
        {
            if (string.IsNullOrWhiteSpace(m.Nombre))
                return;
            if (cache.ContainsKey(m.Nombre))
                return; // primer archivo gana en base
            cache[m.Nombre] = m;
        }

        // Normalización movida a RarezaNormalizer.Normalizar()
        public IReadOnlyCollection<MaterialJson> GetAll()
        {
            EnsureLoaded();
            return cache.Values as IReadOnlyCollection<MaterialJson> ?? new List<MaterialJson>(cache.Values);
        }

        public MaterialJson? GetByNombre(string nombre)
        {
            EnsureLoaded();
            cache.TryGetValue(nombre, out var m);
            return m;
        }

        public bool TryGet(string nombre, out MaterialJson? mat)
        {
            EnsureLoaded();
            return cache.TryGetValue(nombre, out mat);
        }

        public void Invalidate()
        {
            cache.Clear();
            loaded = false;
        }

        public void SaveAll(IEnumerable<MaterialJson> mats)
        {
            // Persistencia sólo aplica al overlay para mantener comportamiento anterior.
            try
            {
                var ruta = PathProvider.PjDatosPath("materiales.json");
                Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
                var opciones = new JsonSerializerOptions { WriteIndented = true };
                var lista = new List<object>();
                foreach (var m in mats)
                {
                    lista.Add(new
                    {
                        m.Nombre,
                        Rareza = m.Rareza,
                        Categoria = m.Categoria
                    });
                }
                File.WriteAllText(ruta, JsonSerializer.Serialize(lista, opciones));
            }
            catch (Exception ex)
            {
                Logger.Warn($"[MaterialRepository] SaveAll fallo: {ex.Message}");
            }
            // Actualizar cache (overlay domina)
            foreach (var m in mats)
                cache[m.Nombre] = m;
        }

        /// <summary>
        /// Adaptador a dominio Material (crea instancias de objeto jugable).
        /// </summary>
        /// <returns></returns>
        public Material? ToDomain(string nombre)
        {
            var data = GetByNombre(nombre);
            if (data == null)
                return null;
            return new Material(data.Nombre, data.Rareza, data.Categoria);
        }
    }
}
