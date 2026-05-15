using Microsoft.AspNetCore.Mvc;
using Biblioteca.Data;
using Biblioteca.Models; // Certifique-Object se o namespace da Model Usuario está correto
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biblioteca.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Entrar(string email, string senha)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Email == email && u.Senha == senha);

            if (usuario != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nome),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Perfil)
                };

                var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
                await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity));

                return Json(new { sucesso = true });
            }

            return Json(new { sucesso = false, mensagem = "E-mail ou senha incorretos!" });
        }

        public async Task<IActionResult> Sair()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index");
        }

        // --- NOVO: TELA DE MINHA CONTA ---
        [Authorize]
        public IActionResult MinhaConta()
        {
            var emailLogado = User.FindFirst(ClaimTypes.Email)?.Value;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == emailLogado);

            if (usuario == null) return RedirectToAction("Index");

            return View(usuario);
        }

        [HttpPost]
        [Authorize]
        public IActionResult AtualizarPerfil(int id, string nome, string email, string senha)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario == null) return Json(new { sucesso = false, mensagem = "Usuário não encontrado" });

            var emailLogado = User.FindFirst(ClaimTypes.Email)?.Value;
            if (usuario.Email != emailLogado && !User.IsInRole("Admin")) 
                return Json(new { sucesso = false, mensagem = "Acesso negado" });

            usuario.Nome = nome;
            usuario.Email = email;
            if (!string.IsNullOrEmpty(senha)) usuario.Senha = senha;

            _context.Usuarios.Update(usuario);
            _context.SaveChanges();

            return Json(new { sucesso = true });
        }

        // --- MÉTODO DE EMERGÊNCIA PARA RECRIAR O ADMIN ---
        [AllowAnonymous]
        public IActionResult CriarAdminUrgente()
        {
            // Verifica se o admin já existe para não criar duplicado
            var jaExiste = _context.Usuarios.Any(u => u.Email == "admin@admin.com");
            
            if (jaExiste) 
                return Content("O usuário admin@admin.com já existe no banco de dados.");

            var novoAdmin = new Usuario
            {
                Nome = "Administrador Geral",
                Email = "admin@admin.com",
                Senha = "123", // Lembre-se de mudar depois
                Perfil = "Admin"
            };

            _context.Usuarios.Add(novoAdmin);
            _context.SaveChanges();

            return Content("Admin criado com sucesso! E-mail: admin@admin.com | Senha: 123. Tente logar agora.");
        }
    }
}