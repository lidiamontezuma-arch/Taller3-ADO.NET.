# Taller 3: consultas de productos con ADO.NET

**Entrega:** sábado 14 de noviembre de 2026, hasta la 1:00 p. m.  
**Medio de entrega:** enlace al repositorio de GitHub del estudiante, en la actividad del Taller 3 en Moodle.  
**Valor:** 5 % de la calificación final del curso. El taller se califica sobre 10 puntos.  
**Modalidad:** individual y asíncrona.  

## Resultado esperado

El taller comprende la ejecución y modificación de los ejemplos del tutorial y
la construcción de consultas propias de productos activos por precio máximo,
existencias mínimas y parte del nombre. El resultado esperado son dos búsquedas y un conteo que
funcionen desde C# mediante ADO.NET, con parámetros y sin modificar los
registros del catálogo.

Antes de iniciar, sigue las instrucciones de preparación y ejecución del
[README principal del repositorio](../README.md) para crear y cargar la base
Northwind en tu servidor MySQL y configurar la conexión desde C#.
Al ejecutar el proyecto, comprueba que aparezcan «Conexión abierta correctamente»
y «Cantidad de productos: 77».

Consulta la explicación de [los métodos de ejecución de ADO.NET](metodos-ejecucion-adonet.md)
para revisar `ExecuteReader`, `ExecuteScalar` y `ExecuteNonQuery` y sus ejemplos.

## 1. Exploración del tutorial

El tutorial se encuentra en `Northwind-Catalogo/AspectosAdoNet.cs`. Antes de
cada método hay un comentario con la explicación del aspecto que se trabaja,
los valores que deberán cambiarse y las preguntas que orientan la observación.
Se deberán seguir esas indicaciones en el orden descrito al inicio del archivo.

Primero se ejecutará el ejemplo original; después se cambiarán los valores
indicados y se comparará la nueva salida con la original. Las preguntas de
cada comentario deberán servir de base para explicar lo observado.

La exploración deberá abarcar los límites de la consulta, los identificadores de
productos, los valores cero y NULL, los errores de consulta y conexión, las
claves duplicadas y los importes de las transferencias. Los ejemplos de
escritura trabajan sobre tablas temporales; cada repetición del recorrido crea
un estado inicial nuevo.

Entrega en `RESULTADOS.md` los cambios realizados en el tutorial, sus
resultados y la explicación de lo observado. Las variantes
deberán conservarse para poder explicar lo observado. Al terminar, deberán
restaurarse los cambios de prueba que impidan continuar el recorrido completo.

## 2. Construcción de la búsqueda

Un cliente dispone de un precio máximo por unidad y necesita una cantidad
mínima disponible. La solución deberá encontrar los productos activos que
cumplan ambas condiciones, de acuerdo con los siguientes requisitos:

1. **Organización.** Se deberá crear un archivo nuevo llamado
   `BusquedaProductos.cs` en la carpeta `Northwind-Catalogo`. La clase deberá
   definirse dentro del namespace `Northwind.Catalogo` e incluir un método que
   reciba una conexión abierta. Allí deberán escribirse las consultas de
   búsqueda y conteo y su implementación con ADO.NET.
2. **Validación.** El programa deberá solicitar el precio máximo y las
   existencias mínimas, utilizando `decimal` e `int` respectivamente. Los
   valores negativos y las entradas no numéricas deberán rechazarse antes de
   enviar la consulta. 
3. **Consulta.** El `SELECT` sobre `Products` deberá filtrar `UnitPrice` menor
   o igual al máximo, `UnitsInStock` mayor o igual al mínimo y
   `Discontinued = 0`. Los valores ingresados deberán enviarse mediante
   parámetros, y los resultados deberán ordenarse por precio y después por
   `ProductID`.
4. **Lectura de datos.** El programa deberá mostrar `ProductID`, `ProductName`,
   `UnitPrice`, `UnitsInStock` y `QuantityPerUnit`. El precio deberá leerse como
   `decimal` y mostrarse con dos decimales. `QuantityPerUnit` deberá comprobarse
   con `IsDBNull` antes de leerse; si es NULL, deberá mostrarse
   "Presentación sin registrar".
