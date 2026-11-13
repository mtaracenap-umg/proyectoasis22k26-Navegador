/*
 * ==================================================================
 * Área : Contabilidad
 * Autor : Mario Taracena
 * Carné : 0901-23-9335
 * Fecha : 11/11/2026
 * ==================================================================
 * Propósito :
 *  Entrega al MDI la ventana del reporte de presupuestos.
 * Reglas especificas:
 *  Completa el método parcial de ClsReportePresupuestosContabilidad.cs,
 *  que no se modifica.
 * ===================================================================
*/

using System.Windows.Forms;
using CapaControlador_Contabilidad;

namespace CapaVista_Contabilidad
{
    // Enlace del reporte con el MDI
    public static partial class ClsReportePresupuestosContabilidad
    {
        // Crea la ventana del reporte
        static partial void ContabilidadMetCrearReporte(ClsModeloPermisosPantallaContabilidad Permisos, ref Form Formulario)
        {
            Formulario = new FrmReportePresupuestos(Permisos);
        }
    }
}
