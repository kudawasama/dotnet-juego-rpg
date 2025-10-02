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
// (Vacío intencionalmente)
