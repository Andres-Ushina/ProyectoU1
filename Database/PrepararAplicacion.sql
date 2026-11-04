/* COMENTARIO EXPLICATIVO: Prepara Sakila en SQL Server agregando Usuarios, indices y banderas para eliminación lógica. */
/* Ejecutar en SQL Server, después de importar Sakila y en la base sakila. */
/* Crea Usuarios una sola vez, con restricciones para que rol y relación Sakila sean coherentes. */
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

/* Impide asignar la misma cuenta de cliente mas de una vez. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_Usuarios_CustomerId_Activo' AND object_id=OBJECT_ID(N'dbo.Usuarios'))
    CREATE UNIQUE INDEX UX_Usuarios_CustomerId_Activo ON dbo.Usuarios(CustomerId) WHERE CustomerId IS NOT NULL;
GO

/* Las banderas permiten ocultar películas/copias sin borrar el historial. */
IF COL_LENGTH(N'dbo.film', N'is_active') IS NULL
    ALTER TABLE dbo.film ADD is_active BIT NOT NULL CONSTRAINT DF_film_is_active DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.inventory', N'is_active') IS NULL
    ALTER TABLE dbo.inventory ADD is_active BIT NOT NULL CONSTRAINT DF_inventory_is_active DEFAULT (1);
GO

/* El personal que registra el alquiler debe existir en Sakila. */
IF NOT EXISTS (SELECT 1 FROM dbo.staff WHERE staff_id=1)
    PRINT 'Aviso: configure Rental:StaffId en appsettings.json con un staff_id existente.';
GO
