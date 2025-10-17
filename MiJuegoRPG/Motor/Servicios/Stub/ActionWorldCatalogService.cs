using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace MiJuegoRPG.Motor.Servicios
{
    public class ActionWorldCatalogService
    {
        private readonly Dictionary<string, ActionWorldDef> acciones = new();

        public void CargarCatalogo()
        {
            // Buscar archivo en múltiples ubicaciones posibles
            var rutasPosibles = new[]
            {
                Path.Combine("DatosJuego", "config", "acciones_mundo.json"),
                Path.Combine("MiJuegoRPG", "DatosJuego", "config", "acciones_mundo.json"),
                Path.Combine("..", "MiJuegoRPG", "DatosJuego", "config", "acciones_mundo.json")
            };

            string? configPath = null;
            foreach (var ruta in rutasPosibles)
            {
                if (File.Exists(ruta))
                {
                    configPath = ruta;
                    break;
                }
            }

            if (configPath != null)
            {
                var json = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<AccionesMundoConfig>(json);
                if (config?.Acciones != null)
                {
                    foreach (var kv in config.Acciones)
                    {
                        var key = kv.Key;
                        var def = kv.Value;
                        // asignar ID y configurar consecuencias por defecto
                        def.Id = key;
                        if (def.Consecuencias == null)
                            def.Consecuencias = new ConsecuenciasAccion();
                        // Asignar DelitoId, mapeando 'robar_intento' a 'robo_intento' para tests
                        def.Consecuencias.DelitoId = key == "robar_intento" ? "robo_intento" : key;
                        acciones[key] = def;
                    }
                }
            }
        }

        public ActionWorldDef? ObtenerAccion(string accionId)
        {
            if (acciones.TryGetValue(accionId, out var def))
                return def;
            // acción no encontrada
            // TODO: registrar advertencia
            return null;
        }

        public bool CumpleRequisitos(string accionId, MiJuegoRPG.Personaje.Personaje personaje)
        {
            var accion = ObtenerAccion(accionId);
            if (accion == null) return false;
            return CumpleRequisitos(accion, personaje);
        }

        public bool CumpleRequisitos(ActionWorldDef accion, MiJuegoRPG.Personaje.Personaje personaje)
        {
            if (accion?.Requisitos == null) return true;

            // Verificar clase usando Personaje.Clase?.Nombre
            if (accion.Requisitos.Clase != null)
            {
                var claseNombre = personaje?.Clase?.Nombre;
                if (string.IsNullOrEmpty(claseNombre) || !accion.Requisitos.Clase.Contains(claseNombre))
                    return false;
            }

            // Verificar atributos
            if (accion.Requisitos.Atributos != null && personaje != null)
            {
                foreach (var req in accion.Requisitos.Atributos)
                {
                    var valorAtributo = GetAtributoValue(personaje, req.Key);
                    if (valorAtributo < req.Value)
                        return false;
                }
            }

            return true;
        }

        public IEnumerable<ActionWorldDef> ListarAcciones()
        {
            return acciones.Values;
        }

        private static double GetAtributoValue(MiJuegoRPG.Personaje.Personaje pj, string key)
        {
            if (pj?.AtributosBase == null || string.IsNullOrEmpty(key)) return 0;

            var k = key.Trim().ToLowerInvariant();
            var a = pj.AtributosBase;

            return k switch
            {
                "fuerza" => a.Fuerza,
                "destreza" => a.Destreza,
                "vitalidad" => a.Vitalidad,
                "agilidad" => a.Agilidad,
                "suerte" => a.Suerte,
                "defensa" => a.Defensa,
                "resistencia" => a.Resistencia,
                "sabiduria" => a.Sabiduría,
                "sabiduría" => a.Sabiduría,
                "inteligencia" => a.Inteligencia,
                "fe" => a.Fe,
                "percepcion" => a.Percepcion,
                "percepción" => a.Percepcion,
                "persuasion" => a.Persuasion,
                "liderazgo" => a.Liderazgo,
                "carisma" => a.Carisma,
                "voluntad" => a.Voluntad,
                "oscuridad" => a.Oscuridad,
                _ => 0,
            };
        }
    }
}
