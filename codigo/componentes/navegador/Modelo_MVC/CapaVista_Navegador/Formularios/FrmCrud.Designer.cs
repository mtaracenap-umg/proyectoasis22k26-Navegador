using System.Windows.Forms;
using System.Drawing;


namespace CapaVista_Navegador
{
    partial class FrmCrud
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmCrud));
            this.NavegadorIlImagenes = new System.Windows.Forms.ImageList(this.components);
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.userControl11 = new CapaVista_Navegador.UserControl1();
            this.SuspendLayout();
            // 
            // NavegadorIlImagenes
            // 
            this.NavegadorIlImagenes.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("NavegadorIlImagenes.ImageStream")));
            this.NavegadorIlImagenes.TransparentColor = System.Drawing.Color.Transparent;
            this.NavegadorIlImagenes.Images.SetKeyName(0, "ingresar.png");
            this.NavegadorIlImagenes.Images.SetKeyName(1, "cancelar.png");
            this.NavegadorIlImagenes.Images.SetKeyName(2, "consultar.png");
            this.NavegadorIlImagenes.Images.SetKeyName(3, "eliminar.png");
            this.NavegadorIlImagenes.Images.SetKeyName(4, "Fin.png");
            this.NavegadorIlImagenes.Images.SetKeyName(5, "Guardar.png");
            this.NavegadorIlImagenes.Images.SetKeyName(6, "icono aterior.png");
            this.NavegadorIlImagenes.Images.SetKeyName(7, "imprimir.png");
            this.NavegadorIlImagenes.Images.SetKeyName(8, "inicio.png");
            this.NavegadorIlImagenes.Images.SetKeyName(9, "modificar.png");
            this.NavegadorIlImagenes.Images.SetKeyName(10, "refrescar.png");
            this.NavegadorIlImagenes.Images.SetKeyName(11, "salir.png");
            this.NavegadorIlImagenes.Images.SetKeyName(12, "siguiente.png");
            this.NavegadorIlImagenes.Images.SetKeyName(13, "Fin.png");
            this.NavegadorIlImagenes.Images.SetKeyName(14, "ayuda.png");
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // userControl11
            // 
            this.userControl11.Location = new System.Drawing.Point(21, 22);
            this.userControl11.Name = "userControl11";
            this.userControl11.Size = new System.Drawing.Size(1110, 284);
            this.userControl11.TabIndex = 15;
            // 
            // FrmCrud
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(233)))), ((int)(((byte)(217)))));
            this.ClientSize = new System.Drawing.Size(1293, 653);
            this.Controls.Add(this.userControl11);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmCrud";
            this.Text = "1001 – Crud";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ImageList NavegadorIlImagenes;
        private ImageList imageList1;
        private UserControl1 userControl11;
    }
}
