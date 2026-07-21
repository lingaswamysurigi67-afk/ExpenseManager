using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExpenseManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class RestructureFeeTypesToCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FEE_TYPE_CATALOG",
                schema: "Config",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_DATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_DATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FEE_TYPE_CATALOG", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SUB_CATEGORY_FEE_TYPES",
                schema: "Config",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SUB_CATEGORY_ID = table.Column<int>(type: "integer", nullable: false),
                    FEE_TYPE_CATALOG_ID = table.Column<int>(type: "integer", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_DATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_DATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SUB_CATEGORY_FEE_TYPES", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SUB_CATEGORY_FEE_TYPES_FEE_TYPE_CATALOG_ID",
                schema: "Config",
                table: "SUB_CATEGORY_FEE_TYPES",
                column: "FEE_TYPE_CATALOG_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SUB_CATEGORY_FEE_TYPES_SUB_CATEGORY_ID",
                schema: "Config",
                table: "SUB_CATEGORY_FEE_TYPES",
                column: "SUB_CATEGORY_ID");

            // Existing per-sub-category fee types are not needed; drop the old table.
            migrationBuilder.DropTable(
                name: "FEE_TYPES",
                schema: "Config");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FEE_TYPES",
                schema: "Config",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_DATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    NAME = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SUB_CATEGORY_ID = table.Column<int>(type: "integer", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_DATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FEE_TYPES", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FEE_TYPES_SUB_CATEGORY_ID",
                schema: "Config",
                table: "FEE_TYPES",
                column: "SUB_CATEGORY_ID");

            migrationBuilder.DropTable(
                name: "SUB_CATEGORY_FEE_TYPES",
                schema: "Config");

            migrationBuilder.DropTable(
                name: "FEE_TYPE_CATALOG",
                schema: "Config");
        }
    }
}
