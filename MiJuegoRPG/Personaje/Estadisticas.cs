namespace MiJuegoRPG.Personaje
{
    public class Estadisticas
    {
        //Estadisticas Fisicas
        public double Critico { get; set; } // Probabilidad de golpe crítico
        public double Evasion { get; set; } // Probabilidad de evadir un ataque
        public double Velocidad { get; set; }    // Velocidad de movimiento y ataque
        public double Ataque { get; set; } // Daño físico infligido
        public double Regeneracion { get; set; } // Regeneración de salud y recursos
        public double Resistencia { get; set; } // Resistencia a efectos negativos
        public double Salud { get; set; } // Salud máxima
        public double Mana { get; set; } // Mana máxima
        public double Energia { get; set; } // Energía máxima
        public double Carga { get; set; } // Capacidad de carga

        // Nuevos campos de cálculo de combate (base para pipeline de daño)
        public double Precision { get; set; } // Precisión para calcular acierto (0..1)
        public double CritChance { get; set; } // Probabilidad de crítico (alias moderno de Critico)
        public double CritMult { get; set; } // Multiplicador de crítico (>=1.0)
        public double Penetracion { get; set; } // Porcentaje de penetración de defensa (0..1)
        public double Daño { get; set; } // Daño físico infligido
        public double DefensaFisica { get; set; } // Reducción de daño físico recibido
        public double PoderOfensivoFisico { get; set; } // Potencia de habilidades físicas ofensivas
        public double PoderDefensivoFisico { get; set; } // Potencia de habilidades físicas defensivas


        //Estadisticas Magicas
        public double PoderMagico { get; set; } // Potencia de hechizos
        public double DefensaMagica { get; set; } // Reducción de daño mágico recibido
        public double RegeneracionMana { get; set; } // Regeneración de mana
        public double PoderDefensivoMagico { get; set; } // Defensa contra hechizos
        public double PoderOfensivoMagico { get; set; } // Potencia de hechizos ofensivos
        public double PoderEspiritual { get; set; } // Potencia de habilidades espirituales
        public double ResistenciaMagica { get; set; } // Resistencia a efectos mágicos
        public double AfinidadElemental { get; set; } // Conexión con elementos
        public double PoderElemental { get; set; } // Potencia de habilidades elementales
        public double ResistenciaElemental { get; set; } // Resistencia a efectos elementales
        public double PoderCurativo { get; set; } // Potencia de habilidades curativas
        public double PoderDeControl { get; set; } // Potencia de habilidades de control
        public double PoderDeSoporte { get; set; } // Potencia de habilidades de soporte
        public double PoderDeInvocacion { get; set; } // Potencia de habilidades de invocación
        public double PoderDeTransmutacion { get; set; } // Potencia de habilidades de transmutación
        public double PoderDeAlteracion { get; set; } // Potencia de habilidades de alteración
        public double PoderDeIlusion { get; set; } // Potencia de habilidades de ilusión
        public double PoderDeConjuracion { get; set; } // Potencia de habilidades de conjuración
        public double PoderDeDestruccion { get; set; } // Potencia de habilidades de destrucción
        public double PoderDeRestauracion { get; set; } // Potencia de habilidades de restauración
        public double PoderDeTransporte { get; set; } // Potencia de habilidades de transporte 
        public double PoderDeManipulacion { get; set; } // Potencia de habilidades de manipulación


    public Estadisticas() { }



    public Estadisticas(AtributosBase atributosBase)
        {
            Critico = atributosBase.Destreza * 0.01 + atributosBase.Suerte * 0.01; // Probabilidad de golpe crítico basada en Destreza y Suerte
            Evasion = atributosBase.Agilidad * 0.01 + atributosBase.Suerte * 0.01; // Probabilidad de evadir un ataque basada en Agilidad y Suerte
            Velocidad = atributosBase.Agilidad * 0.01; // Velocidad de movimiento y ataque basada en Agilidad
            Ataque = atributosBase.Fuerza * 0.01 + atributosBase.Destreza * 0.01; // Daño físico infligido basado en Fuerza y Destreza
            // Nuevos (defaults conservadores, alineados a progresión lenta):
            // Precisión: depende principalmente de Destreza y Percepción
            // Caps de combate desde configuración (centralizados en CombatBalanceConfig)
            MiJuegoRPG.Motor.Servicios.CombatBalanceConfig.EnsureLoaded();
            Precision = MiJuegoRPG.Motor.Servicios.CombatBalanceConfig.ClampPrecision(
                atributosBase.Destreza * 0.01 + atributosBase.Percepcion * 0.005);
            // CritChance: mantener compatibilidad usando el mismo cálculo que 'Critico'
            CritChance = MiJuegoRPG.Motor.Servicios.CombatBalanceConfig.ClampCritChance(Critico);
            // CritMult: base 1.5x con ajuste suave por Sabiduría (ciencia táctica)
            CritMult = MiJuegoRPG.Motor.Servicios.CombatBalanceConfig.ClampCritMult(1.5 + (atributosBase.Sabiduría * 0.001));
            // Penetración: muy baja por defecto (requiere equipo/habilidades para crecer)
            Penetracion = MiJuegoRPG.Motor.Servicios.CombatBalanceConfig.ClampPenetracion(
                atributosBase.Destreza * 0.002);
            Regeneracion = atributosBase.Vitalidad * 0.01; // Regeneración de salud y recursos basada en Vitalidad
            Resistencia = atributosBase.Vitalidad * 0.01 + atributosBase.Sabiduría * 0.01; // Resistencia a efectos negativos basada en Vitalidad y Sabiduría
            Salud = atributosBase.Vitalidad * 10; // Salud máxima basada en Vitalidad
            Mana =
                atributosBase.Inteligencia * 10 +
                atributosBase.Sabiduría * 3 +
                atributosBase.Fe * 2 +
                atributosBase.Voluntad * 1 +
                atributosBase.Carisma * 0.5 +
                atributosBase.Liderazgo * 0.5 +
                atributosBase.Vitalidad * 0.2 +
                atributosBase.Fuerza * 0.1 +
                atributosBase.Destreza * 0.1 +
                atributosBase.Agilidad * 0.1 +
                atributosBase.Suerte * 0.1 +
                atributosBase.Defensa * 0.1 +
                atributosBase.Resistencia * 0.1 +
                atributosBase.Percepcion * 0.1 +
                atributosBase.Persuasion * 0.1;
            Energia = atributosBase.Agilidad * 10; // Energía máxima basada en Agilidad
            Carga = atributosBase.Fuerza * 5; // Capacidad de carga basada en Fuerza
            Daño = atributosBase.Fuerza * 0.01 + atributosBase.Destreza * 0.01; // Daño físico infligido basado en Fuerza y Destreza
            DefensaFisica = atributosBase.Defensa * 0.01 + atributosBase.Vitalidad * 0.01; // Reducción de daño físico recibido basada en Defensa y Vitalidad
            PoderOfensivoFisico = atributosBase.Fuerza * 0.01 + atributosBase.Destreza * 0.01; // Potencia de habilidades físicas ofensivas basada en Fuerza y Destreza
            PoderDefensivoFisico = atributosBase.Defensa * 0.01 + atributosBase.Vitalidad * 0.01; // Potencia de habilidades físicas defensivas basada en Defensa y Vitalidad
            PoderMagico = atributosBase.Inteligencia * 0.01 + atributosBase.Sabiduría * 0.01; // Potencia de hechizos basada en Inteligencia y Sabiduría
            DefensaMagica = atributosBase.Resistencia * 0.01 + atributosBase.Sabiduría * 0.01; // Reducción de daño mágico recibido basada en Resistencia y Sabiduría
            RegeneracionMana = atributosBase.Inteligencia * 0.01; // Regeneración de mana basada en Inteligencia
            PoderDefensivoMagico = atributosBase.Resistencia * 0.01 + atributosBase.Sabiduría * 0.01; // Defensa contra hechizos basada en Resistencia y Sabiduría
            PoderOfensivoMagico = atributosBase.Inteligencia * 0.01 + atributosBase.Sabiduría * 0.01; // Potencia de hechizos ofensivos basada en Inteligencia y Sabiduría
            PoderEspiritual = atributosBase.Fe * 0.01 + atributosBase.Sabiduría * 0.01; // Potencia de habilidades espirituales basada en Fe y Sabiduría
            ResistenciaMagica = atributosBase.Resistencia * 0.01 + atributosBase.Fe * 0.01; // Resistencia a efectos mágicos basada en Resistencia y Fe
            AfinidadElemental = atributosBase.Sabiduría * 0.01; // Conexión con elementos basada en Afinidad Elemental
            PoderElemental = atributosBase.Sabiduría * 0.01 + atributosBase.Inteligencia * 0.01; // Potencia de habilidades elementales basada en Afinidad Elemental e Inteligencia
            ResistenciaElemental = atributosBase.Sabiduría * 0.01 + atributosBase.Resistencia * 0.01; // Resistencia a efectos elementales basada en Afinidad Elemental y Resistencia
            PoderCurativo = atributosBase.Fe * 0.01 + atributosBase.Sabiduría * 0.01; // Potencia de habilidades curativas basada en Fe y Sabiduría
            PoderDeControl = atributosBase.Inteligencia * 0.01 + atributosBase.Sabiduría * 0.01; // Potencia de habilidades de control basada en Inteligencia y Sabiduría
            PoderDeSoporte = atributosBase.Fe * 0.01 + atributosBase.Sabiduría * 0.01; // Potencia de habilidades de soporte basada en Fe y Sabiduría
            PoderDeInvocacion = atributosBase.Fe * 0.01 + atributosBase.Inteligencia * 0.01; // Potencia de habilidades de invocación basada en Fe e Inteligencia
            PoderDeTransmutacion = atributosBase.Inteligencia * 0.01 + atributosBase.Sabiduría * 0.01; // Potencia de habilidades de transmutación basada en Inteligencia y Sabiduría
            PoderDeAlteracion = atributosBase.Inteligencia * 0.01 + atributosBase.Sabiduría * 0.01; // Potencia de habilidades de alteración basada en Inteligencia y Sabiduría
            PoderDeIlusion = atributosBase.Inteligencia * 0.01 + atributosBase.Sabiduría * 0.01; // Potencia de habilidades de ilusión basada en Inteligencia y Sabiduría
            PoderDeConjuracion = atributosBase.Inteligencia * 0.01 + atributosBase.Sabiduría * 0.01; // Potencia de habilidades de conjuración basada en Inteligencia y Sabiduría
            PoderDeDestruccion = atributosBase.Inteligencia * 0.01 + atributosBase.Sabiduría * 0.01; // Potencia de habilidades de destrucción basada en Inteligencia y Sabiduría
            PoderDeRestauracion = atributosBase.Fe * 0.01 + atributosBase.Sabiduría * 0.01; // Potencia de habilidades de restauración basada en Fe y Sabiduría
            PoderDeTransporte = atributosBase.Inteligencia * 0.01 + atributosBase.Sabiduría * 0.01; // Potencia de habilidades de transporte basada en Inteligencia y Sabiduría

        }
    }
}
