using System;

namespace MiJuegoRPG.Motor.Servicios
{
    /// <summary>
    /// Normaliza strings de rareza a una forma canónica usada en repositorios/objetos.
    /// Política: mapear variantes históricas (Comun/Normal, Raro→Rara, Epico, Legendario...).
    /// Mantiene capitalización simple (PrimeraMayúscula resto igual) y elimina espacios.
    /// </summary>
    public static class RarezaNormalizer
    {
        public static string Normalizar(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Comun"; // fallback seguro
            var r = QuitarAcentos(input.Trim());
            var lower = r.ToLowerInvariant().Replace(" ", "");
            return lower switch
            {
                "normal" or "comun" => "Comun",
                "pococomun" => "Superior",
                "raro" => "Rara",
                "epico" => "Epica",
                "legendario" => "Legendaria",
                _ => char.ToUpper(r[0]) + r.Substring(1)
            };
        }

        private static string QuitarAcentos(string s)
        {
            s = s.Replace("ó", "o").Replace("Ó", "O")
                 .Replace("ú", "u").Replace("Ú", "U")
                 .Replace("á", "a").Replace("Á", "A")
                 .Replace("é", "e").Replace("É", "E")
                 .Replace("í", "i").Replace("Í", "I");
            return s;
        }
    }
}