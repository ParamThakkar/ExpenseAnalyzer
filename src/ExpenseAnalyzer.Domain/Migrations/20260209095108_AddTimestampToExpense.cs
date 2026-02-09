using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseAnalyzer.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddTimestampToExpense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Timestamp",
                table: "Expense",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Expense_AccountId",
                table: "Expense",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Expense_CategoryId",
                table: "Expense",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Expense_Timestamp",
                table: "Expense",
                column: "Timestamp");

            migrationBuilder.AddForeignKey(
                name: "FK_Expense_Account_AccountId",
                table: "Expense",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expense_Category_CategoryId",
                table: "Expense",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expense_Account_AccountId",
                table: "Expense");

            migrationBuilder.DropForeignKey(
                name: "FK_Expense_Category_CategoryId",
                table: "Expense");

            migrationBuilder.DropIndex(
                name: "IX_Expense_AccountId",
                table: "Expense");

            migrationBuilder.DropIndex(
                name: "IX_Expense_CategoryId",
                table: "Expense");

            migrationBuilder.DropIndex(
                name: "IX_Expense_Timestamp",
                table: "Expense");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Expense");
        }
    }
}
