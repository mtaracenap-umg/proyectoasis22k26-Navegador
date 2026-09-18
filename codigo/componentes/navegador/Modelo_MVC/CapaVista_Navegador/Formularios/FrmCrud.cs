using CapaVista_Navegador.formularios;
using System;
using System.Windows.Forms;

namespace CapaVista_Navegador
{
    public partial class FrmCrud : Form
    {
        // CAMBIAR AQUÍ MANUALMENTE LA TABLA A LA QUE SE DESEA HACER CRUD
        private string _NombreTabla = "tbl_empleados";

        private ClsCrudEventos _Eventos;

        public string NombreTabla
        {
            get { return _NombreTabla; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _NombreTabla = value.Trim();

                    if (userControl11 != null)
                    {
                        userControl11.NavegadorMetCambiarTabla(
                            _NombreTabla);
                    }

                    if (_Eventos != null)
                    {
                        _Eventos.NavegadorMetConsultar();
                    }
                }
            }
        }

        public FrmCrud()
            : this("USUARIO_PRUEBA", "EMPLEADOS", null)
        {
        }

        public FrmCrud(string UsuarioActual, string CodigoModulo)
            : this(UsuarioActual, CodigoModulo, null)
        {
        }

        public FrmCrud(
            string UsuarioActual,
            string CodigoModulo,
            string Tabla)
        {
            InitializeComponent();

            if (!string.IsNullOrWhiteSpace(Tabla))
            {
                _NombreTabla = Tabla.Trim();
            }

            _Eventos = new ClsCrudEventos(
                this,
                UsuarioActual,
                CodigoModulo);

            userControl11.NavegadorMetConfigurar(
                _NombreTabla,
                UsuarioActual,
                CodigoModulo);

            userControl11.NavegadorAccionSolicitada +=
                NavegadorMetEjecutarAccion;

            Load += (Origen, Evento) =>
                _Eventos.NavegadorMetCargar();

            Resize += (Origen, Evento) =>
                _Eventos.NavegadorMetPosicionar();
        }

        private void NavegadorMetEjecutarAccion(string Accion)
        {
            switch (Accion)
            {
                case "INGRESAR":
                    _Eventos.NavegadorMetIngresar();
                    break;
                case "CONSULTAR":
                    _Eventos.NavegadorMetConsultar();
                    break;
                case "MODIFICAR":
                    _Eventos.NavegadorMetModificar();
                    break;
                case "ELIMINAR":
                    _Eventos.NavegadorMetEliminar();
                    break;
                case "REFRESCAR":
                    _Eventos.NavegadorMetRefrescar();
                    break;
                case "GUARDAR":
                    _Eventos.NavegadorMetGuardar();
                    break;
                case "CANCELAR":
                    _Eventos.NavegadorMetCancelar();
                    break;
                case "INICIO":
                    _Eventos.NavegadorMetInicio();
                    break;
                case "ANTERIOR":
                    _Eventos.NavegadorMetAnterior();
                    break;
                case "SIGUIENTE":
                    _Eventos.NavegadorMetSiguiente();
                    break;
                case "FIN":
                    _Eventos.NavegadorMetFin();
                    break;
                case "SALIR":
                    Close();
                    break;
            }
        }
    }
}
