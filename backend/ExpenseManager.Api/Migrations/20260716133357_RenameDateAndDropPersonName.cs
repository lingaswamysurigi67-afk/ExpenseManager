using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameDateAndDropPersonName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PersonName",
                table: "Incomes");

            migrationBuilder.DropColumn(
                name: "PersonName",
                table: "Expenses");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Incomes",
                newName: "Money Came Date");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Expenses",
                newName: "Expenditure Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Money Came Date",
                table: "Incomes",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Expenditure Date",
                table: "Expenses",
                newName: "Date");

            migrationBuilder.AddColumn<string>(
                name: "PersonName",
                table: "Incomes",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PersonName",
                table: "Expenses",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");
        }
    }
}
