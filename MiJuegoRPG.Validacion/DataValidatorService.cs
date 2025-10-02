using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using MiJuegoRPG.PjDatos;
using MiJuegoRPG.Motor.Servicios; // PathProvider

namespace MiJuegoRPG.Validacion
{
    public static class DataValidatorService
    {
        public class Resultado
        {
            public int Errores { get; set; }
            public int Advertencias { get; set; }
            public List<string> Mensajes { get; } = new();
            public bool IsOk => Errores == 0;
        }

        // (Contenido reducido a las validaciones ya implementadas) Copiado del original
        public static Resultado ValidarReferenciasBasicas(bool generarReporte = false, string? rutaReporte = null)
        {
            // Para mantener el enfoque, invocar internamente a los sub-validadores que ya se usaban.
            var res = new Resultado();
            try
            {
                var armas = ValidarArmasBasico();
                Acum(res, armas);
                var poc = ValidarPocionesBasico();
                Acum(res, poc);
                var eq = ValidarEquipoNoArmaBasico();
                Acum(res, eq);
                var enem = ValidarEnemigosBasico();
                Acum(res, enem);
            }
            catch (Exception ex)
            {
                res.Errores++;
                res.Mensajes.Add($"[Validador][EXC] {ex.GetType().Name}: {ex.Message}");
            }
            return res;
        }

        private static void Acum(Resultado total, Resultado parte)
        {
            total.Errores += parte.Errores; total.Advertencias += parte.Advertencias; total.Mensajes.AddRange(parte.Mensajes);
        }

        public static Resultado ValidarArmasBasico()
        {
            var res = new Resultado();
            var ruta = PathProvider.EquipoPath("armas.json");
            if (!File.Exists(ruta)) { res.Advertencias++; res.Mensajes.Add($"[Armas][WARN] No existe archivo armas.json en {ruta}"); return res; }
            try
            {
                var json = File.ReadAllText(ruta);
                if (string.IsNullOrWhiteSpace(json)) return res;
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                { res.Errores++; res.Mensajes.Add("[Armas][ERR] Raíz no es array"); return res; }
                var nombres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    string nombre = el.TryGetProperty("Nombre", out var pn) ? pn.GetString() ?? "" : "";
                    string rareza = el.TryGetProperty("Rareza", out var pr) ? pr.GetString() ?? "" : "";
                    int perfeccion = el.TryGetProperty("Perfeccion", out var pp) && pp.TryGetInt32(out var pf) ? pf : 50;
                    if (string.IsNullOrWhiteSpace(nombre)) { res.Errores++; res.Mensajes.Add("[Armas][ERR] Arma sin Nombre"); }
                    else if (!nombres.Add(nombre)) { res.Errores++; res.Mensajes.Add($"[Armas][ERR] Nombre duplicado '{nombre}'"); }
                    if (perfeccion < 0 || perfeccion > 200) { res.Errores++; res.Mensajes.Add($"[Armas][ERR] Perfeccion fuera de [0..200] en '{nombre}' ({perfeccion})"); }
                    else if (perfeccion > 100) { res.Advertencias++; res.Mensajes.Add($"[Armas][WARN] Perfeccion >100 en '{nombre}' ({perfeccion}) (overquality legacy)"); }
                    if (!string.IsNullOrWhiteSpace(rareza))
                    {
                        var catalogo = new[] { "Rota","Pobre","Normal","Superior","Rara","Epica","Legendaria","Ornamentada" };
                        if (!catalogo.Contains(rareza.Trim(), StringComparer.OrdinalIgnoreCase))
                            res.Advertencias++; res.Mensajes.Add($"[Armas][WARN] Rareza desconocida '{rareza}' en '{nombre}'");
                    }
                }
                res.Mensajes.Add($"[Armas] Validadas {nombres.Count} armas");
            }
            catch (Exception ex) { res.Errores++; res.Mensajes.Add($"[Armas][ERR] Excepción leyendo armas.json: {ex.Message}"); }
            return res;
        }

