using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string Nome { get; set; }

        [StringLength(150)] // Isso resolve o erro de incompatibilidade no "Email == ..."
        public string Email { get; set; }

        [StringLength(100)]
        public string Senha { get; set; }

        [StringLength(50)]
        public string Perfil { get; set; } // Aqui salvaremos "Admin" ou "Leitor"
    }
}