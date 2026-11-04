# Sistema de renta de películas Sakila

Aplicación web académica de renta de películas desarrollada con ASP.NET Core MVC, Razor, Bootstrap, C#, ADO.NET y SQL Server. Usa las tablas `film`, `inventory`, `rental`, `customer` y `staff` de Sakila para administrar el catálogo, consultar disponibilidad, registrar alquileres y devoluciones, y generar reportes.

## Requisitos

- Windows con SQL Server (por ejemplo, SQL Server Express) y SQL Server Management Studio.
- .NET 10 SDK.
- Una base Sakila migrada a SQL Server con las tablas `film`, `inventory`, `rental`, `customer`, `staff`, `language` y `store`.

> Los archivos originales `sakila-mv-schema.sql` y `sakila-mv-data.sql` son scripts de MySQL. SQL Server no los ejecuta directamente. Antes de iniciar la aplicación, importa o migra Sakila a SQL Server y comprueba que las tablas y columnas tengan los nombres que usan los repositorios.

## Preparar la base de datos

1. Crea o importa la base Sakila en SQL Server.
2. Abre `Database/PrepararAplicacion.sql` en SQL Server Management Studio, selecciona la base Sakila y ejecuta el script. Este crea `dbo.Usuarios` y agrega las columnas `is_active` a `film` e `inventory` para implementar bajas lógicas.
3. Verifica que exista al menos un registro de personal activo en `dbo.staff`. El sistema usa `Rental:StaffId` para registrar quién procesó cada alquiler.
4. `Database/ConsultasExploracion.sql` contiene las tres consultas SQL de exploración del informe. Ejecútalas después de preparar la base para obtener resultados de tu instancia.

## Configurar y ejecutar

La aplicación lee la conexión desde `ConnectionStrings:Sakila` en `appsettings.json`. El archivo incluye un ejemplo con autenticación integrada para SQL Server Express. Ajusta el nombre de instancia y base de datos a tu entorno. No guardes contraseñas reales en el repositorio.

También puedes configurar la conexión en PowerShell mediante una variable de entorno, que reemplaza el valor del archivo de configuración durante esa sesión:

```powershell
$env:ConnectionStrings__Sakila = "Server=.\SQLEXPRESS;Database=sakila;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
```

Desde PowerShell, abre la carpeta del proyecto y ejecuta:

```powershell
cd C:\Users\Admin\source\repos\ProyectoU1_AU\ProyectoU1
dotnet restore
dotnet run
```

El perfil HTTP está configurado en `http://localhost:5019`; el perfil HTTPS está en `https://localhost:7172`. También puedes abrir `ProyectoU1.csproj` en Visual Studio 2022 con soporte para .NET 10 y ejecutar el proyecto desde allí.

## Primer inicio de sesión

1. Si todavía no hay cuentas, abre `http://localhost:5019/Account/Setup`.
2. Crea la cuenta inicial de administrador y asígnala a un `staff_id` existente en Sakila. La contraseña debe cumplir el mínimo indicado en el formulario; se almacena como hash con `PasswordHasher` de ASP.NET Core.
3. Inicia sesión. En **Cuentas**, el administrador puede crear cuentas para clientes Sakila activos que aún no tengan una.
4. Cierra sesión y prueba una cuenta de cliente para revisar el catálogo, alquilar una copia disponible, consultar **Mis alquileres** y devolver una renta propia.

La configuración inicial de administrador deja de estar disponible cuando se crea la primera cuenta. Session conserva temporalmente la identidad y el rol durante la navegación. Las acciones administrativas verifican el rol en el servidor.

## Funcionalidades

- Inicio y cierre de sesión con contraseñas protegidas mediante hash.
- Roles de administrador y cliente, con navegación y autorización según el rol.
- Catálogo y detalle de películas con disponibilidad calculada por copia.
- Administración de películas e inventario, incluida reactivación de registros desactivados.
- Alquiler transaccional de copias disponibles, historial por cliente y devolución sin borrar el registro.
- Reporte de las diez películas con mayor cantidad de alquileres.
- Validaciones de modelos, formularios y reglas de negocio.

La disponibilidad se calcula con `inventory` y `rental`: una copia está libre cuando no tiene una renta cuyo `return_date` sea `NULL`. La eliminación lógica conserva las películas y copias para mantener las referencias del historial.

## Organización del proyecto

- `Controllers/`: acciones MVC y coordinación del flujo de cada pantalla.
- `Models/`: modelos de dominio y formularios con DataAnnotations.
- `Data/`: repositorios ADO.NET y consultas SQL parametrizadas.
- `Filters/SessionAuthorizeAttribute.cs`: restricción de acceso basada en Session y rol.
- `Views/`: vistas Razor y componentes compartidos.
- `Database/`: scripts de preparación y consultas de exploración.
- `wwwroot/`: estilos, JavaScript y bibliotecas de interfaz.

## Solución de problemas

- **No se puede conectar a SQL Server:** revisa el nombre de instancia, que el servicio esté iniciado y que `ConnectionStrings:Sakila` apunte a la base correcta.
- **Falta una tabla o columna:** confirma que Sakila se migró a SQL Server y ejecuta `Database/PrepararAplicacion.sql`.
- **No aparece la configuración inicial:** ya existe al menos una cuenta en `dbo.Usuarios`; inicia sesión con ella.
- **No se puede registrar un alquiler:** comprueba que la película tenga copias activas disponibles y que `Rental:StaffId` corresponda a un empleado existente.

## Entrega académica

Antes de entregar, incorpora al informe las capturas reales solicitadas, actualiza sus índices y añade los enlaces públicos al repositorio y al video demostrativo. No incluyas credenciales ni datos sensibles en el repositorio.
