// COMENTARIO: Controla el alta inicial del administrador, el inicio y el cierre de sesión, y el almacenamiento seguro del contexto autenticado.
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoU1.Data;
using ProyectoU1.Models;

namespace ProyectoU1.Controllers;

public sealed class AccountController(
    LoginRepository loginRepository,
    IPasswordHasher<UsuarioLogin> passwordHasher) : Controller
{
    [HttpGet]
    // Muestra el formulario de acceso y conserva el returnUrl local.
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["NeedsSetup"] = !await loginRepository.HayUsuariosAsync();
        return View(new LoginViewModel());
    }

    [HttpGet]
    // Solo permite configurar el administrador inicial si aun no existen cuentas.
    public async Task<IActionResult> Setup()
    {
        if (await loginRepository.HayUsuariosAsync()) return NotFound();
        return View(new CrearAdministradorViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Solo permite configurar el administrador inicial si aun no existen cuentas.
    public async Task<IActionResult> Setup(CrearAdministradorViewModel model)
    {
        if (await loginRepository.HayUsuariosAsync()) return NotFound();
        if (!ModelState.IsValid) return View(model);
        if (!await loginRepository.ExisteStaffActivoAsync(model.StaffId))
        {
            ModelState.AddModelError(nameof(model.StaffId), "Seleccione el ID de un empleado activo de Sakila.");
            return View(model);
        }

        var usuario = new UsuarioLogin { NombreUsuario = model.NombreUsuario.Trim(), Rol = "Administrador", StaffId = model.StaffId };
        usuario.PasswordHash = passwordHasher.HashPassword(usuario, model.Password);
        try
        {
            await loginRepository.CrearUsuarioAsync(usuario);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2601 or 2627)
        {
            ModelState.AddModelError(nameof(model.NombreUsuario), "Ese nombre de usuario ya existe.");
            return View(model);
        }
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Muestra el formulario de acceso y conserva el returnUrl local.
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["NeedsSetup"] = !await loginRepository.HayUsuariosAsync();
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = await loginRepository.BuscarPorNombreAsync(model.NombreUsuario.Trim());
        if (usuario is null || passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View(model);
        }

        HttpContext.Session.SetInt32("UsuarioId", usuario.UsuarioId);
        HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
        HttpContext.Session.SetString("Rol", usuario.Rol);
        if (usuario.CustomerId.HasValue)
        {
            HttpContext.Session.SetInt32("CustomerId", usuario.CustomerId.Value);
        }
        if (usuario.StaffId.HasValue)
        {
            HttpContext.Session.SetInt32("StaffId", usuario.StaffId.Value);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Catalogo");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Elimina los datos de Session para cerrar la sesión actual.
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
