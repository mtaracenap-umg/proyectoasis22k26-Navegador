using CapaVista_Navegador.formularios;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
// Inicio cambio - Gabriel André Guillén Pocón - 0901-23-1998
// Se agrega este using para poder usar TipoPermiso (Insertar/Editar/Eliminar/Imprimir), que es del componente Seguridad.
using CapaControlador_Seguridad.Objetos_de_valor;
// Fin cambio - Gabriel André Guillén Pocón - 0901-23-1998

namespace CapaVista_Navegador
{
    public partial class FrmCrud : Form
    {
        // CAMBIAR AQUÍ MANUALMENTE LA TABLA A LA QUE SE DESEA HACER CRUD
        private string _NombreTabla = "tbl_PruebaValidaciones";

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

        // Inicio cambio - Gabriel André Guillén Pocón - 0901-23-1998
        // Los constructores de arriba usan por defecto el módulo "Seguridad" (idModulo 4) y la
        // aplicación "Empleados" (idAplicacion 4) del script dbSistemaEmbutidos_v1.7.sql, que ya
        // tienen permisos distintos para Administrador/Supervisor/Operativo. Si otro módulo quiere
        // usar el navegador con sus propios permisos, usa el constructor de 5 parámetros.
        public FrmCrud(
            string UsuarioActual,
            string CodigoModulo,
            string Tabla)
            : this(UsuarioActual, CodigoModulo, Tabla, 4, 4)
        {
        }
        // Fin cambio - Gabriel André Guillén Pocón - 0901-23-1998

        public FrmCrud(
            string UsuarioActual,
            string CodigoModulo,
            string Tabla,
            int IdModulo,
            int IdAplicacion)
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

            // Inicio cambio - Gabriel André Guillén Pocón - 0901-23-1998
            // Se arma un diccionario que dice: "este botón necesita este permiso para poder usarse".
            // Los botones viven dentro del UserControl, por eso se buscan por su nombre.
            // Guardar no se mapea porque se usa tanto para insertar como para modificar; el filtro
            // real ya pasó al poder abrir el panel con Ingresar o con Modificar.
            var MapaBotones = new Dictionary<Control, TipoPermiso>();
            NavegadorMetMapearBoton(MapaBotones, "NavegadorBtnIngresar", TipoPermiso.Insertar);
            NavegadorMetMapearBoton(MapaBotones, "NavegadorBtnModificar", TipoPermiso.Editar);
            NavegadorMetMapearBoton(MapaBotones, "NavegadorBtnEliminar", TipoPermiso.Eliminar);
            NavegadorMetMapearBoton(MapaBotones, "NavegadorBtnImprimir", TipoPermiso.Imprimir);

            // Aquí es donde realmente se habilitan o deshabilitan los botones según el usuario en sesión.
            new ClsCrudSeguridad(IdModulo, IdAplicacion)
                .NavegadorMetAplicarPermisos(this, MapaBotones);
            // Fin cambio - Gabriel André Guillén Pocón - 0901-23-1998

            Load += (Origen, Evento) =>
                _Eventos.NavegadorMetCargar();

            Resize += (Origen, Evento) =>
                _Eventos.NavegadorMetPosicionar();
        }

        // Inicio cambio - Gabriel André Guillén Pocón - 0901-23-1998
        // Busca un botón por su nombre dentro del UserControl y, si existe, lo agrega al diccionario
        // junto con el permiso que necesita.
        private void NavegadorMetMapearBoton(
            Dictionary<Control, TipoPermiso> MapaBotones,
            string NombreBoton,
            TipoPermiso Permiso)
        {
            Control[] Encontrados = userControl11.Controls.Find(NombreBoton, true);

            if (Encontrados.Length > 0)
            {
                MapaBotones[Encontrados[0]] = Permiso;
            }
        }
        // Fin cambio - Gabriel André Guillén Pocón - 0901-23-1998

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
