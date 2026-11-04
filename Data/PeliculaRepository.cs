// COMENTARIO EXPLICATIVO: Acceso ADO.NET a películas, idiomas e inventario; centraliza filtros de disponibilidad y eliminación lógica.
using Microsoft.Data.SqlClient;
using ProyectoU1.Models;

namespace ProyectoU1.Data;

public sealed class PeliculaRepository(IConfiguration configuration)
{
    private string ConnectionString => configuration.GetConnectionString("Sakila")
        ?? throw new InvalidOperationException("Falta configurar ConnectionStrings:Sakila.");

    private const string SelectColumns = """
        SELECT f.film_id, f.title, f.description, f.release_year, f.language_id,
               f.rental_duration, f.rental_rate, f.length, f.replacement_cost, f.rating,
               (SELECT COUNT(*) FROM dbo.inventory i
                WHERE i.film_id=f.film_id AND i.is_active=1) AS TotalCopies,
               (SELECT COUNT(*) FROM dbo.inventory i
                WHERE i.film_id=f.film_id AND i.is_active=1
                  AND NOT EXISTS (SELECT 1 FROM dbo.rental r
                                  WHERE r.inventory_id=i.inventory_id AND r.return_date IS NULL)) AS AvailableCopies,
               f.is_active
        FROM dbo.film f
        """ + " ";

