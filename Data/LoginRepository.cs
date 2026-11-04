// COMENTARIO EXPLICATIVO: Consultas parametrizadas de acceso: busca cuentas, valida asociaciones Sakila y crea la cuenta inicial.
using Microsoft.Data.SqlClient;
using ProyectoU1.Models;

namespace ProyectoU1.Data;

public sealed class LoginRepository(IConfiguration configuration)
{
    private string ConnectionString => configuration.GetConnectionString("Sakila")
        ?? throw new InvalidOperationException("Falta configurar ConnectionStrings:Sakila.");

    // Busca una cuenta activa por nombre de usuario para el proceso de acceso.
    public async Task<UsuarioLogin?> BuscarPorNombreAsync(string nombreUsuario)
    {
        const string sql = """
            SELECT UsuarioId, NombreUsuario, PasswordHash, Rol, CustomerId, StaffId
            FROM dbo.Usuarios
            WHERE NombreUsuario = @NombreUsuario AND Activo = 1;
            """;

        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new UsuarioLogin
        {
            UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
            NombreUsuario = reader.GetString(reader.GetOrdinal("NombreUsuario")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            Rol = reader.GetString(reader.GetOrdinal("Rol")),
            CustomerId = reader.IsDBNull(reader.GetOrdinal("CustomerId")) ? null : reader.GetInt32(reader.GetOrdinal("CustomerId")),
            StaffId = reader.IsDBNull(reader.GetOrdinal("StaffId")) ? null : reader.GetInt32(reader.GetOrdinal("StaffId"))
        };
    }

    // Determina si ya hay cuentas creadas y bloquea la configuracion inicial.
    public async Task<bool> HayUsuariosAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand("SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Usuarios) THEN 1 ELSE 0 END;", connection);
        await connection.OpenAsync();
        return Convert.ToInt32(await command.ExecuteScalarAsync()) == 1;
    }

    // Comprueba que el identificador de empleado corresponda a staff activo.
    public async Task<bool> ExisteStaffActivoAsync(int staffId)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand("SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.staff WHERE staff_id=@StaffId AND active=1) THEN 1 ELSE 0 END;", connection);
        command.Parameters.AddWithValue("@StaffId", staffId);
        await connection.OpenAsync();
        return Convert.ToInt32(await command.ExecuteScalarAsync()) == 1;
    }

    // Guarda la cuenta inicial con los datos ya preparados por el controlador.
    public async Task CrearUsuarioAsync(UsuarioLogin usuario)
    {
        const string sql = """
            INSERT INTO dbo.Usuarios (NombreUsuario, PasswordHash, Rol, CustomerId, StaffId, Activo)
            VALUES (@NombreUsuario, @PasswordHash, @Rol, @CustomerId, @StaffId, 1);
            """;
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario);
        command.Parameters.AddWithValue("@PasswordHash", usuario.PasswordHash);
        command.Parameters.AddWithValue("@Rol", usuario.Rol);
        command.Parameters.AddWithValue("@CustomerId", (object?)usuario.CustomerId ?? DBNull.Value);
        command.Parameters.AddWithValue("@StaffId", (object?)usuario.StaffId ?? DBNull.Value);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }
}
