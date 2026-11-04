// COMENTARIO EXPLICATIVO: Expone las operaciones de cliente para alquilar, consultar sus propias rentas y devolverlas usando el CustomerId guardado en Session.
using Microsoft.AspNetCore.Mvc;
using ProyectoU1.Data;
using ProyectoU1.Filters;

namespace ProyectoU1.Controllers;

[SessionAuthorize("Cliente")]
public sealed class AlquileresController(AlquilerRepository alquileres, PeliculaRepository peliculas) : Controller
{
    [HttpPost, ValidateAntiForgeryToken]
    // Toma el cliente de Session y solicita al repositorio una copia disponible.
    public async Task<IActionResult> Alquilar(int filmId)
    {
        var customerId = HttpContext.Session.GetInt32("CustomerId");
        if (customerId is null) return StatusCode(StatusCodes.Status403Forbidden);
        if (await peliculas.ObtenerAsync(filmId) is null) return NotFound();
        var id = await alquileres.AlquilarAsync(filmId, customerId.Value);
        TempData[id.HasValue ? "Mensaje" : "Error"] = id.HasValue
            ? "Alquiler registrado correctamente."
            : "No hay copias disponibles de esa película.";
        return RedirectToAction("Detalle", "Catalogo", new { id = filmId });
    }

    // Consulta y muestra exclusivamente las rentas del cliente autenticado.
    public async Task<IActionResult> MisAlquileres()
    {
        var customerId = HttpContext.Session.GetInt32("CustomerId");
        if (customerId is null) return StatusCode(StatusCodes.Status403Forbidden);
        return View(await alquileres.MisAlquileresAsync(customerId.Value));
    }

    [HttpPost, ValidateAntiForgeryToken]
    // Pide registrar la devolucion verificando propiedad y estado pendiente.
    public async Task<IActionResult> Devolver(int id)
    {
        var customerId = HttpContext.Session.GetInt32("CustomerId");
        if (customerId is null) return StatusCode(StatusCodes.Status403Forbidden);
        if (!await alquileres.DevolverAsync(id, customerId.Value))
            TempData["Error"] = "No se pudo devolver: el alquiler no te pertenece o ya fue devuelto.";
        else TempData["Mensaje"] = "Devolución registrada.";
        return RedirectToAction(nameof(MisAlquileres));
    }
}
