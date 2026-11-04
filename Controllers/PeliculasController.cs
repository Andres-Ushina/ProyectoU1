// COMENTARIO EXPLICATIVO: Acciones administrativas para crear, editar, activar o desactivar películas y gestionar copias de inventario.
using Microsoft.AspNetCore.Mvc;
using ProyectoU1.Data;
using ProyectoU1.Filters;
using ProyectoU1.Models;

namespace ProyectoU1.Controllers;

[SessionAuthorize("Administrador")]
public sealed class PeliculasController(PeliculaRepository peliculas) : Controller
{
    // Obtiene los datos necesarios para la pantalla principal de este modulo.
    public async Task<IActionResult> Index(string? q) => View(await peliculas.ListarAsync(q, incluirInactivas: true));

    [HttpGet]
    // Valida el formulario y registra una nueva entidad cuando ModelState es valido.
    public async Task<IActionResult> Crear()
    {
        await CargarIdiomasAsync();
        return View("Formulario", new Pelicula());
    }

    [HttpPost, ValidateAntiForgeryToken]
    // Valida el formulario y registra una nueva entidad cuando ModelState es valido.
    public async Task<IActionResult> Crear(Pelicula pelicula)
    {
        if (!ModelState.IsValid)
        {
            await CargarIdiomasAsync();
            return View("Formulario", pelicula);
        }
        await peliculas.CrearAsync(pelicula);
        TempData["Mensaje"] = "Película creada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    // Carga o actualiza la entidad; comprueba que la ruta y el modelo tengan el mismo identificador.
    public async Task<IActionResult> Editar(int id)
    {
        var pelicula = await peliculas.ObtenerAsync(id, incluirInactiva: true);
        if (pelicula is null) return NotFound();
        await CargarIdiomasAsync();
        return View("Formulario", pelicula);
    }

    [HttpPost, ValidateAntiForgeryToken]
    // Carga o actualiza la entidad; comprueba que la ruta y el modelo tengan el mismo identificador.
    public async Task<IActionResult> Editar(int id, Pelicula pelicula)
    {
        if (id != pelicula.FilmId) return BadRequest();
        if (!ModelState.IsValid)
        {
            await CargarIdiomasAsync();
            return View("Formulario", pelicula);
        }
        if (!await peliculas.ActualizarAsync(pelicula)) return NotFound();
        TempData["Mensaje"] = "Cambios guardados.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    // Aplica baja lógica y conserva el registro histórico.
    public async Task<IActionResult> Desactivar(int id)
    {
        if (!await peliculas.DesactivarAsync(id)) TempData["Error"] = "No se encontró la película activa.";
        else TempData["Mensaje"] = "Película desactivada; el registro histórico permanece en Sakila.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    // Vuelve a habilitar un registro previamente desactivado.
    public async Task<IActionResult> Reactivar(int id)
    {
        if (!await peliculas.ReactivarAsync(id)) TempData["Error"] = "No se encontró una película desactivada.";
        else TempData["Mensaje"] = "Película reactivada y disponible nuevamente en el catálogo.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    // Prepara la película y sus copias para la vista de inventario.
    public async Task<IActionResult> Inventario(int id)
    {
        var pelicula = await peliculas.ObtenerAsync(id, incluirInactiva: true);
        if (pelicula is null) return NotFound();
        ViewBag.Pelicula = pelicula;
        ViewBag.Copias = await peliculas.InventarioAsync(id);
        return View(new CopiaInventario { FilmId = id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    // Valida y agrega una copia al inventario de la película indicada.
    public async Task<IActionResult> AgregarCopia(int id, CopiaInventario model)
    {
        if (!ModelState.IsValid) return await Inventario(id);
        if (await peliculas.ObtenerAsync(id) is null) return NotFound();
        await peliculas.AgregarCopiaAsync(id, model.NewStoreId);
        TempData["Mensaje"] = "Copia agregada al inventario.";
        return RedirectToAction(nameof(Inventario), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    // Retira logicamente una copia solo si no tiene alquiler pendiente.
    public async Task<IActionResult> DesactivarCopia(int id, int filmId)
    {
        if (!await peliculas.DesactivarCopiaAsync(id)) TempData["Error"] = "La copia está alquilada, ya fue desactivada o no existe.";
        else TempData["Mensaje"] = "Copia desactivada lógicamente.";
        return RedirectToAction(nameof(Inventario), new { id = filmId });
    }

    // Implementa la operación CargarIdiomasAsync dentro de este componente.
    private async Task CargarIdiomasAsync()
    {
        ViewBag.Idiomas = await peliculas.ListarIdiomasAsync();
    }
}
