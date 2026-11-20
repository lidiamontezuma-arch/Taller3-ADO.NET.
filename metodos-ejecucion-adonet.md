# Métodos de ejecución de ADO.NET

Un `MySqlCommand` une una instrucción SQL con la conexión que la ejecutará.
Crear el comando no envía nada a MySQL: la instrucción se envía cuando se llama
a uno de sus tres métodos de ejecución. La elección del método depende de lo que
la instrucción devuelve.

| Método | Se utiliza cuando la instrucción... | Devuelve |
|---|---|---|
| `ExecuteReader` | devuelve filas. | Un lector (`MySqlDataReader`) para recorrerlas. |
| `ExecuteScalar` | devuelve un solo valor. | Ese valor, como `object`. |
| `ExecuteNonQuery` | modifica datos o estructura y no devuelve filas. | La cantidad de filas afectadas, como `int`. |

### ExecuteReader: recorrer filas

`ExecuteReader` se utiliza con un `SELECT` que puede devolver varias filas o
varias columnas, por ejemplo, un listado de productos.

El método no entrega todas las filas de una vez. Devuelve un lector que se
coloca **antes** de la primera fila. Cada llamada a `Read()` avanza a la fila
siguiente y devuelve `true` si encontró una; cuando ya no quedan filas,
devuelve `false`. Por eso el recorrido se escribe con `while (lector.Read())`.

Dentro del ciclo, cada columna de la fila actual se lee con el método que
corresponde a su tipo: `GetInt32` para enteros, `GetString` para texto y
`GetDecimal` para importes. La columna se indica por su posición, empezando en
0, o se localiza por nombre con `GetOrdinal`. Si una columna puede contener
NULL, deberá comprobarse con `IsDBNull` antes de leerla, porque `GetString`
o `GetDecimal` producen una excepción al leer un NULL.

```csharp
string consulta = "SELECT ProductID, ProductName FROM Products ORDER BY ProductID LIMIT 10;";
using MySqlCommand comando = new(consulta, conexion);
using MySqlDataReader lector = comando.ExecuteReader();

while (lector.Read())
{
    int id = lector.GetInt32(0);          // columna 0: ProductID
    string nombre = lector.GetString(1);  // columna 1: ProductName
    Console.WriteLine(id + " - " + nombre);
}
```

Si la consulta no encuentra filas, no se produce un error: la primera llamada a
`Read()` devuelve `false` y el ciclo no se ejecuta. Para informar ese caso, el
programa deberá comprobar si leyó alguna fila.

Mientras el lector está abierto, la conexión queda ocupada con sus resultados y
no puede ejecutar otro comando. El lector deberá liberarse, mediante `using`,
antes de ejecutar la consulta siguiente en la misma conexión.

### ExecuteScalar: obtener un solo valor

`ExecuteScalar` se utiliza cuando solo interesa un valor: un conteo con
`COUNT(*)`, una suma o el nombre de un producto buscado por su ID. Devuelve la
primera columna de la primera fila del resultado e ignora el resto.

El valor se recibe como `object`, porque el método no conoce de antemano su
tipo. Antes de usarlo, deberá convertirse al tipo correspondiente. Hay tres
resultados posibles:

| Situación | Valor devuelto |
|---|---|
| La consulta encontró un valor. | El valor, que deberá convertirse, por ejemplo con `Convert.ToInt64`. |
| La consulta no produjo ninguna fila. | `null`. |
| La fila existe, pero el valor es NULL en la base. | `DBNull.Value`, que es distinto de `null`. |

```csharp
string consulta = "SELECT COUNT(*) FROM Products;";
using MySqlCommand comando = new(consulta, conexion);

// En MySQL, COUNT(*) devuelve un entero de tipo long.
long cantidad = Convert.ToInt64(comando.ExecuteScalar());
Console.WriteLine("Cantidad de productos: " + cantidad);
```

Un `COUNT(*)` siempre devuelve una fila, aunque el conteo sea 0. En cambio, una
búsqueda por ID que no encuentra el producto devuelve `null`, como se observa en
`MostrarParametros`.

### ExecuteNonQuery: modificar sin leer filas

`ExecuteNonQuery` se utiliza con instrucciones que cambian datos o estructura:
`INSERT`, `UPDATE`, `DELETE` o `CREATE TABLE`. Estas instrucciones no devuelven
filas para recorrer. El método devuelve un `int` con la cantidad de filas
afectadas, que permite comprobar si la operación tuvo efecto: si ninguna fila
cumple la condición del `WHERE`, devuelve 0 y no se produce un error.

```csharp
string instruccion = "UPDATE DemoAdoNetCuentas SET Saldo = Saldo + @importe WHERE Id = @id;";
using MySqlCommand comando = new(instruccion, conexion);
comando.Parameters.Add("@importe", MySqlDbType.Decimal).Value = 20m;
comando.Parameters.Add("@id", MySqlDbType.Int32).Value = 2;

int filasAfectadas = comando.ExecuteNonQuery();
Console.WriteLine("Filas actualizadas: " + filasAfectadas);
```

En el tutorial, `ExecuteNonQuery` trabaja solo sobre tablas temporales, para no
modificar el catálogo. El taller no lo requiere, porque sus consultas solo leen
datos.

### Los métodos en el tutorial y en el taller

- `ExecuteReader`: `MostrarRecursos`, `ListarProductos` y `MostrarTiposYNulos`.
- `ExecuteScalar`: `Program.cs` para el conteo inicial y `MostrarParametros`
  para la búsqueda por ID.
- `ExecuteNonQuery`: `MostrarErrorRestriccion` y `MostrarTransacciones`.

El taller requiere `ExecuteReader` para las dos búsquedas y `ExecuteScalar` para el
conteo.