    // Consulta las filas del modulo; utiliza parámetros para filtros recibidos.
    public async Task<IReadOnlyList<Pelicula>> ListarAsync(string? busqueda = null, bool incluirInactivas = false)
    {
        const string sql = SelectColumns + """
            WHERE (@Buscar IS NULL OR f.title LIKE @Buscar)
              AND (@IncluirInactivas = 1 OR f.is_active = 1)
            ORDER BY f.title;
            """;
        var resultados = new List<Pelicula>();
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Buscar", string.IsNullOrWhiteSpace(busqueda) ? DBNull.Value : $"%{busqueda.Trim()}%");
        command.Parameters.AddWithValue("@IncluirInactivas", incluirInactivas);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) resultados.Add(Map(reader));
        return resultados;
    }

    // Busca una entidad por clave y devuelve null cuando no existe.
    public async Task<Pelicula?> ObtenerAsync(int id, bool incluirInactiva = false)
    {
        var sql = SelectColumns + """
            WHERE f.film_id = @Id AND (@IncluirInactiva = 1 OR f.is_active = 1)
            ;
            """;
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@IncluirInactiva", incluirInactiva);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task<IReadOnlyList<(int Id, string Nombre)>> ListarIdiomasAsync()
    {
        const string sql = "SELECT language_id, name FROM dbo.language ORDER BY name;";
        var idiomas = new List<(int Id, string Nombre)>();
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            idiomas.Add((Convert.ToInt32(reader.GetValue(0)), Convert.ToString(reader.GetValue(1))?.Trim() ?? string.Empty));
        }
        return idiomas;
    }

    // Inserta una película mediante parámetros y devuelve el identificador generado.
    public async Task<int> CrearAsync(Pelicula pelicula)
    {
        const string sql = """
            INSERT INTO dbo.film (title, description, release_year, language_id, rental_duration,
                                  rental_rate, length, replacement_cost, rating, last_update, is_active)
            OUTPUT INSERTED.film_id
            VALUES (@Title, @Description, @ReleaseYear, @LanguageId, @RentalDuration,
                    @RentalRate, @Length, @ReplacementCost, @Rating, GETDATE(), 1);
            """;
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        AddFilmParameters(command, pelicula);
        await connection.OpenAsync();
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    // Actualiza los datos de una película activa y comunica si hubo una fila afectada.
    public async Task<bool> ActualizarAsync(Pelicula pelicula)
    {
        const string sql = """
            UPDATE dbo.film
            SET title=@Title, description=@Description, release_year=@ReleaseYear, language_id=@LanguageId,
                rental_duration=@RentalDuration, rental_rate=@RentalRate, length=@Length,
                replacement_cost=@ReplacementCost, rating=@Rating, last_update=GETDATE()
            WHERE film_id=@Id AND is_active=1;
            """;
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        AddFilmParameters(command, pelicula);
        command.Parameters.AddWithValue("@Id", pelicula.FilmId);
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() == 1;
    }

    // Implementa la operación DesactivarAsync dentro de este componente.
    public async Task<bool> DesactivarAsync(int id)
    {
        const string sql = "UPDATE dbo.film SET is_active=0, last_update=GETDATE() WHERE film_id=@Id AND is_active=1;";
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() == 1;
    }

    // Activa nuevamente una película deshabilitada sin recrear su registro.
    public async Task<bool> ReactivarAsync(int id)
    {
        const string sql = "UPDATE dbo.film SET is_active=1, last_update=GETDATE() WHERE film_id=@Id AND is_active=0;";
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() == 1;
    }

    // Lee las copias activas e indica si cada una tiene alquiler pendiente.
    public async Task<IReadOnlyList<CopiaInventario>> InventarioAsync(int filmId)
    {
        const string sql = """
            SELECT i.inventory_id, i.film_id, f.title, i.store_id,
                   CAST(CASE WHEN r.rental_id IS NULL THEN 0 ELSE 1 END AS bit) AS IsRented
            FROM dbo.inventory i
            INNER JOIN dbo.film f ON f.film_id=i.film_id
            OUTER APPLY (SELECT TOP (1) rental_id FROM dbo.rental r
                         WHERE r.inventory_id=i.inventory_id AND r.return_date IS NULL) r
            WHERE i.film_id=@FilmId AND i.is_active=1
            ORDER BY i.inventory_id;
            """;
        var items = new List<CopiaInventario>();
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FilmId", filmId);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new CopiaInventario
            {
                InventoryId = Convert.ToInt32(reader.GetValue(0)), FilmId = Convert.ToInt32(reader.GetValue(1)), FilmTitle = reader.GetString(2),
                StoreId = Convert.ToInt32(reader.GetValue(3)), IsRented = Convert.ToBoolean(reader.GetValue(4))
            });
        }
        return items;
    }

    // Inserta una copia de inventario con los identificadores validados por el controlador.
    public async Task AgregarCopiaAsync(int filmId, int storeId)
    {
        const string sql = "INSERT INTO dbo.inventory (film_id, store_id, last_update, is_active) VALUES (@FilmId, @StoreId, GETDATE(), 1);";
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FilmId", filmId);
        command.Parameters.AddWithValue("@StoreId", storeId);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    // Desactiva una copia solo cuando no existe un alquiler abierto asociado.
    public async Task<bool> DesactivarCopiaAsync(int inventoryId)
    {
        const string sql = """
            UPDATE i SET is_active=0, last_update=GETDATE()
            FROM dbo.inventory i
            WHERE i.inventory_id=@Id AND i.is_active=1
              AND NOT EXISTS (SELECT 1 FROM dbo.rental r WHERE r.inventory_id=i.inventory_id AND r.return_date IS NULL);
            """;
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", inventoryId);
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() == 1;
    }

    // Asigna cada campo de película a un parametro SQL para evitar concatenar datos.
    private static void AddFilmParameters(SqlCommand command, Pelicula p)
    {
        command.Parameters.AddWithValue("@Title", p.Title.Trim());
        command.Parameters.AddWithValue("@Description", (object?)p.Description?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@ReleaseYear", (object?)p.ReleaseYear ?? DBNull.Value);
        command.Parameters.AddWithValue("@LanguageId", p.LanguageId);
        command.Parameters.AddWithValue("@RentalDuration", p.RentalDuration);
        command.Parameters.AddWithValue("@RentalRate", p.RentalRate);
        command.Parameters.AddWithValue("@Length", (object?)p.Length ?? DBNull.Value);
        command.Parameters.AddWithValue("@ReplacementCost", p.ReplacementCost);
        command.Parameters.AddWithValue("@Rating", p.Rating);
    }

    // Convierte las columnas de un SqlDataReader al modelo de película.
    private static Pelicula Map(SqlDataReader r) => new()
    {
        FilmId = Convert.ToInt32(r.GetValue(0)),
        Title = Convert.ToString(r.GetValue(1)) ?? string.Empty,
        Description = r.IsDBNull(2) ? null : Convert.ToString(r.GetValue(2)),
        ReleaseYear = r.IsDBNull(3) ? null : Convert.ToInt32(r.GetValue(3)),
        LanguageId = Convert.ToInt32(r.GetValue(4)),
        RentalDuration = Convert.ToInt32(r.GetValue(5)),
        RentalRate = Convert.ToDecimal(r.GetValue(6)),
        Length = r.IsDBNull(7) ? null : Convert.ToInt32(r.GetValue(7)),
        ReplacementCost = Convert.ToDecimal(r.GetValue(8)),
        Rating = r.IsDBNull(9) ? "G" : Convert.ToString(r.GetValue(9))?.Trim() ?? "G",
        TotalCopies = Convert.ToInt32(r.GetValue(10)),
        AvailableCopies = Convert.ToInt32(r.GetValue(11)),
        IsActive = Convert.ToBoolean(r.GetValue(12))
    };
}
