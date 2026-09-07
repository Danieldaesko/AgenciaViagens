using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgenciaViagens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrigemPacote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Origem",
                table: "Pacotes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Origem",
                table: "Pacotes");
        }
    }
}
