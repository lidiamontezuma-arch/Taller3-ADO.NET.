using System;
using MySql.Data.MySqlClient;

namespace Northwind.Catalogo
{
    /*
     * TUTORIAL: CONSULTAS CON ADO.NET
     *
     * El recorrido comprende los siguientes métodos, en este orden:
     * 1. MostrarRecursos: apertura, consulta y liberación de recursos.
     * 2. ListarProductos y MostrarParametros: lectura de filas y búsqueda por ID.
     * 3. MostrarTiposYNulos: diferencia entre cero, NULL y ausencia de filas.
     * 4. MostrarErrorConsulta, MostrarErrorConexion y MostrarErrorRestriccion.
     * 5. MostrarTransacciones y sus métodos de apoyo.
     *
     * Ejecuta cada ejemplo, cambia los valores propuestos y compara los resultados.
     * Después de cada cambio, busca qué instrucción explica el resultado.
     * El recorrido completo solicita un ID para realizar la búsqueda.
     * Los ejemplos de restricciones y transacciones requieren permisos para
     * crear tablas temporales. Sus datos desaparecen al cerrar sus sesiones.
     *
     * Las consultas que construirás se describen en el README.md de este proyecto.
     */
    internal class AspectosAdoNet
    {
        public static void Ejecutar(string cadenaConexion)
        {
            MostrarRecursos(cadenaConexion);

            {
                using MySqlConnection conexion = new(cadenaConexion);
                conexion.Open();
                Console.WriteLine("Conexión abierta correctamente.");

                ListarProductos(conexion);
                MostrarParametros(conexion);
                MostrarTiposYNulos(conexion);
                MostrarErrorConsulta(conexion);
            }

            MostrarErrorConexion();
            MostrarErrorRestriccion(cadenaConexion);
            MostrarTransacciones(cadenaConexion);

            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("FIN DEL TUTORIAL ADO.NET");
            Console.WriteLine("============================================================");
        }

        /*
         * 1. RECURSOS
         * Open establece la conexión. ExecuteReader abre un lector de resultados.
         * Cada declaración using llama Dispose al salir del bloque que la contiene,
         * incluso por return o excepción. Se liberan en orden inverso a su declaración.
         * Liberar un comando no cierra su conexión. El pool es una reserva de
         * conexiones físicas reutilizables; liberar una conexión no apaga MySQL.
         * Cambia LIMIT 2 por LIMIT 5. ¿Cuántos nombres aparecen ahora?
         * Prueba una segunda consulta después de liberar el primer lector.
         * Observa State antes de Open, después de Open y al liberar el lector.
         * ¿Liberar el lector cierra también la conexión?
         * Las llaves delimitan la vida de los recursos. Compara estas declaraciones
         * con el using que importa namespaces al inicio: ¿cumplen la misma función?
         */
        // Caso de uso: consultar dos productos y liberar los recursos al terminar.
        // Este método crea su propia conexión y se encarga de liberarla.
        public static void MostrarRecursos(string cadenaConexion)
        {
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("1. RECURSOS: ABRIR, CONSULTAR Y LIBERAR CON USING");
            Console.WriteLine("============================================================");

            {
                using MySqlConnection conexion = new(cadenaConexion);
                Console.WriteLine("Antes de Open: " + conexion.State);
                conexion.Open();
                Console.WriteLine("Después de Open: " + conexion.State);

                string consulta =
                    "SELECT ProductName FROM Products ORDER BY ProductID LIMIT 5;";

                {
                    using MySqlCommand comando = new(consulta, conexion);
                    {
                        using MySqlDataReader lector = comando.ExecuteReader();
                        while (lector.Read())
                        {
                            Console.WriteLine(lector.GetString(0));
                        }
                    } // Dispose del lector: deja de ocupar la conexión con resultados.

                    Console.WriteLine("Lector liberado; conexión: " + conexion.State);
                    // Segunda consulta después de liberar el primer lector.
                    string segundaConsulta = "SELECT COUNT(*) FROM Products;";

                    using MySqlCommand segundoComando = new(segundaConsulta, conexion);

                    object? cantidad = segundoComando.ExecuteScalar();

                    Console.WriteLine("Segunda consulta: cantidad de productos = " + cantidad);
                } // Dispose del comando. La conexión continúa abierta.
            } // Dispose de la conexión: devuelve la conexión física al pool si está habilitado.

            Console.WriteLine("Conexión liberada; terminó la consulta.");
            // using también libera los recursos si hay un return o una excepción.
            // No captura la excepción: el catch de Main puede manejarla.
        }

