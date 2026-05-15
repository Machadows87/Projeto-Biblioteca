using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models
{
    public class Emprestimo
    {
        public int Id { get; set; }

        [Required]
        public int LivroId { get; set; }
        public Livro? Livro { get; set; } // Propriedade de Navegação

        [Required]
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; } // Propriedade de Navegação

        public DateTime DataEmprestimo { get; set; } = DateTime.Now;
        public DateTime? DataDevolucao { get; set; } // Se for NULL, o livro está com o usuário
    }
}