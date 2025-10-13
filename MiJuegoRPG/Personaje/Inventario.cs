using MiJuegoRPG.Objetos;
using System;
using System.Collections.Generic;
using System.Linq;
using MiJuegoRPG.Personaje;
using MiJuegoRPG.Habilidades;
using MiJuegoRPG.Interfaces;
using MiJuegoRPG.Motor;
using MiJuegoRPG.Motor.Servicios;




namespace MiJuegoRPG.Personaje
{
    // SA1402: Equipo se movió a Equipo.cs para cumplir con SA1402 (un tipo por archivo)
    // SA1402: NewBaseType se movió a NewBaseType.cs para cumplir con SA1402 (un tipo por archivo)

    public class Inventario : NewBaseType
    {
        private IUserInterface Ui => Juego.ObtenerInstanciaActual()?.Ui ?? new ConsoleUserInterface();

        /// <summary>
        /// Equipa un objeto (arma, armadura o accesorio) y muestra un aviso centralizado con comparación de estadísticas.
        /// </summary>
        public void EquiparObjeto(Objeto objeto, Personaje? personaje = null)
        {
            // Si tenemos personaje, capturamos ratio de recursos antes de equipar para preservarlo tras cambios de máximos
            double manaRatio = 1.0;
            double energiaRatio = 1.0;
            if (personaje != null)
            {
                manaRatio = personaje.ManaMaxima > 0 ? (double)personaje.ManaActual / personaje.ManaMaxima : 1.0;
                var energiaMaxActual = personaje.EnergiaMaximaConBonos;
                energiaRatio = energiaMaxActual > 0 ? (double)personaje.EnergiaActual / energiaMaxActual : 1.0;
            }
            string aviso = "";
            string mejora = "";
            if (objeto is MiJuegoRPG.Objetos.Arma armaNueva)
            {
                var armaAnterior = Equipo.Arma as MiJuegoRPG.Objetos.Arma;
                Equipo.Arma = armaNueva;
                aviso += $"Anterior: {(armaAnterior != null ? armaAnterior.Nombre + " (Daño Físico: " + armaAnterior.DañoFisico + ")" : "Ninguna")}\n";
                aviso += $"Nuevo: {armaNueva.Nombre} (Daño Físico: {armaNueva.DañoFisico})\n";
                if (armaAnterior != null)
                {
                    int diff = armaNueva.DañoFisico - armaAnterior.DañoFisico;
                    mejora = diff > 0 ? $"¡Mejora! (+{diff} de daño físico)" : (diff < 0 ? $"¡Advertencia! ({diff} de daño físico)" : "Sin cambios en daño físico");
                }
            }
            else if (objeto is MiJuegoRPG.Objetos.Casco cascoNuevo)
            {
                var cascoAnterior = Equipo.Casco as MiJuegoRPG.Objetos.Casco;
                Equipo.Casco = cascoNuevo;
                aviso += $"Anterior: {(cascoAnterior != null ? cascoAnterior.Nombre + " (Defensa: " + cascoAnterior.Defensa + ")" : "Ninguno")}\n";
                aviso += $"Nuevo: {cascoNuevo.Nombre} (Defensa: {cascoNuevo.Defensa})\n";
                if (cascoAnterior != null)
                {
                    int diff = cascoNuevo.Defensa - cascoAnterior.Defensa;
                    mejora = diff > 0 ? $"¡Mejora! (+{diff} DEF)" : (diff < 0 ? $"¡Advertencia! ({diff} DEF)" : "Sin cambios en defensa");
                }
            }
            else if (objeto is MiJuegoRPG.Objetos.Armadura armaduraNueva)
            {
                var armaduraAnterior = Equipo.Armadura as MiJuegoRPG.Objetos.Armadura;
                Equipo.Armadura = armaduraNueva;
                aviso += $"Anterior: {(armaduraAnterior != null ? armaduraAnterior.Nombre + " (Defensa: " + armaduraAnterior.Defensa + ")" : "Ninguna")}\n";
                aviso += $"Nuevo: {armaduraNueva.Nombre} (Defensa: {armaduraNueva.Defensa})\n";
                if (armaduraAnterior != null)
                {
                    int diff = armaduraNueva.Defensa - armaduraAnterior.Defensa;
                    mejora = diff > 0 ? $"¡Mejora! (+{diff} de defensa)" : (diff < 0 ? $"¡Advertencia! ({diff} de defensa)" : "Sin cambios en defensa");
                }
            }
            else if (objeto is MiJuegoRPG.Objetos.Pantalon pantalonNuevo)
            {
                var pantalonAnterior = Equipo.Pantalon as MiJuegoRPG.Objetos.Pantalon;
                Equipo.Pantalon = pantalonNuevo;
                aviso += $"Anterior: {(pantalonAnterior != null ? pantalonAnterior.Nombre + " (Defensa: " + pantalonAnterior.Defensa + ")" : "Ninguno")}\n";
                aviso += $"Nuevo: {pantalonNuevo.Nombre} (Defensa: {pantalonNuevo.Defensa})\n";
                if (pantalonAnterior != null)
                {
                    int diff = pantalonNuevo.Defensa - pantalonAnterior.Defensa;
                    mejora = diff > 0 ? $"¡Mejora! (+{diff} DEF)" : (diff < 0 ? $"¡Advertencia! ({diff} DEF)" : "Sin cambios en defensa");
                }
            }
            else if (objeto is MiJuegoRPG.Objetos.Botas botasNuevas)
            {
                var botasAnteriores = Equipo.Zapatos as MiJuegoRPG.Objetos.Botas;
                Equipo.Zapatos = botasNuevas;
                aviso += $"Anterior: {(botasAnteriores != null ? botasAnteriores.Nombre + " (Defensa: " + botasAnteriores.Defensa + ")" : "Ningunas")}\n";
                aviso += $"Nuevo: {botasNuevas.Nombre} (Defensa: {botasNuevas.Defensa})\n";
                if (botasAnteriores != null)
                {
                    int diff = botasNuevas.Defensa - botasAnteriores.Defensa;
                    mejora = diff > 0 ? $"¡Mejora! (+{diff} DEF)" : (diff < 0 ? $"¡Advertencia! ({diff} DEF)" : "Sin cambios en defensa");
                }
            }
            else if (objeto is MiJuegoRPG.Objetos.Collar collarNuevo)
            {
                var collarAnterior = Equipo.Collar as MiJuegoRPG.Objetos.Collar;
                Equipo.Collar = collarNuevo;
                aviso += $"Anterior: {(collarAnterior != null ? collarAnterior.Nombre + " (Def: " + collarAnterior.BonificacionDefensa + ", Recurso: " + collarAnterior.BonificacionEnergia + ")" : "Ninguno")}\n";
                aviso += $"Nuevo: {collarNuevo.Nombre} (Def: {collarNuevo.BonificacionDefensa}, Recurso: {collarNuevo.BonificacionEnergia})\n";
                if (collarAnterior != null)
                {
                    int diffDef = collarNuevo.BonificacionDefensa - collarAnterior.BonificacionDefensa;
                    int diffRes = collarNuevo.BonificacionEnergia - collarAnterior.BonificacionEnergia;
                    mejora = (diffDef > 0 ? $"+{diffDef} DEF " : (diffDef < 0 ? $"{diffDef} DEF " : "")) + (diffRes > 0 ? $"+{diffRes} RECURSO" : (diffRes < 0 ? $"{diffRes} RECURSO" : ""));
                    if (string.IsNullOrWhiteSpace(mejora))
                        mejora = "Sin cambios en bonificaciones";
                }
            }
            else if (objeto is MiJuegoRPG.Objetos.Cinturon cinturonNuevo)
            {
                var cinturonAnterior = Equipo.Cinturon as MiJuegoRPG.Objetos.Cinturon;
                Equipo.Cinturon = cinturonNuevo;
                aviso += $"Anterior: {(cinturonAnterior != null ? cinturonAnterior.Nombre + " (Carga: " + cinturonAnterior.BonificacionCarga + ")" : "Ninguno")}\n";
                aviso += $"Nuevo: {cinturonNuevo.Nombre} (Carga: {cinturonNuevo.BonificacionCarga})\n";
                if (cinturonAnterior != null)
                {
                    int diff = cinturonNuevo.BonificacionCarga - cinturonAnterior.BonificacionCarga;
                    mejora = diff > 0 ? $"¡Mejora! (+{diff} Carga)" : (diff < 0 ? $"¡Advertencia! ({diff} Carga)" : "Sin cambios en carga");
                }
            }
            else if (objeto is MiJuegoRPG.Objetos.Accesorio accesorioNuevo)
            {
                // Por simplicidad, equipar en Accesorio1 si está vacío, si no en Accesorio2
                var accesorioAnterior = Equipo.Accesorio1 as MiJuegoRPG.Objetos.Accesorio;
                if (Equipo.Accesorio1 == null)
                {
                    Equipo.Accesorio1 = accesorioNuevo;
                }
                else
                {
                    accesorioAnterior = Equipo.Accesorio2 as MiJuegoRPG.Objetos.Accesorio;
                    Equipo.Accesorio2 = accesorioNuevo;
                }
                aviso += $"Anterior: {(accesorioAnterior != null ? accesorioAnterior.Nombre + " (Ataque: " + accesorioAnterior.BonificacionAtaque + ", Defensa: " + accesorioAnterior.BonificacionDefensa + ")" : "Ninguno")}\n";
                aviso += $"Nuevo: {accesorioNuevo.Nombre} (Ataque: {accesorioNuevo.BonificacionAtaque}, Defensa: {accesorioNuevo.BonificacionDefensa})\n";
                if (accesorioAnterior != null)
                {
                    int diffAtk = accesorioNuevo.BonificacionAtaque - accesorioAnterior.BonificacionAtaque;
                    int diffDef = accesorioNuevo.BonificacionDefensa - accesorioAnterior.BonificacionDefensa;
                    mejora = (diffAtk > 0 ? $"+{diffAtk} ATK " : (diffAtk < 0 ? $"{diffAtk} ATK " : "")) + (diffDef > 0 ? $"+{diffDef} DEF" : (diffDef < 0 ? $"{diffDef} DEF" : ""));
                    if (string.IsNullOrWhiteSpace(mejora))
                        mejora = "Sin cambios en bonificaciones";
                }
            }
            else
            {
                aviso = "Este objeto no es equipable.";
            }
            MiJuegoRPG.Motor.AvisosAventura.MostrarAviso(
                "Equipo Equipado",
                objeto.Nombre,
                aviso + (string.IsNullOrWhiteSpace(mejora) ? "" : mejora));
            // Restaurar ratios de recursos si corresponde
            if (personaje != null)
            {
                // Mana
                personaje.ManaActual = (int)Math.Round(personaje.ManaMaxima * manaRatio);
                if (personaje.ManaActual > personaje.ManaMaxima)
                    personaje.ManaActual = personaje.ManaMaxima;
                if (personaje.ManaActual < 0)
                    personaje.ManaActual = 0;
                // Energía (usar máximo con bonos)
                int eMax = personaje.EnergiaMaximaConBonos;
                personaje.EnergiaActual = (int)Math.Round(eMax * energiaRatio);
                if (personaje.EnergiaActual > eMax)
                    personaje.EnergiaActual = eMax;
                if (personaje.EnergiaActual < 0)
                    personaje.EnergiaActual = 0;
                // Sincronizar habilidades otorgadas por equipo y bonos de set
                SincronizarHabilidadesYBonosSet(personaje);
            }
        }
        public List<ObjetoConCantidad> NuevosObjetos { get; set; } = new List<ObjetoConCantidad>();
        public Equipo Equipo { get; set; } = new Equipo();
        public int CapacidadMaxima { get; set; } = 30;

