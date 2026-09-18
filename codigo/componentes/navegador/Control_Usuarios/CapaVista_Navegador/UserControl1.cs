using System;
using System.Windows.Forms;

namespace CapaVista_Navegador
{
    public partial class UserControl1 : UserControl
    {
        public event Action<string> NavegadorAccionSolicitada;

        private string _Tabla;
        private string _Usuario;
        private string _Modulo;

        public UserControl1()
        {
            InitializeComponent();

            NavegadorMetCablearBotones();
        }

        // CONFIGURAR NAVEGADOR
        public void NavegadorMetConfigurar(
            string Tabla,
            string Usuario,
            string Modulo)
        {
            _Tabla = Tabla;
            _Usuario = Usuario;
            _Modulo = Modulo;
        }

        // CAMBIAR TABLA
        public void NavegadorMetCambiarTabla(string Tabla)
        {
            if (!string.IsNullOrWhiteSpace(Tabla))
            {
                _Tabla = Tabla.Trim();
            }
        }

        // OBTENER TABLA
        public string NavegadorFuncObtenerTabla()
        {
            return _Tabla;
        }

        // CABLEAR BOTONES
        private void NavegadorMetCablearBotones()
        {
            NavegadorBtnIngresar.Click +=
                (Sender, Evento) =>
                NavegadorMetSolicitarAccion("INGRESAR");

            NavegadorBtnConsultar.Click +=
                (Sender, Evento) =>
                NavegadorMetSolicitarAccion("CONSULTAR");

            NavegadorBtnModificar.Click +=
                (Sender, Evento) =>
                NavegadorMetSolicitarAccion("MODIFICAR");

            NavegadorBtnEliminar.Click +=
                (Sender, Evento) =>
                NavegadorMetSolicitarAccion("ELIMINAR");

            NavegadorBtnRefrescar.Click +=
                (Sender, Evento) =>
                NavegadorMetSolicitarAccion("REFRESCAR");

            NavegadorBtnGuardar.Click +=
                (Sender, Evento) =>
                NavegadorMetSolicitarAccion("GUARDAR");

            NavegadorBtnCancelar.Click +=
                (Sender, Evento) =>
                NavegadorMetSolicitarAccion("CANCELAR");

            NavegadorBtnInicio.Click +=
                (Sender, Evento) =>
                NavegadorMetSolicitarAccion("INICIO");

            NavegadorBtnAnterior.Click +=
                (Sender, Evento) =>
                NavegadorMetSolicitarAccion("ANTERIOR");

            NavegadorBtnSiguiente.Click +=
                (Sender, Evento) =>
                NavegadorMetSolicitarAccion("SIGUIENTE");

            NavegadorBtnFin.Click +=
                (Sender, Evento) =>
                NavegadorMetSolicitarAccion("FIN");

            NavegadorBtnImprimir.Click +=
                (Sender, Evento) =>
                NavegadorMetSolicitarAccion("IMPRIMIR");

            NavegadorBtnAyuda.Click +=
                (Sender, Evento) =>
                NavegadorMetSolicitarAccion("AYUDA");

            NavegadorBtnSalir.Click +=
                (Sender, Evento) =>
                NavegadorMetSolicitarAccion("SALIR");
        }

        // SOLICITAR ACCIÓN AL FORMULARIO
        private void NavegadorMetSolicitarAccion(string Accion)
        {
            NavegadorAccionSolicitada?.Invoke(Accion);
        }
    }
}