using System;
using MySql.Data.MySqlClient;

namespace Northwind.Catalogo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("============================================================");
            Console.WriteLine("NORTHWIND - CATÁLOGO DE PRODUCTOS");
            Console.WriteLine("============================================================");
            Console.WriteLine();
            Console.WriteLine("--- Comprobación inicial de conexión y cantidad de productos ---");

            // Completa los datos de tu servidor MySQL antes de ejecutar.
            string cadenaConexion =
                "Server=localhost;Port=3306;Database=northwind;" +
                "User ID=root;Password=;SslMode=Required;";

            try
            {
                // Comprobación inicial: conectar y recuperar un solo valor.
                using (MySqlConnection conexion = new(cadenaConexion))
                {
                    conexion.Open();
                    Console.WriteLine("Conexión abierta correctamente.");

                    string consulta = "SELECT COUNT(*) FROM Products;";
                    using MySqlCommand comando = new(consulta, conexion);
                    long cantidad = Convert.ToInt64(comando.ExecuteScalar());
                    Console.WriteLine("Cantidad de productos: " + cantidad);
                }
                using (MySqlConnection conexionBusqueda = new MySqlConnection(cadenaConexion))
                {
                    conexionBusqueda.Open();

                    BusquedaProductos busqueda = new BusquedaProductos();

                    decimal precioMaximo = busqueda.SolicitarPrecioMaximo();

                    int existenciasMinimas = busqueda.SolicitarExistenciasMinimas();

                    busqueda.BuscarPorPrecioYExistencias(
                        conexionBusqueda,
                        precioMaximo,
                        existenciasMinimas);
                    string nombreBuscado = busqueda.SolicitarNombre();

                    busqueda.BuscarPorNombre(
                        conexionBusqueda,
                        nombreBuscado);
                }

                // Los ejemplos de cada aspecto se desarrollan en el otro archivo.
                AspectosAdoNet.Ejecutar(cadenaConexion);

            }
            catch (ArgumentException)
            {
                Console.WriteLine("La cadena de conexión tiene un formato u opción no válida. Revisa cadenaConexion en Program.cs.");
            }
            catch (MySqlException error)
            {
                Console.WriteLine("Error de MySQL: " + error.Message);
            }
        }

    }
}