        public void Agregar(Material material, int cantidad = 1)
        {
            AgregarObjeto(material, cantidad);
        }



        public void AgregarObjeto(Objeto objeto, int cantidad = 1, Personaje? personaje = null)
        {
            int totalSlots = NuevosObjetos.Count;
            var existente = NuevosObjetos.FirstOrDefault(o => o.Objeto.Nombre == objeto.Nombre && o.Objeto.GetType() == objeto.GetType());
            if (existente != null)
            {
                existente.Cantidad += cantidad;
                Ui.WriteLine($"Se apiló {cantidad}x {objeto.Nombre} (Total: {existente.Cantidad})");
            }
            else
            {
                if (totalSlots >= CapacidadMaxima)
                {
                    Ui.WriteLine("No se puede agregar más objetos. Inventario lleno.");
                    return;
                }
                NuevosObjetos.Add(new ObjetoConCantidad(objeto, cantidad));
                Ui.WriteLine($"{objeto.Nombre} ha sido agregado al inventario.");
            }
            // Avisos automáticos si se pasa el personaje
            if (personaje != null)
                MiJuegoRPG.Motor.GestorDesbloqueos.VerificarDesbloqueos(personaje);
        }


        public void QuitarObjeto(Objeto objeto, int cantidad = 1)
        {
            var existente = NuevosObjetos.FirstOrDefault(o => o.Objeto.Nombre == objeto.Nombre && o.Objeto.GetType() == objeto.GetType());
            if (existente != null)
            {
                if (existente.Cantidad > cantidad)
                {
                    existente.Cantidad -= cantidad;
                    Ui.WriteLine($"Se quitaron {cantidad}x {objeto.Nombre} (Quedan: {existente.Cantidad})");
                }
                else
                {
                    NuevosObjetos.Remove(existente);
                    Ui.WriteLine($"Se eliminó {objeto.Nombre} del inventario.");
                }
            }
            else
            {
                Ui.WriteLine($"No tienes {objeto.Nombre} en el inventario.");
            }
        }

