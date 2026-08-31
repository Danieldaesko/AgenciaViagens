using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgenciaViagens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNovasTabelas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Avaliacao_Pacotes_PacoteId",
                table: "Avaliacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Avaliacao_Utilizadores_UtilizadorId",
                table: "Avaliacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Itinerario_Pacotes_PacoteId",
                table: "Itinerario");

            migrationBuilder.DropForeignKey(
                name: "FK_PontosTransacao_Utilizadores_UtilizadorId",
                table: "PontosTransacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Utilizadores_UtilizadorId",
                table: "Reservas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Utilizadores",
                table: "Utilizadores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Itinerario",
                table: "Itinerario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Avaliacao",
                table: "Avaliacao");

            migrationBuilder.RenameTable(
                name: "Utilizadores",
                newName: "Utilizador");

            migrationBuilder.RenameTable(
                name: "Itinerario",
                newName: "Itinerarios");

            migrationBuilder.RenameTable(
                name: "Avaliacao",
                newName: "Avaliacoes");

            migrationBuilder.RenameIndex(
                name: "IX_Itinerario_PacoteId",
                table: "Itinerarios",
                newName: "IX_Itinerarios_PacoteId");

            migrationBuilder.RenameIndex(
                name: "IX_Avaliacao_UtilizadorId",
                table: "Avaliacoes",
                newName: "IX_Avaliacoes_UtilizadorId");

            migrationBuilder.RenameIndex(
                name: "IX_Avaliacao_PacoteId",
                table: "Avaliacoes",
                newName: "IX_Avaliacoes_PacoteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Utilizador",
                table: "Utilizador",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Itinerarios",
                table: "Itinerarios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Avaliacoes",
                table: "Avaliacoes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Avaliacoes_Pacotes_PacoteId",
                table: "Avaliacoes",
                column: "PacoteId",
                principalTable: "Pacotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Avaliacoes_Utilizador_UtilizadorId",
                table: "Avaliacoes",
                column: "UtilizadorId",
                principalTable: "Utilizador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Itinerarios_Pacotes_PacoteId",
                table: "Itinerarios",
                column: "PacoteId",
                principalTable: "Pacotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PontosTransacao_Utilizador_UtilizadorId",
                table: "PontosTransacao",
                column: "UtilizadorId",
                principalTable: "Utilizador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Utilizador_UtilizadorId",
                table: "Reservas",
                column: "UtilizadorId",
                principalTable: "Utilizador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Avaliacoes_Pacotes_PacoteId",
                table: "Avaliacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Avaliacoes_Utilizador_UtilizadorId",
                table: "Avaliacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Itinerarios_Pacotes_PacoteId",
                table: "Itinerarios");

            migrationBuilder.DropForeignKey(
                name: "FK_PontosTransacao_Utilizador_UtilizadorId",
                table: "PontosTransacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Utilizador_UtilizadorId",
                table: "Reservas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Utilizador",
                table: "Utilizador");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Itinerarios",
                table: "Itinerarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Avaliacoes",
                table: "Avaliacoes");

            migrationBuilder.RenameTable(
                name: "Utilizador",
                newName: "Utilizadores");

            migrationBuilder.RenameTable(
                name: "Itinerarios",
                newName: "Itinerario");

            migrationBuilder.RenameTable(
                name: "Avaliacoes",
                newName: "Avaliacao");

            migrationBuilder.RenameIndex(
                name: "IX_Itinerarios_PacoteId",
                table: "Itinerario",
                newName: "IX_Itinerario_PacoteId");

            migrationBuilder.RenameIndex(
                name: "IX_Avaliacoes_UtilizadorId",
                table: "Avaliacao",
                newName: "IX_Avaliacao_UtilizadorId");

            migrationBuilder.RenameIndex(
                name: "IX_Avaliacoes_PacoteId",
                table: "Avaliacao",
                newName: "IX_Avaliacao_PacoteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Utilizadores",
                table: "Utilizadores",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Itinerario",
                table: "Itinerario",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Avaliacao",
                table: "Avaliacao",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Avaliacao_Pacotes_PacoteId",
                table: "Avaliacao",
                column: "PacoteId",
                principalTable: "Pacotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Avaliacao_Utilizadores_UtilizadorId",
                table: "Avaliacao",
                column: "UtilizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Itinerario_Pacotes_PacoteId",
                table: "Itinerario",
                column: "PacoteId",
                principalTable: "Pacotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PontosTransacao_Utilizadores_UtilizadorId",
                table: "PontosTransacao",
                column: "UtilizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Utilizadores_UtilizadorId",
                table: "Reservas",
                column: "UtilizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
