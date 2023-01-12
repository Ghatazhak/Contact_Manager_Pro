using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contact_Manager_Pro.Migrations
{
    public partial class ChangedAppUserIDtoAppUserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_AspNetUsers_AppUserID",
                table: "Contacts");

            migrationBuilder.RenameColumn(
                name: "AppUserID",
                table: "Contacts",
                newName: "AppUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Contacts_AppUserID",
                table: "Contacts",
                newName: "IX_Contacts_AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_AspNetUsers_AppUserId",
                table: "Contacts",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_AspNetUsers_AppUserId",
                table: "Contacts");

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "Contacts",
                newName: "AppUserID");

            migrationBuilder.RenameIndex(
                name: "IX_Contacts_AppUserId",
                table: "Contacts",
                newName: "IX_Contacts_AppUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_AspNetUsers_AppUserID",
                table: "Contacts",
                column: "AppUserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