        public Inventario()
        {
            NuevosObjetos = new List<ObjetoConCantidad>();
        }

        public void MostrarInventario()
        {
            int totalObjetos = NuevosObjetos.Sum(o => o.Cantidad);
            // Peso total (por ahora 0, para implementar después)
            double pesoTotal = 0;
            UIStyle.Header(Ui, $"Inventario");
            UIStyle.Hint(Ui, $"Resumen: {totalObjetos} objetos (Peso: {pesoTotal} / --)");
            Ui.WriteLine("----------------------------------------");
            Ui.WriteLine($"#  {"Nombre",-20} {"Categoría",-12} {"Cantidad",-8}");
            Ui.WriteLine("----------------------------------------");
            for (int i = 0; i < NuevosObjetos.Count; i++)
            {
                var objCant = NuevosObjetos[i];
                Ui.WriteLine($"{i + 1,2} {objCant.Objeto.Nombre,-20} {objCant.Objeto.Categoria,-12} {objCant.Cantidad,-8}");
            }
            Ui.WriteLine("----------------------------------------");
            Ui.WriteLine($"Capacidad: {NuevosObjetos.Count}/{CapacidadMaxima}");
        }

        public int ContarMaterial(string nombreMaterial)
        {
            return NuevosObjetos
                .Where(o => o.Objeto is MiJuegoRPG.Objetos.Material && o.Objeto.Nombre == nombreMaterial)
                .Sum(o => o.Cantidad);
        }

