using System.Text.Json.Serialization;

namespace MiJuegoRPG.Personaje
{
    public class AtributosBase
    {
        //Atributos Base Fisicos
        public double Fuerza
        {
            get; set;
        }
        public double Destreza
        {
            get; set;
        }
        public double Vitalidad
        {
            get; set;
        }
        public double Agilidad
        {
            get; set;
        }
        public double Suerte
        {
            get; set;
        }
        public double Defensa
        {
            get; set;
        }
        public double Resistencia
        {
            get; set;
        }
        public double Sabiduría
        {
            get; set;
        }
        public double Inteligencia
        {
            get; set;
        }
        public double Fe
        {
            get; set;
        }
        public double Percepcion
        {
            get; set;
        }
        public double Persuasion
        {
            get; set;
        }
        public double Liderazgo
        {
            get; set;
        }
        public double Carisma
        {
            get; set;
        }
        public double Voluntad
        {
            get; set;
        }
        // Atributo especial para clases oscuras / alineaciones negativas
        public double Oscuridad
        {
            get; set;
        }

        [JsonConstructor]
        public AtributosBase()
        {
        }


        public AtributosBase(double fuerza, double inteligencia, double agilidad, double vitalidad, double suerte, double resistencia, double sabiduria, double carisma, double destreza, double fe, double liderazgo, double percepcion, double persuasión, double voluntad, double defensa)
        {
            Fuerza = fuerza;
            Destreza = destreza;
            Vitalidad = vitalidad;
            Agilidad = agilidad;
            Suerte = suerte;
            Defensa = defensa;
            Inteligencia = inteligencia;
            Resistencia = resistencia;
            Sabiduría = sabiduria;
            Fe = fe;
            Carisma = carisma;
            Liderazgo = liderazgo;
            Percepcion = percepcion;
            Persuasion = persuasión;
            Voluntad = voluntad;
        }
    }
}


