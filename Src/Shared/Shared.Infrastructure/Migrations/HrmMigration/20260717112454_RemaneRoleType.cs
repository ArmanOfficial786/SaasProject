using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations.HrmMigration
{
    /// <inheritdoc />
    public partial class RemaneRoleType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_roles_users_EntryByUserId",
                schema: "userManagement",
                table: "user_roles");

            migrationBuilder.DropIndex(
                name: "IX_user_roles_EntryByUserId",
                schema: "userManagement",
                table: "user_roles");

            migrationBuilder.DropColumn(
                name: "IsSuperAdmin",
                schema: "userManagement",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "IsSystemRole",
                schema: "userManagement",
                table: "roles");

            migrationBuilder.RenameIndex(
                name: "IX_UserRole_UserId_CompanyId",
                schema: "userManagement",
                table: "user_roles",
                newName: "IX_user_roles_UserId_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_UserRole_SuperAdmin",
                schema: "userManagement",
                table: "user_roles",
                newName: "IX_user_roles_UserId_RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_user_roles_UserId_RoleId",
                schema: "userManagement",
                table: "user_roles",
                newName: "IX_UserRole_SuperAdmin");

            migrationBuilder.RenameIndex(
                name: "IX_user_roles_UserId_CompanyId",
                schema: "userManagement",
                table: "user_roles",
                newName: "IX_UserRole_UserId_CompanyId");

            migrationBuilder.AddColumn<bool>(
                name: "IsSuperAdmin",
                schema: "userManagement",
                table: "roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemRole",
                schema: "userManagement",
                table: "roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_EntryByUserId",
                schema: "userManagement",
                table: "user_roles",
                column: "EntryByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_user_roles_users_EntryByUserId",
                schema: "userManagement",
                table: "user_roles",
                column: "EntryByUserId",
                principalSchema: "userManagement",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
