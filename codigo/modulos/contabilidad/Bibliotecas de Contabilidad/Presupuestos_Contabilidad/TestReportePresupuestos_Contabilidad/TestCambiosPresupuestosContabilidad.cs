/*
 * ==================================================================
 * Área : Contabilidad
 * Autor : Mario Taracena
 * Carné : 0901-23-9335
 * Fecha : 12/11/2026
 * ==================================================================
 * Propósito :
 *  Pruebas de los cambios al mantenimiento de presupuestos: la fecha
 *  de cierre no puede quedar antes del inicio, modificar un
 *  presupuesto pone al día lo ejecutado con las pólizas y cada cambio
 *  queda en la bitácora.
 * Reglas especificas:
 *  Cada prueba sigue la triple A. Las que escriben en la base crean un
 *  presupuesto de prueba y al final lo borran, junto con sus partidas y
 *  sus registros de bitácora. La sesión se simula con un usuario que
 *  ya existe y se limpia al terminar.
 * ===================================================================
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using CapaControlador_Contabilidad;
using CapaControlador_Seguridad.Objetos_de_valor;
using CapaModelo_Contabilidad.Entidades;
using CapaModelo_Contabilidad.Repositorios;
using CapaVista_Contabilidad;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Pruebas_Contabilidad
{
    // Pruebas de fechas, edición y bitácora de presupuestos
    [TestClass]
    public class TestCambiosPresupuestosContabilidad
    {
        // Nombre de los presupuestos de prueba
        private const string _NombrePrueba = "Prueba automática de presupuesto";

        // Controlador del mantenimiento
        private ClsControladorPresupuestosContabilidad _Controlador;

        // Repositorio para revisar y limpiar la base
        private ClsRepositorioReportePresupuestosContabilidad _Base;

        // Prepara cada prueba
        [TestInitialize]
        public void ContabilidadMetIniciar()
        {
            _Controlador = new ClsControladorPresupuestosContabilidad();
            _Base = new ClsRepositorioReportePresupuestosContabilidad();
            ContabilidadMetLimpiarBase();
        }

        // Limpia la base y la sesión al terminar
        [TestCleanup]
        public void ContabilidadMetTerminar()
        {
            ClsSesionSeguridad.IdUsuario = 0;
            ContabilidadMetLimpiarBase();
        }

        //  Apoyos de las pruebas
        // ==================================================================

        // Borra los presupuestos de prueba y su bitácora
        private void ContabilidadMetLimpiarBase()
        {
            using (DataTable Ids = _Base.ContabilidadFuncEjecutarConsulta(
                "SELECT idPresupuestoEncabezado FROM tblPresupuestoEncabezado WHERE nombrePresupuestoEncabezado LIKE ?", CommandType.Text,
                new List<OdbcParameter> { new OdbcParameter("Nombre", _NombrePrueba + "%") }))
                foreach (DataRow Fila in Ids.Rows)
                {
                    int Id = Convert.ToInt32(Fila[0]);
                    foreach (string Sentencia in new[]
                    {
                        "DELETE FROM tblbitacora WHERE tablaBitacora = 'tblPresupuestoEncabezado' AND idRegistroBitacora = ?",
                        "DELETE FROM tblPresupuestoDetalle WHERE idPresupuestoEncabezado = ?",
                        "DELETE FROM tblPresupuestoEncabezado WHERE idPresupuestoEncabezado = ?"
                    })
                        _Base.ContabilidadFuncEjecutarNonQuery(Sentencia, new List<OdbcParameter> { new OdbcParameter("Id", Id) }, CommandType.Text);
                }
        }

        // Detalle de partidas de ejemplo
        private DataTable ContabilidadFuncDetalle(params decimal[] Cantidades)
        {
            string[] Modulos = _Controlador.ContabilidadFuncModulos();
            ClsModeloPresupuestoContabilidad Modelo = new ClsModeloPresupuestoContabilidad();
            for (int Indice = 0; Indice < Cantidades.Length; Indice++)
                Modelo.Partidas.Add(new ClsModeloPartidaPresupuestoContabilidad
                    { Nombre = "Partida " + (Indice + 1), Modulo = Modulos[Indice % Modulos.Length], Cantidad = Cantidades[Indice] });
            return _Controlador.ContabilidadFuncPresupuesto(Modelo);
        }

        // Presupuesto guardado por su id
        private ClsModeloPresupuestoContabilidad ContabilidadFuncLeer(int Id)
        {
            return _Controlador.ContabilidadFuncPresupuestos().FirstOrDefault(Presupuesto => Presupuesto.IdPresupuesto == Id);
        }

        // Registros de bitácora de un presupuesto
        private List<DataRow> ContabilidadFuncBitacora(int Id)
        {
            using (DataTable Tabla = _Base.ContabilidadFuncEjecutarConsulta(
                "SELECT accionBitacora, idUsuario, detallesBitacora FROM tblbitacora WHERE tablaBitacora = 'tblPresupuestoEncabezado' " +
                "AND idRegistroBitacora = ? ORDER BY idBitacora", CommandType.Text, new List<OdbcParameter> { new OdbcParameter("Id", Id) }))
                return Tabla.Rows.Cast<DataRow>().ToList();
        }

        // Ejecuta la prueba de una ventana en un hilo STA
        private static void ContabilidadMetEnVentana(Func<Form> Crear, Action<Form> Comprobar)
        {
            Exception Fallo = null;
            Thread Hilo = new Thread(delegate ()
            {
                Form Ventana = null;
                try
                {
                    Ventana = Crear();
                    Comprobar(Ventana);
                }
                catch (Exception Excepcion)
                {
                    Fallo = Excepcion;
                }
                finally
                {
                    if (Ventana != null) Ventana.Dispose();
                }
            });
            Hilo.SetApartmentState(ApartmentState.STA);
            Hilo.Start();
            Hilo.Join();
            if (Fallo != null) ExceptionDispatchInfo.Capture(Fallo).Throw();
        }

        // Busca un control por nombre
        private static TControl ContabilidadFuncControl<TControl>(Form Ventana, string Nombre) where TControl : Control
        {
            Control[] Encontrados = Ventana.Controls.Find(Nombre, true);
            Assert.AreEqual(1, Encontrados.Length, "No se encontró el control " + Nombre + ".");
            return (TControl)Encontrados[0];
        }

        //  Fecha de cierre
        // ==================================================================

        // Prueba: edición marca cierre antes del inicio
        [TestMethod]
        public void Test_EdicionMarcaCierreAntesDelInicio()
        {
            //Arrange: inicializar las variables
            ClsModeloPresupuestoContabilidad Presupuesto = new ClsModeloPresupuestoContabilidad
            {
                IdPresupuesto = 99, Nombre = "Presupuesto de prueba", FechaInicio = new DateTime(2026, 3, 1), FechaCierre = new DateTime(2026, 9, 30),
                Partidas = { new ClsModeloPartidaPresupuestoContabilidad { Nombre = "Sueldos", Modulo = "Contabilidad", Cantidad = 1500m } }
            };
            //Act: ejecucion del metodo a probar
            ContabilidadMetEnVentana(() => new FrmPresupuestoEdicion(ClsModeloPermisosPantallaContabilidad.Total, Presupuesto), Ventana =>
            {
                DateTimePicker Cierre = ContabilidadFuncControl<DateTimePicker>(Ventana, "ContabilidadDtpCierre");
                Button Guardar = ContabilidadFuncControl<Button>(Ventana, "ContabilidadBtnGuardar");
                Label Aviso = ContabilidadFuncControl<Label>(Ventana, "ContabilidadLblErrorContabilidadDtpCierre");
                Cierre.Value = new DateTime(2026, 2, 1);
                bool GuardarConError = Guardar.Enabled;
                string AvisoConError = Aviso.Text;
                Cierre.Value = new DateTime(2026, 12, 31);
                //Assert: comprobacion de los resultados
                Assert.IsFalse(GuardarConError);
                Assert.AreEqual("La fecha final no puede ser anterior a la inicial.", AvisoConError);
                Assert.IsTrue(Guardar.Enabled);
                Assert.AreEqual(string.Empty, Aviso.Text);
            });
        }

        // Prueba: edición marca inicio después del cierre
        [TestMethod]
        public void Test_EdicionMarcaInicioDespuesDelCierre()
        {
            //Arrange: inicializar las variables
            ClsModeloPresupuestoContabilidad Presupuesto = new ClsModeloPresupuestoContabilidad
            {
                IdPresupuesto = 99, Nombre = "Presupuesto de prueba", FechaInicio = new DateTime(2026, 3, 1), FechaCierre = new DateTime(2026, 9, 30),
                Partidas = { new ClsModeloPartidaPresupuestoContabilidad { Nombre = "Sueldos", Modulo = "Contabilidad", Cantidad = 1500m } }
            };
            //Act: ejecucion del metodo a probar
            ContabilidadMetEnVentana(() => new FrmPresupuestoEdicion(ClsModeloPermisosPantallaContabilidad.Total, Presupuesto), Ventana =>
            {
                ContabilidadFuncControl<DateTimePicker>(Ventana, "ContabilidadDtpInicio").Value = new DateTime(2026, 10, 1);
                bool GuardarConError = ContabilidadFuncControl<Button>(Ventana, "ContabilidadBtnGuardar").Enabled;
                ContabilidadFuncControl<DateTimePicker>(Ventana, "ContabilidadDtpCierre").Checked = false;
                ContabilidadFuncControl<DateTimePicker>(Ventana, "ContabilidadDtpInicio").Value = new DateTime(2026, 10, 2);
                //Assert: comprobacion de los resultados
                Assert.IsFalse(GuardarConError);
                Assert.IsTrue(ContabilidadFuncControl<Button>(Ventana, "ContabilidadBtnGuardar").Enabled);
            });
        }

        // Prueba: controlador rechaza cierre antes del inicio
        [TestMethod]
        public void Test_ControladorRechazaCierreAntesDelInicio()
        {
            //Arrange: inicializar las variables
            int Id = 0;
            string Mensaje;
            //Act: ejecucion del metodo a probar
            bool Grabado;
            using (DataTable Detalle = ContabilidadFuncDetalle(100m))
                Grabado = _Controlador.ContabilidadFuncGrabarCambios(EstadoEntidadContabilidad.Agregar, ref Id, _NombrePrueba,
                    new DateTime(2026, 6, 1), new DateTime(2026, 5, 1), 0m, Detalle, out Mensaje);
            //Assert: comprobacion de los resultados
            Assert.IsFalse(Grabado);
            Assert.AreEqual(0, Id);
            Assert.AreEqual("La fecha de cierre debe ser igual o posterior al inicio.", Mensaje);
        }

        // Prueba: reporte bloquea rango invertido
        [TestMethod]
        public void Test_ReporteBloqueaRangoInvertido()
        {
            //Arrange: inicializar las variables
            Func<Form> Crear = () => new FrmReportePresupuestos();
            //Act: ejecucion del metodo a probar
            ContabilidadMetEnVentana(Crear, Ventana =>
            {
                ContabilidadFuncControl<ComboBox>(Ventana, "ContabilidadCboFiltro").SelectedIndex = 2;
                ContabilidadFuncControl<DateTimePicker>(Ventana, "ContabilidadDtpHasta").Value = new DateTime(DateTime.Today.Year - 1, 6, 30);
                bool GenerarConError = ContabilidadFuncControl<Button>(Ventana, "ContabilidadBtnGenerar").Enabled;
                string Aviso = ContabilidadFuncControl<Label>(Ventana, "ContabilidadLblErrorContabilidadDtpHasta").Text;
                ContabilidadFuncControl<ComboBox>(Ventana, "ContabilidadCboFiltro").SelectedIndex = 0;
                //Assert: comprobacion de los resultados
                Assert.IsFalse(GenerarConError);
                Assert.AreEqual("La fecha final no puede ser anterior a la inicial.", Aviso);
                Assert.IsTrue(ContabilidadFuncControl<Button>(Ventana, "ContabilidadBtnGenerar").Enabled);
            });
        }

        //  Modificar y pólizas
        // ==================================================================

        // Prueba: modificar presupuesto en la base
        [TestMethod]
        public void Test_ModificarPresupuestoEnLaBase()
        {
            //Arrange: inicializar las variables
            int Id = 0;
            string Mensaje;
            using (DataTable Detalle = ContabilidadFuncDetalle(1000m, 500m))
                Assert.IsTrue(_Controlador.ContabilidadFuncGrabarCambios(EstadoEntidadContabilidad.Agregar, ref Id, _NombrePrueba,
                    new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), 0m, Detalle, out Mensaje), Mensaje);
            //Act: ejecucion del metodo a probar
            bool Modificado;
            using (DataTable Detalle = ContabilidadFuncDetalle(2000m, 300m, 700m))
                Modificado = _Controlador.ContabilidadFuncGrabarCambios(EstadoEntidadContabilidad.Modificar, ref Id, _NombrePrueba + " editado",
                    new DateTime(2026, 2, 1), null, 0m, Detalle, out Mensaje);
            ClsModeloPresupuestoContabilidad Guardado = ContabilidadFuncLeer(Id);
            //Assert: comprobacion de los resultados
            Assert.IsTrue(Modificado, Mensaje);
            Assert.AreEqual(_NombrePrueba + " editado", Guardado.Nombre);
            Assert.AreEqual(new DateTime(2026, 2, 1), Guardado.FechaInicio);
            Assert.IsFalse(Guardado.FechaCierre.HasValue);
            Assert.AreEqual(3, Guardado.Partidas.Count);
            Assert.AreEqual(3000m, Guardado.Partidas.Sum(Partida => Partida.Cantidad));
        }

        // Prueba: sincronizar gastado con las pólizas
        [TestMethod]
        public void Test_SincronizarGastadoConLasPolizas()
        {
            //Arrange: inicializar las variables
            int Id = 0;
            string Mensaje;
            using (DataTable Detalle = ContabilidadFuncDetalle(1000m))
                Assert.IsTrue(_Controlador.ContabilidadFuncGrabarCambios(EstadoEntidadContabilidad.Agregar, ref Id, _NombrePrueba,
                    new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), 0m, Detalle, out Mensaje), Mensaje);
            ClsModeloPresupuestoContabilidad Presupuesto = ContabilidadFuncLeer(Id);
            //Act: ejecucion del metodo a probar
            bool Cambio = _Controlador.ContabilidadFuncSincronizarGastado(Presupuesto, 250.75m);
            bool SinCambio = _Controlador.ContabilidadFuncSincronizarGastado(Presupuesto, 250.75m);
            bool Negativo = _Controlador.ContabilidadFuncSincronizarGastado(Presupuesto, -40m);
            //Assert: comprobacion de los resultados
            Assert.IsTrue(Cambio);
            Assert.IsFalse(SinCambio);
            Assert.IsTrue(Negativo);
            Assert.AreEqual(0m, ContabilidadFuncLeer(Id).Gastado);
            Assert.IsTrue(_Controlador.ContabilidadFuncSincronizarGastado(Presupuesto, 250.75m));
            Assert.AreEqual(250.75m, ContabilidadFuncLeer(Id).Gastado);
        }

        // Prueba: modificar recalcula el ejecutado de las pólizas
        [TestMethod]
        public void Test_ModificarRecalculaElEjecutadoDeLasPolizas()
        {
            //Arrange: inicializar las variables
            DateTime Inicio = new DateTime(2026, 1, 1), Cierre = new DateTime(2026, 12, 31);
            //Act: ejecucion del metodo a probar
            using (DataTable Detalle = ContabilidadFuncDetalle(1000m, 500m))
            using (DataTable Ejecucion = _Controlador.ContabilidadFuncEjecucion(Inicio, Cierre, Detalle))
            {
                Dictionary<string, decimal> Polizas = new ClsRepositorioPresupuestosContabilidad().ContabilidadFuncObtenerEjecutado(Inicio, Cierre);
                //Assert: comprobacion de los resultados
                foreach (DataRow Fila in Ejecucion.Rows)
                {
                    string Modulo = (string)Fila["Modulo"];
                    Assert.AreEqual(Polizas.ContainsKey(Modulo) ? Polizas[Modulo] : 0m, (decimal)Fila["Ejecutado"]);
                }
            }
        }

        // Prueba: lista muestra el nombre al recargar
        [TestMethod]
        public void Test_ListaMuestraElNombreAlRecargar()
        {
            //Arrange: inicializar las variables
            int Id = 0;
            string Mensaje;
            using (DataTable Detalle = ContabilidadFuncDetalle(1000m))
                Assert.IsTrue(_Controlador.ContabilidadFuncGrabarCambios(EstadoEntidadContabilidad.Agregar, ref Id, _NombrePrueba,
                    new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), 0m, Detalle, out Mensaje), Mensaje);
            System.Reflection.MethodInfo Recargar = typeof(FrmPresupuestos).GetMethod("ContabilidadMetCargarLista",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            //Act: ejecucion del metodo a probar
            ContabilidadMetEnVentana(() => new FrmPresupuestos(), Ventana =>
            {
                Recargar.Invoke(Ventana, new object[] { Id });
                Recargar.Invoke(Ventana, new object[] { Id });
                ComboBox Lista = ContabilidadFuncControl<ComboBox>(Ventana, "ContabilidadCboPresupuesto");
                //Assert: comprobacion de los resultados
                Assert.AreEqual("Nombre", Lista.DisplayMember);
                Assert.AreEqual(_NombrePrueba, Lista.GetItemText(Lista.SelectedItem));
                foreach (ClsModeloPresupuestoContabilidad Elemento in Lista.Items)
                    Assert.AreEqual(Elemento.Nombre, Lista.GetItemText(Elemento));
            });
        }

        //  Bitácora
        // ==================================================================

        // Prueba: bitácora registra crear modificar y baja
        [TestMethod]
        public void Test_BitacoraRegistraCrearModificarYBaja()
        {
            //Arrange: inicializar las variables
            ClsSesionSeguridad.IdUsuario = 1;
            int Id = 0;
            string Mensaje;
            //Act: ejecucion del metodo a probar
            using (DataTable Detalle = ContabilidadFuncDetalle(1000m))
                Assert.IsTrue(_Controlador.ContabilidadFuncGrabarCambios(EstadoEntidadContabilidad.Agregar, ref Id, _NombrePrueba,
                    new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), 0m, Detalle, out Mensaje), Mensaje);
            int IdCreado = Id;
            using (DataTable Detalle = ContabilidadFuncDetalle(1500m, 200m))
                Assert.IsTrue(_Controlador.ContabilidadFuncGrabarCambios(EstadoEntidadContabilidad.Modificar, ref Id, _NombrePrueba + " editado",
                    new DateTime(2026, 1, 1), new DateTime(2026, 6, 30), 0m, Detalle, out Mensaje), Mensaje);
            using (DataTable Detalle = ContabilidadFuncDetalle(1500m, 200m))
                Assert.IsTrue(_Controlador.ContabilidadFuncGrabarCambios(EstadoEntidadContabilidad.Borrar, ref Id, _NombrePrueba + " editado",
                    new DateTime(2026, 1, 1), null, 0m, Detalle, out Mensaje), Mensaje);
            List<DataRow> Registros = ContabilidadFuncBitacora(IdCreado);
            //Assert: comprobacion de los resultados
            CollectionAssert.AreEqual(new[] { "INSERT", "UPDATE", "DELETE" }, Registros.Select(Fila => (string)Fila["accionBitacora"]).ToArray());
            Assert.IsTrue(Registros.All(Fila => Convert.ToInt32(Fila["idUsuario"]) == 1));
            StringAssert.StartsWith((string)Registros[0]["detallesBitacora"], "Presupuesto creado: '" + _NombrePrueba + "'");
            StringAssert.Contains((string)Registros[1]["detallesBitacora"], "Antes: '" + _NombrePrueba + "', vigencia 01/01/2026 al 31/12/2026, 1 partida");
            StringAssert.Contains((string)Registros[1]["detallesBitacora"], "Ahora: '" + _NombrePrueba + " editado', vigencia 01/01/2026 al 30/06/2026, 2 partidas, asignado Q 1,700.00");
            StringAssert.StartsWith((string)Registros[2]["detallesBitacora"], "Presupuesto dado de baja: '" + _NombrePrueba + " editado'");
        }

        // Prueba: sin sesión guarda sin bitácora
        [TestMethod]
        public void Test_SinSesionGuardaSinBitacora()
        {
            //Arrange: inicializar las variables
            ClsSesionSeguridad.IdUsuario = 0;
            int Id = 0;
            string Mensaje;
            //Act: ejecucion del metodo a probar
            bool Grabado;
            using (DataTable Detalle = ContabilidadFuncDetalle(1000m))
                Grabado = _Controlador.ContabilidadFuncGrabarCambios(EstadoEntidadContabilidad.Agregar, ref Id, _NombrePrueba,
                    new DateTime(2026, 1, 1), null, 0m, Detalle, out Mensaje);
            //Assert: comprobacion de los resultados
            Assert.IsTrue(Grabado, Mensaje);
            Assert.AreEqual(0, ContabilidadFuncBitacora(Id).Count);
        }

        // Prueba: resumen de bitácora
        [TestMethod]
        public void Test_ResumenDeBitacora()
        {
            //Arrange: inicializar las variables
            DateTime Inicio = new DateTime(2026, 1, 1);
            //Act: ejecucion del metodo a probar
            string ConCierre = ClsControladorPresupuestosContabilidad.ContabilidadFuncResumenBitacora("Anual", Inicio, new DateTime(2026, 12, 31), 2, 1500.5m, 20m);
            string SinCierre = ClsControladorPresupuestosContabilidad.ContabilidadFuncResumenBitacora("Abierto", Inicio, null, 1, 10m, 0m);
            //Assert: comprobacion de los resultados
            Assert.AreEqual("'Anual', vigencia 01/01/2026 al 31/12/2026, 2 partidas, asignado Q 1,500.50, ejecutado Q 20.00", ConCierre);
            Assert.AreEqual("'Abierto', vigencia 01/01/2026 sin cierre, 1 partida, asignado Q 10.00, ejecutado Q 0.00", SinCierre);
        }
    }
}