        /*
         * 2. PARÁMETROS Y RESULTADOS
         * MySqlCommand une una instrucción SQL con la conexión que la ejecutará.
         * @id es un marcador de valor SQL; Parameters asigna su tipo y valor.
         * Los parámetros separan datos de instrucciones sin concatenar la entrada
         * en el SQL; la validación comprueba las reglas de entrada.
         * ExecuteScalar recupera la primera columna de la primera fila.
         * ExecuteReader permite recorrer filas, como en ListarProductos.
         * Prueba las entradas 1, 99999, 0 y texto. Observa qué muestra cada una.
         * ¿Cuáles llegan a ejecutar SQL y cuáles se rechazan antes?
         */
        // Caso de uso: consultar el nombre de un producto por el ID ingresado.
        public static void MostrarParametros(MySqlConnection conexion)
        {
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("2.2. PARÁMETROS: BÚSQUEDA POR IDENTIFICADOR");
            Console.WriteLine("============================================================");
            Console.Write("Ingresa el ID del producto: ");
            string? entrada = Console.ReadLine();

            if (!int.TryParse(entrada, out int idProducto) || idProducto <= 0)
            {
                Console.WriteLine("El ID debe ser un número entero mayor que cero.");
                return;
            }

            // El SQL mantiene su estructura; el valor se asigna por separado.
            string consulta =
                "SELECT ProductName FROM Products WHERE ProductID = @id;";

            {
                using MySqlCommand comando = new(consulta, conexion);
                // @id debe coincidir con el marcador del SQL.
                // Int32 es el tipo del parámetro que enviamos a MySQL.
                comando.Parameters.Add("@id", MySqlDbType.Int32).Value = idProducto;

                // ProductID es único: esperamos un nombre o ninguna fila.
                object? resultado = comando.ExecuteScalar();

                if (resultado == null)
                {
                    Console.WriteLine("No existe un producto con ese ID.");
                }
                else
                {
                    Console.WriteLine("Producto encontrado: " + resultado);
                }
            }
        }

        static void ListarProductos(MySqlConnection conexion)
        {
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("2.1. RESULTADOS: LISTADO DE PRODUCTOS");
            Console.WriteLine("============================================================");

            string consulta =
                "SELECT ProductID, ProductName FROM Products " +
                "ORDER BY ProductID LIMIT 10;";

            {
                using MySqlCommand comando = new(consulta, conexion);
                {
                    using MySqlDataReader lector = comando.ExecuteReader();
                    Console.WriteLine("Primeros diez productos:");

                    // Read avanza a la siguiente fila; devuelve false al terminar.
                    while (lector.Read())
                    {
                        int id = lector.GetInt32(0);
                        string nombre = lector.GetString(1);
                        Console.WriteLine(id + " - " + nombre);
                    }
                }
            }
        }

