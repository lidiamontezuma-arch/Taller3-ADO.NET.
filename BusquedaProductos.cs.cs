
using MySql.Data.MySqlClient;
using System;

namespace Northwind.Catalogo
{
    internal class BusquedaProductos
    {
        public void BuscarPorPrecioYExistencias(
            MySqlConnection conexion,
            decimal precioMaximo,
            int existenciasMinimas)
        {
            if (conexion.State != System.Data.ConnectionState.Open)
            {
                Console.WriteLine("La conexión debe estar abierta.");
                return;
            }

            string consulta = @"
                SELECT ProductID, ProductName, UnitPrice,
                       UnitsInStock, QuantityPerUnit
                FROM Products
                WHERE UnitPrice <= @precioMaximo
                  AND UnitsInStock >= @existenciasMinimas
                  AND Discontinued = 0
                ORDER BY UnitPrice ASC, ProductID ASC;";

            using MySqlCommand comando = new MySqlCommand(consulta, conexion);

            comando.Parameters.AddWithValue("@precioMaximo", precioMaximo);
            comando.Parameters.AddWithValue("@existenciasMinimas", existenciasMinimas);

            int cantidad = 0;

            using (MySqlDataReader lector = comando.ExecuteReader())
            {
                if (!lector.HasRows)
                {
                    Console.WriteLine("No se encontraron productos con esos criterios.");
                }
                else
                {
                    Console.WriteLine("\nPRODUCTOS ENCONTRADOS");
                    Console.WriteLine("----------------------------------------------");

                    while (lector.Read())
                    {
                        int id = lector.GetInt32("ProductID");
                        string nombre = lector.GetString("ProductName");
                        decimal precio = lector.GetDecimal("UnitPrice");
                        int existencias = lector.GetInt32("UnitsInStock");

                        string presentacion = lector.IsDBNull(
                            lector.GetOrdinal("QuantityPerUnit"))
                            ? "Presentación sin registrar"
                            : lector.GetString("QuantityPerUnit");

                        Console.WriteLine($"ID: {id}");
                        Console.WriteLine($"Producto: {nombre}");
                        Console.WriteLine($"Precio: {precio:F2}");
                        Console.WriteLine($"Existencias: {existencias}");
                        Console.WriteLine($"Presentación: {presentacion}");
                        Console.WriteLine("----------------------------------------------");

                        cantidad++;
                    }
                }
            }

            Console.WriteLine($"Total de productos encontrados: {cantidad}");
            string consultaConteo = @"
            SELECT COUNT(*)
            FROM Products
            WHERE UnitPrice <= @precioMaximo
            AND UnitsInStock >= @existenciasMinimas
            AND Discontinued = 0;";

            using MySqlCommand comandoConteo =
                new MySqlCommand(consultaConteo, conexion);

            comandoConteo.Parameters.AddWithValue("@precioMaximo", precioMaximo);
            comandoConteo.Parameters.AddWithValue("@existenciasMinimas", existenciasMinimas);

            int total = Convert.ToInt32(comandoConteo.ExecuteScalar());

            Console.WriteLine($"Conteo realizado en MySQL: {total}");

        }

        public decimal SolicitarPrecioMaximo()
        {
            decimal precio;

            while (true)
            {
                Console.Write("Ingrese el precio máximo: ");
                string entrada = Console.ReadLine() ?? "";

                if (decimal.TryParse(entrada, out precio) && precio >= 0)
                {
                    return precio;
                }

                Console.WriteLine("Error: ingrese un precio válido y no negativo.");
            }
        }

        public int SolicitarExistenciasMinimas()
        {
            int existencias;

            while (true)
            {
                Console.Write("Ingrese las existencias mínimas: ");
                string entrada = Console.ReadLine() ?? "";

                if (int.TryParse(entrada, out existencias) && existencias >= 0)
                {
                    return existencias;
                }

                Console.WriteLine("Error: ingrese un número entero no negativo.");
            }
        }
        public void BuscarPorNombre(MySqlConnection conexion, string nombreBuscado)
        {
            if (conexion.State != System.Data.ConnectionState.Open)
            {
                Console.WriteLine("La conexión debe estar abierta.");
                return;
            }

            string consulta = @"
            SELECT ProductID, ProductName, UnitPrice, UnitsInStock
            FROM Products
            WHERE ProductName LIKE @nombre
            AND Discontinued = 0
            ORDER BY ProductName ASC, ProductID ASC;";

            using MySqlCommand comando = new MySqlCommand(consulta, conexion);

            comando.Parameters.AddWithValue("@nombre", "%" + nombreBuscado + "%");

            using MySqlDataReader lector = comando.ExecuteReader();

            if (!lector.HasRows)
            {
                Console.WriteLine("No se encontraron productos con ese nombre.");
                return;
            }

            Console.WriteLine("\nPRODUCTOS ENCONTRADOS POR NOMBRE");

            while (lector.Read())
            {
                Console.WriteLine($"ID: {lector.GetInt32("ProductID")}");
                Console.WriteLine($"Producto: {lector.GetString("ProductName")}");
                Console.WriteLine($"Precio: {lector.GetDecimal("UnitPrice"):F2}");
                Console.WriteLine($"Existencias: {lector.GetInt32("UnitsInStock")}");
                Console.WriteLine("--------------------------------");

            }
         
        }
        
        public string SolicitarNombre()
        {
            while (true)
            {
                Console.Write("Ingrese el nombre o una parte del nombre del producto: ");
                string nombre = Console.ReadLine() ?? "";

                nombre = nombre.Trim();

                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    return nombre;
                }

                Console.WriteLine("Error: debe ingresar un nombre válido.");
            }
        }
    }
}