        public static Resultado ValidarPocionesBasico()
        {
            var res = new Resultado();
            var ruta = PathProvider.PocionesPath("pociones.json");
            if (!File.Exists(ruta)) { res.Advertencias++; res.Mensajes.Add($"[Pociones][WARN] No existe pociones.json en {ruta}"); return res; }
            try
            {
                var json = File.ReadAllText(ruta);
                if (string.IsNullOrWhiteSpace(json)) return res;
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Array) { res.Errores++; res.Mensajes.Add("[Pociones][ERR] Raíz no es array"); return res; }
                var nombres = new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    var nombre = el.TryGetProperty("Nombre", out var pn) ? pn.GetString() ?? "" : "";
                    var rareza = el.TryGetProperty("Rareza", out var pr) ? pr.GetString() ?? "" : "";
                    if (string.IsNullOrWhiteSpace(nombre)) { res.Errores++; res.Mensajes.Add("[Pociones][ERR] Pocion sin Nombre"); continue; }
                    if (!nombres.ContainsKey(nombre)) nombres[nombre] = 0; nombres[nombre]++;
                    if (string.IsNullOrWhiteSpace(rareza)) { res.Advertencias++; res.Mensajes.Add($"[Pociones][WARN] Rareza vacía en '{nombre}'"); }
                }
                foreach (var kv in nombres) if (kv.Value > 1) { res.Errores++; res.Mensajes.Add($"[Pociones][ERR] Nombre duplicado '{kv.Key}' (x{kv.Value})"); }
                res.Mensajes.Add($"[Pociones] Validadas {nombres.Count} pociones");
            }
            catch (Exception ex) { res.Errores++; res.Mensajes.Add($"[Pociones][ERR] Excepción leyendo pociones.json: {ex.Message}"); }
            return res;
        }

        public static Resultado ValidarEquipoNoArmaBasico()
        {
            var res = new Resultado();
            var tipos = new[] { "armaduras", "cascos", "botas", "cinturones", "collares", "pantalones", "accesorios" };
            var catalogoRarezas = new[] { "Rota","Pobre","Normal","Superior","Rara","Epica","Legendaria","Ornamentada" };
            foreach (var tipo in tipos)
            {
                var dir = PathProvider.CombineData("Equipo", tipo);
                if (!Directory.Exists(dir)) { res.Advertencias++; res.Mensajes.Add($"[Equipo:{tipo}][WARN] Carpeta no encontrada: {dir}"); continue; }
                var nombres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                int archivos = 0; int validos = 0;
                foreach (var file in Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories))
                {
                    archivos++;
                    try
                    {
                        var json = File.ReadAllText(file);
                        if (string.IsNullOrWhiteSpace(json)) continue;
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.ValueKind == JsonValueKind.Array)
                            foreach (var el in doc.RootElement.EnumerateArray()) ValidarUno(el, file, tipo, res, nombres, catalogoRarezas, ref validos);
                        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                            ValidarUno(doc.RootElement, file, tipo, res, nombres, catalogoRarezas, ref validos);
                        else { res.Errores++; res.Mensajes.Add($"[Equipo:{tipo}][ERR] Raíz no válida (no es objeto ni array) en {file}"); }
                    }
                    catch (Exception ex) { res.Errores++; res.Mensajes.Add($"[Equipo:{tipo}][ERR] Excepción leyendo {file}: {ex.Message}"); }
                }
                res.Mensajes.Add($"[Equipo:{tipo}] Archivos={archivos} Items={validos} UnicosNombre={nombres.Count}");
            }
            return res;

            static void ValidarUno(JsonElement el, string origen, string tipo, Resultado res, HashSet<string> nombres, string[] catalogoRarezas, ref int validos)
            {
                string nombre = el.TryGetProperty("Nombre", out var pn) ? pn.GetString() ?? "" : "";
                string rareza = el.TryGetProperty("Rareza", out var pr) ? pr.GetString() ?? "" : "";
                int perfeccion = el.TryGetProperty("Perfeccion", out var pp) && pp.TryGetInt32(out var pf) ? pf : 50;
                if (string.IsNullOrWhiteSpace(nombre)) { res.Errores++; res.Mensajes.Add($"[Equipo:{tipo}][ERR] Item sin Nombre en {origen}"); }
                else if (!nombres.Add(nombre)) { res.Errores++; res.Mensajes.Add($"[Equipo:{tipo}][ERR] Nombre duplicado '{nombre}' ({origen})"); }
                if (perfeccion < 0 || perfeccion > 200) { res.Errores++; res.Mensajes.Add($"[Equipo:{tipo}][ERR] Perfeccion fuera de [0..200] '{nombre}'={perfeccion} ({origen})"); }
                else if (perfeccion > 100) { res.Advertencias++; res.Mensajes.Add($"[Equipo:{tipo}][WARN] Perfeccion >100 '{nombre}'={perfeccion} (overquality) ({origen})"); }
                if (!string.IsNullOrWhiteSpace(rareza) && !catalogoRarezas.Contains(rareza.Trim(), StringComparer.OrdinalIgnoreCase)) { res.Advertencias++; res.Mensajes.Add($"[Equipo:{tipo}][WARN] Rareza desconocida '{rareza}' en '{nombre}' ({origen})"); }
                ChequearRango(el, "NivelMin", "NivelMax", tipo, nombre, origen, res, 0, 500);
                ChequearRango(el, "PerfeccionMin", "PerfeccionMax", tipo, nombre, origen, res, 0, 200);
                ChequearRango(el, "DefensaMin", "DefensaMax", tipo, nombre, origen, res, 0, int.MaxValue);
                ChequearRango(el, "BonificacionDefensaMin", "BonificacionDefensaMax", tipo, nombre, origen, res, -100000, 1000000);
                ChequearRarezasPermitidas(el, tipo, nombre, origen, res, catalogoRarezas);
                validos++;
            }
            static void ChequearRango(JsonElement el, string minKey, string maxKey, string tipo, string nombre, string origen, Resultado res, int limiteMin, int limiteMax)
            {
                int vmin = 0, vmax = 0;
                bool tieneMin = el.TryGetProperty(minKey, out var pmin) && pmin.TryGetInt32(out vmin);
                bool tieneMax = el.TryGetProperty(maxKey, out var pmax) && pmax.TryGetInt32(out vmax);
                if (!tieneMin && !tieneMax) return;
                if (tieneMin && (vmin < limiteMin || vmin > limiteMax)) { res.Errores++; res.Mensajes.Add($"[Equipo:{tipo}][ERR] {minKey} fuera de rango ({vmin}) '{nombre}' ({origen})"); }
                if (tieneMax && (vmax < limiteMin || vmax > limiteMax)) { res.Errores++; res.Mensajes.Add($"[Equipo:{tipo}][ERR] {maxKey} fuera de rango ({vmax}) '{nombre}' ({origen})"); }
                if (tieneMin && tieneMax && vmin > vmax) { res.Errores++; res.Mensajes.Add($"[Equipo:{tipo}][ERR] {minKey}>{maxKey} ({vmin}>{vmax}) en '{nombre}' ({origen})"); }
            }
            static void ChequearRarezasPermitidas(JsonElement el, string tipo, string nombre, string origen, Resultado res, string[] catalogo)
            {
                if (!el.TryGetProperty("RarezasPermitidasCsv", out var prp) || prp.ValueKind != JsonValueKind.String) return;
                var csv = prp.GetString(); if (string.IsNullOrWhiteSpace(csv)) return;
                var partes = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var r in partes) if (!catalogo.Contains(r, StringComparer.OrdinalIgnoreCase)) { res.Advertencias++; res.Mensajes.Add($"[Equipo:{tipo}][WARN] RarezasPermitidasCsv rareza desconocida '{r}' en '{nombre}' ({origen})"); }
            }
        }

        public static Resultado ValidarEnemigosBasico()
        {
            var res = new Resultado();
            var dir = PathProvider.EnemigosDir();
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            opciones.Converters.Add(new JsonStringEnumConverter());
            var porNombre = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var porId = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!Directory.Exists(dir)) { res.Advertencias++; res.Mensajes.Add($"[Enemigos][WARN] Carpeta no encontrada: {dir}"); return res; }
            foreach (var file in Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    if (string.IsNullOrWhiteSpace(json)) continue;
                    using var doc = JsonDocument.Parse(json);
                    var kind = doc.RootElement.ValueKind;
                    if (kind == JsonValueKind.Array)
                    {
                        var lista = JsonSerializer.Deserialize<List<EnemigoData>>(json, opciones);
                        if (lista != null) foreach (var e in lista) ValidarUno(e, file, res, porNombre, porId); else { res.Errores++; res.Mensajes.Add($"[Enemigos][ERR] Lista inválida en '{file}'"); }
                    }
                    else if (kind == JsonValueKind.Object)
                    {
                        var uno = JsonSerializer.Deserialize<EnemigoData>(json, opciones);
                        if (uno != null) ValidarUno(uno, file, res, porNombre, porId); else { res.Errores++; res.Mensajes.Add($"[Enemigos][ERR] Objeto inválido en '{file}'"); }
                    }
                    else { res.Errores++; res.Mensajes.Add($"[Enemigos][ERR] Raíz JSON no válida en '{file}'"); }
                }
                catch (Exception ex) { res.Errores++; res.Mensajes.Add($"[Enemigos][ERR] Archivo inválido '{file}': {ex.Message}"); }
            }
            res.Mensajes.Add($"[Enemigos] Catálogo: {porNombre.Count} únicos por Nombre.");
            return res;

            static void ValidarUno(EnemigoData e, string origen, Resultado res, HashSet<string> porNombre, HashSet<string> porId)
            {
                if (e == null) { res.Errores++; res.Mensajes.Add($"[Enemigos][ERR] Entrada nula en {origen}"); return; }
                if (string.IsNullOrWhiteSpace(e.Nombre)) { res.Errores++; res.Mensajes.Add($"[Enemigos][ERR] Enemigo sin Nombre en {origen}"); }
                else if (!porNombre.Add(e.Nombre.Trim())) { res.Errores++; res.Mensajes.Add($"[Enemigos][ERR] Nombre duplicado '{e.Nombre}' en {origen}"); }
                if (!string.IsNullOrWhiteSpace(e.Id) && !porId.Add(e.Id.Trim())) { res.Errores++; res.Mensajes.Add($"[Enemigos][ERR] Id duplicado '{e.Id}' en {origen}"); }
                if (e.VidaBase <= 0) { res.Errores++; res.Mensajes.Add($"[Enemigos][ERR] VidaBase <=0 '{e.Nombre}' ({origen})"); }
                if (e.AtaqueBase < 1) { res.Advertencias++; res.Mensajes.Add($"[Enemigos][WARN] AtaqueBase <1 '{e.Nombre}'"); }
                if (e.DefensaBase < 0 || e.DefensaMagicaBase < 0) { res.Errores++; res.Mensajes.Add($"[Enemigos][ERR] Defensas negativas '{e.Nombre}'"); }
                if (e.Nivel < 1) { res.Errores++; res.Mensajes.Add($"[Enemigos][ERR] Nivel <1 '{e.Nombre}'"); }
            }
        }
    }
}