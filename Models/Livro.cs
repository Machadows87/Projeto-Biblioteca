namespace Biblioteca.Models
{
    public class Livro
    {
        public int Id { get; set; }

        // O "?" evita o erro de "entity changes" se o campo estiver vazio
        public string? ISBN { get; set; }

        public string? Titulo { get; set; }

        public string? Autor { get; set; }

        public int? Ano { get; set; }
    }
}