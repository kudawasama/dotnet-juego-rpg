namespace MiJuegoRPG.Personaje
{
    public class AtributosBase
    {
        //Atributos Base Fisicos
        public int Fuerza { get; set; }  //Potencia física Daño físico, capacidad de carga, interacción física
        public int Destreza { get; set; } //Precisión y coordinación Precisión, críticos, uso de armas ligeras/herramientas
        public int Vitalidad { get; set; } //Resistencia y salud física Aumenta HP, defensa física, resistencia a estados
        public int Agilidad { get; set; } //Velocidad y evasión Velocidad de ataque, evasión, reducción de delay
        public int Suerte { get; set; } //Probabilidad de eventos positivos Aumenta críticos, evasión, recompensas
        public int Defensa { get; set; } //Protección física Reducción de daño físico, resistencia a estados físicos


        // Atributos Mágicos
        public int Resistencia { get; set; } //Defensa mágica Resistencia a daño mágico, reducción de efectos mágicos
        public int Sabiduría { get; set; } //Poder mágico defensivo Defensa mágica, regeneración de MP, resistencia a estados mágicos
        public int Inteligencia { get; set; } //Poder mágico ofensivo Daño mágico, MP, casteo
        public int Fe { get; set; } //Creencia y devoción Mejora de habilidades espirituales, resistencia a efectos negativos
       

        // Atributos de Interacción y Percepción
        public int Percepcion { get; set; } //Capacidad de observación y detección Mejora de habilidades de sigilo, detección de trampas
        public int Persuasion { get; set; } //Habilidad de convencer
        public int Liderazgo { get; set; } //Capacidad de guiar y motivar Mejora de habilidades sociales, liderazgo en grupos
        public int Carisma { get; set; } //Atractivo personal Mejora de habilidades sociales, influencia en NPCs
        public int Voluntad { get; set; } //Determinación y fuerza de voluntad Resistencia a efectos negativos, mejora de habilidades mentales




        public AtributosBase(int fuerza, int inteligencia, int agilidad, int vitalidad, int suerte, int resistencia, int sabiduria, int carisma, int destreza, int fe, int liderazgo, int percepcion, int persuasión, int voluntad, int defensa)
        {
            Fuerza = fuerza; // Potencia física
            Destreza = destreza; // Precisión y coordinación
            Vitalidad = vitalidad; // Resistencia y salud física
            Agilidad = agilidad; // Velocidad y evasión
            Suerte = suerte; // Probabilidad de eventos positivos
            Defensa = 0; // Valor por defecto

            Inteligencia = inteligencia; // Poder mágico ofensivo
            Resistencia = resistencia; // Valor por defecto                
            Sabiduría = sabiduria;
            Fe = fe; // Valor por defecto
            

            Carisma = carisma; // Valor por defecto
            Liderazgo = liderazgo; // Valor por defecto
            Percepcion = percepcion; // Valor por defecto
            Persuasion = persuasión; // Valor por defecto 
            Voluntad = voluntad; // Valor por defecto



        }

    }
}