5. **Recursos y errores.** El comando y el lector deberán liberarse mediante
   `using`. Si no hay filas, deberá mostrarse "No hay productos que cumplan los
   filtros". Este resultado deberá distinguirse del manejo de `MySqlException`.
6. **Integración.** El método deberá llamarse desde `Program` con una conexión
   abierta. La llamada al tutorial podrá comentarse temporalmente para comprobar
   la búsqueda por separado.

## 3. Ampliación y contraste de resultados

Se deberá escribir una segunda consulta con `COUNT(*)` y `ExecuteScalar` que
cuente los productos con exactamente los mismos filtros de la búsqueda. La
consulta deberá utilizar parámetros y ejecutarse después de liberar el lector
anterior. El total deberá coincidir con la cantidad de filas mostradas.

También deberá probarse el orden por precio descendente e incluir en
`RESULTADOS.md` si cambian los productos encontrados o solo su posición. Para la entrega, deberá
restaurarse el orden ascendente.

## 4. Búsqueda por parte del nombre

Agrega una tercera consulta en `BusquedaProductos.cs`, dentro de un método
separado que reciba una conexión abierta. Llámalo desde `Program` después de
la búsqueda por precio y existencias y de su conteo.

1. **Entrada y validación.** Solicita un fragmento del nombre de un producto.
   Elimina los espacios al inicio y al final con `Trim()`. Si el texto queda
   vacío, informa el problema y vuelve a solicitarlo antes de ejecutar SQL.
2. **Consulta.** Busca en `Products` los productos activos
   (`Discontinued = 0`) cuyo `ProductName` contenga el texto ingresado.
   Utiliza `LIKE` con un parámetro. El patrón debe incluir `%` antes y después
   del fragmento y asignarse como valor del parámetro, sin concatenar la
   entrada en la instrucción SQL. En `LIKE`, `%` representa cualquier
   secuencia de caracteres.
3. **Resultados.** Muestra `ProductID`, `ProductName`, `UnitPrice` y
   `UnitsInStock`, ordenados por `ProductName` y después por `ProductID`,
   ambos en orden ascendente. Lee el precio como `decimal` y muéstralo con
   dos decimales. Esta búsqueda es independiente de los filtros numéricos
   anteriores.
4. **Recursos y errores.** Ejecuta la consulta con `ExecuteReader` y libera
   el comando y el lector mediante `using`. Si no hay coincidencias, muestra
   «No hay productos activos que coincidan con el nombre».
   Distingue ese resultado de una `MySqlException`.

## 5. Comprobación de la solución

Entrega en `RESULTADOS.md` los resultados de los siguientes once casos de
prueba. Todas las consultas deberán
realizarse sobre la misma base, sin modificarla entre ellas:

1. **Precio máximo 20 y stock mínimo 10:** todas las filas deberán cumplir los
   límites y estar activas; el conteo deberá coincidir con el listado.
2. **Precio máximo 0 y stock mínimo 0:** deberá comprobarse la respuesta del
   programa ante la ausencia de coincidencias en el catálogo inicial.
3. **Precio máximo 1000 y stock mínimo 10000:** la ausencia de coincidencias
   deberá distinguirse de un error de conexión o de SQL.
4. **Precio o stock negativo:** la entrada deberá rechazarse antes de ejecutar SQL.
5. **Texto en cada entrada numérica:** la aplicación deberá informar el problema
   sin cerrarse inesperadamente.
6. **Stock con decimales:** la entrada deberá rechazarse porque se requiere un
   entero.
7. **Límites inclusivos:** se deberá elegir un producto activo y utilizar su
   precio y stock exactos como límites. El producto deberá aparecer en los
   resultados.
