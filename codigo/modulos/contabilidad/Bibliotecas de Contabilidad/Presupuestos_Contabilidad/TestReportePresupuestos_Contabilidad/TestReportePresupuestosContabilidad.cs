/*
 * ==================================================================
 * Área : Contabilidad
 * Autor : Mario Taracena
 * Carné : 0901-23-9335
 * Fecha : 11/11/2026
 * ==================================================================
 * Propósito :
 *  Pruebas del reporte de ejecución presupuestaria: filtros y fechas,
 *  cálculo de disponible y porcentaje, consulta contra la base, la
 *  ventana del reporte, su enlace con el MDI y el RDLC.
 * Reglas especificas:
 *  Cada prueba sigue la triple A. Las pruebas de base usan el DSN
 *  EmbutidosS.A y solo consultan. Las ventanas se crean en un hilo STA
 *  y se liberan al terminar. El RDLC se renderiza a PDF con datos de
 *  ejemplo en memoria.
 * ===================================================================
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CapaControlador_Contabilidad;
using CapaModelo_Contabilidad.Entidades;
using CapaModelo_Contabilidad.Repositorios;
using CapaVista_Contabilidad;
using Microsoft.Reporting.WinForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Pruebas_Contabilidad
{
    // Pruebas del reporte de presupuestos
    [TestClass]
    public class TestReportePresupuestosContabilidad
    {
        // Fecha fija para las pruebas sin base
        private static readonly DateTime _Hoy = new DateTime(2026, 11, 11);

        // Controlador del reporte
        private ClsControladorReportePresupuestosContabilidad _Controlador;

        // Prepara cada prueba
        [TestInitialize]
        public void ContabilidadMetIniciar()
        {
            _Controlador = new ClsControladorReportePresupuestosContabilidad();
        }

        //  Apoyos de las pruebas
        // ==================================================================

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

        // Presupuestos de ejemplo
        private static List<ClsModeloPresupuestoContabilidad> ContabilidadFuncPresupuestosDePrueba()
        {
            return new List<ClsModeloPresupuestoContabilidad>
            {
                new ClsModeloPresupuestoContabilidad
                {
                    IdPresupuesto = 1, Nombre = "Presupuesto 2026", FechaInicio = new DateTime(2026, 1, 1), FechaCierre = new DateTime(2026, 12, 31),
                    Partidas =
                    {
                        new ClsModeloPartidaPresupuestoContabilidad { Nombre = "Sueldos de contabilidad", Modulo = "Contabilidad", Cantidad = 30000m },
                        new ClsModeloPartidaPresupuestoContabilidad { Nombre = "Papelería", Modulo = "Contabilidad", Cantidad = 10000m },
                        new ClsModeloPartidaPresupuestoContabilidad { Nombre = "Comisiones", Modulo = "Ventas", Cantidad = 20000m },
                        new ClsModeloPartidaPresupuestoContabilidad { Nombre = "Fletes", Modulo = "Logística", Cantidad = 5000m }
                    }
                },
                new ClsModeloPresupuestoContabilidad
                {
                    IdPresupuesto = 2, Nombre = "Proyecto bodega", FechaInicio = new DateTime(2026, 6, 1), FechaCierre = null,
                    Partidas = { new ClsModeloPartidaPresupuestoContabilidad { Nombre = "Mantenimiento", Modulo = "Compras", Cantidad = 8000m } }
                }
            };
        }

        // Ejecutado de ejemplo por presupuesto
        private static IDictionary<string, decimal> ContabilidadFuncEjecutadoDePrueba(ClsModeloPresupuestoContabilidad Presupuesto, DateTime Hoy)
        {
            if (Presupuesto.IdPresupuesto == 1)
                return new Dictionary<string, decimal> { { "Contabilidad", 10000m }, { "Ventas", 23000m }, { "Logística", 4600m } };
            return new Dictionary<string, decimal> { { "Compras", 2000m } };
        }

        // Filas de ejemplo
        private static DataTable ContabilidadFuncFilasDePrueba()
        {
            return ClsControladorReportePresupuestosContabilidad.ContabilidadFuncArmarFilas(ContabilidadFuncPresupuestosDePrueba(),
                ContabilidadFuncEjecutadoDePrueba, _Hoy);
        }

        // Primera fila de un módulo
        private static DataRow ContabilidadFuncFilaModulo(DataTable Filas, int IdPresupuesto, string Modulo)
        {
            return Filas.Rows.Cast<DataRow>().First(Fila => (int)Fila["IdPresupuesto"] == IdPresupuesto && (string)Fila["Modulo"] == Modulo);
        }

        //  Filtros y fechas
        // ==================================================================

        // Prueba: filtro vigentes no revisa fechas
        [TestMethod]
        public void Test_FiltroVigentesNoRevisaFechas()
        {
            //Arrange: inicializar las variables
            DateTime Desde = new DateTime(2026, 12, 31), Hasta = new DateTime(2026, 1, 1);
            //Act: ejecucion del metodo a probar
            string Error = _Controlador.ContabilidadFuncErrorFiltro(FiltroReportePresupuestoContabilidad.Vigentes, Desde, Hasta);
            //Assert: comprobacion de los resultados
            Assert.AreEqual(string.Empty, Error);
        }

        // Prueba: filtro rango invertido da error
        [TestMethod]
        public void Test_FiltroRangoInvertidoDaError()
        {
            //Arrange: inicializar las variables
            DateTime Desde = new DateTime(2026, 12, 31), Hasta = new DateTime(2026, 1, 1);
            //Act: ejecucion del metodo a probar
            string Error = _Controlador.ContabilidadFuncErrorFiltro(FiltroReportePresupuestoContabilidad.RangoFechas, Desde, Hasta);
            //Assert: comprobacion de los resultados
            Assert.AreEqual("La fecha final no puede ser anterior a la inicial.", Error);
        }

        // Prueba: filtro rango válido y de un día
        [TestMethod]
        public void Test_FiltroRangoValidoYDeUnDia()
        {
            //Arrange: inicializar las variables
            DateTime Dia = new DateTime(2026, 5, 15);
            //Act: ejecucion del metodo a probar
            string ErrorRango = _Controlador.ContabilidadFuncErrorFiltro(FiltroReportePresupuestoContabilidad.RangoFechas, Dia, Dia.AddMonths(2));
            string ErrorDia = _Controlador.ContabilidadFuncErrorFiltro(FiltroReportePresupuestoContabilidad.RangoFechas, Dia, Dia);
            //Assert: comprobacion de los resultados
            Assert.AreEqual(string.Empty, ErrorRango);
            Assert.AreEqual(string.Empty, ErrorDia);
        }

        // Prueba: filtro desconocido da error
        [TestMethod]
        public void Test_FiltroDesconocidoDaError()
        {
            //Arrange: inicializar las variables
            FiltroReportePresupuestoContabilidad Filtro = (FiltroReportePresupuestoContabilidad)9;
            //Act: ejecucion del metodo a probar
            string Error = _Controlador.ContabilidadFuncErrorFiltro(Filtro, _Hoy, _Hoy);
            //Assert: comprobacion de los resultados
            Assert.AreNotEqual(string.Empty, Error);
        }

        // Prueba: filtro con fecha fuera de la base
        [TestMethod]
        public void Test_FiltroConFechaFueraDeLaBase()
        {
            //Arrange: inicializar las variables
            DateTime Desde = new DateTime(900, 1, 1);
            //Act: ejecucion del metodo a probar
            string Error = _Controlador.ContabilidadFuncErrorFiltro(FiltroReportePresupuestoContabilidad.RangoFechas, Desde, _Hoy);
            //Assert: comprobacion de los resultados
            Assert.AreEqual("Las fechas están fuera del rango permitido.", Error);
        }

        // Prueba: descripción del filtro
        [TestMethod]
        public void Test_DescripcionDelFiltro()
        {
            //Arrange: inicializar las variables
            DateTime Desde = new DateTime(2026, 1, 1), Hasta = new DateTime(2026, 6, 30);
            //Act: ejecucion del metodo a probar
            string Vigentes = _Controlador.ContabilidadFuncDescripcionFiltro(FiltroReportePresupuestoContabilidad.Vigentes, Desde, Hasta, _Hoy);
            string Todos = _Controlador.ContabilidadFuncDescripcionFiltro(FiltroReportePresupuestoContabilidad.Todos, Desde, Hasta, _Hoy);
            string Rango = _Controlador.ContabilidadFuncDescripcionFiltro(FiltroReportePresupuestoContabilidad.RangoFechas, Desde, Hasta, _Hoy);
            //Assert: comprobacion de los resultados
            Assert.AreEqual("Presupuestos vigentes al 11/11/2026", Vigentes);
            Assert.AreEqual("Todos los presupuestos activos", Todos);
            Assert.AreEqual("Presupuestos con vigencia entre el 01/01/2026 y el 30/06/2026", Rango);
        }

        // Prueba: reporte con rango invertido no consulta
        [TestMethod]
        public void Test_ReporteConRangoInvertidoNoConsulta()
        {
            //Arrange: inicializar las variables
            DateTime Desde = new DateTime(2026, 12, 31), Hasta = new DateTime(2026, 1, 1);
            //Act: ejecucion del metodo a probar
            ArgumentException Error = Assert.ThrowsException<ArgumentException>(() =>
                _Controlador.ContabilidadFuncReporte(FiltroReportePresupuestoContabilidad.RangoFechas, Desde, Hasta));
            //Assert: comprobacion de los resultados
            Assert.AreEqual("La fecha final no puede ser anterior a la inicial.", Error.Message);
        }

        // Prueba: estado del presupuesto
        [TestMethod]
        public void Test_EstadoDelPresupuesto()
        {
            //Arrange: inicializar las variables
            DateTime Inicio = new DateTime(2026, 1, 1), Cierre = new DateTime(2026, 6, 30);
            //Act: ejecucion del metodo a probar
            string PorIniciar = ClsControladorReportePresupuestosContabilidad.ContabilidadFuncEstado(Inicio, Cierre, new DateTime(2025, 12, 31));
            string Vigente = ClsControladorReportePresupuestosContabilidad.ContabilidadFuncEstado(Inicio, Cierre, Cierre);
            string Cerrado = ClsControladorReportePresupuestosContabilidad.ContabilidadFuncEstado(Inicio, Cierre, Cierre.AddDays(1));
            string Abierto = ClsControladorReportePresupuestosContabilidad.ContabilidadFuncEstado(Inicio, null, _Hoy);
            //Assert: comprobacion de los resultados
            Assert.AreEqual("Por iniciar", PorIniciar);
            Assert.AreEqual("Vigente", Vigente);
            Assert.AreEqual("Cerrado", Cerrado);
            Assert.AreEqual("Vigente", Abierto);
        }

        // Prueba: vigencia y corte de la ejecución
        [TestMethod]
        public void Test_VigenciaYCorteDeLaEjecucion()
        {
            //Arrange: inicializar las variables
            DateTime Inicio = new DateTime(2026, 1, 1), Cierre = new DateTime(2026, 6, 30);
            //Act: ejecucion del metodo a probar
            string ConCierre = ClsControladorReportePresupuestosContabilidad.ContabilidadFuncVigencia(Inicio, Cierre);
            string SinCierre = ClsControladorReportePresupuestosContabilidad.ContabilidadFuncVigencia(Inicio, null);
            DateTime CorteCerrado = ClsControladorReportePresupuestosContabilidad.ContabilidadFuncCorte(Cierre, _Hoy);
            DateTime CorteAbierto = ClsControladorReportePresupuestosContabilidad.ContabilidadFuncCorte(null, _Hoy);
            //Assert: comprobacion de los resultados
            Assert.AreEqual("01/01/2026 al 30/06/2026", ConCierre);
            Assert.AreEqual("Desde 01/01/2026, sin cierre", SinCierre);
            Assert.AreEqual(Cierre, CorteCerrado);
            Assert.AreEqual(_Hoy, CorteAbierto);
        }

        //  Disponible y porcentaje de ejecución
        // ==================================================================

        // Prueba: porcentaje de ejecución
        [TestMethod]
        public void Test_PorcentajeDeEjecucion()
        {
            //Arrange: inicializar las variables
            decimal Asignado = 40000m, Ejecutado = 10000m;
            //Act: ejecucion del metodo a probar
            decimal Porcentaje = ClsControladorReportePresupuestosContabilidad.ContabilidadFuncPorcentaje(Asignado, Ejecutado);
            decimal SinAsignado = ClsControladorReportePresupuestosContabilidad.ContabilidadFuncPorcentaje(0m, Ejecutado);
            //Assert: comprobacion de los resultados
            Assert.AreEqual(0.25m, Porcentaje);
            Assert.AreEqual(0m, SinAsignado);
        }

        // Prueba: alerta según la ejecución
        [TestMethod]
        public void Test_AlertaSegunLaEjecucion()
        {
            //Arrange: inicializar las variables
            decimal Asignado = 1000m;
            //Act: ejecucion del metodo a probar
            string Sobregiro = ClsControladorReportePresupuestosContabilidad.ContabilidadFuncAlerta(Asignado, 1000.01m);
            string Limite = ClsControladorReportePresupuestosContabilidad.ContabilidadFuncAlerta(Asignado, 900m);
            string Exacto = ClsControladorReportePresupuestosContabilidad.ContabilidadFuncAlerta(Asignado, 1000m);
            string EnRango = ClsControladorReportePresupuestosContabilidad.ContabilidadFuncAlerta(Asignado, 899.99m);
            //Assert: comprobacion de los resultados
            Assert.AreEqual(ClsControladorReportePresupuestosContabilidad.AlertaSobregiro, Sobregiro);
            Assert.AreEqual(ClsControladorReportePresupuestosContabilidad.AlertaLimite, Limite);
            Assert.AreEqual(ClsControladorReportePresupuestosContabilidad.AlertaLimite, Exacto);
            Assert.AreEqual(string.Empty, EnRango);
        }

        // Prueba: armar filas una por partida
        [TestMethod]
        public void Test_ArmarFilasUnaPorPartida()
        {
            //Arrange: inicializar las variables
            List<ClsModeloPresupuestoContabilidad> Presupuestos = ContabilidadFuncPresupuestosDePrueba();
            //Act: ejecucion del metodo a probar
            DataTable Filas = ContabilidadFuncFilasDePrueba();
            //Assert: comprobacion de los resultados
            Assert.AreEqual(Presupuestos.Sum(Presupuesto => Presupuesto.Partidas.Count), Filas.Rows.Count);
            Assert.AreEqual(21, Filas.Columns.Count);
            CollectionAssert.AreEqual(new[] { "Contabilidad", "Contabilidad", "Logística", "Ventas", "Compras" },
                Filas.Rows.Cast<DataRow>().Select(Fila => (string)Fila["Modulo"]).ToArray());
        }

        // Prueba: armar filas calcula el módulo
        [TestMethod]
        public void Test_ArmarFilasCalculaElModulo()
        {
            //Arrange: inicializar las variables
            DataTable Filas = ContabilidadFuncFilasDePrueba();
            //Act: ejecucion del metodo a probar
            DataRow Contabilidad = ContabilidadFuncFilaModulo(Filas, 1, "Contabilidad");
            DataRow Ventas = ContabilidadFuncFilaModulo(Filas, 1, "Ventas");
            DataRow Logistica = ContabilidadFuncFilaModulo(Filas, 1, "Logística");
            //Assert: comprobacion de los resultados
            Assert.AreEqual(40000m, Contabilidad["AsignadoModulo"]);
            Assert.AreEqual(10000m, Contabilidad["EjecutadoModulo"]);
            Assert.AreEqual(30000m, Contabilidad["DisponibleModulo"]);
            Assert.AreEqual(0.25m, Contabilidad["PorcentajeModulo"]);
            Assert.AreEqual(string.Empty, Contabilidad["AlertaModulo"]);
            Assert.AreEqual(-3000m, Ventas["DisponibleModulo"]);
            Assert.AreEqual(1.15m, Ventas["PorcentajeModulo"]);
            Assert.AreEqual("Sobregirado", Ventas["AlertaModulo"]);
            Assert.AreEqual(0.92m, Logistica["PorcentajeModulo"]);
            Assert.AreEqual("Al límite", Logistica["AlertaModulo"]);
        }

        // Prueba: armar filas totales por presupuesto y generales
        [TestMethod]
        public void Test_ArmarFilasTotalesPorPresupuestoYGenerales()
        {
            //Arrange: inicializar las variables
            DataTable Filas = ContabilidadFuncFilasDePrueba();
            //Act: ejecucion del metodo a probar
            DataRow Anual = ContabilidadFuncFilaModulo(Filas, 1, "Ventas");
            DataRow Bodega = ContabilidadFuncFilaModulo(Filas, 2, "Compras");
            //Assert: comprobacion de los resultados
            Assert.AreEqual(65000m, Anual["AsignadoPresupuesto"]);
            Assert.AreEqual(37600m, Anual["EjecutadoPresupuesto"]);
            Assert.AreEqual(27400m, Anual["DisponiblePresupuesto"]);
            Assert.AreEqual(0.5785m, Anual["PorcentajePresupuesto"]);
            Assert.AreEqual(8000m, Bodega["AsignadoPresupuesto"]);
            Assert.AreEqual(6000m, Bodega["DisponiblePresupuesto"]);
            Assert.AreEqual("Vigente", Bodega["Estado"]);
            Assert.AreEqual("Desde 01/06/2026, sin cierre", Bodega["Vigencia"]);
            foreach (DataRow Fila in Filas.Rows)
            {
                Assert.AreEqual(73000m, Fila["AsignadoGeneral"]);
                Assert.AreEqual(39600m, Fila["EjecutadoGeneral"]);
                Assert.AreEqual(33400m, Fila["DisponibleGeneral"]);
                Assert.AreEqual(0.5425m, Fila["PorcentajeGeneral"]);
            }
        }

        // Prueba: armar filas sin ejecutado ni partidas
        [TestMethod]
        public void Test_ArmarFilasSinEjecutadoNiPartidas()
        {
            //Arrange: inicializar las variables
            List<ClsModeloPresupuestoContabilidad> Presupuestos = ContabilidadFuncPresupuestosDePrueba();
            Presupuestos.Add(new ClsModeloPresupuestoContabilidad { IdPresupuesto = 3, Nombre = "Vacío", FechaInicio = _Hoy });
            //Act: ejecucion del metodo a probar
            DataTable Filas = ClsControladorReportePresupuestosContabilidad.ContabilidadFuncArmarFilas(Presupuestos, (Presupuesto, Hoy) => null, _Hoy);
            //Assert: comprobacion de los resultados
            Assert.IsFalse(Filas.Rows.Cast<DataRow>().Any(Fila => (int)Fila["IdPresupuesto"] == 3));
            Assert.IsTrue(Filas.Rows.Cast<DataRow>().All(Fila => (decimal)Fila["EjecutadoModulo"] == 0m));
            Assert.AreEqual(65000m, ContabilidadFuncFilaModulo(Filas, 1, "Ventas")["DisponiblePresupuesto"]);
        }

        // Prueba: resumen del reporte
        [TestMethod]
        public void Test_ResumenDelReporte()
        {
            //Arrange: inicializar las variables
            DataTable Filas = ContabilidadFuncFilasDePrueba();
            //Act: ejecucion del metodo a probar
            string Resumen = _Controlador.ContabilidadFuncResumen(Filas);
            string Vacio = _Controlador.ContabilidadFuncResumen(ClsControladorReportePresupuestosContabilidad.ContabilidadFuncCrearTabla());
            //Assert: comprobacion de los resultados
            StringAssert.StartsWith(Resumen, "2 presupuestos, 5 partidas.");
            StringAssert.EndsWith(Resumen, "1 módulo sobregirado.");
            Assert.AreEqual("No hay presupuestos para el filtro elegido.", Vacio);
        }

        //  Consulta contra la base
        // ==================================================================

        // Prueba: consulta todos contra la base
        [TestMethod]
        public void Test_ConsultaTodosContraLaBase()
        {
            //Arrange: inicializar las variables
            ClsRepositorioReportePresupuestosContabilidad Repositorio = new ClsRepositorioReportePresupuestosContabilidad();
            int Esperados;
            using (DataTable Conteo = Repositorio.ContabilidadFuncEjecutarConsulta(
                "SELECT COUNT(DISTINCT e.idPresupuestoEncabezado) FROM tblPresupuestoEncabezado e INNER JOIN tblPresupuestoDetalle d " +
                "ON d.idPresupuestoEncabezado = e.idPresupuestoEncabezado AND d.is_active = TRUE WHERE e.is_active = TRUE", CommandType.Text))
                Esperados = Convert.ToInt32(Conteo.Rows[0][0]);
            //Act: ejecucion del metodo a probar
            List<ClsPresupuestoContabilidad> Presupuestos = Repositorio.ContabilidadFuncObtenerPresupuestos(null, null);
            //Assert: comprobacion de los resultados
            Assert.AreEqual(Esperados, Presupuestos.Count);
            Assert.AreEqual(Presupuestos.Count, Presupuestos.Select(Presupuesto => Presupuesto.IdPresupuesto).Distinct().Count());
            foreach (ClsPresupuestoContabilidad Presupuesto in Presupuestos)
            {
                Assert.IsTrue(Presupuesto.Detalle.Count > 0);
                Assert.AreEqual(Presupuesto.Detalle.Sum(Partida => Partida.Cantidad), Presupuesto.Total);
                Assert.IsTrue(Presupuesto.Detalle.All(Partida => !string.IsNullOrWhiteSpace(Partida.Modulo)));
            }
        }

        // Prueba: consulta filtra por vigencia
        [TestMethod]
        public void Test_ConsultaFiltraPorVigencia()
        {
            //Arrange: inicializar las variables
            ClsRepositorioReportePresupuestosContabilidad Repositorio = new ClsRepositorioReportePresupuestosContabilidad();
            DateTime Desde = new DateTime(DateTime.Today.Year, 1, 1), Hasta = new DateTime(DateTime.Today.Year, 12, 31);
            //Act: ejecucion del metodo a probar
            List<ClsPresupuestoContabilidad> EnElAnio = Repositorio.ContabilidadFuncObtenerPresupuestos(Desde, Hasta);
            List<ClsPresupuestoContabilidad> MuyAntiguos = Repositorio.ContabilidadFuncObtenerPresupuestos(new DateTime(1001, 1, 1), new DateTime(1001, 12, 31));
            //Assert: comprobacion de los resultados
            Assert.AreEqual(0, MuyAntiguos.Count);
            foreach (ClsPresupuestoContabilidad Presupuesto in EnElAnio)
            {
                Assert.IsTrue(Presupuesto.FechaInicio.Date <= Hasta);
                Assert.IsTrue(!Presupuesto.FechaCierre.HasValue || Presupuesto.FechaCierre.Value.Date >= Desde);
            }
        }

        // Prueba: reporte de la base cuadra con la pantalla
        [TestMethod]
        public void Test_ReporteDeLaBaseCuadraConLaPantalla()
        {
            //Arrange: inicializar las variables
            ClsControladorPresupuestosContabilidad Pantalla = new ClsControladorPresupuestosContabilidad();
            List<ClsModeloPresupuestoContabilidad> Presupuestos = Pantalla.ContabilidadFuncPresupuestos().Where(Presupuesto => Presupuesto.Partidas.Count > 0).ToList();
            //Act: ejecucion del metodo a probar
            using (DataTable Filas = _Controlador.ContabilidadFuncReporte(FiltroReportePresupuestoContabilidad.Todos, DateTime.Today, DateTime.Today))
            {
                //Assert: comprobacion de los resultados
                Assert.AreEqual(Presupuestos.Sum(Presupuesto => Presupuesto.Partidas.Count), Filas.Rows.Count);
                foreach (ClsModeloPresupuestoContabilidad Presupuesto in Presupuestos)
                {
                    DataRow Fila = Filas.Rows.Cast<DataRow>().First(Elemento => (int)Elemento["IdPresupuesto"] == Presupuesto.IdPresupuesto);
                    using (DataTable Detalle = Pantalla.ContabilidadFuncPresupuesto(Presupuesto))
                    using (DataTable Ejecucion = Pantalla.ContabilidadFuncEjecucion(Presupuesto.FechaInicio, Presupuesto.FechaCierre, Detalle))
                    {
                        Assert.AreEqual(Pantalla.ContabilidadFuncTotalPresupuesto(Detalle), (decimal)Fila["AsignadoPresupuesto"]);
                        Assert.AreEqual(Pantalla.ContabilidadFuncTotalEjecutado(Ejecucion), (decimal)Fila["EjecutadoPresupuesto"]);
                    }
                }
            }
        }

        // Prueba: reporte vigentes de la base
        [TestMethod]
        public void Test_ReporteVigentesDeLaBase()
        {
            //Arrange: inicializar las variables
            FiltroReportePresupuestoContabilidad Filtro = FiltroReportePresupuestoContabilidad.Vigentes;
            //Act: ejecucion del metodo a probar
            using (DataTable Filas = _Controlador.ContabilidadFuncReporte(Filtro, DateTime.Today, DateTime.Today))
            {
                //Assert: comprobacion de los resultados
                Assert.IsTrue(Filas.Rows.Cast<DataRow>().All(Fila => (string)Fila["Estado"] == "Vigente"));
                Assert.IsTrue(Filas.Rows.Cast<DataRow>().All(Fila =>
                    (decimal)Fila["DisponibleModulo"] == (decimal)Fila["AsignadoModulo"] - (decimal)Fila["EjecutadoModulo"]));
            }
        }

        //  Ventana del reporte y enlace con el MDI
        // ==================================================================

        // Prueba: crear reporte ya no devuelve nulo
        [TestMethod]
        public void Test_CrearReporteYaNoDevuelveNulo()
        {
            //Arrange: inicializar las variables
            ClsModeloPermisosPantallaContabilidad Permisos = ClsModeloPermisosPantallaContabilidad.Total;
            //Act: ejecucion del metodo a probar
            ContabilidadMetEnVentana(() => ClsReportePresupuestosContabilidad.ContabilidadFuncCrearReporte(Permisos), Ventana =>
            {
                //Assert: comprobacion de los resultados
                Assert.IsNotNull(Ventana);
                Assert.IsInstanceOfType(Ventana, typeof(FrmReportePresupuestos));
            });
        }

        // Prueba: reporte prepara la ventana
        [TestMethod]
        public void Test_ReportePreparaLaVentana()
        {
            //Arrange: inicializar las variables
            Func<Form> Crear = () => new FrmReportePresupuestos();
            //Act: ejecucion del metodo a probar
            ContabilidadMetEnVentana(Crear, Ventana =>
            {
                ComboBox Filtro = ContabilidadFuncControl<ComboBox>(Ventana, "ContabilidadCboFiltro");
                //Assert: comprobacion de los resultados
                Assert.AreEqual("10002 - ReportePresupuestos", Ventana.Text);
                Assert.AreEqual(3, Filtro.Items.Count);
                Assert.AreEqual(0, Filtro.SelectedIndex);
                Assert.IsNotNull(ContabilidadFuncControl<ReportViewer>(Ventana, "ContabilidadRpvReporte"));
                Assert.IsNotNull(ContabilidadFuncControl<Button>(Ventana, "ContabilidadBtnGenerar"));
                Assert.IsNotNull(ContabilidadFuncControl<Button>(Ventana, "ContabilidadBtnSalir"));
            });
        }

        // Prueba: reporte habilita fechas solo en rango
        [TestMethod]
        public void Test_ReporteHabilitaFechasSoloEnRango()
        {
            //Arrange: inicializar las variables
            Func<Form> Crear = () => new FrmReportePresupuestos();
            //Act: ejecucion del metodo a probar
            ContabilidadMetEnVentana(Crear, Ventana =>
            {
                DateTimePicker Desde = ContabilidadFuncControl<DateTimePicker>(Ventana, "ContabilidadDtpDesde");
                DateTimePicker Hasta = ContabilidadFuncControl<DateTimePicker>(Ventana, "ContabilidadDtpHasta");
                bool AntesDesde = Desde.Enabled, AntesHasta = Hasta.Enabled;
                ContabilidadFuncControl<ComboBox>(Ventana, "ContabilidadCboFiltro").SelectedIndex = 2;
                //Assert: comprobacion de los resultados
                Assert.IsFalse(AntesDesde);
                Assert.IsFalse(AntesHasta);
                Assert.IsTrue(Desde.Enabled);
                Assert.IsTrue(Hasta.Enabled);
                Assert.AreEqual(FiltroReportePresupuestoContabilidad.RangoFechas, ((FrmReportePresupuestos)Ventana).Filtro);
            });
        }

        // Prueba: reporte sin permiso de imprimir
        [TestMethod]
        public void Test_ReporteSinPermisoDeImprimir()
        {
            //Arrange: inicializar las variables
            ClsModeloPermisosPantallaContabilidad SinImprimir = ClsModeloPermisosPantallaContabilidad.Total;
            SinImprimir.PuedeImprimir = false;
            //Act: ejecucion del metodo a probar
            ContabilidadMetEnVentana(() => new FrmReportePresupuestos(SinImprimir), Ventana =>
            {
                ReportViewer Visor = ContabilidadFuncControl<ReportViewer>(Ventana, "ContabilidadRpvReporte");
                //Assert: comprobacion de los resultados
                Assert.IsFalse(Visor.ShowPrintButton);
                Assert.IsFalse(Visor.ShowExportButton);
            });
            ContabilidadMetEnVentana(() => new FrmReportePresupuestos(ClsModeloPermisosPantallaContabilidad.Total), Ventana =>
            {
                ReportViewer Visor = ContabilidadFuncControl<ReportViewer>(Ventana, "ContabilidadRpvReporte");
                Assert.IsTrue(Visor.ShowPrintButton);
                Assert.IsTrue(Visor.ShowExportButton);
            });
        }

        // Prueba: reporte usa íconos y centrado
        [TestMethod]
        public void Test_ReporteUsaIconosYCentrado()
        {
            //Arrange: inicializar las variables
            Func<Form> Crear = () => new FrmReportePresupuestos();
            //Act: ejecucion del metodo a probar
            ContabilidadMetEnVentana(Crear, Ventana =>
            {
                Button Generar = ContabilidadFuncControl<Button>(Ventana, "ContabilidadBtnGenerar");
                Button Salir = ContabilidadFuncControl<Button>(Ventana, "ContabilidadBtnSalir");
                //Assert: comprobacion de los resultados
                Assert.AreEqual(FormStartPosition.CenterScreen, Ventana.StartPosition);
                foreach (Button Boton in new[] { Generar, Salir })
                {
                    Assert.AreEqual(string.Empty, Boton.Text);
                    Assert.IsNotNull(Boton.GetType().GetProperty("Icono").GetValue(Boton));
                }
            });
        }

        //  RDLC
        // ==================================================================

        // Prueba: RDLC esta embebido
        [TestMethod]
        public void Test_RdlcEstaEmbebido()
        {
            //Arrange: inicializar las variables
            System.Reflection.Assembly Ensamblado = typeof(FrmReportePresupuestos).Assembly;
            //Act: ejecucion del metodo a probar
            string[] Recursos = Ensamblado.GetManifestResourceNames();
            //Assert: comprobacion de los resultados
            CollectionAssert.Contains(Recursos, FrmReportePresupuestos.RecursoReporte);
        }

        // Prueba: RDLC se renderiza a PDF
        [TestMethod]
        public void Test_RdlcSeRenderizaAPdf()
        {
            //Arrange: inicializar las variables
            LocalReport Reporte = new LocalReport();
            DataTable Filas = ContabilidadFuncFilasDePrueba();
            //Act: ejecucion del metodo a probar
            FrmReportePresupuestos.ContabilidadMetPrepararReporte(Reporte, Filas, "Presupuestos vigentes al 11/11/2026");
            byte[] Pdf = Reporte.Render("PDF");
            //Assert: comprobacion de los resultados
            Assert.IsTrue(Pdf.Length > 1000);
            Assert.AreEqual("%PDF", Encoding.ASCII.GetString(Pdf, 0, 4));
            Reporte.Dispose();
            Filas.Dispose();
        }

        // Prueba: RDLC sin datos se renderiza
        [TestMethod]
        public void Test_RdlcSinDatosSeRenderiza()
        {
            //Arrange: inicializar las variables
            LocalReport Reporte = new LocalReport();
            DataTable Filas = ClsControladorReportePresupuestosContabilidad.ContabilidadFuncCrearTabla();
            //Act: ejecucion del metodo a probar
            FrmReportePresupuestos.ContabilidadMetPrepararReporte(Reporte, Filas, "Todos los presupuestos activos");
            byte[] Pdf = Reporte.Render("PDF");
            //Assert: comprobacion de los resultados
            Assert.AreEqual("%PDF", Encoding.ASCII.GetString(Pdf, 0, 4));
            Reporte.Dispose();
            Filas.Dispose();
        }
    }
}
