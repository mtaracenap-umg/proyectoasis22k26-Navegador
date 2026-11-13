/*
 * ==================================================================
 * Área : Contabilidad
 * Autor : Mario Taracena
 * Carné : 0901-23-9335
 * Fecha : 11/11/2026
 * ==================================================================
 * Propósito :
 *  Arma los datos del reporte de ejecución presupuestaria: por
 *  presupuesto, lo asignado por módulo y partida, lo ejecutado, lo
 *  disponible y el porcentaje de ejecución, con totales y alertas.
 * Reglas especificas:
 *  Lo ejecutado se mide por módulo, igual que en la pantalla de
 *  Presupuestos: cargos menos abonos de cuentas 5 en pólizas aplicadas
 *  dentro de la vigencia (del inicio al cierre, o a hoy si no tiene).
 *  Un módulo está sobregirado si lo ejecutado supera lo asignado y al
 *  límite si va en 90 % o más. La Vista recibe un DataTable plano, una
 *  fila por partida, con los totales repetidos en cada fila.
 * ===================================================================
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CapaModelo_Contabilidad.Contratos;
using CapaModelo_Contabilidad.Entidades;
using CapaModelo_Contabilidad.Repositorios;

namespace CapaControlador_Contabilidad
{
    // Opciones del filtro del reporte
    public enum FiltroReportePresupuestoContabilidad
    {
        Vigentes,
        Todos,
        RangoFechas
    }

    // Controlador del reporte de presupuestos
    public class ClsControladorReportePresupuestosContabilidad
    {
        // Porcentaje desde el que se avisa
        public const decimal LimiteAviso = 0.90m;

        // Texto de sobregiro
        public const string AlertaSobregiro = "Sobregirado";

        // Texto de aviso
        public const string AlertaLimite = "Al límite";

        // Repositorio de la consulta
        private readonly ClsRepositorioReportePresupuestosContabilidad _Repositorio = new ClsRepositorioReportePresupuestosContabilidad();

        // Repositorio que calcula lo ejecutado
        private readonly IRepositorioPresupuestosContabilidad _RepositorioPresupuestos = new ClsRepositorioPresupuestosContabilidad();

        // Revisa el filtro
        public string ContabilidadFuncErrorFiltro(FiltroReportePresupuestoContabilidad Filtro, DateTime Desde, DateTime Hasta)
        {
            if (!Enum.IsDefined(typeof(FiltroReportePresupuestoContabilidad), Filtro)) return "Seleccione qué presupuestos desea ver.";
            if (Filtro != FiltroReportePresupuestoContabilidad.RangoFechas) return string.Empty;
            if (Desde.Year < 1000 || Hasta.Year > 9999) return "Las fechas están fuera del rango permitido.";
            return ClsValidacionDatosContabilidad.ContabilidadFuncErrorRangoFechas(Desde, Hasta);
        }

        // Describe el filtro para el encabezado
        public string ContabilidadFuncDescripcionFiltro(FiltroReportePresupuestoContabilidad Filtro, DateTime Desde, DateTime Hasta, DateTime Hoy)
        {
            switch (Filtro)
            {
                case FiltroReportePresupuestoContabilidad.Todos:
                    return "Todos los presupuestos activos";
                case FiltroReportePresupuestoContabilidad.RangoFechas:
                    return "Presupuestos con vigencia entre el " + Desde.ToString("dd/MM/yyyy") + " y el " + Hasta.ToString("dd/MM/yyyy");
                default:
                    return "Presupuestos vigentes al " + Hoy.ToString("dd/MM/yyyy");
            }
        }

        // Arma las filas del reporte
        public DataTable ContabilidadFuncReporte(FiltroReportePresupuestoContabilidad Filtro, DateTime Desde, DateTime Hasta)
        {
            string Error = ContabilidadFuncErrorFiltro(Filtro, Desde, Hasta);
            if (Error != string.Empty) throw new ArgumentException(Error);
            DateTime Hoy = DateTime.Today;
            DateTime? Inicio = null, Fin = null;
            if (Filtro == FiltroReportePresupuestoContabilidad.Vigentes) { Inicio = Hoy; Fin = Hoy; }
            if (Filtro == FiltroReportePresupuestoContabilidad.RangoFechas) { Inicio = Desde.Date; Fin = Hasta.Date; }

            List<ClsModeloPresupuestoContabilidad> Presupuestos = _Repositorio.ContabilidadFuncObtenerPresupuestos(Inicio, Fin)
                .Select(ContabilidadFuncModelo).ToList();
            return ContabilidadFuncArmarFilas(Presupuestos, ContabilidadFuncEjecutado, Hoy);
        }

        // Resume el reporte en una línea
        public string ContabilidadFuncResumen(DataTable Filas)
        {
            if (Filas == null || Filas.Rows.Count == 0) return "No hay presupuestos para el filtro elegido.";
            List<DataRow> Lista = Filas.Rows.Cast<DataRow>().ToList();
            int Presupuestos = Lista.Select(Fila => (int)Fila["IdPresupuesto"]).Distinct().Count();
            int Sobregirados = Lista.Where(Fila => (string)Fila["AlertaModulo"] == AlertaSobregiro)
                .Select(Fila => (int)Fila["IdPresupuesto"] + "|" + (string)Fila["Modulo"]).Distinct().Count();
            DataRow Primera = Lista[0];
            return Presupuestos + (Presupuestos == 1 ? " presupuesto" : " presupuestos") + ", " + Lista.Count
                + (Lista.Count == 1 ? " partida" : " partidas") + ". Ejecutado " + ((decimal)Primera["PorcentajeGeneral"]).ToString("P1")
                + (Sobregirados == 0 ? ", sin módulos sobregirados." : ", " + Sobregirados + (Sobregirados == 1 ? " módulo sobregirado." : " módulos sobregirados."));
        }

        // Crea la tabla vacía del reporte
        public static DataTable ContabilidadFuncCrearTabla()
        {
            DataTable Tabla = new DataTable("DsReportePresupuestos");
            Tabla.Columns.Add("IdPresupuesto", typeof(int));
            foreach (string Columna in new[] { "Presupuesto", "Vigencia", "Estado", "Modulo", "Partida" }) Tabla.Columns.Add(Columna, typeof(string));
            Tabla.Columns.Add("AsignadoPartida", typeof(decimal));
            foreach (string Nivel in new[] { "Modulo", "Presupuesto", "General" })
            {
                foreach (string Medida in new[] { "Asignado", "Ejecutado", "Disponible", "Porcentaje" }) Tabla.Columns.Add(Medida + Nivel, typeof(decimal));
                if (Nivel != "General") Tabla.Columns.Add("Alerta" + Nivel, typeof(string));
            }
            return Tabla;
        }

        // Llena las filas con lo asignado y lo ejecutado
        public static DataTable ContabilidadFuncArmarFilas(IEnumerable<ClsModeloPresupuestoContabilidad> Presupuestos,
            Func<ClsModeloPresupuestoContabilidad, DateTime, IDictionary<string, decimal>> Ejecutado, DateTime Hoy)
        {
            DataTable Tabla = ContabilidadFuncCrearTabla();
            decimal AsignadoGeneral = 0m, EjecutadoGeneral = 0m;
            foreach (ClsModeloPresupuestoContabilidad Presupuesto in Presupuestos.Where(Presupuesto => Presupuesto.Partidas.Count > 0))
            {
                IDictionary<string, decimal> PorModulo = Ejecutado(Presupuesto, Hoy) ?? new Dictionary<string, decimal>();
                var Modulos = Presupuesto.Partidas.GroupBy(Partida => Partida.Modulo).OrderBy(Grupo => Grupo.Key, StringComparer.CurrentCulture)
                    .Select(Grupo => new
                    {
                        Modulo = Grupo.Key, Partidas = Grupo.ToList(), Asignado = Grupo.Sum(Partida => Partida.Cantidad),
                        Ejecutado = PorModulo.ContainsKey(Grupo.Key) ? PorModulo[Grupo.Key] : 0m
                    }).ToList();
                decimal AsignadoPresupuesto = Modulos.Sum(Modulo => Modulo.Asignado);
                decimal EjecutadoPresupuesto = Modulos.Sum(Modulo => Modulo.Ejecutado);
                AsignadoGeneral += AsignadoPresupuesto;
                EjecutadoGeneral += EjecutadoPresupuesto;
                string Vigencia = ContabilidadFuncVigencia(Presupuesto.FechaInicio, Presupuesto.FechaCierre);
                string Estado = ContabilidadFuncEstado(Presupuesto.FechaInicio, Presupuesto.FechaCierre, Hoy);
                foreach (var Modulo in Modulos)
                    foreach (ClsModeloPartidaPresupuestoContabilidad Partida in Modulo.Partidas)
                        Tabla.Rows.Add(Presupuesto.IdPresupuesto, Presupuesto.Nombre, Vigencia, Estado, Modulo.Modulo, Partida.Nombre, Partida.Cantidad,
                            Modulo.Asignado, Modulo.Ejecutado, Modulo.Asignado - Modulo.Ejecutado, ContabilidadFuncPorcentaje(Modulo.Asignado, Modulo.Ejecutado),
                            ContabilidadFuncAlerta(Modulo.Asignado, Modulo.Ejecutado),
                            AsignadoPresupuesto, EjecutadoPresupuesto, AsignadoPresupuesto - EjecutadoPresupuesto,
                            ContabilidadFuncPorcentaje(AsignadoPresupuesto, EjecutadoPresupuesto), ContabilidadFuncAlerta(AsignadoPresupuesto, EjecutadoPresupuesto),
                            0m, 0m, 0m, 0m);
            }
            foreach (DataRow Fila in Tabla.Rows)
            {
                Fila["AsignadoGeneral"] = AsignadoGeneral;
                Fila["EjecutadoGeneral"] = EjecutadoGeneral;
                Fila["DisponibleGeneral"] = AsignadoGeneral - EjecutadoGeneral;
                Fila["PorcentajeGeneral"] = ContabilidadFuncPorcentaje(AsignadoGeneral, EjecutadoGeneral);
            }
            return Tabla;
        }

        // Porcentaje ejecutado
        public static decimal ContabilidadFuncPorcentaje(decimal Asignado, decimal Ejecutado)
        {
            return Asignado <= 0m ? 0m : Math.Round(Ejecutado / Asignado, 4);
        }

        // Alerta según lo ejecutado
        public static string ContabilidadFuncAlerta(decimal Asignado, decimal Ejecutado)
        {
            if (Ejecutado > Asignado) return AlertaSobregiro;
            return Asignado > 0m && Ejecutado / Asignado >= LimiteAviso ? AlertaLimite : string.Empty;
        }

        // Estado del presupuesto
        public static string ContabilidadFuncEstado(DateTime Inicio, DateTime? Cierre, DateTime Hoy)
        {
            if (Hoy.Date < Inicio.Date) return "Por iniciar";
            return Cierre.HasValue && Cierre.Value.Date < Hoy.Date ? "Cerrado" : "Vigente";
        }

        // Texto de la vigencia
        public static string ContabilidadFuncVigencia(DateTime Inicio, DateTime? Cierre)
        {
            return Cierre.HasValue ? Inicio.ToString("dd/MM/yyyy") + " al " + Cierre.Value.ToString("dd/MM/yyyy")
                : "Desde " + Inicio.ToString("dd/MM/yyyy") + ", sin cierre";
        }

        // Fecha hasta la que se mide lo ejecutado
        public static DateTime ContabilidadFuncCorte(DateTime? Cierre, DateTime Hoy)
        {
            return (Cierre ?? Hoy).Date;
        }

        // Ejecutado por módulo de un presupuesto
        private IDictionary<string, decimal> ContabilidadFuncEjecutado(ClsModeloPresupuestoContabilidad Presupuesto, DateTime Hoy)
        {
            DateTime Corte = ContabilidadFuncCorte(Presupuesto.FechaCierre, Hoy);
            return Corte < Presupuesto.FechaInicio.Date ? new Dictionary<string, decimal>()
                : _RepositorioPresupuestos.ContabilidadFuncObtenerEjecutado(Presupuesto.FechaInicio, Corte);
        }

        // Pasa la entidad al modelo
        private static ClsModeloPresupuestoContabilidad ContabilidadFuncModelo(ClsPresupuestoContabilidad Presupuesto)
        {
            return new ClsModeloPresupuestoContabilidad
            {
                IdPresupuesto = Presupuesto.IdPresupuesto, Nombre = Presupuesto.Nombre, FechaInicio = Presupuesto.FechaInicio,
                FechaCierre = Presupuesto.FechaCierre,
                Partidas = Presupuesto.Detalle.Select(Partida => new ClsModeloPartidaPresupuestoContabilidad
                    { Nombre = Partida.Nombre, Modulo = Partida.Modulo, Cantidad = Partida.Cantidad }).ToList()
            };
        }
    }
}
