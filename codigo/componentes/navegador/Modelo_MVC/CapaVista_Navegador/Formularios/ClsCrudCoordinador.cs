using CapaControlador_Navegador;
using CapaEntidades_Navegador;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace CapaVista_Navegador
{
    public class ClsCrudCoordinador
    {
        private readonly FrmCrud _Vista;

        private readonly ClsCtrlTabla _CtrlTabla;
        private readonly ClsCrudGrid _Grid;
        private readonly ClsCrudFormulario _Formulario;
        private readonly ClsCrudAcciones _Acciones;
        private readonly ClsSelectorLlave _SelectorLlave;
        private readonly ClsCrudSeguridad _Seguridad;

        private List<ClsColumnaInfo> _EsquemaActual;
        private Dictionary<string, string> _PkModificar;

        public ClsCrudCoordinador(
            FrmCrud Vista,
            string UsuarioActual,
            string CodigoModulo)
        {
            _Vista = Vista;

            _CtrlTabla = new ClsCtrlTabla();

            _Grid = new ClsCrudGrid(_Vista);

            _Formulario = new ClsCrudFormulario(_Vista);

            _SelectorLlave = new ClsSelectorLlave(_Vista);

            _Seguridad = new ClsCrudSeguridad(
                UsuarioActual,
                CodigoModulo);

            _Acciones = new ClsCrudAcciones();
        }

        // CONSULTAR TABLA
        private bool NavegadorFuncConsultarTabla()
        {
            try
            {
                DataTable Datos =
                    _CtrlTabla.NavegadorFuncLlenarDgv(
                        _Vista.NombreTabla);

                _EsquemaActual =
                    _SelectorLlave.NavegadorFuncObtenerEsquemaConLlaves(
                        _Vista.NombreTabla);

                _Grid.NavegadorMetMostrar(Datos);

                NavegadorMetPosicionar();

                _Vista.Text =
                    "1001 – Crud " + _Vista.NombreTabla;

                return true;
            }
            catch (Exception Excepcion)
            {
                _EsquemaActual = null;

                _Formulario.NavegadorMetCerrar();

                _Grid.NavegadorMetOcultar();

                MessageBox.Show(
                    _Acciones.NavegadorFuncMensajeAmigable(
                        Excepcion),
                    "Error al consultar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
        }

        // POSICIONAR CONTROLES
        public void NavegadorMetPosicionar()
        {
            int Inicio =
                NavegadorFuncObtenerInicioContenido();

            int PosicionY =
                _Formulario.Visible
                ? _Formulario.Bottom + 10
                : Inicio;

            _Grid.NavegadorMetPosicionar(PosicionY);
        }

        // OBTENER INICIO DEL CONTENIDO
        private int NavegadorFuncObtenerInicioContenido()
        {
            int MaxBottom = 0;

            foreach (Control ControlActual in _Vista.Controls)
            {
                if (ControlActual.Visible &&
                    (ControlActual is Button ||
                     ControlActual is UserControl) &&
                    ControlActual.Bottom > MaxBottom)
                {
                    MaxBottom = ControlActual.Bottom;
                }
            }

            return MaxBottom + 15;
        }

        // INGRESAR
        public void NavegadorMetIngresar()
        {
            if (!_Seguridad.NavegadorFuncTieneAcceso())
                return;

            if (!NavegadorFuncConsultarTabla())
                return;

            _PkModificar = null;

            if (_EsquemaActual == null ||
                _EsquemaActual.Count == 0)
            {
                MessageBox.Show(
                    "No se pudo obtener la estructura de la tabla indicada.",
                    "Ingresar registro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            _Formulario.NavegadorMetAbrir(
                _Vista.NombreTabla,
                _EsquemaActual,
                false,
                null,
                _Grid,
                NavegadorFuncObtenerInicioContenido());

            NavegadorMetPosicionar();
        }

        // CONSULTAR
        public void NavegadorMetConsultar()
        {
            if (!_Seguridad.NavegadorFuncTieneAcceso())
                return;

            _Formulario.NavegadorMetCerrar();

            NavegadorFuncConsultarTabla();
        }

        // REFRESCAR
        public void NavegadorMetRefrescar()
        {
            if (!_Seguridad.NavegadorFuncTieneAcceso())
                return;

            _Formulario.NavegadorMetCerrar();

            _Grid.NavegadorMetOcultar();
        }

        // MODIFICAR
        public void NavegadorMetModificar()
        {
            if (!_Seguridad.NavegadorFuncTieneAcceso())
                return;

            DataGridViewRow Fila =
                _Grid.NavegadorDgvDatos != null
                ? _Grid.NavegadorDgvDatos.CurrentRow
                : null;

            if (Fila == null || Fila.IsNewRow)
            {
                MessageBox.Show(
                    "Seleccione un registro en la tabla para Modificar.",
                    "Modificar registro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            _EsquemaActual =
                _SelectorLlave.NavegadorFuncObtenerEsquemaConLlaves(
                    _Vista.NombreTabla);

            _PkModificar =
                _Grid.NavegadorFuncObtenerClavesPrimarias(
                    _EsquemaActual,
                    Fila);

            if (_PkModificar.Count == 0)
            {
                MessageBox.Show(
                    "No se pudo obtener la llave primaria del registro seleccionado.",
                    "Modificar registro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            _Formulario.NavegadorMetAbrir(
                _Vista.NombreTabla,
                _EsquemaActual,
                true,
                Fila,
                _Grid,
                NavegadorFuncObtenerInicioContenido());

            NavegadorMetPosicionar();
        }

        // ELIMINAR
        public void NavegadorMetEliminar()
        {
            if (!_Seguridad.NavegadorFuncTieneAcceso())
                return;

            DataGridViewRow Fila =
                _Grid.NavegadorDgvDatos != null
                ? _Grid.NavegadorDgvDatos.CurrentRow
                : null;

            if (Fila == null || Fila.IsNewRow)
            {
                MessageBox.Show(
                    "Seleccione un registro en la tabla para eliminar.",
                    "Eliminar registro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            Dictionary<string, string> ClavesPrimarias =
                _Grid.NavegadorFuncObtenerClavesPrimarias(
                    _EsquemaActual,
                    Fila);

            string Mensaje;

            try
            {
                if (_Acciones.NavegadorFuncEliminar(
                    _Vista.NombreTabla,
                    ClavesPrimarias,
                    out Mensaje))
                {
                    MessageBox.Show(
                        "Registro eliminado correctamente.",
                        "Eliminación exitosa",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    NavegadorFuncConsultarTabla();
                }
                else if (!string.IsNullOrEmpty(Mensaje))
                {
                    MessageBox.Show(
                        Mensaje,
                        "Eliminar registro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception Excepcion)
            {
                MessageBox.Show(
                    _Acciones.NavegadorFuncMensajeAmigable(
                        Excepcion),
                    "Error al eliminar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // GUARDAR
        public void NavegadorMetGuardar()
        {
            if (!_Formulario.Visible)
            {
                MessageBox.Show(
                    "Abra un registro con Ingresar o Modificar antes de guardar.",
                    "Guardar registro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            Dictionary<string, string> Datos =
                _Formulario.NavegadorFuncObtenerDatos();

            string Mensaje;

            try
            {
                if (_Acciones.NavegadorFuncGuardar(
                    _Vista.NombreTabla,
                    _EsquemaActual,
                    Datos,
                    _Formulario.ModoModificar,
                    _PkModificar,
                    out Mensaje))
                {
                    MessageBox.Show(
                        "Registro guardado correctamente.",
                        "Guardado exitoso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    _Formulario.NavegadorMetCerrar();

                    _PkModificar = null;

                    NavegadorFuncConsultarTabla();
                }
                else if (!string.IsNullOrEmpty(Mensaje))
                {
                    MessageBox.Show(
                        Mensaje,
                        "Guardar registro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception Excepcion)
            {
                MessageBox.Show(
                    _Acciones.NavegadorFuncMensajeAmigable(
                        Excepcion),
                    "Error al guardar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // CANCELAR
        public void NavegadorMetCancelar()
        {
            _Formulario.NavegadorMetCerrar();

            _PkModificar = null;

            NavegadorMetPosicionar();
        }

        // OCULTAR GRID
        public void NavegadorMetOcultarGrid()
        {
            _Grid.NavegadorMetOcultar();
        }

        // INICIO
        public void NavegadorMetInicio()
        {
            _Grid.NavegadorMetInicio();
        }

        // ANTERIOR
        public void NavegadorMetAnterior()
        {
            _Grid.NavegadorMetAnterior();
        }

        // SIGUIENTE
        public void NavegadorMetSiguiente()
        {
            _Grid.NavegadorMetSiguiente();
        }

        // FIN
        public void NavegadorMetFin()
        {
            _Grid.NavegadorMetFin();
        }
    }
}
