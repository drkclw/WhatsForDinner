using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WhatsForDinner.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "recipes",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "recipes",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "recipes",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "recipes",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "recipes",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "weekly_plans",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "name",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "display_name",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "users",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "google_id",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_login_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<string>(
                name: "picture_url",
                table: "users",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_users_google_id",
                table: "users",
                column: "google_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_users_google_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "display_name",
                table: "users");

            migrationBuilder.DropColumn(
                name: "email",
                table: "users");

            migrationBuilder.DropColumn(
                name: "google_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "last_login_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "picture_url",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "created_at", "name" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Default User" });

            migrationBuilder.InsertData(
                table: "recipes",
                columns: new[] { "id", "cook_time_minutes", "created_at", "description", "ingredients", "name", "preparation", "updated_at", "user_id" },
                values: new object[,]
                {
                    { 1, 45, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Classic Italian pasta with meat sauce", "Spaghetti\nGround beef\nTomato sauce\nOnion\nGarlic\nOlive oil\nSalt\nPepper\nParmesan", "Spaghetti Bolognese", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 2, 25, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fresh salad with grilled chicken breast", "Chicken breast\nMixed greens\nCherry tomatoes\nCucumber\nRed onion\nOlive oil\nLemon juice", "Grilled Chicken Salad", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 3, 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quick and healthy Asian-inspired dish", "Broccoli\nBell peppers\nCarrots\nSnap peas\nSoy sauce\nGarlic\nGinger\nSesame oil\nRice", "Vegetable Stir Fry", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 4, 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mexican-style tacos with seasoned beef", "Ground beef\nTaco shells\nLettuce\nTomatoes\nCheese\nSour cream\nTaco seasoning", "Beef Tacos", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 5, 25, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Simple Italian pizza with fresh ingredients", "Pizza dough\nTomato sauce\nFresh mozzarella\nBasil\nOlive oil", "Margherita Pizza", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 }
                });

            migrationBuilder.InsertData(
                table: "weekly_plans",
                columns: new[] { "id", "created_at", "updated_at", "user_id" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 });
        }
    }
}
