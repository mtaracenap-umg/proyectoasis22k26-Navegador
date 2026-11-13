/*
 * ==================================================================
 * Área : Contabilidad
 * Autor : Mario Taracena
 * Carné : 0901-23-9335
 * Fecha : 11/11/2026
 * ==================================================================
 * Propósito :
 *  Reporte de ejecución presupuestaria: filtra los presupuestos por
 *  vigencia y muestra en un ReportViewer lo asignado, lo ejecutado,
 *  lo disponible y el porcentaje de ejecución por módulo y partida.
 * Reglas especificas:
 *  La ventana solo habla con ClsControladorReportePresupuestosContabilidad.
 *  El reporte RpReportePresupuestos.rdlc va embebido en este ensamblado.
 *  Imprimir y exportar solo se muestran con el permiso de imprimir.
 * ===================================================================
*/

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using CapaControlador_Contabilidad;
using CapaVista_Contabilidad.Componentes;

namespace CapaVista_Contabilidad
{
    // Ventana del reporte de presupuestos
    public partial class FrmReportePresupuestos : Form
    {
        // Nombre del reporte embebido
        public const string RecursoReporte = "CapaVista_Contabilidad.Reportes.RpReportePresupuestos.rdlc";

        // Nombre del conjunto de datos del reporte
        public const string NombreDatos = "DsReportePresupuestos";

        // Controlador del reporte
        private readonly ClsControladorReportePresupuestosContabilidad _Controlador = new ClsControladorReportePresupuestosContabilidad();

        // Permisos de la pantalla
        private readonly ClsModeloPermisosPantallaContabilidad _Permisos;

        // Filas que muestra el reporte
        private DataTable _Filas;

        // Avisos en rojo de los campos
        private readonly ClsValidacionContabilidad _Validacion = new ClsValidacionContabilidad();

        // Abre con todos los permisos
        public FrmReportePresupuestos() : this(ClsModeloPermisosPantallaContabilidad.Total)
        {
        }

        // Abre con los permisos del usuario
        public FrmReportePresupuestos(ClsModeloPermisosPantallaContabilidad Permisos)
        {
            InitializeComponent();
            _Permisos = Permisos ?? ClsModeloPermisosPantallaContabilidad.Ninguno;
            ClsPresentacionContabilidad.ContabilidadMetPrepararVentana(this, "Reporte de presupuestos", new Size(1180, 720));
            ContabilidadCboFiltro.Items.AddRange(new object[] { "Vigentes hoy", "Todos los activos", "Por rango de fechas" });
            ContabilidadCboFiltro.SelectedIndex = 0;
            ContabilidadDtpDesde.Value = new DateTime(DateTime.Today.Year, 1, 1);
            ContabilidadDtpHasta.Value = new DateTime(DateTime.Today.Year, 12, 31);
            _Validacion.ContabilidadFuncValidar(ContabilidadDtpHasta, null);
            ContabilidadMetActivarFechas();
            ContabilidadMetAplicarPermisos();
            ContabilidadCboFiltro.SelectedIndexChanged += (Remitente, Argumentos) => ContabilidadMetActivarFechas();
            ContabilidadDtpDesde.ValueChanged += (Remitente, Argumentos) => ContabilidadFuncValidarFechas();
            ContabilidadDtpHasta.ValueChanged += (Remitente, Argumentos) => ContabilidadFuncValidarFechas();
            ContabilidadBtnGenerar.Click += (Remitente, Argumentos) => ContabilidadMetGenerar();
            ContabilidadBtnSalir.Click += (Remitente, Argumentos) => Close();
            Load += (Remitente, Argumentos) => ContabilidadMetGenerar();
            Disposed += (Remitente, Argumentos) => { if (_Filas != null) _Filas.Dispose(); _Validacion.Dispose(); };
        }

        // Filtro elegido
        public FiltroReportePresupuestoContabilidad Filtro
        {
            get { return (FiltroReportePresupuestoContabilidad)Math.Max(0, ContabilidadCboFiltro.SelectedIndex); }
        }

        // Carga el reporte con sus datos
        public static void ContabilidadMetPrepararReporte(LocalReport Reporte, DataTable Filas, string Filtro)
        {
            Reporte.ReportEmbeddedResource = RecursoReporte;
            Reporte.DataSources.Clear();
            Reporte.DataSources.Add(new ReportDataSource(NombreDatos, Filas));
            Reporte.SetParameters(new[] { new ReportParameter("Filtro", Filtro ?? string.Empty) });
        }

        // Consulta y muestra el reporte
        private void ContabilidadMetGenerar()
        {
            DateTime Desde = ContabilidadDtpDesde.Value.Date, Hasta = ContabilidadDtpHasta.Value.Date;
            string Error = _Controlador.ContabilidadFuncErrorFiltro(Filtro, Desde, Hasta);
            if (Error != string.Empty)
            {
                ClsDialogosContabilidad.ContabilidadMetError(this, Error);
                return;
            }
            try
            {
                DataTable Nuevas = _Controlador.ContabilidadFuncReporte(Filtro, Desde, Hasta);
                if (_Filas != null) _Filas.Dispose();
                _Filas = Nuevas;
                ContabilidadMetPrepararReporte(ContabilidadRpvReporte.LocalReport, _Filas,
                    _Controlador.ContabilidadFuncDescripcionFiltro(Filtro, Desde, Hasta, DateTime.Today));
                ContabilidadRpvReporte.SetDisplayMode(DisplayMode.PrintLayout);
                ContabilidadRpvReporte.ZoomMode = ZoomMode.PageWidth;
                ContabilidadRpvReporte.RefreshReport();
                ContabilidadLblResumen.Text = _Controlador.ContabilidadFuncResumen(_Filas);
            }
            catch (Exception Excepcion)
            {
                ContabilidadLblResumen.Text = string.Empty;
                ClsDialogosContabilidad.ContabilidadMetError(this, "No se pudo generar el reporte: " + Excepcion.Message);
            }
        }

        // Habilita las fechas solo para el rango
        private void ContabilidadMetActivarFechas()
        {
            bool Rango = Filtro == FiltroReportePresupuestoContabilidad.RangoFechas;
            ContabilidadDtpDesde.Enabled = Rango;
            ContabilidadDtpHasta.Enabled = Rango;
            ContabilidadFuncValidarFechas();
        }

        // Marca en rojo la fecha final si queda antes de la inicial
        private bool ContabilidadFuncValidarFechas()
        {
            string Error = _Controlador.ContabilidadFuncErrorFiltro(Filtro, ContabilidadDtpDesde.Value.Date, ContabilidadDtpHasta.Value.Date);
            bool Valido = _Validacion.ContabilidadFuncValidar(ContabilidadDtpHasta, Error);
            ContabilidadBtnGenerar.Enabled = Valido;
            return Valido;
        }

        // Muestra imprimir y exportar según el permiso
        private void ContabilidadMetAplicarPermisos()
        {
            ContabilidadRpvReporte.ShowPrintButton = _Permisos.PuedeImprimir;
            ContabilidadRpvReporte.ShowExportButton = _Permisos.PuedeImprimir;
        }
    }
}
