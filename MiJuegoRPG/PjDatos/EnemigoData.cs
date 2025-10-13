namespace MiJuegoRPG.PjDatos
{
    using System.Collections.Generic;

    public class EnemigoData
    {
        // Corrección: Asignamos un valor predeterminado para evitar el error.
        public string Nombre { get; set; } = string.Empty;

        public int VidaBase
        {
            get; set;
        }

        public int AtaqueBase
        {
            get; set;
        }

        public int DefensaBase
        {
            get; set;
        }

        public int DefensaMagicaBase
        {
            get; set;
        }

        public int Nivel
        {
            get; set;
        }

        public int ExperienciaRecompensa
        {
            get; set;
        }

        public int OroRecompensa
        {
            get; set;
        }

        public Familia Familia
        {
            get; set;
        }

        public string Rareza { get; set; } = "Comun";

        public string Categoria { get; set; } = "Normal";

        // Permite asignar el nombre de un arma desde el JSON
        public string ArmaNombre { get; set; } = string.Empty;

        // NUEVO: Campos opcionales para configuración avanzada (data-driven)
        // Diccionario de inmunidades por palabra clave ("veneno", "sangrado", etc.)
        public Dictionary<string, bool>? Inmunidades
        {
            get; set;
        }

        // Mitigaciones porcentuales adicionales (0..1)
        public double? MitigacionFisicaPorcentaje
        {
            get; set;
        }

        public double? MitigacionMagicaPorcentaje
        {
            get; set;
        }

        // Etiquetas adicionales para filtros/contadores (además de Nombre)
        public List<string>? Tags
        {
            get; set;
        }

        // Id opcional único para el enemigo (útil a futuro)
        public string? Id
        {
            get; set;
        }

        // NUEVO: Configuración de spawn
        // Probabilidad absoluta 0..1. Si se define junto a Weight, Chance tiene prioridad dura.
        public double? SpawnChance
        {
            get; set;
        }

        // Peso relativo para selección ponderada dentro de una bolsa.
        public int? SpawnWeight
        {
            get; set;
        }

        // NUEVO: Resistencias elementales (0..0.9) por tipo ("fuego","hielo","rayo","veneno","sangrado", etc.)
        public Dictionary<string, double>? ResistenciasElementales
        {
            get; set;
        }

        // NUEVO: Vulnerabilidades elementales (factor >= 1.0). Se aplica post-mitigación
        // Por política de progresión lenta, se recomienda 1.00..1.50 (el validador lo exigirá)
        public Dictionary<string, double>? VulnerabilidadesElementales
        {
            get; set;
        }

        // NUEVO: Daño elemental base adicional (aplicado en ataque mágico/según arma)
        public Dictionary<string, int>? DanioElementalBase
        {
            get; set;
        }

        // NUEVO: Equipo inicial (por ahora arma por nombre, ampliable a armadura/accesorios)
        public EquipoInicialData? EquipoInicial
        {
            get; set;
        }

        // NUEVO: Reglas de botín data-driven
        public List<DropRuleData>? Drops
        {
            get; set;
        }

        // NUEVO: Evasión (0..0.95). Si no se especifica, se asume 0.
        public double? EvasionFisica
        {
            get; set;
        }

        public double? EvasionMagica
        {
            get; set;
        }
    }
}
