// COMENTARIO EXPLICATIVO: Acciones de las paginas generales y de la página de error de MVC.
using Microsoft.AspNetCore.Mvc;
using ProyectoU1.Models;
using System.Diagnostics;

namespace ProyectoU1.Controllers
{
    public class HomeController : Controller
    {
        // Obtiene los datos necesarios para la pantalla principal de este modulo.
        public IActionResult Index()
        {
            return View();
        }

        // Implementa la operación Privacy dentro de este componente.
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // Implementa la operación Error dentro de este componente.
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
