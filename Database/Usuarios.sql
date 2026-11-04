/* COMENTARIO EXPLICATIVO: Crea la tabla de cuentas con rol, hash, estado y restricciones sobre su asociacion Sakila. */
/* Ejecutar en SQL Server sobre la base utilizada por la aplicación. */
/* La tabla guarda el hash, rol y relación opcional con la identidad Sakila. */
IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios
    (
        UsuarioId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
        NombreUsuario NVARCHAR(50) NOT NULL CONSTRAINT UQ_Usuarios_NombreUsuario UNIQUE,
        PasswordHash NVARCHAR(500) NOT NULL,
        Rol NVARCHAR(20) NOT NULL,
        CustomerId INT NULL,
        StaffId INT NULL,
        Activo BIT NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT (1),
        CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN (N'Administrador', N'Cliente')),
        CONSTRAINT CK_Usuarios_Asociacion CHECK
        (
            (Rol = N'Cliente' AND CustomerId IS NOT NULL AND StaffId IS NULL)
            OR (Rol = N'Administrador' AND StaffId IS NOT NULL AND CustomerId IS NULL)
        )
    );
END;
GO
