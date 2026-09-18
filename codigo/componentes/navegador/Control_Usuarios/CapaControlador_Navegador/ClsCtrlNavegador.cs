using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaModelo_Navegador;

namespace CapaControlador_Navegador
{
    public class ClsCtrlNavegador
    {
        private readonly ClsEstadoNavegador _Estado;

        public event Action<string> NavegadorAccionSolicitada;

        public ClsCtrlNavegador()
        {
            _Estado = new ClsEstadoNavegador();
        }

        public void NavegadorMetConfigurar(
            string Tabla,
            string Usuario,
            string Modulo)
        {
            _Estado.NavegadorMetConfigurar(
                Tabla,
                Usuario,
                Modulo);
        }

        public bool NavegadorFuncConfiguracionValida()
        {
            return
                !string.IsNullOrWhiteSpace(_Estado.Tabla) &&
                !string.IsNullOrWhiteSpace(_Estado.Usuario) &&
                !string.IsNullOrWhiteSpace(_Estado.Modulo);
        }

        public string NavegadorFuncObtenerTabla()
        {
            return _Estado.Tabla;
        }

        public void NavegadorMetCambiarTabla(string Tabla)
        {
            _Estado.Tabla = string.IsNullOrWhiteSpace(Tabla)
                ? string.Empty
                : Tabla.Trim();
        }

        public void NavegadorMetEjecutarAccion(string Accion)
        {
            if (!NavegadorFuncConfiguracionValida())
            {
                return;
            }

            NavegadorAccionSolicitada?.Invoke(Accion);
        }
    }
}
