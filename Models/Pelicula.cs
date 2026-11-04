// COMENTARIO EXPLICATIVO: Modelo de película para formularios, catálogo e informes; DataAnnotations limita valores ingresados.
using System.ComponentModel.DataAnnotations;

namespace ProyectoU1.Models;

public sealed class Pelicula
{
    // Identificador de la película en Sakila (film_id).
    public int FilmId { get; set; }

    [Required, StringLength(255, MinimumLength = 2)]
    [Display(Name = "Título")]
    // Dato del modelo: titulo.
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Descripción")]
    // Dato del modelo: descripcion.
    public string? Description { get; set; }

    [Range(1888, 2100)]
    [Display(Name = "Año de lanzamiento")]
    // Dato del modelo: ReleaseYear.
    public int? ReleaseYear { get; set; }

    [Range(1, 255)]
    [Display(Name = "Idioma (ID)")]
    // Dato del modelo: Language identificador.
    public int LanguageId { get; set; }

    [Range(1, 20)]
    [Display(Name = "Duración del alquiler (días)")]
    // Dato del modelo: alquiler Duration.
    public int RentalDuration { get; set; } = 3;

    [Range(0.01, 999.99, ErrorMessage = "La tarifa debe estar entre 0,01 y 999,99.")]
    [Display(Name = "Tarifa de alquiler")]
    // Dato del modelo: alquiler Rate.
    public decimal RentalRate { get; set; } = 4.99m;

    [Range(1, 1000)]
    [Display(Name = "Duración (minutos)")]
    // Dato del modelo: Length.
    public int? Length { get; set; }

    [Range(0.01, 999.99, ErrorMessage = "El costo debe estar entre 0,01 y 999,99.")]
    [Display(Name = "Costo de reposición")]
    // Dato del modelo: ReplacementCost.
    public decimal ReplacementCost { get; set; } = 19.99m;

    [Required]
    [RegularExpression("^(G|PG|PG-13|R|NC-17)$", ErrorMessage = "Seleccione una clasificación válida.")]
    [Display(Name = "Clasificación")]
    // Dato del modelo: Rating.
    public string Rating { get; set; } = "G";

    // Dato del modelo: total Copies.
    public int TotalCopies { get; set; }
    // Dato del modelo: disponibles Copies.
    public int AvailableCopies { get; set; }
    // Dato del modelo: alquiler Count.
    public int RentalCount { get; set; }
    // Dato del modelo: estado  activo.
    public bool IsActive { get; set; } = true;
}
