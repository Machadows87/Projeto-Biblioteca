using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Biblioteca.Models;
using Microsoft.AspNetCore.Authorization;

namespace Biblioteca.Controllers
{
    [Authorize] // Garante que só quem logou veja a Home
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Esta é a página que criamos para barrar o acesso indevido
        [AllowAnonymous] // Permite que o erro seja exibido mesmo se houver bug no cookie
        public IActionResult AcessoNegado()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}