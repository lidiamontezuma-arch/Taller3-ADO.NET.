Resultados del taller ADO.NET
1. Preparación y comprobación de la base de datos

Se utilizó una adaptación reducida de Northwind para MySQL. Se creó la base de datos northwind, que contiene ocho tablas y un total de 600 registros.

Se verificó la cantidad de productos mediante la consulta:

USE northwind;

SELECT COUNT(*) AS CantidadProductos
FROM Products;

El resultado obtenido fue de 77 productos, coincidiendo con la cantidad esperada en las instrucciones del taller.

También se comprobó que la base de datos estuviera disponible para la conexión desde C# mediante ADO.NET.

1. Exploración del tutorial

1.1 Límites de consulta

Se cambió LIMIT 2 por LIMIT 5. Se comprobó que la consulta muestra dos y cinco productos, respectivamente. LIMIT establece la cantidad máxima de registros que devuelve una consulta, sin modificar la base de datos.

1.2 Estado de la conexión y liberación de recursos

La conexión pasó de Closed a Open. Al liberar el lector, la conexión permaneció abierta y se cerró al finalizar su ámbito. Se comprobó que using permite importar espacios de nombres o liberar recursos automáticamente, según su uso.

Segunda consulta: Después de liberar el lector, se utilizó ExecuteScalar() y se obtuvo un total de 77 productos, demostrando que la conexión podía seguir utilizándose.

1.3 Búsqueda de productos por identificador

Se probó el ID 1, que encontró el producto Chai; el ID 99999, que no encontró resultados; el ID 0 y el texto «hola», que fueron rechazados. Se utilizó TryParse() para validar la entrada y el parámetro @id para realizar la consulta de forma segura.

1.4 Valores cero y NULL

Se obtuvo el precio de Chai (18.00), un precio simulado de 0.00 y otro valor NULL. Se comprobó que cero es un valor numérico, mientras que NULL representa la ausencia de un dato. IsDBNull() permite identificarlo antes de leer el valor.

1.5 Manejo de errores de consulta

Se probó una consulta válida y otra con una tabla inexistente. La segunda produjo el error 1146. Después, SELECT 1 confirmó que la conexión seguía funcionando.

1.6 Manejo de errores de conexión

Se intentó conectar al puerto 1 y se obtuvo el error 1042. El bloque catch permitió capturar y mostrar el problema sin interrumpir inesperadamente el programa.

1.7 Manejo de claves primarias duplicadas

Se insertaron los ID 1 y 2 correctamente. Después, se repitió el ID 1 y se produjo el error 1062. Se comprobó que una clave primaria no permite identificadores duplicados.

1.8 Transacciones: COMMIT y ROLLBACK

Se probaron transferencias de 10 y posteriormente de 20. COMMIT confirmó los cambios y ROLLBACK deshizo el descuento cuando ocurrió un error de clave duplicada.

Resultados finales con importe de 20:

Estado

Cuenta 1

Cuenta 2

Inicial

100.00

50.00

Después del COMMIT

80.00

70.00

Después del ROLLBACK

80.00

70.00

Conclusión: Se comprobó que ADO.NET permite ejecutar consultas parametrizadas, administrar conexiones y recursos, manejar errores y controlar transacciones sin modificar los datos permanentes de Northwind.
2. Búsqueda de productos por precio y existencias

Se creó BusquedaProductos.cs para buscar productos activos mediante parámetros de precio máximo y existencias mínimas.

2.1. Primera prueba

Datos: Precio máximo: 20.00; existencias mínimas: 10.

Resultado: Se encontraron 32 productos, ordenados por precio ascendente, mostrando sus datos principales.

2.2. Validación de datos

Se comprobó que el programa rechaza precios negativos y textos. También rechaza existencias negativas, letras y números decimales. 
Se verificó que acepta el valor cero, aunque en esa búsqueda no se encontraron productos.

2.3. Conteo de productos

Se utilizó COUNT(*) y ExecuteScalar() para contar los productos que cumplen los filtros. Con un precio máximo de 20 y existencias mínimas de 10, ambos conteos coincidieron en 32 productos.

3. Búsqueda de productos por nombre

Se implementó una búsqueda con LIKE y parámetros. 
Se comprobó que encuentra productos como Chai, informa cuando no existen coincidencias y rechaza entradas vacías.