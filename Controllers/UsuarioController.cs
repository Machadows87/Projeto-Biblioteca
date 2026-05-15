using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Adicionado para facilitar verificações assíncronas
using System.Linq;

namespace Biblioteca.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public IActionResult Listar()
        {
            return Json(_context.Usuarios.ToList());
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Salvar(Usuario usuario)
        {
            try
            {
                if (usuario.Id == 0)
                {
                    _context.Usuarios.Add(usuario);
                }
                else
                {
                    _context.Usuarios.Update(usuario);
                }

                _context.SaveChanges();
                return Json(new { sucesso = true });
            }
            catch (Exception ex)
            {
                var mensagemErro = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { sucesso = false, mensagem = mensagemErro });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Excluir(int id)
        {
            try
            {
                // 1. VERIFICAÇÃO DE PENDÊNCIAS (Lógica B)
                // Verificamos se existe algum empréstimo para este usuário que ainda não foi devolvido
                var temLivroPendente = _context.Emprestimos
                    .Any(e => e.UsuarioId == id && e.DataDevolucao == null);

                if (temLivroPendente)
                {
                    return Json(new { 
                        sucesso = false, 
                        mensagem = "Não é possível excluir este usuário pois ele possui livros pendentes de devolução!" 
                    });
                }

                // 2. BUSCA O USUÁRIO
                var user = _context.Usuarios.Find(id);
                
                if (user == null)
                {
                    return Json(new { sucesso = false, mensagem = "Usuário não encontrado." });
                }

                // 3. REMOVE SE ESTIVER TUDO OK
                _context.Usuarios.Remove(user);
                _context.SaveChanges();

                return Json(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    sucesso = false, 
                    mensagem = "Erro ao excluir: " + ex.Message 
                });
            }
        }
    }
}