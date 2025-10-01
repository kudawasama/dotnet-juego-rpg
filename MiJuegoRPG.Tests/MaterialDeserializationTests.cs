using System.Text.Json;
using Xunit;
using MiJuegoRPG.Objetos;

namespace MiJuegoRPG.Tests
{
    public class MaterialDeserializationTests
    {
        private T RoundTrip<T>(string json) where T: class
        {
            var opts = new JsonSerializerOptions();
            opts.Converters.Add(new ObjetoJsonConverter());
            return JsonSerializer.Deserialize<T>(json, opts)!;
        }

        [Theory]
        [InlineData("Normal", "Comun")]
        [InlineData("PocoComun", "Superior")]
        [InlineData("Poco Comun", "Superior")]
        [InlineData("Raro", "Rara")]
        [InlineData("Epico", "Epica")]
        [InlineData("Legendario", "Legendaria")]
        [InlineData("2", "Comun")]
        [InlineData("5", "Legendaria")]
        public void Material_Alias_Rareza_Normaliza(string input, string esperado)
        {
            string json = $"{{ \"Nombre\": \"TestMat\", \"Rareza\": \"{input}\", \"Categoria\": \"Material\", \"TipoObjeto\": \"Material\" }}";
            var obj = RoundTrip<Objeto>(json) as Material;
            Assert.NotNull(obj);
            Assert.Equal(esperado, obj!.Rareza);
        }

        [Theory]
        [InlineData(0, "Rota")]
        [InlineData(2, "Comun")]
        [InlineData(3, "Superior")]
        [InlineData(5, "Legendaria")]
        [InlineData(99, "Comun")] // fuera de rango fallback
        public void Material_Rareza_Numero_Legacy(int numero, string esperado)
        {
            string json = $"{{ \"Nombre\": \"NumMat\", \"Rareza\": {numero}, \"Categoria\": \"Material\", \"TipoObjeto\": \"Material\" }}";
            var obj = RoundTrip<Objeto>(json) as Material;
            Assert.NotNull(obj);
            Assert.Equal(esperado, obj!.Rareza);
        }

        [Fact]
        public void Material_Sin_Rareza_Fallback_Comun()
        {
            string json = "{ \"Nombre\": \"TestMat\", \"Categoria\": \"Material\", \"TipoObjeto\": \"Material\" }";
            var obj = RoundTrip<Objeto>(json) as Material;
            Assert.NotNull(obj);
            Assert.Equal("Comun", obj!.Rareza);
        }
    }
}