        /*
         * 3. TIPOS Y NULL
         * GetString lee texto y GetDecimal lee un decimal. GetOrdinal localiza
         * una columna por nombre. IsDBNull comprueba NULL antes de leer el valor.
         * NULL no equivale a cero ni a una cadena vacía. Para representar un
         * decimal opcional en una variable C#, puede utilizarse decimal?.
         * Cambia el valor del parámetro @id de 5 a 1 y después a 99999.
         * Conserva las dos filas simuladas y compara las salidas: ¿cuál desaparece?
         * ¿Por qué sustituir un precio ausente por cero cambiaría su significado?
         */
        // UnitPrice es NOT NULL en esta adaptación de Northwind.
        // UNION ALL agrega ejemplos al resultado, sin insertar ni modificar filas.
        public static void MostrarTiposYNulos(MySqlConnection conexion)
        {
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("3. TIPOS Y NULL: PRECIO REGISTRADO, CERO Y AUSENCIA");
            Console.WriteLine("============================================================");

            string consulta =
                "SELECT ProductName, UnitPrice FROM Products WHERE ProductID = @id " +
                "UNION ALL SELECT '[Simulado] Precio cero', CAST(0 AS DECIMAL(10,2)) " +
                "UNION ALL SELECT '[Simulado] Sin precio', CAST(NULL AS DECIMAL(10,2));";

            {
                using MySqlCommand comando = new(consulta, conexion);
                comando.Parameters.Add("@id", MySqlDbType.Int32).Value = 1;

                {
                    using MySqlDataReader lector = comando.ExecuteReader();
                    int columnaNombre = lector.GetOrdinal("ProductName");
                    int columnaPrecio = lector.GetOrdinal("UnitPrice");

                    while (lector.Read())
                    {
                        string nombre = lector.GetString(columnaNombre);
                        Console.WriteLine("Producto: " + nombre);

                        // NULL de SQL se comprueba antes de leer como decimal.
                        if (lector.IsDBNull(columnaPrecio))
                        {
                            Console.WriteLine("Precio: sin registrar.");
                        }
                        else
                        {
                            decimal precio = lector.GetDecimal(columnaPrecio);
                            Console.WriteLine("Precio: " + precio.ToString("F2"));
                        }
                    }
                }
            }

            // Cero es un valor conocido. NULL expresa ausencia de valor.
            // Una consulta sin filas es otro caso: Read devuelve false.
            // decimal? permite guardar en C# un precio que también puede estar ausente.
        }

        /*
         * 4. ERRORES
         * Una conexión puede fallar antes de enviar SQL. Una consulta puede
         * fallar con la conexión abierta. Una instrucción válida puede incumplir
         * una restricción. Number y Message permiten distinguir esos casos.
         * catch maneja el error; using libera recursos aunque haya una excepción.
         * Un filtro when captura solo el código indicado; otros errores continúan
         * hacia el catch de Program. Un resultado vacío no es una excepción.
         * Cambia TablaInexistenteDemoAdoNet por Products y ejecuta de nuevo.
         * ¿Se activa el mismo catch? Restaura el nombre para reproducir el error.
         * Compara el fallo de consulta con el de conexión: ¿cuál llega a abrirla?
         */
        // Caso 1: intentar conectar a un puerto local de prueba.
        // La dirección 127.0.0.1 apunta al equipo que ejecuta el programa; los datos de acceso son ficticios.
        public static void MostrarErrorConexion()
        {
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("4.2. ERROR DE CONEXIÓN: SERVIDOR NO DISPONIBLE");
            Console.WriteLine("============================================================");
            Console.WriteLine("Este ejemplo provoca un error intencionalmente.");
            string cadenaPrueba =
                "Server=127.0.0.1;Port=1;User ID=ejemplo;Password=ejemplo;" +
                "Connection Timeout=2;Pooling=false;SslMode=Required;";

            try
            {
                {
                    using MySqlConnection conexion = new(cadenaPrueba);
                    conexion.Open();
                    Console.WriteLine("El puerto respondió; no se reprodujo el fallo esperado.");
                }
            }
            catch (MySqlException error)
            {
                Console.WriteLine("Falló la conexión de prueba.");
                Console.WriteLine("Código: " + error.Number);
                Console.WriteLine("Detalle: " + error.Message);
            }
        }

