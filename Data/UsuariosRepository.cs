// COMENTARIO EXPLICATIVO: Acceso ADO.NET a cuentas y clientes; crea usuarios asociados y aplica desactivacion lógica.
using Microsoft.Data.SqlClient;
using ProyectoU1.Models;

namespace ProyectoU1.Data;

public sealed class UsuariosRepository(IConfiguration configuration)
{
    private string ConnectionString => configuration.GetConnectionString("Sakila")
        ?? throw new InvalidOperationException("Falta configurar ConnectionStrings:Sakila.");

    public async Task<IReadOnlyList<(int Id, string Nombre)>> ClientesDisponiblesAsync()
    {
        const string sql = """
            SELECT c.customer_id, CONCAT(c.first_name, ' ', c.last_name)
            FROM dbo.customer c
            WHERE c.active=1 AND NOT EXISTS
                (SELECT 1 FROM dbo.Usuarios u WHERE u.CustomerId=c.customer_id)
            ORDER BY c.last_name, c.first_name;
            """;
        var result = new List<(int, string)>();
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) result.Add((Convert.ToInt32(reader.GetValue(0)), reader.GetString(1)));
        return result;
    }

    public async Task<IReadOnlyList<(int Id, string Username, int? CustomerId, int? StaffId, string Role, bool Active)>> ListarAsync()
    {
        const string sql = "SELECT UsuarioId, NombreUsuario, CustomerId, StaffId, Rol, Activo FROM dbo.Usuarios ORDER BY Rol, NombreUsuario;";
        var result = new List<(int, string, int?, int?, string, bool)>();
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add((reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? null : Convert.ToInt32(reader.GetValue(2)),
                reader.IsDBNull(3) ? null : Convert.ToInt32(reader.GetValue(3)), reader.GetString(4), reader.GetBoolean(5)));
        return result;
    }

    // Implementa la operación CrearClienteAsync dentro de este componente.
    public async Task CrearClienteAsync(string username, string passwordHash, int customerId)
    {
        const string sql = """
            INSERT INTO dbo.Usuarios (NombreUsuario, PasswordHash, Rol, CustomerId, StaffId, Activo)
            SELECT @Username, @PasswordHash, N'Cliente', c.customer_id, NULL, 1
            FROM dbo.customer c
            WHERE c.customer_id=@CustomerId AND c.active=1
              AND NOT EXISTS (SELECT 1 FROM dbo.Usuarios u WHERE u.CustomerId=c.customer_id);
            """;
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Username", username);
        command.Parameters.AddWithValue("@PasswordHash", passwordHash);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        await connection.OpenAsync();
        if (await command.ExecuteNonQueryAsync() != 1) throw new InvalidOperationException("El cliente no existe, está inactivo o ya tiene una cuenta.");
    }

    // Implementa la operación DesactivarAsync dentro de este componente.
    public async Task<bool> DesactivarAsync(int id)
    {
        const string sql = "UPDATE dbo.Usuarios SET Activo=0 WHERE UsuarioId=@Id AND Rol=N'Cliente' AND Activo=1;";
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() == 1;
    }
}
