using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Adicione este usando para o AnyAsync/ToListAsync

namespace Biblioteca.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class LivroController : Controller
    {
        private readonly AppDbContext _context;

        public LivroController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

       [HttpGet]
public IActionResult Listar()
{
    var livros = _context.Livros.Select(l => new {
        l.Id,
        // Se der erro aqui, tente trocar para l.ISBN (tudo maiúsculo)
        Isbn = l.ISBN, 
        l.Titulo,
        l.Autor,
        l.Ano,
        disponivel = !_context.Emprestimos.Any(e => e.LivroId == l.Id && e.DataDevolucao == null)
    }).ToList();

    return Json(livros);
}

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Salvar(Livro livro)
        {
            try
            {
                _context.Livros.Add(livro);
                _context.SaveChanges();
                return Json(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Excluir(int id)
        {
            try
            {
                var livro = _context.Livros.Find(id);
                if (livro == null) 
                    return Json(new { sucesso = false, mensagem = "Livro não encontrado." });

                // TRAVA DE SEGURANÇA:
                var estaEmprestado = _context.Emprestimos.Any(e => e.LivroId == id && e.DataDevolucao == null);
                if (estaEmprestado)
                {
                    return Json(new { sucesso = false, mensagem = "Ops! Este livro está emprestado e não pode ser excluído agora." });
                }

                _context.Livros.Remove(livro);
                _context.SaveChanges();
                
                return Json(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }
    }
}