        // Caso 2: la conexión funciona, pero la consulta referencia una tabla inexistente.
        public static void MostrarErrorConsulta(MySqlConnection conexion)
        {
            int tableNotFoundErrorCode = (int)MySqlErrorCode.NoSuchTable;

            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("4.1. ERROR DE CONSULTA: TABLA INEXISTENTE");
            Console.WriteLine("============================================================");
            Console.WriteLine("Este ejemplo provoca un error intencionalmente.");

            try
            {
                string consulta = "SELECT * FROM TablaInexistenteDemoAdoNet;";
                {
                    using MySqlCommand comando = new(consulta, conexion);
                    comando.ExecuteScalar();
                    Console.WriteLine("La tabla existe; no se reprodujo el fallo esperado.");
                }
            }
            catch (MySqlException error) when (error.Number == tableNotFoundErrorCode)
            {
                Console.WriteLine("La tabla indicada no existe.");
                Console.WriteLine("Código: " + error.Number);
                Console.WriteLine("Detalle: " + error.Message);
            }

            // Observa el resultado de SELECT 1: ¿el error anterior cerró la conexión?
            {
                using MySqlCommand comprobacion = new("SELECT 1;", conexion);
                Console.WriteLine("La conexión sigue funcionando: " + comprobacion.ExecuteScalar());
            }
        }

        // Caso 3: SQL válido que intenta duplicar una clave primaria.
        // Prueba un segundo ID distinto de 1 antes de la segunda ejecución.
        // ¿Qué ocurre con el catch si las dos claves son diferentes?
        public static void MostrarErrorRestriccion(string cadenaConexion)
        {
            int duplicateKeyErrorCode = (int)MySqlErrorCode.DuplicateKeyEntry;

            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("4.3. ERROR DE RESTRICCIÓN: CLAVE PRIMARIA DUPLICADA");
            Console.WriteLine("============================================================");
            Console.WriteLine("Este ejemplo provoca un error en una tabla temporal.");

            // Sin pool, Dispose cierra la sesión y MySQL elimina su tabla temporal.
            MySqlConnectionStringBuilder configuracion = new(cadenaConexion)
            {
                Pooling = false
            };

            {
                using MySqlConnection conexion = new(configuracion.ConnectionString);
                conexion.Open();
                string crearTabla = "CREATE TEMPORARY TABLE DemoAdoNetClaves (Id INT PRIMARY KEY);";
                {
                    using MySqlCommand comando = new(crearTabla, conexion);
                    comando.ExecuteNonQuery();
                }

                try
                {
                    string insertar = "INSERT INTO DemoAdoNetClaves (Id) VALUES (@id);";
                    {
                        using MySqlCommand comando = new(insertar, conexion);

                        comando.Parameters.Add("@id", MySqlDbType.Int32).Value = 1;
                        comando.ExecuteNonQuery();
                        Console.WriteLine("Primera inserción correcta: ID 1.");

                        comando.ExecuteNonQuery(); // El segundo ID 1 viola la clave primaria.
                    }
                }
                catch (MySqlException error) when (error.Number == duplicateKeyErrorCode)
                {
                    Console.WriteLine("No se puede repetir una clave primaria.");
                    Console.WriteLine("Código: " + error.Number);
                    Console.WriteLine("Detalle: " + error.Message);
                }
            }

            Console.WriteLine("Sesión de prueba cerrada; tabla temporal eliminada.");
            // Requiere permiso CREATE TEMPORARY TABLES.
            // Otros errores se propagan hasta el catch de Main.
        }

