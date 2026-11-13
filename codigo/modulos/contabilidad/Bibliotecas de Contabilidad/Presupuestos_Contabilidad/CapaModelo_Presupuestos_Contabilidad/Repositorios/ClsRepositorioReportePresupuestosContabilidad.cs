/*
 * ==================================================================
 * Área : Contabilidad
 * Autor : Mario Taracena
 * Carné : 0901-23-9335
 * Fecha : 11/11/2026
 * ==================================================================
 * Propósito :
 *  Consulta del reporte de ejecución presupuestaria: los presupuestos
 *  activos con sus partidas y el módulo de cada una, filtrados por
 *  vigencia.
 * Reglas especificas:
 *  Solo lee. Un presupuesto entra si su vigencia toca el rango pedido;
 *  sin cierre se toma como abierto. Sin rango entran todos los activos.
 *  Lo ejecutado no se calcula aquí: lo da ClsRepositorioPresupuestosContabilidad.
 * ===================================================================
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using CapaModelo_Contabilidad.Entidades;

namespace CapaModelo_Contabilidad.Repositorios
{
    // Repositorio del reporte de presupuestos
    public class ClsRepositorioReportePresupuestosContabilidad : ClsRepositorioMaestroContabilidad
    {
        // Consulta base del reporte
        private const string _SelectPartidas =
            "SELECT e.idPresupuestoEncabezado, e.nombrePresupuestoEncabezado, e.fechaInicioPresupuestoEncabezado, " +
            "e.fechaCierrePresupuestoEncabezado, d.nombrePresupuestoDetalle, m.nombreModuloContable, d.cantidadPresupuestoDetalle " +
            "FROM tblPresupuestoEncabezado e " +
            "INNER JOIN tblPresupuestoDetalle d ON d.idPresupuestoEncabezado = e.idPresupuestoEncabezado AND d.is_active = TRUE " +
            "INNER JOIN tblModuloContable m ON m.idModuloContable = d.idModuloContable " +
            "WHERE e.is_active = TRUE";

        // Filtro de vigencia
        private const string _FiltroVigencia =
            " AND e.fechaInicioPresupuestoEncabezado <= ? " +
            "AND (e.fechaCierrePresupuestoEncabezado IS NULL OR e.fechaCierrePresupuestoEncabezado >= ?)";

        // Orden del reporte
        private const string _Orden =
            " ORDER BY e.fechaInicioPresupuestoEncabezado DESC, e.idPresupuestoEncabezado DESC, m.nombreModuloContable, d.idPresupuestoDetalle";

        // Trae los presupuestos con sus partidas
        public List<ClsPresupuestoContabilidad> ContabilidadFuncObtenerPresupuestos(DateTime? Desde, DateTime? Hasta)
        {
            List<OdbcParameter> Parametros = new List<OdbcParameter>();
            string Consulta = _SelectPartidas;
            if (Desde.HasValue && Hasta.HasValue)
            {
                Consulta += _FiltroVigencia;
                Parametros.Add(new OdbcParameter("Hasta", OdbcType.Date) { Value = Hasta.Value.Date });
                Parametros.Add(new OdbcParameter("Desde", OdbcType.Date) { Value = Desde.Value.Date });
            }
            Consulta += _Orden;

            List<ClsPresupuestoContabilidad> Presupuestos = new List<ClsPresupuestoContabilidad>();
            using (DataTable Tabla = ContabilidadFuncEjecutarConsulta(Consulta, CommandType.Text, Parametros))
            {
                ClsPresupuestoContabilidad Actual = null;
                foreach (DataRow Fila in Tabla.Rows)
                {
                    int Id = Convert.ToInt32(Fila["idPresupuestoEncabezado"]);
                    if (Actual == null || Actual.IdPresupuesto != Id)
                    {
                        Actual = new ClsPresupuestoContabilidad
                        {
                            IdPresupuesto = Id,
                            Nombre = Convert.ToString(Fila["nombrePresupuestoEncabezado"]),
                            FechaInicio = Convert.ToDateTime(Fila["fechaInicioPresupuestoEncabezado"]),
                            FechaCierre = Fila["fechaCierrePresupuestoEncabezado"] == DBNull.Value
                                ? (DateTime?)null : Convert.ToDateTime(Fila["fechaCierrePresupuestoEncabezado"])
                        };
                        Presupuestos.Add(Actual);
                    }
                    Actual.Detalle.Add(new ClsPresupuestoDetalleContabilidad
                    {
                        Nombre = Convert.ToString(Fila["nombrePresupuestoDetalle"]),
                        Modulo = Convert.ToString(Fila["nombreModuloContable"]),
                        Cantidad = Convert.ToDecimal(Fila["cantidadPresupuestoDetalle"])
                    });
                }
            }
            foreach (ClsPresupuestoContabilidad Presupuesto in Presupuestos)
                foreach (ClsPresupuestoDetalleContabilidad Partida in Presupuesto.Detalle) Presupuesto.Total += Partida.Cantidad;
            return Presupuestos;
        }
    }
}
