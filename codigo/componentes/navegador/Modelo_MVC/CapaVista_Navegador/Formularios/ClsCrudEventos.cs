using System;

namespace CapaVista_Navegador.formularios
{
    public class ClsCrudEventos
    {
        private readonly ClsCrudCoordinador _Coordinador;

        public ClsCrudEventos(
            FrmCrud Formulario,
            string UsuarioActual,
            string CodigoModulo)
        {
            _Coordinador = new ClsCrudCoordinador(
                Formulario,
                UsuarioActual,
                CodigoModulo);
        }

        public void NavegadorMetCargar()
        {
            _Coordinador.NavegadorMetOcultarGrid();
        }

        public void NavegadorMetIngresar()
        {
            _Coordinador.NavegadorMetIngresar();
        }

        public void NavegadorMetConsultar()
        {
            _Coordinador.NavegadorMetConsultar();
        }

        public void NavegadorMetRefrescar()
        {
            _Coordinador.NavegadorMetRefrescar();
        }

        public void NavegadorMetModificar()
        {
            _Coordinador.NavegadorMetModificar();
        }

        public void NavegadorMetEliminar()
        {
            _Coordinador.NavegadorMetEliminar();
        }

        public void NavegadorMetGuardar()
        {
            _Coordinador.NavegadorMetGuardar();
        }

        public void NavegadorMetCancelar()
        {
            _Coordinador.NavegadorMetCancelar();
        }

        public void NavegadorMetInicio()
        {
            _Coordinador.NavegadorMetInicio();
        }

        public void NavegadorMetAnterior()
        {
            _Coordinador.NavegadorMetAnterior();
        }

        public void NavegadorMetSiguiente()
        {
            _Coordinador.NavegadorMetSiguiente();
        }

        public void NavegadorMetFin()
        {
            _Coordinador.NavegadorMetFin();
        }

        public void NavegadorMetPosicionar()
        {
            _Coordinador.NavegadorMetPosicionar();
        }
    }
}