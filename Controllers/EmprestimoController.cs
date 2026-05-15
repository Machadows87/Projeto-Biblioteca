using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Biblioteca.Models;
using Biblioteca.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Biblioteca.Controllers
{
    [Authorize]
    public class EmprestimoController : Controller
    {
        private readonly AppDbContext _context;

        public EmprestimoController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var emailLogado = User.FindFirst(ClaimTypes.Email)?.Value;
            var usuarioLogado = _context.Usuarios.FirstOrDefault(u => u.Email == emailLogado);

            if (usuarioLogado == null) return Json(new List<object>());

            var consulta = _context.Emprestimos
                .Include(e => e.Livro)
                .Include(e => e.Usuario)
                .AsQueryable();

            // REGRA DE PRIVACIDADE: Leitor só vê os seus próprios registros
            if (!User.IsInRole("Admin"))
            {
                consulta = consulta.Where(e => e.UsuarioId == usuarioLogado.Id);
            }

            var emprestimos = await consulta
                .Select(e => new {
                    id = e.Id,
                    livro = e.Livro != null ? e.Livro.Titulo : "Excluído",
                    usuario = e.Usuario != null ? e.Usuario.Nome : "Excluído",
                    dataEmprestimo = e.DataEmprestimo.ToString("dd/MM/yyyy"),
                    status = e.DataDevolucao == null ? "Pendente" : "Devolvido"
                }).ToListAsync();

            return Json(emprestimos);
        }

        [HttpGet]
        public async Task<IActionResult> ObterDadosParaEmprestimo()
        {
            var livrosEmprestadosIds = await _context.Emprestimos
                .Where(e => e.DataDevolucao == null)
                .Select(e => e.LivroId)
                .ToListAsync();

            var livros = await _context.Livros
                .Where(l => !livrosEmprestadosIds.Contains(l.Id))
                .Select(l => new { l.Id, l.Titulo })
                .ToListAsync();

            object usuarios = null;
            if (User.IsInRole("Admin"))
            {
                usuarios = await _context.Usuarios.Select(u => new { u.Id, u.Nome }).ToListAsync();
            }

            return Json(new { livros, usuarios });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Salvar(int LivroId, int? UsuarioId)
        {
            try 
            {
                var emailLogado = User.FindFirst(ClaimTypes.Email)?.Value;
                var usuarioLogado = _context.Usuarios.FirstOrDefault(u => u.Email == emailLogado);

                if (usuarioLogado == null) 
                    return Json(new { sucesso = false, mensagem = "Erro ao identificar usuário." });

                int idFinal = (User.IsInRole("Admin") && UsuarioId.HasValue) 
                              ? UsuarioId.Value 
                              : usuarioLogado.Id;

                var jaEmprestado = await _context.Emprestimos
                    .AnyAsync(e => e.LivroId == LivroId && e.DataDevolucao == null);

                if (jaEmprestado)
                    return Json(new { sucesso = false, mensagem = "Este livro já está emprestado!" });

                var novoEmprestimo = new Emprestimo
                {
                    LivroId = LivroId,
                    UsuarioId = idFinal,
                    DataEmprestimo = DateTime.Now
                };

                _context.Emprestimos.Add(novoEmprestimo);
                await _context.SaveChangesAsync();

                return Json(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Erro: " + ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Devolver(int id)
        {
            var emprestimo = await _context.Emprestimos
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprestimo == null || emprestimo.DataDevolucao != null) 
                return Json(new { sucesso = false, mensagem = "Empréstimo inválido ou já devolvido." });

            // SEGURANÇA: Se não for Admin, só pode devolver se o empréstimo for dele
            if (!User.IsInRole("Admin"))
            {
                var emailLogado = User.FindFirst(ClaimTypes.Email)?.Value;
                if (emprestimo.Usuario?.Email != emailLogado)
                {
                    return Json(new { sucesso = false, mensagem = "Você não tem permissão para devolver este livro." });
                }
            }

            emprestimo.DataDevolucao = DateTime.Now;
            _context.Emprestimos.Update(emprestimo);
            await _context.SaveChangesAsync();

            return Json(new { sucesso = true });
        }
    }
}