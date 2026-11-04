// COMENTARIO EXPLICATIVO: Prepara el catálogo publico y el detalle de una película para sus vistas.
using Microsoft.AspNetCore.Mvc;
using ProyectoU1.Data;

namespace ProyectoU1.Controllers;

public sealed class CatalogoController(PeliculaRepository peliculas) : Controller
{
    // Obtiene los datos necesarios para la pantalla principal de este modulo.
    public async Task<IActionResult> Index(string? q) => View(await peliculas.ListarAsync(q));

    // Busca una película por identificador y responde NotFound si no existe.
    public async Task<IActionResult> Detalle(int id)
    {
        var pelicula = await peliculas.ObtenerAsync(id);
        return pelicula is null ? NotFound() : View(pelicula);
    }
}
