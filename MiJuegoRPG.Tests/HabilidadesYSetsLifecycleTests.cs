using System;
using System.Collections.Generic;
using System.Linq;
using MiJuegoRPG.Habilidades;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Servicios;
using MiJuegoRPG.Objetos;
using MiJuegoRPG.Personaje;
using Xunit;

namespace MiJuegoRPG.Tests
{
    public class HabilidadesYSetsLifecycleTests
    {
        private Personaje.Personaje NuevoPJ()
        {
            var pj = new Personaje.Personaje("Tester");
            // Forzar carga de equipo y sets
            GeneradorObjetos.CargarEquipoAuto();
            SetBonusService.Instancia.CargarSets();
            return pj;
        }

        [Fact]
        public void HabilidadDeEquipo_SeAgregaYSeQuita_AlEquiparDesequipar()
        {
            var pj = NuevoPJ();
            // Crear un objeto dummy que otorgue una habilidad conocida del catálogo si existe; fallback a id ficticia
            // Usamos una habilidad dummy no presente en catálogo para evitar gating por requisitos
            const string HabilidadDummy = "HabilidadTemporal_Prueba";
            var obj = new Accesorio("Amuleto de Prueba", 0, 0)
            {
                HabilidadesOtorgadas = new System.Collections.Generic.List<HabilidadOtorgadaRef>
                {
                    new HabilidadOtorgadaRef{ Id = HabilidadDummy, NivelMinimo = 1 }
                }
            };

            // Equipar: va al slot Accesorio1
            pj.Inventario.EquiparObjeto(obj, pj);
            // Sincronizar ya es llamado dentro de EquiparObjeto cuando pasa pj
            Assert.Contains(HabilidadDummy, pj.Habilidades.Keys);
            Assert.Contains(HabilidadDummy, pj.HabilidadesTemporalesEquipo);

            // Desequipar: limpiar Accesorio1 y re-sincronizar
            pj.Inventario.Equipo.Accesorio1 = null;
            pj.Inventario.SincronizarHabilidadesYBonosSet(pj);
            Assert.DoesNotContain(HabilidadDummy, pj.Habilidades.Keys);
            Assert.DoesNotContain(HabilidadDummy, pj.HabilidadesTemporalesEquipo);
        }

        [Fact]
        public void SetGM_BonosPorUmbral_AplicanYSeLimpian()
        {
            var pj = NuevoPJ();

            // Construir 6 piezas con SetId = GM de tipos distintos para simular set completo
            var casco = new Casco { Nombre = "Casco DIVINO GM GOD", Defensa = 1, SetId = "GM" };
            var armadura = new Armadura { Nombre = "Armadura DIVINO GM GOD", Defensa = 1, SetId = "GM" };
            var pantalon = new Pantalon { Nombre = "Pantalon DIVINO GM GOD", Defensa = 1, SetId = "GM" };
            var botas = new Botas { Nombre = "Botas DIVINO GM GOD", Defensa = 1, SetId = "GM" };
            var cinturon = new Cinturon { Nombre = "Cinturon DIVINO GM GOD", BonificacionCarga = 0, SetId = "GM" };
            var collar = new Collar { Nombre = "Collar DIVINO GM GOD", BonificacionDefensa = 0, BonificacionEnergia = 0, SetId = "GM" };

            // Equipar progresivamente y validar umbrales 2,4,6
            pj.Inventario.Equipo.Casco = casco; pj.Inventario.Equipo.Armadura = armadura;
            pj.Inventario.SincronizarHabilidadesYBonosSet(pj);
            Assert.True(pj.BonosTemporalesSet.TryGetValue("Defensa", out var def2) && def2 >= 5000);

            pj.Inventario.Equipo.Pantalon = pantalon; pj.Inventario.Equipo.Zapatos = botas;
            pj.Inventario.SincronizarHabilidadesYBonosSet(pj);
            Assert.True(pj.BonosTemporalesSet.TryGetValue("Ataque", out var atk4) && atk4 >= 5000);

            pj.Inventario.Equipo.Cinturon = cinturon; pj.Inventario.Equipo.Collar = collar;
            pj.Inventario.SincronizarHabilidadesYBonosSet(pj);
            Assert.True(pj.BonosTemporalesSet.TryGetValue("Mana", out var mana6) && mana6 >= 20000);
            Assert.True(pj.BonosTemporalesSet.TryGetValue("Energia", out var ener6) && ener6 >= 20000);

            // Perder umbral: quitar una pieza y verificar limpieza (pierde 6p y 4p si baja a 3)
            pj.Inventario.Equipo.Collar = null;
            pj.Inventario.SincronizarHabilidadesYBonosSet(pj);
            Assert.False(pj.BonosTemporalesSet.ContainsKey("Mana"));
            Assert.False(pj.BonosTemporalesSet.ContainsKey("Energia"));
            // Con 5 piezas deberían permanecer los bonos de 2 y 4
            Assert.True(pj.BonosTemporalesSet.ContainsKey("Defensa"));
            Assert.True(pj.BonosTemporalesSet.ContainsKey("Ataque"));

            // Bajar a 1 pieza → no queda bono
            pj.Inventario.Equipo.Armadura = null;
            pj.Inventario.Equipo.Pantalon = null;
            pj.Inventario.Equipo.Zapatos = null;
            pj.Inventario.Equipo.Cinturon = null;
            pj.Inventario.SincronizarHabilidadesYBonosSet(pj);
            Assert.False(pj.BonosTemporalesSet.ContainsKey("Defensa"));
            Assert.False(pj.BonosTemporalesSet.ContainsKey("Ataque"));
        }

