// COMENTARIO EXPLICATIVO: Filtro MVC que exige Session y, cuando se especifica, valida el rol antes de ejecutar una accion.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProyectoU1.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SessionAuthorizeAttribute(string? requiredRole = null) : Attribute, IAuthorizationFilter
{
    // Redirige a Login si no hay Session y devuelve 403 cuando el rol no corresponde.
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Se consulta el contexto de la solicitud antes de permitir ejecutar la accion.
        var session = context.HttpContext.Session;
        // Sin identificador de usuario, se exige autenticación redirigiendo al formulario.
        if (session.GetInt32("UsuarioId") is null)
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
            return;
        }

        // La presencia de Session no basta: también se verifica el rol requerido.
        if (requiredRole is not null && session.GetString("Rol") != requiredRole)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
    }
}
