// COMENTARIO EXPLICATIVO: Modelo de lectura que transporta los datos de un alquiler desde el repositorio hasta la vista.
namespace ProyectoU1.Models;

public sealed class Alquiler
{
    // Dato del modelo: alquiler  identificador.
    public int RentalId { get; set; }
    // Dato del modelo: Inventory identificador.
    public int InventoryId { get; set; }
    // Dato del modelo: película.
    public string Pelicula { get; set; } = string.Empty;
    // Dato del modelo: alquiler  fecha.
    public DateTime RentalDate { get; set; }
    // Dato del modelo: Return fecha.
    public DateTime? ReturnDate { get; set; }
    // Dato del modelo: Cliente.
    public string? Cliente { get; set; }
}