        /*
         * 5. TRANSACCIONES
         * Una operación de negocio puede necesitar varios comandos.
         * BeginTransaction inicia un grupo de operaciones que deben confirmarse
         * juntas. Cada comando recibe la misma conexión y Transaction.
         * Commit confirma los cambios; Rollback los deshace.
         * Cambia ambos importes de TransferirSaldo de 20m a 10m y observa los saldos.
         * Ajusta también los mensajes que anuncian el importe para interpretar la salida.
         * Compara el caso correcto con el caso que falla: ¿qué conserva Rollback?
         */
        // Caso de uso: transferir 20 entre dos cuentas de ejemplo.
        // Descontar del origen y abonar al destino deben confirmarse juntos.
        public static void MostrarTransacciones(string cadenaConexion)
        {
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("5. TRANSACCIONES: COMMIT Y ROLLBACK");
            Console.WriteLine("============================================================");
            MySqlConnectionStringBuilder configuracion = new(cadenaConexion)
            {
                Pooling = false
            };

            {
                using MySqlConnection conexion = new(configuracion.ConnectionString);
                conexion.Open();
                // La tabla se crea antes de iniciar las transacciones.
                // InnoDB permite deshacer sus cambios de datos con Rollback.
                string crearTabla =
                    "CREATE TEMPORARY TABLE DemoAdoNetCuentas " +
                    "(Id INT PRIMARY KEY, Saldo DECIMAL(10,2) NOT NULL) ENGINE=InnoDB;";
                {
                    using MySqlCommand comando = new(crearTabla, conexion);
                    comando.ExecuteNonQuery();
                    comando.CommandText =
                        "INSERT INTO DemoAdoNetCuentas (Id, Saldo) VALUES (1, 100), (2, 50);";
                    comando.ExecuteNonQuery();
                }

                Console.WriteLine();
                Console.WriteLine("--- Estado inicial ---");
                MostrarSaldos(conexion);

                Console.WriteLine();
                Console.WriteLine("--- Caso A: transferencia correcta (Commit) ---");
                Console.WriteLine("Transferir 20 de la cuenta 1 a la 2.");
                TransferirSaldo(conexion, false);
                Console.WriteLine("Saldos después del Commit:");
                MostrarSaldos(conexion); // Esperado: 80 y 70.

                Console.WriteLine();
                Console.WriteLine("--- Caso B: transferencia con error intencional (Rollback) ---");
                Console.WriteLine("Descontar 20 y fallar antes de abonar.");
                TransferirSaldo(conexion, true);
                Console.WriteLine("Saldos después del Rollback:");
                MostrarSaldos(conexion); // Esperado: siguen en 80 y 70.
            }

            // Al cerrar esta sesión sin pool, desaparece la tabla temporal.
        }

        static void TransferirSaldo(MySqlConnection conexion, bool provocarError)
        {
            int duplicateKeyErrorCode = (int)MySqlErrorCode.DuplicateKeyEntry;

            {
                using MySqlTransaction transaccion = conexion.BeginTransaction();
                try
                {
                    string descontar = "UPDATE DemoAdoNetCuentas SET Saldo = Saldo - @importe WHERE Id = 1;";
                    {
                        using MySqlCommand comando = new(descontar, conexion);
                        // Cada comando usa la misma conexión y transacción.
                        comando.Transaction = transaccion;
                        comando.Parameters.Add("@importe", MySqlDbType.Decimal).Value = 20m;
                        comando.ExecuteNonQuery();
                    }

                    if (provocarError)
                    {
                        string duplicar = "INSERT INTO DemoAdoNetCuentas (Id, Saldo) VALUES (1, 0);";
                        {
                            using MySqlCommand comando = new(duplicar, conexion);
                            comando.Transaction = transaccion;
                            comando.ExecuteNonQuery(); // Falla porque ya existe el ID 1.
                        }
                    }

                    string abonar = "UPDATE DemoAdoNetCuentas SET Saldo = Saldo + @importe WHERE Id = 2;";
                    {
                        using MySqlCommand comando = new(abonar, conexion);
                        comando.Transaction = transaccion;
                        comando.Parameters.Add("@importe", MySqlDbType.Decimal).Value = 20m;
                        comando.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    Console.WriteLine("Commit: ambos cambios confirmados.");
                }
                catch (MySqlException error)
                {
                    // El error de clave duplicada no deshace por sí solo el descuento.
                    transaccion.Rollback();
                    if (!provocarError || error.Number != duplicateKeyErrorCode)
                    {
                        throw;
                    }

                    Console.WriteLine("Error esperado " + error.Number + ": clave duplicada.");
                    Console.WriteLine("Rollback: se deshizo también el descuento anterior.");
                }
            }
        }

        static void MostrarSaldos(MySqlConnection conexion)
        {
            string consulta = "SELECT Id, Saldo FROM DemoAdoNetCuentas ORDER BY Id;";
            {
                using MySqlCommand comando = new(consulta, conexion);
                {
                    using MySqlDataReader lector = comando.ExecuteReader();
                    while (lector.Read())
                    {
                        int id = lector.GetInt32(0);
                        decimal saldo = lector.GetDecimal(1);
                        Console.WriteLine("Cuenta " + id + ": " + saldo.ToString("F2"));
                    }
                }
            }
        }

    }
}
