using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIDevSteamJau.Migrations
{
    /// <inheritdoc />
    public partial class preco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "precoOriginal",
                table: "Jogos",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "precoOriginal",
                table: "Jogos");
        }
    }
}
