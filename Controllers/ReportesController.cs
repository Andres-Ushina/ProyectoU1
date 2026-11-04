// COMENTARIO EXPLICATIVO: Solicita los reportes administrativos y entrega sus resultados a las vistas.
using Microsoft.AspNetCore.Mvc;
using ProyectoU1.Data;
using ProyectoU1.Filters;

namespace ProyectoU1.Controllers;

[SessionAuthorize("Administrador")]
public sealed class ReportesController(AlquilerRepository alquileres) : Controller
{
    // Solicita el top de películas ordenadas por cantidad de rentas.
    public async Task<IActionResult> PeliculasMasAlquiladas() => View(await alquileres.MasAlquiladasAsync());

    // Solicita el listado de alquileres recientes para administración.
    public async Task<IActionResult> Alquileres() => View(await alquileres.TodosAsync());
}
