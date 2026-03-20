using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WhatsForDinner.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recipes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ingredients = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    cook_time_minutes = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipes", x => x.id);
                    table.ForeignKey(
                        name: "FK_recipes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "weekly_plans",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weekly_plans", x => x.id);
                    table.ForeignKey(
                        name: "FK_weekly_plans_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "weekly_plan_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    weekly_plan_id = table.Column<int>(type: "integer", nullable: false),
                    recipe_id = table.Column<int>(type: "integer", nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weekly_plan_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_weekly_plan_items_recipes_recipe_id",
                        column: x => x.recipe_id,
                        principalTable: "recipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_weekly_plan_items_weekly_plans_weekly_plan_id",
                        column: x => x.weekly_plan_id,
                        principalTable: "weekly_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "created_at", "name" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Default User" });

            migrationBuilder.InsertData(
                table: "recipes",
                columns: new[] { "id", "cook_time_minutes", "created_at", "description", "ingredients", "name", "updated_at", "user_id" },
                values: new object[,]
                {
                    { 1, 45, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Classic Italian pasta with meat sauce", "Spaghetti\nGround beef\nTomato sauce\nOnion\nGarlic\nOlive oil\nSalt\nPepper\nParmesan", "Spaghetti Bolognese", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 2, 25, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fresh salad with grilled chicken breast", "Chicken breast\nMixed greens\nCherry tomatoes\nCucumber\nRed onion\nOlive oil\nLemon juice", "Grilled Chicken Salad", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 3, 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quick and healthy Asian-inspired dish", "Broccoli\nBell peppers\nCarrots\nSnap peas\nSoy sauce\nGarlic\nGinger\nSesame oil\nRice", "Vegetable Stir Fry", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 4, 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mexican-style tacos with seasoned beef", "Ground beef\nTaco shells\nLettuce\nTomatoes\nCheese\nSour cream\nTaco seasoning", "Beef Tacos", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 5, 25, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Simple Italian pizza with fresh ingredients", "Pizza dough\nTomato sauce\nFresh mozzarella\nBasil\nOlive oil", "Margherita Pizza", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 }
                });

            migrationBuilder.InsertData(
                table: "weekly_plans",
                columns: new[] { "id", "created_at", "updated_at", "user_id" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 });

            migrationBuilder.CreateIndex(
                name: "ix_recipes_user_id",
                table: "recipes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_weekly_plan_items_recipe_id",
                table: "weekly_plan_items",
                column: "recipe_id");

            migrationBuilder.CreateIndex(
                name: "ix_weekly_plan_items_weekly_plan_id",
                table: "weekly_plan_items",
                column: "weekly_plan_id");

            migrationBuilder.CreateIndex(
                name: "uq_weekly_plan_user_id",
                table: "weekly_plans",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "weekly_plan_items");

            migrationBuilder.DropTable(
                name: "recipes");

            migrationBuilder.DropTable(
                name: "weekly_plans");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
