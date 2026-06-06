using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatsForDinner.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPreparationField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "preparation",
                table: "recipes",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "recipes",
                keyColumn: "id",
                keyValue: 1,
                column: "preparation",
                value: null);

            migrationBuilder.UpdateData(
                table: "recipes",
                keyColumn: "id",
                keyValue: 2,
                column: "preparation",
                value: null);

            migrationBuilder.UpdateData(
                table: "recipes",
                keyColumn: "id",
                keyValue: 3,
                column: "preparation",
                value: null);

            migrationBuilder.UpdateData(
                table: "recipes",
                keyColumn: "id",
                keyValue: 4,
                column: "preparation",
                value: null);

            migrationBuilder.UpdateData(
                table: "recipes",
                keyColumn: "id",
                keyValue: 5,
                column: "preparation",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "preparation",
                table: "recipes");
        }
    }
}
