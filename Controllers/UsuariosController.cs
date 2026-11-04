// COMENTARIO EXPLICATIVO: Administra las cuentas de cliente, valida sus datos y guarda contraseñas como hashes.
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoU1.Data;
using ProyectoU1.Filters;
using ProyectoU1.Models;

namespace ProyectoU1.Controllers;

[SessionAuthorize("Administrador")]
public sealed class UsuariosController(UsuariosRepository usuarios, IPasswordHasher<UsuarioLogin> hasher) : Controller
{
    // Obtiene los datos necesarios para la pantalla principal de este modulo.
    public async Task<IActionResult> Index()
    {
        ViewBag.ClientesDisponibles = await usuarios.ClientesDisponiblesAsync();
        ViewBag.Cuentas = await usuarios.ListarAsync();
        return View(new CrearCuentaClienteViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    // Valida datos, calcula el hash de contraseña y crea la cuenta asociada.
    public async Task<IActionResult> CrearCliente(CrearCuentaClienteViewModel model)
    {
        if (!ModelState.IsValid) return await IndexWithModel(model);
        var user = new UsuarioLogin { NombreUsuario = model.NombreUsuario.Trim(), Rol = "Cliente", CustomerId = model.CustomerId };
        user.PasswordHash = hasher.HashPassword(user, model.Password);
        try
        {
            await usuarios.CrearClienteAsync(user.NombreUsuario, user.PasswordHash, model.CustomerId);
            TempData["Mensaje"] = "Cuenta de cliente creada.";
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2601 or 2627)
        {
            ModelState.AddModelError(nameof(model.NombreUsuario), "Ese nombre de usuario ya está registrado.");
            return await IndexWithModel(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.CustomerId), ex.Message);
            return await IndexWithModel(model);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    // Aplica baja lógica y conserva el registro histórico.
    public async Task<IActionResult> Desactivar(int id)
    {
        if (!await usuarios.DesactivarAsync(id)) TempData["Error"] = "No se encontró una cuenta de cliente activa.";
        else TempData["Mensaje"] = "Cuenta desactivada; los alquileres históricos se conservan.";
        return RedirectToAction(nameof(Index));
    }

    // Implementa la operación IndexWithModel dentro de este componente.
    private async Task<IActionResult> IndexWithModel(CrearCuentaClienteViewModel model)
    {
        ViewBag.ClientesDisponibles = await usuarios.ClientesDisponiblesAsync();
        ViewBag.Cuentas = await usuarios.ListarAsync();
        return View("Index", model);
    }
}
