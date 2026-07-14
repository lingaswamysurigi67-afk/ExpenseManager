using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExpenseManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenamePersonAndAddToIncome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename the table + its index/PK (preserves existing people rows).
            migrationBuilder.RenameTable(name: "ExpenditureOns", newName: "People");
            migrationBuilder.RenameIndex(name: "IX_ExpenditureOns_UserId", table: "People", newName: "IX_People_UserId");
            migrationBuilder.Sql(@"ALTER TABLE ""People"" RENAME CONSTRAINT ""PK_ExpenditureOns"" TO ""PK_People"";");

            // Rename the Expense columns (preserves existing values).
            migrationBuilder.RenameColumn(name: "ExpenditureOnId", table: "Expenses", newName: "PersonId");
            migrationBuilder.RenameColumn(name: "ExpenditureOn", table: "Expenses", newName: "PersonName");

            // Add the same person reference to Income.
            migrationBuilder.AddColumn<int>(
                name: "PersonId",
                table: "Incomes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonName",
                table: "Incomes",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PersonId", table: "Incomes");
            migrationBuilder.DropColumn(name: "PersonName", table: "Incomes");

            migrationBuilder.RenameColumn(name: "PersonName", table: "Expenses", newName: "ExpenditureOn");
            migrationBuilder.RenameColumn(name: "PersonId", table: "Expenses", newName: "ExpenditureOnId");

            migrationBuilder.Sql(@"ALTER TABLE ""People"" RENAME CONSTRAINT ""PK_People"" TO ""PK_ExpenditureOns"";");
            migrationBuilder.RenameIndex(name: "IX_People_UserId", table: "People", newName: "IX_ExpenditureOns_UserId");
            migrationBuilder.RenameTable(name: "People", newName: "ExpenditureOns");
        }
    }
}