        public void ConsumirMaterial(string nombreMaterial, int cantidad)
        {
            int restante = cantidad;
            foreach (var objCant in NuevosObjetos.Where(o => o.Objeto is MiJuegoRPG.Objetos.Material && o.Objeto.Nombre == nombreMaterial).ToList())
            {
                if (restante <= 0)
                    break;
                if (objCant.Cantidad > restante)
                {
                    objCant.Cantidad -= restante;
                    restante = 0;
                }
                else
                {
                    restante -= objCant.Cantidad;
                    NuevosObjetos.Remove(objCant);
                }
            }
        }

        /// <summary>
        /// Agrega o quita habilidades en el personaje en función del equipo actual y aplica bonos de set GM (simple 2/4/6 piezas).
        /// Las habilidades otorgadas por equipo son TEMPORALES: se remueven al desequipar.
        /// </summary>
        public void SincronizarHabilidadesYBonosSet(Personaje pj)
        {
            // 1) Habilidades por equipo
            var otorgadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var obj in pj.ObtenerObjetosEquipados())
            {
                if (obj.HabilidadesOtorgadas == null)
                    continue;
                foreach (var h in obj.HabilidadesOtorgadas)
                {
                    if (string.IsNullOrWhiteSpace(h.Id))
                        continue;
                    otorgadas.Add(h.Id);
                    // Si no la tiene, aprenderla con nivel base 1
                    if (!pj.Habilidades.ContainsKey(h.Id) && pj.Nivel >= h.NivelMinimo)
                    {
                        // Intentar usar el catálogo (si existe la habilidad en DatosJuego) para traer evoluciones/reqs
                        var cat = HabilidadCatalogService.Todas.FirstOrDefault(x => string.Equals(x.Id, h.Id, StringComparison.OrdinalIgnoreCase));
                        var prog = cat != null ? HabilidadCatalogService.AProgreso(cat) : new HabilidadProgreso { Id = h.Id, Nombre = h.Id, Exp = 0 };
                        pj.AprenderHabilidad(prog);
                        pj.HabilidadesTemporalesEquipo.Add(h.Id);
                    }
                    else if (pj.Habilidades.ContainsKey(h.Id))
                    {
                        // Si ya la tenía (posible por set previo), aseguremos marcarla como temporal vigente
                        pj.HabilidadesTemporalesEquipo.Add(h.Id);
                    }
                }
            }
            // 1.b) También considerar habilidades otorgadas por sets data-driven
            try
            {
                var servicio = MiJuegoRPG.Motor.Servicios.SetBonusService.Instancia;
                var equipados = pj.ObtenerObjetosEquipados();
                var (bonosSet, habsSet) = servicio.CalcularBonosYHabilidades(equipados);
                // Aplicar bonos de set (sobrescribe la lógica hardcoded más abajo; primero limpiamos)
                pj.BonosTemporalesSet.Clear();
                foreach (var kv in bonosSet)
                {
                    pj.BonosTemporalesSet[kv.Key] = kv.Value;
                }
                // Incluir habilidades de set en la lista "otorgadas"
                foreach (var (hid, nivel) in habsSet)
                {
                    if (!string.IsNullOrWhiteSpace(hid))
                        otorgadas.Add(hid);
                    if (!pj.Habilidades.ContainsKey(hid))
                    {
                        var cat = HabilidadCatalogService.Todas.FirstOrDefault(x => string.Equals(x.Id, hid, StringComparison.OrdinalIgnoreCase));
                        var prog = cat != null ? HabilidadCatalogService.AProgreso(cat) : new HabilidadProgreso { Id = hid, Nombre = hid, Exp = 0 };
                        pj.AprenderHabilidad(prog);
                        pj.HabilidadesTemporalesEquipo.Add(hid);
                    }
                    else
                    {
                        pj.HabilidadesTemporalesEquipo.Add(hid);
                    }
                }
            }
            catch { /* tolerante: si no hay servicio/definiciones, seguimos con hardcoded GM */ }

