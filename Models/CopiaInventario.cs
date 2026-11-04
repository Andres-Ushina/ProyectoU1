// COMENTARIO EXPLICATIVO: Modelo de una copia de inventario y sus datos de estado; incluye la validación de la tienda al crear copias.
using System.ComponentModel.DataAnnotations;

namespace ProyectoU1.Models;

public sealed class CopiaInventario
{
    // Dato del modelo: Inventory identificador.
    public int InventoryId { get; set; }
    // Identificador de la película en Sakila (film_id).
    public int FilmId { get; set; }
    // Dato del modelo: película  titulo.
    public string FilmTitle { get; set; } = string.Empty;
    // Dato del modelo: tienda  identificador.
    public int StoreId { get; set; }
    // Dato del modelo: estado  alquilada.
    public bool IsRented { get; set; }

    [Range(1, 255, ErrorMessage = "Seleccione una tienda válida.")]
    [Display(Name = "Tienda (ID)")]
    // Dato del modelo: New tienda  identificador.
    public int NewStoreId { get; set; } = 1;
}
