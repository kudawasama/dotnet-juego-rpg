using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using MiJuegoRPG.PjDatos;

namespace MiJuegoRPG.Motor.Servicios.Validacion
{
    /// <summary>
    /// Validador referencial de datos del juego.
    /// - No lanza excepciones: acumula errores/advertencias y los reporta por consola y opcionalmente a archivo.
    /// - Cubre referencias de mapa, facciones, misiones, NPC y enemigos (básico).
    /// </summary>
    public static class DataValidatorService
    {
        public class Resultado
        {
            public int Errores { get; set; }
            public int Advertencias { get; set; }
            public List<string> Mensajes { get; } = new();
            public bool IsOk => Errores == 0;
        }

        public static Resultado ValidarReferenciasBasicas(bool generarReporte = false, string? rutaReporte = null)
        {
            var res = new Resultado();
            string? rutaEscrita = null;
            try
            {
                // 1) IDs de mapa disponibles (Sectores)
                var carpetaMapas = PathProvider.MapasDir();
                var idsMapa = CargarIdsMapa(carpetaMapas);
                res.Mensajes.Add($"[Validador] Sectores disponibles: {idsMapa.Count}");

                // 2) facciones_ubicacion.json: claves deben mapear a un sector ID o nombre de ciudad existente
                var rutaFaccionesUbic = PathProvider.CombineData("facciones_ubicacion.json");
                if (File.Exists(rutaFaccionesUbic))
                {
                    try
                    {
                        var json = File.ReadAllText(rutaFaccionesUbic);
                        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                                   ?? new Dictionary<string, string>();
                        foreach (var kv in dict)
                        {
                            var clave = kv.Key?.Trim();
                            if (string.IsNullOrWhiteSpace(clave))
                            {
                                res.Advertencias++;
                                res.Mensajes.Add("[Validador][WARN] Entrada vacía en facciones_ubicacion.json");
                                continue;
                            }

                            // Aceptar tanto IDs tipo "8_23" como nombres de ubicaciones (modo transición)
                            var esIdSector = EsSectorId(clave);
                            if (esIdSector && !idsMapa.Contains(clave))
                            {
                                res.Errores++;
                                res.Mensajes.Add($"[Validador][ERR] Sector id no encontrado en mapa: {clave}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        res.Errores++;
                        res.Mensajes.Add($"[Validador][ERR] facciones_ubicacion.json inválido: {ex.Message}");
                    }
                }
                else
                {
                    res.Advertencias++;
                    res.Mensajes.Add($"[Validador][WARN] No existe facciones_ubicacion.json en {rutaFaccionesUbic}");
                }

                // 3) misiones.json: cargar IDs y validar referencias internas
                var rutaMisiones = PathProvider.MisionesPath("misiones.json");
                var idsMisiones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (File.Exists(rutaMisiones))
                {
                    try
                    {
                        var texto = File.ReadAllText(rutaMisiones);
                        var lista = JsonSerializer.Deserialize<List<MisionEntry>>(texto, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                        foreach (var m in lista)
                        {
                            if (!string.IsNullOrWhiteSpace(m?.Id))
                            {
                                idsMisiones.Add(m.Id!);
                            }
                            else
                            {
                                res.Advertencias++;
                                res.Mensajes.Add("[Validador][WARN] Misión sin Id en misiones.json");
                            }
                        }
                        foreach (var m in lista)
                        {
                            if (m == null) continue;
                            // UbicacionNPC puede ser nombre o id; si parece id de sector, validar
                            if (!string.IsNullOrWhiteSpace(m.UbicacionNPC) && EsSectorId(m.UbicacionNPC!) && !idsMapa.Contains(m.UbicacionNPC!))
                            {
                                res.Errores++;
                                res.Mensajes.Add($"[Validador][ERR] Misión {m.Id ?? "<sin-id>"} referencia sector inexistente en UbicacionNPC: {m.UbicacionNPC}");
                            }

                            if (!string.IsNullOrWhiteSpace(m.SiguienteMisionId))
                            {
                                var sig = m.SiguienteMisionId!;
                                if (sig.Length == 0)
                                {
                                    res.Advertencias++;
                                    res.Mensajes.Add($"[Validador][WARN] Misión {m.Id ?? "<sin-id>"} tiene SiguienteMisionId vacío (\"\"). Considerar null.");
                                }
                                else if (!idsMisiones.Contains(sig))
                                {
                                    res.Errores++;
                                    res.Mensajes.Add($"[Validador][ERR] Misión {m.Id ?? "<sin-id>"} apunta a SiguienteMisionId inexistente: {sig}");
                                }
                            }

                            if (m.Condiciones != null)
                            {
                                foreach (var c in m.Condiciones)
                                {
                                    if (string.IsNullOrWhiteSpace(c)) continue;
                                    const string pref = "Completar ";
                                    if (c.StartsWith(pref, StringComparison.OrdinalIgnoreCase))
                                    {
                                        var dep = c.Substring(pref.Length).Trim();
                                        if (dep.Length > 0 && !idsMisiones.Contains(dep))
                                        {
                                            res.Errores++;
                                            res.Mensajes.Add($"[Validador][ERR] Misión {m.Id ?? "<sin-id>"} depende de misión inexistente en Condiciones: {dep}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        res.Errores++;
                        res.Mensajes.Add($"[Validador][ERR] misiones.json inválido: {ex.Message}");
                    }
                }
                else
                {
                    res.Advertencias++;
                    res.Mensajes.Add($"[Validador][WARN] No existe misiones.json en {rutaMisiones}");
                }

                // 4) npc.json: validar Ubicacion (sector si aplica) y que Misiones existan
                var rutaNpcs = PathProvider.NpcsPath("npc.json");
                if (File.Exists(rutaNpcs))
                {
                    try
                    {
                        var texto = File.ReadAllText(rutaNpcs);
                        var npcs = JsonSerializer.Deserialize<List<NpcEntry>>(texto, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                        foreach (var npc in npcs)
                        {
                            if (string.IsNullOrWhiteSpace(npc?.Id))
                            {
                                res.Advertencias++;
                                res.Mensajes.Add("[Validador][WARN] NPC sin Id en npc.json");
                            }

                            if (!string.IsNullOrWhiteSpace(npc?.Ubicacion) && EsSectorId(npc.Ubicacion!) && !idsMapa.Contains(npc.Ubicacion!))
                            {
                                res.Errores++;
                                res.Mensajes.Add($"[Validador][ERR] NPC {(npc.Id ?? "<sin-id>")} referencia sector inexistente en Ubicacion: {npc.Ubicacion}");
                            }

                            if (npc?.Misiones != null)
                            {
                                foreach (var mid in npc.Misiones)
                                {
                                    if (string.IsNullOrWhiteSpace(mid)) continue;
                                    if (!idsMisiones.Contains(mid))
                                    {
                                        res.Errores++;
                                        res.Mensajes.Add($"[Validador][ERR] NPC {(npc.Id ?? "<sin-id>")} refiere misión inexistente: {mid}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        res.Errores++;
                        res.Mensajes.Add($"[Validador][ERR] npc.json inválido: {ex.Message}");
                    }
                }
                else
                {
                    res.Advertencias++;
                    res.Mensajes.Add($"[Validador][WARN] No existe npc.json en {rutaNpcs}");
                }

                // 5) Sectores: nodosRecoleccion con materiales vacíos o inválidos
                try
                {
                    var archivosSectores = Directory.GetFiles(carpetaMapas, "*.json", SearchOption.AllDirectories);
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    foreach (var archivo in archivosSectores)
                    {
                        try
                        {
                            var texto = File.ReadAllText(archivo);
                            if (string.IsNullOrWhiteSpace(texto)) continue;
                            var sector = JsonSerializer.Deserialize<PjDatos.SectorData>(texto, opciones);
                            if (sector != null && sector.NodosRecoleccion != null)
                            {
                                int idxNodo = 0;
                                foreach (var nodo in sector.NodosRecoleccion)
                                {
                                    idxNodo++;
                                    if (nodo == null)
                                    {
                                        res.Advertencias++;
                                        res.Mensajes.Add($"[Validador][WARN] Nodo nulo en {sector.Id} ({archivo})");
                                        continue;
                                    }
                                    if (nodo.Materiales == null)
                                    {
                                        res.Advertencias++;
                                        res.Mensajes.Add($"[Validador][WARN] Nodo '{nodo.Nombre ?? ("#"+idxNodo)}' en {sector.Id} sin lista de Materiales ({archivo})");
                                        continue;
                                    }
                                    foreach (var mat in nodo.Materiales)
                                    {
                                        if (mat == null)
                                        {
                                            res.Errores++;
                                            res.Mensajes.Add($"[Validador][ERR] Nodo '{nodo.Nombre ?? ("#"+idxNodo)}' en {sector.Id} contiene material nulo ({archivo})");
                                            continue;
                                        }
                                        if (string.IsNullOrWhiteSpace(mat.Nombre))
                                        {
                                            res.Errores++;
                                            res.Mensajes.Add($"[Validador][ERR] Nodo '{nodo.Nombre ?? ("#"+idxNodo)}' en {sector.Id} contiene material con Nombre vacío ({archivo})");
                                        }
                                        if (mat.Cantidad <= 0)
                                        {
                                            res.Advertencias++;
                                            res.Mensajes.Add($"[Validador][WARN] Nodo '{nodo.Nombre ?? ("#"+idxNodo)}' en {sector.Id} contiene material '{mat.Nombre}' con Cantidad <= 0 ({archivo})");
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // ignorar archivos que no correspondan a SectorData
                        }
                    }
                }
                catch (Exception ex)
                {
                    res.Advertencias++;
                    res.Mensajes.Add($"[Validador][WARN] No se pudo validar nodos de recolección: {ex.Message}");
                }

                // 6) Enemigos: validar catálogo data-driven (rango de mitigaciones, duplicados, campos básicos)
                try
                {
                    var resEnem = ValidarEnemigosBasico();
                    // Merge resultados
                    res.Errores += resEnem.Errores;
                    res.Advertencias += resEnem.Advertencias;
                    foreach (var m in resEnem.Mensajes) res.Mensajes.Add(m);
                }
                catch (Exception ex)
                {
                    res.Advertencias++;
                    res.Mensajes.Add($"[Validador][WARN] No se pudo validar enemigos: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                res.Errores++;
                res.Mensajes.Add($"[Validador][EXC] {ex.GetType().Name}: {ex.Message}");
            }
            finally
            {
                if (!res.IsOk)
                {
                    Console.WriteLine($"[Validador] Finalizado con {res.Errores} errores y {res.Advertencias} advertencias");
                    foreach (var m in res.Mensajes) Console.WriteLine(m);
                }
                else
                {
                    Console.WriteLine("[Validador] OK sin errores");
                }

                try
                {
                    if (generarReporte)
                    {
                        // Preparar ruta por defecto si no se proporcionó
                        if (string.IsNullOrWhiteSpace(rutaReporte))
                        {
                            var carpeta = PathProvider.PjDatosPath("validacion");
                            Directory.CreateDirectory(carpeta);
                            var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                            rutaReporte = Path.Combine(carpeta, $"validacion-{stamp}.txt");
                        }

                        var lineas = new List<string>();
                        lineas.Add($"[Validador] Fecha: {DateTime.Now:O}");
                        lineas.Add($"[Validador] Resultado: Errores={res.Errores}, Advertencias={res.Advertencias}");
                        lineas.Add("");
                        lineas.AddRange(res.Mensajes);
                        File.WriteAllLines(rutaReporte!, lineas);
                        rutaEscrita = rutaReporte;
                        Console.WriteLine($"[Validador] Reporte escrito en: {rutaReporte}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Validador][WARN] No se pudo escribir el reporte: {ex.Message}");
                }
            }
            return res;
        }

        /// <summary>
        /// Valida los datos de enemigos bajo DatosJuego/enemigos (recursivo):
        /// - Campos básicos válidos (Nombre, VidaBase>0, AtaqueBase>=1, defensas >=0, Nivel>=1)
        /// - Mitigaciones y resistencias en [0..0.9]; daño elemental base >= 0
        /// - Duplicados por Nombre o Id (si existe)
        /// - Advertencias informativas por NoMuerto sin inmunidad explícita a veneno (se aplicará por defecto en runtime)
        /// - Ignora por convención los JSONs en la raíz de cada carpeta nivel_* bajo por_bioma
        /// </summary>
        public static Resultado ValidarEnemigosBasico()
        {
            var res = new Resultado();
            var dir = PathProvider.EnemigosDir();
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            opciones.Converters.Add(new JsonStringEnumConverter());
            var porNombre = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var porId = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int archivosLeidos = 0;

            if (!Directory.Exists(dir))
            {
                res.Advertencias++;
                res.Mensajes.Add($"[Enemigos][WARN] Carpeta no encontrada: {dir}");
                return res;
            }

            foreach (var file in Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    if (string.IsNullOrWhiteSpace(json)) continue;
                    archivosLeidos++;

                    if (DebeIgnorarseArchivoEnemigoPorConvencion(file))
                    {
                        res.Mensajes.Add($"[Enemigos][INFO] Ignorado por convención (raíz nivel_*) '{file}'");
                        continue;
                    }

                    using var doc = JsonDocument.Parse(json);
                    var kind = doc.RootElement.ValueKind;
                    if (kind == JsonValueKind.Array)
                    {
                        var lista = JsonSerializer.Deserialize<List<EnemigoData>>(json, opciones);
                        if (lista != null)
                        {
                            foreach (var e in lista) ValidarUno(e, file, res, porNombre, porId);
                        }
                        else
                        {
                            res.Errores++;
                            res.Mensajes.Add($"[Enemigos][ERR] Lista inválida en '{file}'");
                        }
                    }
                    else if (kind == JsonValueKind.Object)
                    {
                        var uno = JsonSerializer.Deserialize<EnemigoData>(json, opciones);
                        if (uno != null) ValidarUno(uno, file, res, porNombre, porId);
                        else
                        {
                            res.Errores++;
                            res.Mensajes.Add($"[Enemigos][ERR] Objeto inválido en '{file}'");
                        }
                    }
                    else
                    {
                        res.Errores++;
                        res.Mensajes.Add($"[Enemigos][ERR] Raíz JSON no válida en '{file}' (se esperaba objeto o lista)");
                    }
                }
                catch (Exception ex)
                {
                    res.Errores++;
                    res.Mensajes.Add($"[Enemigos][ERR] Archivo inválido '{file}': {ex.Message}");
                }
            }

            res.Mensajes.Add($"[Enemigos] Archivos procesados: {archivosLeidos}. Catálogo: {porNombre.Count} únicos por Nombre.");
            return res;

            static void ValidarUno(EnemigoData e, string origen, Resultado res, HashSet<string> porNombre, HashSet<string> porId)
            {
                if (e == null)
                {
                    res.Errores++;
                    res.Mensajes.Add($"[Enemigos][ERR] Entrada nula en {origen}");
                    return;
                }
                if (string.IsNullOrWhiteSpace(e.Nombre))
                {
                    res.Errores++;
                    res.Mensajes.Add($"[Enemigos][ERR] Enemigo sin Nombre en {origen}");
                }
                else
                {
                    if (!porNombre.Add(e.Nombre.Trim()))
                    {
                        res.Errores++;
                        res.Mensajes.Add($"[Enemigos][ERR] Nombre duplicado '{e.Nombre}' en {origen}");
                    }
                }

                if (!string.IsNullOrWhiteSpace(e.Id))
                {
                    if (!porId.Add(e.Id.Trim()))
                    {
                        res.Errores++;
                        res.Mensajes.Add($"[Enemigos][ERR] Id duplicado '{e.Id}' en {origen}");
                    }
                }

                if (e.VidaBase <= 0)
                {
                    res.Errores++;
                    res.Mensajes.Add($"[Enemigos][ERR] VidaBase debe ser > 0 para '{e.Nombre}' ({origen})");
                }
                if (e.AtaqueBase < 1)
                {
                    res.Advertencias++;
                    res.Mensajes.Add($"[Enemigos][WARN] AtaqueBase muy bajo (<1) en '{e.Nombre}' ({origen})");
                }
                if (e.DefensaBase < 0 || e.DefensaMagicaBase < 0)
                {
                    res.Errores++;
                    res.Mensajes.Add($"[Enemigos][ERR] Defensas negativas en '{e.Nombre}' ({origen})");
                }
                if (e.Nivel < 1)
                {
                    res.Errores++;
                    res.Mensajes.Add($"[Enemigos][ERR] Nivel < 1 en '{e.Nombre}' ({origen})");
                }

                if (e.MitigacionFisicaPorcentaje.HasValue)
                {
                    var v = e.MitigacionFisicaPorcentaje.Value;
                    if (v < 0 || v > 0.9)
                    {
                        res.Errores++;
                        res.Mensajes.Add($"[Enemigos][ERR] MitigacionFisicaPorcentaje fuera de rango [0..0.9] en '{e.Nombre}' ({v}) ({origen})");
                    }
                }
                if (e.MitigacionMagicaPorcentaje.HasValue)
                {
                    var v = e.MitigacionMagicaPorcentaje.Value;
                    if (v < 0 || v > 0.9)
                    {
                        res.Errores++;
                        res.Mensajes.Add($"[Enemigos][ERR] MitigacionMagicaPorcentaje fuera de rango [0..0.9] en '{e.Nombre}' ({v}) ({origen})");
                    }
                }

                // Spawn
                if (e.SpawnChance.HasValue && (e.SpawnChance < 0 || e.SpawnChance > 1))
                {
                    res.Errores++;
                    res.Mensajes.Add($"[Enemigos][ERR] SpawnChance fuera de [0..1] en '{e.Nombre}' ({e.SpawnChance}) ({origen})");
                }
                if (e.SpawnWeight.HasValue && e.SpawnWeight <= 0)
                {
                    res.Errores++;
                    res.Mensajes.Add($"[Enemigos][ERR] SpawnWeight debe ser > 0 en '{e.Nombre}' ({e.SpawnWeight}) ({origen})");
                }

                // Resistencias elementales
                if (e.ResistenciasElementales != null)
                {
                    foreach (var kv in e.ResistenciasElementales)
                    {
                        if (kv.Value < 0 || kv.Value > 0.9)
                        {
                            res.Errores++;
                            res.Mensajes.Add($"[Enemigos][ERR] Resistencia elemental '{kv.Key}' fuera de [0..0.9] en '{e.Nombre}' ({kv.Value}) ({origen})");
                        }
                    }
                }
                // Vulnerabilidades elementales: factor >= 1.0 y <= 1.5 (conservador)
                if (e.VulnerabilidadesElementales != null)
                {
                    foreach (var kv in e.VulnerabilidadesElementales)
                    {
                        if (kv.Value < 1.0 || kv.Value > 1.5)
                        {
                            res.Errores++;
                            res.Mensajes.Add($"[Enemigos][ERR] Vulnerabilidad elemental '{kv.Key}' fuera de [1.0..1.5] en '{e.Nombre}' ({kv.Value}) ({origen})");
                        }
                    }
                }

                // Daño elemental base no negativo
                if (e.DanioElementalBase != null)
                {
                    foreach (var kv in e.DanioElementalBase)
                    {
                        if (kv.Value < 0)
                        {
                            res.Errores++;
                            res.Mensajes.Add($"[Enemigos][ERR] Daño elemental base negativo '{kv.Key}' en '{e.Nombre}' ({kv.Value}) ({origen})");
                        }
                    }
                }

                // Drops
                if (e.Drops != null)
                {
                    foreach (var d in e.Drops)
                    {
                        if (d == null)
                        {
                            res.Errores++;
                            res.Mensajes.Add($"[Enemigos][ERR] Drop nulo en '{e.Nombre}' ({origen})");
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(d.Tipo) || string.IsNullOrWhiteSpace(d.Nombre))
                        {
                            res.Errores++;
                            res.Mensajes.Add($"[Enemigos][ERR] Drop con Tipo/Nombre vacío en '{e.Nombre}' ({origen})");
                        }
                        if (d.Chance < 0 || d.Chance > 1)
                        {
                            res.Errores++;
                            res.Mensajes.Add($"[Enemigos][ERR] Drop.Chance fuera de [0..1] ('{d.Nombre}' en '{e.Nombre}') ({d.Chance}) ({origen})");
                        }
                        if (d.CantidadMin < 1 || d.CantidadMax < d.CantidadMin)
                        {
                            res.Errores++;
                            res.Mensajes.Add($"[Enemigos][ERR] Drop cantidades inválidas ('{d.Nombre}' {d.CantidadMin}-{d.CantidadMax}) en '{e.Nombre}' ({origen})");
                        }
                    }
                }

                // Info: si NoMuerto sin inmunidad explícita a veneno, runtime la aplicará por defecto
                if (e.Familia == Familia.NoMuerto)
                {
                    bool tieneExpl = e.Inmunidades != null && e.Inmunidades.TryGetValue("veneno", out var val) && val;
                    if (!tieneExpl)
                    {
                        res.Mensajes.Add($"[Enemigos][INFO] NoMuerto '{e.Nombre}' sin 'veneno: true' explícito. Se aplicará por defecto en runtime.");
                    }
                }
            }
        }

        // Ignora archivos ubicados directamente en la carpeta 'nivel_*' dentro de 'enemigos/por_bioma/<bioma>/'
        // Acepta únicamente archivos dentro de subcarpetas por categoría.
        private static bool DebeIgnorarseArchivoEnemigoPorConvencion(string filePath)
        {
            try
            {
                var lower = filePath.ToLowerInvariant();
                if (!lower.Contains(Path.Combine("enemigos", "por_bioma").ToLowerInvariant()))
                    return false;

                var fileDir = Path.GetDirectoryName(filePath);
                if (string.IsNullOrEmpty(fileDir)) return false;
                var dirInfo = new DirectoryInfo(fileDir);
                // Si el directorio inmediato es 'nivel_*', entonces el archivo está en la raíz del nivel
                if (dirInfo.Name.StartsWith("nivel_", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        private static HashSet<string> CargarIdsMapa(string carpetaMapas)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!Directory.Exists(carpetaMapas)) return ids;
            var archivos = Directory.GetFiles(carpetaMapas, "*.json", SearchOption.AllDirectories);
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            foreach (var archivo in archivos)
            {
                try
                {
                    var texto = File.ReadAllText(archivo);
                    if (string.IsNullOrWhiteSpace(texto)) continue;
                    // intentar objeto
                    try
                    {
                        var uno = JsonSerializer.Deserialize<PjDatos.SectorData>(texto, opciones);
                        if (uno?.Id != null) ids.Add(uno.Id);
                    }
                    catch
                    {
                        // intentar lista
                        var lista = JsonSerializer.Deserialize<List<PjDatos.SectorData>>(texto, opciones);
                        if (lista != null)
                        {
                            foreach (var s in lista)
                            {
                                if (!string.IsNullOrWhiteSpace(s?.Id)) ids.Add(s.Id);
                            }
                        }
                    }
                }
                catch
                {
                    // ignorar corruptos; MapaLoader ya informa
                }
            }
            return ids;
        }

        private static bool EsSectorId(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return false;
            var parts = valor.Split('_');
            if (parts.Length != 2) return false;
            return int.TryParse(parts[0], out _) && int.TryParse(parts[1], out _);
        }

        private class NpcEntry
        {
            public string? Id { get; set; }
            public string? Ubicacion { get; set; }
            public List<string>? Misiones { get; set; }
        }

        private class MisionEntry
        {
            public string? Id { get; set; }
            public string? UbicacionNPC { get; set; }
            public string? SiguienteMisionId { get; set; }
            public List<string>? Condiciones { get; set; }
        }
    }
}