            // Remover habilidades temporales que ya no estén otorgadas por equipo/set
            if (pj.HabilidadesTemporalesEquipo.Count > 0)
            {
                var paraQuitar = new List<string>();
                foreach (var id in pj.HabilidadesTemporalesEquipo)
                {
                    if (!otorgadas.Contains(id))
                        paraQuitar.Add(id);
                }
                foreach (var id in paraQuitar)
                {
                    pj.Habilidades.Remove(id);
                    pj.HabilidadesTemporalesEquipo.Remove(id);
                    // Nota: no mostramos aviso aquí para evitar spam visual al reorganizar equipo
                }
            }
            // 2) Fallback temporal: si no hubo sets cargados, mantener compatibilidad con GM por nombre
            if (pj.BonosTemporalesSet.Count == 0)
            {
                int piezasGM = pj.ObtenerObjetosEquipados().Count(o => o.Nombre.IndexOf("GM", StringComparison.OrdinalIgnoreCase) >= 0);
                if (piezasGM >= 2)
                    pj.BonosTemporalesSet["Defensa"] = 5000;
                if (piezasGM >= 4)
                    pj.BonosTemporalesSet["Ataque"] = (pj.BonosTemporalesSet.TryGetValue("Ataque", out var v) ? v : 0) + 5000;
                if (piezasGM >= 6)
                {
                    pj.BonosTemporalesSet["Mana"] = (pj.BonosTemporalesSet.TryGetValue("Mana", out var v1) ? v1 : 0) + 20000;
                    pj.BonosTemporalesSet["Energia"] = (pj.BonosTemporalesSet.TryGetValue("Energia", out var v2) ? v2 : 0) + 20000;
                    pj.BonosTemporalesSet["Energía"] = pj.BonosTemporalesSet["Energia"]; // compatibilidad
                }
            }
        }
    }
}
