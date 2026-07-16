using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class UppercaseSchemasTablesColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_People",
                table: "People");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Incomes",
                table: "Incomes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Expenses",
                table: "Expenses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.EnsureSchema(
                name: "Config");

            migrationBuilder.EnsureSchema(
                name: "People");

            migrationBuilder.EnsureSchema(
                name: "Identity");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "USERS",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "People",
                newName: "PEOPLE",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "Incomes",
                newName: "INCOMES",
                newSchema: "People");

            migrationBuilder.RenameTable(
                name: "Expenses",
                newName: "EXPENSES",
                newSchema: "People");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "CATEGORIES",
                newSchema: "Config");

            migrationBuilder.RenameColumn(
                name: "Email",
                schema: "Identity",
                table: "USERS",
                newName: "EMAIL");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "Identity",
                table: "USERS",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "UserName",
                schema: "Identity",
                table: "USERS",
                newName: "USER_NAME");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                schema: "Identity",
                table: "USERS",
                newName: "UPDATED_DATE");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                schema: "Identity",
                table: "USERS",
                newName: "UPDATED_BY");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                schema: "Identity",
                table: "USERS",
                newName: "PASSWORD_HASH");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                schema: "Identity",
                table: "USERS",
                newName: "CREATED_DATE");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "Identity",
                table: "USERS",
                newName: "CREATED_BY");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                schema: "Identity",
                table: "USERS",
                newName: "IX_USERS_EMAIL");

            migrationBuilder.RenameIndex(
                name: "IX_Users_UserName",
                schema: "Identity",
                table: "USERS",
                newName: "IX_USERS_USER_NAME");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "Config",
                table: "PEOPLE",
                newName: "NAME");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "Config",
                table: "PEOPLE",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "Config",
                table: "PEOPLE",
                newName: "USER_ID");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                schema: "Config",
                table: "PEOPLE",
                newName: "UPDATED_DATE");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                schema: "Config",
                table: "PEOPLE",
                newName: "UPDATED_BY");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                schema: "Config",
                table: "PEOPLE",
                newName: "IS_ACTIVE");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                schema: "Config",
                table: "PEOPLE",
                newName: "CREATED_DATE");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "Config",
                table: "PEOPLE",
                newName: "CREATED_BY");

            migrationBuilder.RenameIndex(
                name: "IX_People_UserId",
                schema: "Config",
                table: "PEOPLE",
                newName: "IX_PEOPLE_USER_ID");

            migrationBuilder.RenameColumn(
                name: "Source",
                schema: "People",
                table: "INCOMES",
                newName: "SOURCE");

            migrationBuilder.RenameColumn(
                name: "Notes",
                schema: "People",
                table: "INCOMES",
                newName: "NOTES");

            migrationBuilder.RenameColumn(
                name: "Category",
                schema: "People",
                table: "INCOMES",
                newName: "CATEGORY");

            migrationBuilder.RenameColumn(
                name: "Amount",
                schema: "People",
                table: "INCOMES",
                newName: "AMOUNT");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "People",
                table: "INCOMES",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "People",
                table: "INCOMES",
                newName: "USER_ID");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                schema: "People",
                table: "INCOMES",
                newName: "UPDATED_DATE");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                schema: "People",
                table: "INCOMES",
                newName: "UPDATED_BY");

            migrationBuilder.RenameColumn(
                name: "PersonId",
                schema: "People",
                table: "INCOMES",
                newName: "PERSON_ID");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                schema: "People",
                table: "INCOMES",
                newName: "PAYMENT_METHOD");

            migrationBuilder.RenameColumn(
                name: "Money Came Date",
                schema: "People",
                table: "INCOMES",
                newName: "MONEY_CAME_DATE");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                schema: "People",
                table: "INCOMES",
                newName: "IS_ACTIVE");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                schema: "People",
                table: "INCOMES",
                newName: "CREATED_DATE");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "People",
                table: "INCOMES",
                newName: "CREATED_BY");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                schema: "People",
                table: "INCOMES",
                newName: "CATEGORY_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Incomes_UserId",
                schema: "People",
                table: "INCOMES",
                newName: "IX_INCOMES_USER_ID");

            migrationBuilder.RenameColumn(
                name: "Notes",
                schema: "People",
                table: "EXPENSES",
                newName: "NOTES");

            migrationBuilder.RenameColumn(
                name: "Category",
                schema: "People",
                table: "EXPENSES",
                newName: "CATEGORY");

            migrationBuilder.RenameColumn(
                name: "Amount",
                schema: "People",
                table: "EXPENSES",
                newName: "AMOUNT");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "People",
                table: "EXPENSES",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "People",
                table: "EXPENSES",
                newName: "USER_ID");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                schema: "People",
                table: "EXPENSES",
                newName: "UPDATED_DATE");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                schema: "People",
                table: "EXPENSES",
                newName: "UPDATED_BY");

            migrationBuilder.RenameColumn(
                name: "PersonId",
                schema: "People",
                table: "EXPENSES",
                newName: "PERSON_ID");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                schema: "People",
                table: "EXPENSES",
                newName: "PAYMENT_METHOD");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                schema: "People",
                table: "EXPENSES",
                newName: "IS_ACTIVE");

            migrationBuilder.RenameColumn(
                name: "Expenditure Date",
                schema: "People",
                table: "EXPENSES",
                newName: "EXPENDITURE_DATE");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                schema: "People",
                table: "EXPENSES",
                newName: "CREATED_DATE");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "People",
                table: "EXPENSES",
                newName: "CREATED_BY");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                schema: "People",
                table: "EXPENSES",
                newName: "CATEGORY_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Expenses_UserId",
                schema: "People",
                table: "EXPENSES",
                newName: "IX_EXPENSES_USER_ID");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "Config",
                table: "CATEGORIES",
                newName: "NAME");

            migrationBuilder.RenameColumn(
                name: "Color",
                schema: "Config",
                table: "CATEGORIES",
                newName: "COLOR");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "Config",
                table: "CATEGORIES",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                schema: "Config",
                table: "CATEGORIES",
                newName: "UPDATED_DATE");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                schema: "Config",
                table: "CATEGORIES",
                newName: "UPDATED_BY");

            migrationBuilder.RenameColumn(
                name: "IsDefault",
                schema: "Config",
                table: "CATEGORIES",
                newName: "IS_DEFAULT");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                schema: "Config",
                table: "CATEGORIES",
                newName: "IS_ACTIVE");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                schema: "Config",
                table: "CATEGORIES",
                newName: "CREATED_DATE");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "Config",
                table: "CATEGORIES",
                newName: "CREATED_BY");

            migrationBuilder.AddPrimaryKey(
                name: "PK_USERS",
                schema: "Identity",
                table: "USERS",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PEOPLE",
                schema: "Config",
                table: "PEOPLE",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_INCOMES",
                schema: "People",
                table: "INCOMES",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EXPENSES",
                schema: "People",
                table: "EXPENSES",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CATEGORIES",
                schema: "Config",
                table: "CATEGORIES",
                column: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_USERS",
                schema: "Identity",
                table: "USERS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PEOPLE",
                schema: "Config",
                table: "PEOPLE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_INCOMES",
                schema: "People",
                table: "INCOMES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EXPENSES",
                schema: "People",
                table: "EXPENSES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CATEGORIES",
                schema: "Config",
                table: "CATEGORIES");

            migrationBuilder.RenameTable(
                name: "USERS",
                schema: "Identity",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "PEOPLE",
                schema: "Config",
                newName: "People");

            migrationBuilder.RenameTable(
                name: "INCOMES",
                schema: "People",
                newName: "Incomes");

            migrationBuilder.RenameTable(
                name: "EXPENSES",
                schema: "People",
                newName: "Expenses");

            migrationBuilder.RenameTable(
                name: "CATEGORIES",
                schema: "Config",
                newName: "Categories");

            migrationBuilder.RenameColumn(
                name: "EMAIL",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "USER_NAME",
                table: "Users",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "UPDATED_DATE",
                table: "Users",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "UPDATED_BY",
                table: "Users",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "PASSWORD_HASH",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "CREATED_DATE",
                table: "Users",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "CREATED_BY",
                table: "Users",
                newName: "CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_USERS_EMAIL",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.RenameIndex(
                name: "IX_USERS_USER_NAME",
                table: "Users",
                newName: "IX_Users_UserName");

            migrationBuilder.RenameColumn(
                name: "NAME",
                table: "People",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "People",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "USER_ID",
                table: "People",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "UPDATED_DATE",
                table: "People",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "UPDATED_BY",
                table: "People",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "IS_ACTIVE",
                table: "People",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "CREATED_DATE",
                table: "People",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "CREATED_BY",
                table: "People",
                newName: "CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_PEOPLE_USER_ID",
                table: "People",
                newName: "IX_People_UserId");

            migrationBuilder.RenameColumn(
                name: "SOURCE",
                table: "Incomes",
                newName: "Source");

            migrationBuilder.RenameColumn(
                name: "NOTES",
                table: "Incomes",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "CATEGORY",
                table: "Incomes",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "AMOUNT",
                table: "Incomes",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Incomes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "USER_ID",
                table: "Incomes",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "UPDATED_DATE",
                table: "Incomes",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "UPDATED_BY",
                table: "Incomes",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "PERSON_ID",
                table: "Incomes",
                newName: "PersonId");

            migrationBuilder.RenameColumn(
                name: "PAYMENT_METHOD",
                table: "Incomes",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "MONEY_CAME_DATE",
                table: "Incomes",
                newName: "Money Came Date");

            migrationBuilder.RenameColumn(
                name: "IS_ACTIVE",
                table: "Incomes",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "CREATED_DATE",
                table: "Incomes",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "CREATED_BY",
                table: "Incomes",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "CATEGORY_ID",
                table: "Incomes",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_INCOMES_USER_ID",
                table: "Incomes",
                newName: "IX_Incomes_UserId");

            migrationBuilder.RenameColumn(
                name: "NOTES",
                table: "Expenses",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "CATEGORY",
                table: "Expenses",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "AMOUNT",
                table: "Expenses",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Expenses",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "USER_ID",
                table: "Expenses",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "UPDATED_DATE",
                table: "Expenses",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "UPDATED_BY",
                table: "Expenses",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "PERSON_ID",
                table: "Expenses",
                newName: "PersonId");

            migrationBuilder.RenameColumn(
                name: "PAYMENT_METHOD",
                table: "Expenses",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "IS_ACTIVE",
                table: "Expenses",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "EXPENDITURE_DATE",
                table: "Expenses",
                newName: "Expenditure Date");

            migrationBuilder.RenameColumn(
                name: "CREATED_DATE",
                table: "Expenses",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "CREATED_BY",
                table: "Expenses",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "CATEGORY_ID",
                table: "Expenses",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_EXPENSES_USER_ID",
                table: "Expenses",
                newName: "IX_Expenses_UserId");

            migrationBuilder.RenameColumn(
                name: "NAME",
                table: "Categories",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "COLOR",
                table: "Categories",
                newName: "Color");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Categories",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "UPDATED_DATE",
                table: "Categories",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "UPDATED_BY",
                table: "Categories",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "IS_DEFAULT",
                table: "Categories",
                newName: "IsDefault");

            migrationBuilder.RenameColumn(
                name: "IS_ACTIVE",
                table: "Categories",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "CREATED_DATE",
                table: "Categories",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "CREATED_BY",
                table: "Categories",
                newName: "CreatedBy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_People",
                table: "People",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Incomes",
                table: "Incomes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Expenses",
                table: "Expenses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");
        }
    }
}
