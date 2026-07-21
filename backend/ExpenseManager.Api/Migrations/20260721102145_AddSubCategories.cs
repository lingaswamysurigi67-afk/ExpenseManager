using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExpenseManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSubCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SUB_CATEGORY",
                schema: "People",
                table: "EXPENSES",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SUB_CATEGORY_ID",
                schema: "People",
                table: "EXPENSES",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SUB_CATEGORIES",
                schema: "Config",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CATEGORY_ID = table.Column<int>(type: "integer", nullable: false),
                    NAME = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_DATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_DATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SUB_CATEGORIES", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SUB_CATEGORIES_CATEGORY_ID",
                schema: "Config",
                table: "SUB_CATEGORIES",
                column: "CATEGORY_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SUB_CATEGORIES",
                schema: "Config");

            migrationBuilder.DropColumn(
                name: "SUB_CATEGORY",
                schema: "People",
                table: "EXPENSES");

            migrationBuilder.DropColumn(
                name: "SUB_CATEGORY_ID",
                schema: "People",
                table: "EXPENSES");
        }
    }
}
