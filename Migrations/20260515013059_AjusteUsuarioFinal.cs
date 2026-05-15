using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblioteca.Migrations
{
    /// <inheritdoc />
    public partial class AjusteUsuarioFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Cidade",
                table: "Usuarios",
                newName: "Senha");

            migrationBuilder.RenameColumn(
                name: "CPF",
                table: "Usuarios",
                newName: "Perfil");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Senha",
                table: "Usuarios",
                newName: "Cidade");

            migrationBuilder.RenameColumn(
                name: "Perfil",
                table: "Usuarios",
                newName: "CPF");
        }
    }
}
