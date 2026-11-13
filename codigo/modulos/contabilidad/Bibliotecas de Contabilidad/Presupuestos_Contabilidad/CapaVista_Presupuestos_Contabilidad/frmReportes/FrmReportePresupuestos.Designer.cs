/* Área: Contabilidad. Autor: Mario Taracena. Carné: 0901-23-9335.
 * Fecha: 11/11/2026. Propósito: presentación de los filtros y del visor del reporte de presupuestos. */
namespace CapaVista_Contabilidad
{
    partial class FrmReportePresupuestos
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ContabilidadTtpBotones = new System.Windows.Forms.ToolTip(this.components);
            this.ContabilidadLblTitulo = new System.Windows.Forms.Label();
            this.ContabilidadBtnSalir = new CapaVista_Contabilidad.Componentes.ClsBotonContabilidad();
            this.ContabilidadPnlFiltros = new CapaVista_Contabilidad.Componentes.ClsPanelRedondeadoContabilidad();
            this.ContabilidadLblFiltro = new System.Windows.Forms.Label();
            this.ContabilidadCboFiltro = new System.Windows.Forms.ComboBox();
            this.ContabilidadLblDesde = new System.Windows.Forms.Label();
            this.ContabilidadDtpDesde = new System.Windows.Forms.DateTimePicker();
            this.ContabilidadLblHasta = new System.Windows.Forms.Label();
            this.ContabilidadDtpHasta = new System.Windows.Forms.DateTimePicker();
            this.ContabilidadBtnGenerar = new CapaVista_Contabilidad.Componentes.ClsBotonContabilidad();
            this.ContabilidadLblResumen = new System.Windows.Forms.Label();
            this.ContabilidadRpvReporte = new Microsoft.Reporting.WinForms.ReportViewer();
            this.ContabilidadPnlFiltros.SuspendLayout();
            this.SuspendLayout();
            this.ContabilidadLblTitulo.Name = "ContabilidadLblTitulo";
            this.ContabilidadLblTitulo.Text = "Reporte de presupuestos";
            this.ContabilidadLblTitulo.Location = new System.Drawing.Point(24, 22);
            this.ContabilidadLblTitulo.Size = new System.Drawing.Size(760, 38);
            this.ContabilidadLblTitulo.ForeColor = System.Drawing.Color.FromArgb(35, 50, 61);
            this.ContabilidadLblTitulo.BackColor = System.Drawing.Color.Transparent;
            this.ContabilidadLblTitulo.Font = new System.Drawing.Font("Lucida Fax", 20F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic);
            this.ContabilidadBtnSalir.Name = "ContabilidadBtnSalir";
            this.ContabilidadBtnSalir.Text = "";
            this.ContabilidadBtnSalir.Location = new System.Drawing.Point(1092, 10);
            this.ContabilidadBtnSalir.Size = new System.Drawing.Size(64, 64);
            this.ContabilidadBtnSalir.TabIndex = 6;
            this.ContabilidadBtnSalir.Icono = global::CapaVista_Contabilidad.Properties.Resources.btn_salir;
            this.ContabilidadTtpBotones.SetToolTip(this.ContabilidadBtnSalir, "Salir");
            this.ContabilidadPnlFiltros.Name = "ContabilidadPnlFiltros";
            this.ContabilidadPnlFiltros.Location = new System.Drawing.Point(24, 84);
            this.ContabilidadPnlFiltros.Size = new System.Drawing.Size(1132, 92);
            this.ContabilidadPnlFiltros.BackColor = System.Drawing.Color.Transparent;
            this.ContabilidadPnlFiltros.ColorRelleno = System.Drawing.Color.White;
            this.ContabilidadPnlFiltros.ColorBorde = System.Drawing.Color.FromArgb(212, 222, 228);
            this.ContabilidadPnlFiltros.Radio = 18;
            this.ContabilidadPnlFiltros.Controls.Add(this.ContabilidadLblFiltro);
            this.ContabilidadPnlFiltros.Controls.Add(this.ContabilidadCboFiltro);
            this.ContabilidadPnlFiltros.Controls.Add(this.ContabilidadLblDesde);
            this.ContabilidadPnlFiltros.Controls.Add(this.ContabilidadDtpDesde);
            this.ContabilidadPnlFiltros.Controls.Add(this.ContabilidadLblHasta);
            this.ContabilidadPnlFiltros.Controls.Add(this.ContabilidadDtpHasta);
            this.ContabilidadPnlFiltros.Controls.Add(this.ContabilidadBtnGenerar);
            this.ContabilidadLblFiltro.Name = "ContabilidadLblFiltro";
            this.ContabilidadLblFiltro.Text = "Presupuestos";
            this.ContabilidadLblFiltro.Location = new System.Drawing.Point(24, 14);
            this.ContabilidadLblFiltro.Size = new System.Drawing.Size(280, 24);
            this.ContabilidadLblFiltro.ForeColor = System.Drawing.Color.FromArgb(35, 50, 61);
            this.ContabilidadLblFiltro.BackColor = System.Drawing.Color.Transparent;
            this.ContabilidadLblFiltro.Font = new System.Drawing.Font("Tahoma", 10F);
            this.ContabilidadCboFiltro.Name = "ContabilidadCboFiltro";
            this.ContabilidadCboFiltro.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ContabilidadCboFiltro.Location = new System.Drawing.Point(24, 40);
            this.ContabilidadCboFiltro.Size = new System.Drawing.Size(280, 31);
            this.ContabilidadCboFiltro.TabIndex = 0;
            this.ContabilidadCboFiltro.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.ContabilidadLblDesde.Name = "ContabilidadLblDesde";
            this.ContabilidadLblDesde.Text = "Desde";
            this.ContabilidadLblDesde.Location = new System.Drawing.Point(328, 14);
            this.ContabilidadLblDesde.Size = new System.Drawing.Size(170, 24);
            this.ContabilidadLblDesde.ForeColor = System.Drawing.Color.FromArgb(35, 50, 61);
            this.ContabilidadLblDesde.BackColor = System.Drawing.Color.Transparent;
            this.ContabilidadLblDesde.Font = new System.Drawing.Font("Tahoma", 10F);
            this.ContabilidadDtpDesde.Name = "ContabilidadDtpDesde";
            this.ContabilidadDtpDesde.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.ContabilidadDtpDesde.CustomFormat = "dd/MM/yyyy";
            this.ContabilidadDtpDesde.Location = new System.Drawing.Point(328, 40);
            this.ContabilidadDtpDesde.Size = new System.Drawing.Size(170, 30);
            this.ContabilidadDtpDesde.TabIndex = 1;
            this.ContabilidadDtpDesde.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.ContabilidadLblHasta.Name = "ContabilidadLblHasta";
            this.ContabilidadLblHasta.Text = "Hasta";
            this.ContabilidadLblHasta.Location = new System.Drawing.Point(522, 14);
            this.ContabilidadLblHasta.Size = new System.Drawing.Size(170, 24);
            this.ContabilidadLblHasta.ForeColor = System.Drawing.Color.FromArgb(35, 50, 61);
            this.ContabilidadLblHasta.BackColor = System.Drawing.Color.Transparent;
            this.ContabilidadLblHasta.Font = new System.Drawing.Font("Tahoma", 10F);
            this.ContabilidadDtpHasta.Name = "ContabilidadDtpHasta";
            this.ContabilidadDtpHasta.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.ContabilidadDtpHasta.CustomFormat = "dd/MM/yyyy";
            this.ContabilidadDtpHasta.Location = new System.Drawing.Point(522, 40);
            this.ContabilidadDtpHasta.Size = new System.Drawing.Size(170, 30);
            this.ContabilidadDtpHasta.TabIndex = 2;
            this.ContabilidadDtpHasta.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.ContabilidadBtnGenerar.Name = "ContabilidadBtnGenerar";
            this.ContabilidadBtnGenerar.Text = "";
            this.ContabilidadBtnGenerar.Location = new System.Drawing.Point(716, 14);
            this.ContabilidadBtnGenerar.Size = new System.Drawing.Size(64, 64);
            this.ContabilidadBtnGenerar.TabIndex = 3;
            this.ContabilidadBtnGenerar.Icono = global::CapaVista_Contabilidad.Properties.Resources.btn_consultar;
            this.ContabilidadTtpBotones.SetToolTip(this.ContabilidadBtnGenerar, "Generar el reporte con el filtro elegido");
            this.ContabilidadLblResumen.Name = "ContabilidadLblResumen";
            this.ContabilidadLblResumen.Text = "";
            this.ContabilidadLblResumen.Location = new System.Drawing.Point(26, 184);
            this.ContabilidadLblResumen.Size = new System.Drawing.Size(1128, 24);
            this.ContabilidadLblResumen.ForeColor = System.Drawing.Color.FromArgb(35, 50, 61);
            this.ContabilidadLblResumen.BackColor = System.Drawing.Color.Transparent;
            this.ContabilidadLblResumen.Font = new System.Drawing.Font("Tahoma", 10F);
            this.ContabilidadRpvReporte.Name = "ContabilidadRpvReporte";
            this.ContabilidadRpvReporte.Location = new System.Drawing.Point(24, 214);
            this.ContabilidadRpvReporte.Size = new System.Drawing.Size(1132, 490);
            this.ContabilidadRpvReporte.TabIndex = 5;
            this.ContabilidadRpvReporte.ServerReport.BearerToken = null;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(246, 247, 248);
            this.ClientSize = new System.Drawing.Size(1180, 720);
            this.Controls.Add(this.ContabilidadLblTitulo);
            this.Controls.Add(this.ContabilidadBtnSalir);
            this.Controls.Add(this.ContabilidadPnlFiltros);
            this.Controls.Add(this.ContabilidadLblResumen);
            this.Controls.Add(this.ContabilidadRpvReporte);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::CapaVista_Contabilidad.Properties.Resources.Icono_Contabilidad;
            this.MaximizeBox = false;
            this.Name = "FrmReportePresupuestos";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "10002 - ReportePresupuestos";
            this.ContabilidadPnlFiltros.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolTip ContabilidadTtpBotones;
        private System.Windows.Forms.Label ContabilidadLblTitulo;
        private CapaVista_Contabilidad.Componentes.ClsBotonContabilidad ContabilidadBtnSalir;
        private CapaVista_Contabilidad.Componentes.ClsPanelRedondeadoContabilidad ContabilidadPnlFiltros;
        private System.Windows.Forms.Label ContabilidadLblFiltro;
        private System.Windows.Forms.ComboBox ContabilidadCboFiltro;
        private System.Windows.Forms.Label ContabilidadLblDesde;
        private System.Windows.Forms.DateTimePicker ContabilidadDtpDesde;
        private System.Windows.Forms.Label ContabilidadLblHasta;
        private System.Windows.Forms.DateTimePicker ContabilidadDtpHasta;
        private CapaVista_Contabilidad.Componentes.ClsBotonContabilidad ContabilidadBtnGenerar;
        private System.Windows.Forms.Label ContabilidadLblResumen;
        private Microsoft.Reporting.WinForms.ReportViewer ContabilidadRpvReporte;
    }
}
