// COMENTARIO EXPLICATIVO: Acceso ADO.NET a alquileres: disponibilidad transaccional, historial, devoluciones y reportes.
using Microsoft.Data.SqlClient;
using System.Data;
using ProyectoU1.Models;

namespace ProyectoU1.Data;

public sealed class AlquilerRepository(IConfiguration configuration)
{
    private string ConnectionString => configuration.GetConnectionString("Sakila")
        ?? throw new InvalidOperationException("Falta configurar ConnectionStrings:Sakila.");

    // En una transacción serializable bloquea una copia libre, registra la renta y confirma los cambios.
    public async Task<int?> AlquilarAsync(int filmId, int customerId)
    {
        var staffId = configuration.GetValue<int?> ("Rental:StaffId") ?? 1;
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        // La transacción serializable coordina la comprobacion de disponibilidad y el alta.
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            // Selecciona una copia activa sin renta abierta; los bloqueos evitan doble asignacion concurrente.
            const string availableSql = """
                SELECT TOP (1) i.inventory_id
                FROM dbo.inventory i WITH (UPDLOCK, HOLDLOCK)
                WHERE i.film_id=@FilmId AND i.is_active=1
                  AND NOT EXISTS (SELECT 1 FROM dbo.rental r WITH (UPDLOCK, HOLDLOCK)
                                  WHERE r.inventory_id=i.inventory_id AND r.return_date IS NULL)
                ORDER BY i.inventory_id;
                """;
            int inventoryId;
            await using (var command = new SqlCommand(availableSql, connection, transaction))
            {
                command.Parameters.AddWithValue("@FilmId", filmId);
                var result = await command.ExecuteScalarAsync();
                // Sin copia libre se revierte y el controlador informa que no hay disponibilidad.
                if (result is null) { await transaction.RollbackAsync(); return null; }
                inventoryId = Convert.ToInt32(result);
            }

            // Registra la renta asociando copia, cliente y empleado dentro de la misma transacción.
            const string insertSql = """
                INSERT INTO dbo.rental (rental_date, inventory_id, customer_id, return_date, staff_id, last_update)
                OUTPUT INSERTED.rental_id
                VALUES (GETDATE(), @InventoryId, @CustomerId, NULL, @StaffId, GETDATE());
                """;
            int rentalId;
            await using (var command = new SqlCommand(insertSql, connection, transaction))
            {
                command.Parameters.AddWithValue("@InventoryId", inventoryId);
                command.Parameters.AddWithValue("@CustomerId", customerId);
                command.Parameters.AddWithValue("@StaffId", staffId);
                rentalId = Convert.ToInt32(await command.ExecuteScalarAsync());
            }
            // Solo se confirma cuando la insercion del alquiler concluyo correctamente.
            await transaction.CommitAsync();
            return rentalId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Obtiene el historial filtrando por el identificador del cliente autenticado.
    public async Task<IReadOnlyList<Alquiler>> MisAlquileresAsync(int customerId)
    {
        const string sql = """
            SELECT r.rental_id, r.inventory_id, f.title, r.rental_date, r.return_date
            FROM dbo.rental r
            INNER JOIN dbo.inventory i ON i.inventory_id=r.inventory_id
            INNER JOIN dbo.film f ON f.film_id=i.film_id
            WHERE r.customer_id=@CustomerId
            ORDER BY r.rental_date DESC;
            """;
        var results = new List<Alquiler>();
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) results.Add(Map(reader));
        return results;
    }

    // Actualiza solo una renta pendiente que pertenezca al cliente indicado.
    public async Task<bool> DevolverAsync(int rentalId, int customerId)
    {
        const string sql = """
            UPDATE dbo.rental SET return_date=GETDATE(), last_update=GETDATE()
            -- El UPDATE solo afecta una renta del cliente actual que siga pendiente.
            WHERE rental_id=@RentalId AND customer_id=@CustomerId AND return_date IS NULL;
            """;
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@RentalId", rentalId);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() == 1;
    }

    // Consulta las rentas recientes incluyendo el nombre del cliente.
    public async Task<IReadOnlyList<Alquiler>> TodosAsync()
    {
        const string sql = """
            SELECT TOP (200) r.rental_id, r.inventory_id, f.title, r.rental_date, r.return_date,
                   CONCAT(c.first_name, ' ', c.last_name) AS Cliente
            FROM dbo.rental r
            INNER JOIN dbo.inventory i ON i.inventory_id=r.inventory_id
            INNER JOIN dbo.film f ON f.film_id=i.film_id
            INNER JOIN dbo.customer c ON c.customer_id=r.customer_id
            ORDER BY r.rental_date DESC;
            """;
        var results = new List<Alquiler>();
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var rental = Map(reader);
            rental.Cliente = reader.GetString(5);
            results.Add(rental);
        }
        return results;
    }

    // Agrupa y cuenta rentas por película para generar el reporte top diez.
    public async Task<IReadOnlyList<Pelicula>> MasAlquiladasAsync()
    {
        const string sql = """
            SELECT TOP (10) f.film_id, f.title, COUNT(r.rental_id) AS TotalAlquileres
            FROM dbo.film f
            INNER JOIN dbo.inventory i ON i.film_id=f.film_id
            INNER JOIN dbo.rental r ON r.inventory_id=i.inventory_id
            WHERE f.is_active=1
            GROUP BY f.film_id, f.title
            ORDER BY COUNT(r.rental_id) DESC, f.title;
            """;
        var results = new List<Pelicula>();
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) results.Add(new Pelicula { FilmId = Convert.ToInt32(reader.GetValue(0)), Title = reader.GetString(1), RentalCount = reader.GetInt32(2) });
        return results;
    }

    // Convierte las columnas de un SqlDataReader al modelo de película.
    private static Alquiler Map(SqlDataReader reader) => new()
    {
        RentalId = Convert.ToInt32(reader.GetValue(0)), InventoryId = Convert.ToInt32(reader.GetValue(1)),
        Pelicula = reader.GetString(2), RentalDate = reader.GetDateTime(3),
        ReturnDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
    };
}
