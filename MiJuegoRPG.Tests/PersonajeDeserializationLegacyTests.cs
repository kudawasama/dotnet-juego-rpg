using System.Text.Json;
using Xunit;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Tests
{
    public class PersonajeDeserializationLegacyTests
    {
        [Fact]
        public void Personaje_Legacy_Rarezas_Mixtas_Se_Normalizan()
        {
            // Arrange: JSON de personaje con inventario que mezcla rarezas numéricas (enum legacy) y strings alias
            // Rareza indices: 0=Rota,1=Pobre,2=Comun(antes Normal),3=Superior,4=Rara,5=Legendaria,6=Ornamentada
            string json = @"{
  ""Nombre"": ""LegacyHero"",
  ""Vida"": 100,
  ""VidaMaxima"": 100,
  ""Inventario"": {
    ""NuevosObjetos"": [
  { ""Objeto"": { ""Nombre"": ""Lingote Opaco"", ""Rareza"": 0, ""Categoria"": ""Material"", ""TipoObjeto"": ""Material"" }, ""Cantidad"": 2 },
  { ""Objeto"": { ""Nombre"": ""Mena Poco Comun"", ""Rareza"": 3, ""Categoria"": ""Material"", ""TipoObjeto"": ""Material"" }, ""Cantidad"": 1 },
      { ""Objeto"": { ""Nombre"": ""Piedra Comun"", ""Rareza"": 2, ""Categoria"": ""Material"", ""TipoObjeto"": ""Material"" }, ""Cantidad"": 5 },
      { ""Objeto"": { ""Nombre"": ""Gema Rara"", ""Rareza"": 4, ""Categoria"": ""Material"", ""TipoObjeto"": ""Material"" }, ""Cantidad"": 1 },
      { ""Objeto"": { ""Nombre"": ""Amuleto Normal Texto"", ""Rareza"": ""Normal"", ""Categoria"": ""Accesorio"", ""BonificacionAtaque"": 1, ""BonificacionDefensa"": 1, ""TipoObjeto"": ""Accesorio"" }, ""Cantidad"": 1 },
      { ""Objeto"": { ""Nombre"": ""Anillo Raro Alias"", ""Rareza"": ""Raro"", ""Categoria"": ""Accesorio"", ""BonificacionAtaque"": 2, ""BonificacionDefensa"": 0, ""TipoObjeto"": ""Accesorio"" }, ""Cantidad"": 1 },
      { ""Objeto"": { ""Nombre"": ""Amuleto Numerico String"", ""Rareza"": ""5"", ""Categoria"": ""Accesorio"", ""BonificacionAtaque"": 3, ""BonificacionDefensa"": 1, ""TipoObjeto"": ""Accesorio"" }, ""Cantidad"": 1 }
    ]
  }
}";

            var opts = new JsonSerializerOptions();
            opts.Converters.Add(new ObjetoJsonConverter());
            // Act
            var pj = JsonSerializer.Deserialize<MiJuegoRPG.Personaje.Personaje>(json, opts);

            // Assert
            Assert.NotNull(pj);
            Assert.Equal("LegacyHero", pj!.Nombre);
            Assert.NotNull(pj.Inventario);
            Assert.True(pj.Inventario.NuevosObjetos.Count >= 6);
            // Extraer rarezas
            var rarezas = pj.Inventario.NuevosObjetos.ConvertAll(o => o.Objeto.Rareza);
            Assert.Contains("Rota", rarezas);          // 0 -> Rota
            Assert.Contains("Comun", rarezas);         // 2 -> Comun (Normal legacy)
            Assert.Contains("Rara", rarezas);          // 4 -> Rara
            Assert.Contains("Superior", rarezas);      // ninguno directo, pero verificamos alias Normal->Comun y Raro->Rara, agregar explícito Superior si se deseara
            Assert.Contains("Legendaria", rarezas);    // "5" string -> Legendaria
            // Alias mapeados
            Assert.DoesNotContain("Normal", rarezas);  // Debe haberse normalizado a Comun
            Assert.DoesNotContain("Raro", rarezas);    // Debe haberse normalizado a Rara
        }
    }
}