        [Fact]
        public void SetGM_HabilidadesPorUmbral_SeAplicanYSeLimpian()
        {
            var pj = NuevoPJ();

            // Armar 6 piezas GM
            var casco = new Casco { Nombre = "Casco DIVINO GM GOD", Defensa = 1, SetId = "GM" };
            var armadura = new Armadura { Nombre = "Armadura DIVINO GM GOD", Defensa = 1, SetId = "GM" };
            var pantalon = new Pantalon { Nombre = "Pantalon DIVINO GM GOD", Defensa = 1, SetId = "GM" };
            var botas = new Botas { Nombre = "Botas DIVINO GM GOD", Defensa = 1, SetId = "GM" };
            var cinturon = new Cinturon { Nombre = "Cinturon DIVINO GM GOD", BonificacionCarga = 0, SetId = "GM" };
            var collar = new Collar { Nombre = "Collar DIVINO GM GOD", BonificacionDefensa = 0, BonificacionEnergia = 0, SetId = "GM" };

            // Consultar definiciones del servicio para saber si existen habilidades configuradas por umbral
            var servicio = SetBonusService.Instancia;
            // 0 → sin piezas
            pj.Inventario.SincronizarHabilidadesYBonosSet(pj);
            var baselineTemporales = pj.HabilidadesTemporalesEquipo.ToHashSet(StringComparer.OrdinalIgnoreCase);

            // 2 piezas
            pj.Inventario.Equipo.Casco = casco; pj.Inventario.Equipo.Armadura = armadura;
            pj.Inventario.SincronizarHabilidadesYBonosSet(pj);
            var (_, habs2) = servicio.CalcularBonosYHabilidades(pj.ObtenerObjetosEquipados());
            foreach (var (hid, _) in habs2)
            {
                Assert.Contains(hid, pj.Habilidades.Keys);
                Assert.Contains(hid, pj.HabilidadesTemporalesEquipo);
            }

            // 4 piezas
            pj.Inventario.Equipo.Pantalon = pantalon; pj.Inventario.Equipo.Zapatos = botas;
            pj.Inventario.SincronizarHabilidadesYBonosSet(pj);
            var (_, habs4) = servicio.CalcularBonosYHabilidades(pj.ObtenerObjetosEquipados());
            foreach (var (hid, _) in habs4)
            {
                Assert.Contains(hid, pj.Habilidades.Keys);
                Assert.Contains(hid, pj.HabilidadesTemporalesEquipo);
            }

            // 6 piezas
            pj.Inventario.Equipo.Cinturon = cinturon; pj.Inventario.Equipo.Collar = collar;
            pj.Inventario.SincronizarHabilidadesYBonosSet(pj);
            var (_, habs6) = servicio.CalcularBonosYHabilidades(pj.ObtenerObjetosEquipados());
            foreach (var (hid, _) in habs6)
            {
                Assert.Contains(hid, pj.Habilidades.Keys);
                Assert.Contains(hid, pj.HabilidadesTemporalesEquipo);
            }

            // Perder umbral 6 → quitar collar
            pj.Inventario.Equipo.Collar = null;
            pj.Inventario.SincronizarHabilidadesYBonosSet(pj);
            var (_, habsDespuesQuitar) = servicio.CalcularBonosYHabilidades(pj.ObtenerObjetosEquipados());
            // Todas las habilidades temporales del set que ya no estén en la lista calculada deben limpiarse
            var vigentes = new HashSet<string>(habsDespuesQuitar.Select(h => h.id), StringComparer.OrdinalIgnoreCase);
            foreach (var id in pj.HabilidadesTemporalesEquipo)
            {
                if (!vigentes.Contains(id) && !baselineTemporales.Contains(id))
                {
                    Assert.DoesNotContain(id, pj.Habilidades.Keys);
                }
            }
        }
        [Fact]
        public void Catalogo_ElegiblesPorNivel_Y_EvolucionPorUso()
        {
            var pj = NuevoPJ();
            // Asegurar que el catálogo carga
            var todas = HabilidadCatalogService.Todas;
            Assert.NotNull(todas);

            // Configurar atributos necesarios para que GolpeFuerte sea elegible
            pj.AtributosBase.Fuerza = 10; pj.AtributosBase.Agilidad = 10; pj.AtributosBase.Destreza = 10;
            pj.Nivel = 10; // suficiente para condiciones básicas si hay

            var elegibles = HabilidadCatalogService.ElegiblesPara(pj).Select(h => h.Id).ToList();
            // No sabemos si siempre estará GolpeFuerte, pero si está y cumple requisitos debería aparecer
            if (todas.Any(h => string.Equals(h.Id, "GolpeFuerte", StringComparison.OrdinalIgnoreCase)))
            {
                Assert.Contains("GolpeFuerte", elegibles);
            }

            // Aprender y usar para subir exp y verificar evolución por NvHabilidad si existe
            var hdata = todas.FirstOrDefault(h => string.Equals(h.Id, "GolpeFuerte", StringComparison.OrdinalIgnoreCase));
            if (hdata != null)
            {
                var prog = HabilidadCatalogService.AProgreso(hdata);
                pj.AprenderHabilidad(prog);
                // Simular uso hasta cumplir primera evolución si la condición es alcanzable en test
                for (int i = 0; i < 60; i++) pj.UsarHabilidad("GolpeFuerte");
                // Verificar la instancia real del personaje (no la referencia local)
                var progPj = pj.Habilidades["GolpeFuerte"];
                // Validar progreso: la habilidad debe haber subido al menos un nivel tras 60 usos.
                // No asumimos desbloqueos de evoluciones específicas ya que suelen requerir AND de varias condiciones.
                Assert.True(progPj.Nivel >= 2);
            }
        }
    }
}