8. **Presentación NULL:** si el catálogo no contiene un caso, deberá probarse
   con `CAST(NULL AS CHAR)` en la columna de presentación, sin
   modificar filas. Después deberá restaurarse la consulta normal.
9. **Nombre con coincidencias:** elige un producto activo del catálogo e
   ingresa un fragmento de su nombre, sin comodines. Comprueba que aparezca,
   que todas las filas estén activas y contengan ese fragmento, y que el
   listado respete el orden por nombre y después por identificador.
10. **Nombre sin coincidencias:** ingresa un texto que no aparezca en ningún
    nombre, por ejemplo `zzzzsincoincidencias999`. Comprueba que aparezca el
    mensaje de ausencia de coincidencias sin tratarlo como un error.
11. **Nombre vacío:** prueba presionar Enter sin escribir y después ingresar
    solo espacios. Ambas entradas deben rechazarse antes de ejecutar SQL.

## Requisitos de entrega

La entrega se realizará mediante un repositorio en la cuenta de GitHub del
estudiante, con el proyecto completo. Debes entregar estos dos archivos
obligatorios dentro del repositorio:

1. **`Northwind-Catalogo/BusquedaProductos.cs`**: tu implementación de las
   tres consultas solicitadas, con sus validaciones, lectura de resultados y
   manejo de recursos y errores.
2. **`RESULTADOS.md`**, en la raíz del repositorio: tus exploraciones del
   tutorial, los resultados de los once casos de prueba, la comparación del
   orden ascendente y descendente, y una explicación en palabras propias de
   la asignación de parámetros, los recursos liberados por cada `using` y la
   diferencia entre cero coincidencias, una columna NULL y una excepción.

Además, incluye los archivos necesarios para compilar y ejecutar el proyecto,
con la integración de las consultas desde `Program.cs` y los archivos de
código modificados para las variantes del tutorial.

La versión evaluada será el último commit publicado antes del plazo de entrega.
Los commits posteriores no formarán parte de esta evaluación.

## Criterios de evaluación

El taller se calificará sobre **10 puntos**:

| Criterio | Puntos | Condición de cumplimiento |
|---|---:|---|
| 1. Exploración del tutorial | 2 | `RESULTADOS.md` incluye los cambios realizados en el tutorial, sus resultados y una explicación coherente con lo observado. |
| 2. Validación y búsquedas | 2 | El precio se lee como `decimal` y el stock como `int`; los valores negativos, el texto y el stock con decimales se rechazan antes de ejecutar SQL, sin que el programa se cierre. La búsqueda numérica contiene los tres filtros pedidos y el orden solicitado. La búsqueda por nombre rechaza entradas vacías o compuestas solo por espacios, utiliza `LIKE`, filtra productos activos y ordena por nombre e identificador. Ambas búsquedas envían los valores mediante parámetros, sin concatenar las entradas en el texto SQL. |
| 3. Lectura de resultados y conteo | 2 | La búsqueda numérica muestra las cinco columnas solicitadas y la búsqueda por nombre muestra sus cuatro columnas, con los tipos correspondientes y el precio con dos decimales. `QuantityPerUnit` se comprueba con `IsDBNull`. En ambas búsquedas, el comando y el lector se liberan con `using`, y se distinguen la ausencia de filas y una `MySqlException`. El conteo usa los mismos filtros de la búsqueda numérica con parámetros y `ExecuteScalar`, y su total coincide con las filas mostradas. |
| 4. Resultados de las pruebas | 2 | `RESULTADOS.md` incluye los resultados de los once casos de prueba y de la comparación del orden ascendente y descendente. Los resultados corresponden al comportamiento del código entregado. |
| 5. Explicación y seguridad | 2 | La explicación en palabras propias aborda la asignación de parámetros, los recursos que libera cada `using` y la diferencia entre cero coincidencias, una columna NULL y una excepción. Los datos reales de acceso de `Program.cs` se sustituyen por los marcadores del ejemplo antes de publicar y ningún archivo ni commit contiene una contraseña real. |
| **Total** | **10** | |
