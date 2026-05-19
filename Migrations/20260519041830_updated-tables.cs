using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace autoease_backend.Migrations
{
    /// <inheritdoc />
    public partial class updatedtables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_users_StaffId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Parts_users_RequesterId",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_RequesterId",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_StaffId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RequestDescription",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequestStatus",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequestedBy",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequesterId",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "Invoices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestDescription",
                table: "Parts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequestStatus",
                table: "Parts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RequestedBy",
                table: "Parts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequesterId",
                table: "Parts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StaffId",
                table: "Invoices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Parts_RequesterId",
                table: "Parts",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_StaffId",
                table: "Invoices",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_users_StaffId",
                table: "Invoices",
                column: "StaffId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_users_RequesterId",
                table: "Parts",
                column: "RequesterId",
                principalTable: "users",
                principalColumn: "Id");
        }
    }
}
