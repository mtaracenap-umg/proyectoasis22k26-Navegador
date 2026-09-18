using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaModelo_Navegador
{
    public class ClsEstadoNavegador
    {
        public string Tabla { get; set; }
        public string Usuario { get; set; }
        public string Modulo { get; set; }

        public ClsEstadoNavegador()
        {
            Tabla = string.Empty;
            Usuario = string.Empty;
            Modulo = string.Empty;
        }

        public void NavegadorMetConfigurar(
            string Tabla,
            string Usuario,
            string Modulo)
        {
            this.Tabla = string.IsNullOrWhiteSpace(Tabla)
                ? string.Empty
                : Tabla.Trim();

            this.Usuario = string.IsNullOrWhiteSpace(Usuario)
                ? string.Empty
                : Usuario.Trim();

            this.Modulo = string.IsNullOrWhiteSpace(Modulo)
                ? string.Empty
                : Modulo.Trim();
        }
    }
}
