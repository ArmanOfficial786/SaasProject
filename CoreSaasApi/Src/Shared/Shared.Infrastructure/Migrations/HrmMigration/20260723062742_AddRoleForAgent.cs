using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Shared.Infrastructure.Migrations.HrmMigration
{
    /// <inheritdoc />
    public partial class AddRoleForAgent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "userManagement",
                table: "user_roles",
                keyColumn: "Id",
                keyValue: new Guid("ac5bc193-a466-4ee7-855d-e2bbbb0c5fc1"));

            migrationBuilder.DeleteData(
                schema: "userManagement",
                table: "user_roles",
                keyColumn: "Id",
                keyValue: new Guid("fc8d6dae-d715-4b04-aec1-bfe6e2bdf993"));

            migrationBuilder.UpdateData(
                schema: "userManagement",
                table: "agents",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "EntryDate",
                value: new DateTime(2026, 7, 23, 6, 27, 39, 628, DateTimeKind.Utc).AddTicks(3465));

            migrationBuilder.UpdateData(
                schema: "userManagement",
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "concurrency_stamp", "EntryDate", "FromDate" },
                values: new object[] { "fa614657-01d6-4246-a755-6ae33ef70c8c", new DateTime(2026, 7, 23, 6, 27, 39, 669, DateTimeKind.Utc).AddTicks(9363), new DateTime(2026, 7, 23, 6, 27, 39, 669, DateTimeKind.Utc).AddTicks(9367) });

            migrationBuilder.UpdateData(
                schema: "userManagement",
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "concurrency_stamp", "EntryDate", "FromDate" },
                values: new object[] { "921fa3d7-33d9-40f5-ac26-7dc3377e5968", new DateTime(2026, 7, 23, 6, 27, 39, 670, DateTimeKind.Utc).AddTicks(7598), new DateTime(2026, 7, 23, 6, 27, 39, 670, DateTimeKind.Utc).AddTicks(7600) });

            migrationBuilder.UpdateData(
                schema: "userManagement",
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "concurrency_stamp", "EntryDate", "FromDate" },
                values: new object[] { "4642257d-dae3-4ae1-9d60-dbc1454be3b5", new DateTime(2026, 7, 23, 6, 27, 39, 670, DateTimeKind.Utc).AddTicks(7610), new DateTime(2026, 7, 23, 6, 27, 39, 670, DateTimeKind.Utc).AddTicks(7611) });

            migrationBuilder.UpdateData(
                schema: "userManagement",
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "concurrency_stamp", "EntryDate", "FromDate" },
                values: new object[] { "3f4645e2-7b39-44b6-9c25-d2608024d51e", new DateTime(2026, 7, 23, 6, 27, 39, 670, DateTimeKind.Utc).AddTicks(7621), new DateTime(2026, 7, 23, 6, 27, 39, 670, DateTimeKind.Utc).AddTicks(7621) });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "user_roles",
                columns: new[] { "Id", "ApprovedDate", "CompanyId", "EntryByUserId", "EntryDate", "IsActive", "IsApproved", "IsDeleted", "RoleId", "RoleId1", "ToDate", "UpdatedByUserId", "UpdatedDate", "UserId", "VerificationStatus" },
                values: new object[,]
                {
                    { new Guid("82a70fce-abeb-4bc4-b51e-a72b71db2da9"), new DateTime(2026, 7, 23, 6, 27, 39, 686, DateTimeKind.Utc).AddTicks(1876), 1, new Guid("30000000-0000-0000-0000-000000000001"), new DateTime(2026, 7, 23, 6, 27, 39, 686, DateTimeKind.Utc).AddTicks(1864), true, true, false, new Guid("10000000-0000-0000-0000-000000000002"), null, null, null, new DateTime(2026, 7, 23, 6, 27, 39, 686, DateTimeKind.Utc).AddTicks(1875), new Guid("30000000-0000-0000-0000-000000000001"), 0 },
                    { new Guid("cdf3d4ca-d832-4374-a785-3c514601ffd1"), new DateTime(2026, 7, 23, 6, 27, 39, 686, DateTimeKind.Utc).AddTicks(1552), 1, new Guid("30000000-0000-0000-0000-000000000001"), new DateTime(2026, 7, 23, 6, 27, 39, 685, DateTimeKind.Utc).AddTicks(7239), true, true, false, new Guid("10000000-0000-0000-0000-000000000001"), null, null, null, new DateTime(2026, 7, 23, 6, 27, 39, 686, DateTimeKind.Utc).AddTicks(330), new Guid("30000000-0000-0000-0000-000000000001"), 0 }
                });

            migrationBuilder.UpdateData(
                schema: "userManagement",
                table: "user_statuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "FromDate",
                value: new DateTime(2026, 7, 23, 6, 27, 39, 688, DateTimeKind.Utc).AddTicks(3808));

            migrationBuilder.UpdateData(
                schema: "userManagement",
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                columns: new[] { "ConcurrencyStamp", "EntryDate", "SecurityStamp" },
                values: new object[] { "19b607a6-3966-4b78-8527-5584d1ea4428", new DateTime(2026, 7, 23, 6, 27, 39, 679, DateTimeKind.Utc).AddTicks(3420), "64721ff0-bcec-4cdc-9afd-e15b76104a8b" });

            migrationBuilder.CreateIndex(
                name: "IX_company_roles_CompanyId",
                schema: "userManagement",
                table: "company_roles",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_company_roles_companies_CompanyId",
                schema: "userManagement",
                table: "company_roles",
                column: "CompanyId",
                principalSchema: "userManagement",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_company_roles_companies_CompanyId",
                schema: "userManagement",
                table: "company_roles");

            migrationBuilder.DropIndex(
                name: "IX_company_roles_CompanyId",
                schema: "userManagement",
                table: "company_roles");

            migrationBuilder.DeleteData(
                schema: "userManagement",
                table: "user_roles",
                keyColumn: "Id",
                keyValue: new Guid("82a70fce-abeb-4bc4-b51e-a72b71db2da9"));

            migrationBuilder.DeleteData(
                schema: "userManagement",
                table: "user_roles",
                keyColumn: "Id",
                keyValue: new Guid("cdf3d4ca-d832-4374-a785-3c514601ffd1"));

            migrationBuilder.UpdateData(
                schema: "userManagement",
                table: "agents",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "EntryDate",
                value: new DateTime(2026, 7, 22, 10, 32, 59, 37, DateTimeKind.Utc).AddTicks(1560));

            migrationBuilder.UpdateData(
                schema: "userManagement",
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "concurrency_stamp", "EntryDate", "FromDate" },
                values: new object[] { "1b1e4565-e32f-4510-a6e7-7d54796540c5", new DateTime(2026, 7, 22, 10, 32, 59, 73, DateTimeKind.Utc).AddTicks(9092), new DateTime(2026, 7, 22, 10, 32, 59, 73, DateTimeKind.Utc).AddTicks(9095) });

            migrationBuilder.UpdateData(
                schema: "userManagement",
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "concurrency_stamp", "EntryDate", "FromDate" },
                values: new object[] { "8c05e73f-2afd-4259-a9e5-caba554d9825", new DateTime(2026, 7, 22, 10, 32, 59, 74, DateTimeKind.Utc).AddTicks(7476), new DateTime(2026, 7, 22, 10, 32, 59, 74, DateTimeKind.Utc).AddTicks(7478) });

            migrationBuilder.UpdateData(
                schema: "userManagement",
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "concurrency_stamp", "EntryDate", "FromDate" },
                values: new object[] { "28f15bfd-69df-4831-a998-3cd30f4157e7", new DateTime(2026, 7, 22, 10, 32, 59, 74, DateTimeKind.Utc).AddTicks(7548), new DateTime(2026, 7, 22, 10, 32, 59, 74, DateTimeKind.Utc).AddTicks(7548) });

            migrationBuilder.UpdateData(
                schema: "userManagement",
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "concurrency_stamp", "EntryDate", "FromDate" },
                values: new object[] { "53bbb331-e396-4e22-9e20-edffd36cea52", new DateTime(2026, 7, 22, 10, 32, 59, 74, DateTimeKind.Utc).AddTicks(7553), new DateTime(2026, 7, 22, 10, 32, 59, 74, DateTimeKind.Utc).AddTicks(7553) });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "user_roles",
                columns: new[] { "Id", "ApprovedDate", "CompanyId", "EntryByUserId", "EntryDate", "IsActive", "IsApproved", "IsDeleted", "RoleId", "RoleId1", "ToDate", "UpdatedByUserId", "UpdatedDate", "UserId", "VerificationStatus" },
                values: new object[,]
                {
                    { new Guid("ac5bc193-a466-4ee7-855d-e2bbbb0c5fc1"), new DateTime(2026, 7, 22, 10, 32, 59, 89, DateTimeKind.Utc).AddTicks(6502), 1, new Guid("30000000-0000-0000-0000-000000000001"), new DateTime(2026, 7, 22, 10, 32, 59, 89, DateTimeKind.Utc).AddTicks(6489), true, true, false, new Guid("10000000-0000-0000-0000-000000000002"), null, null, null, new DateTime(2026, 7, 22, 10, 32, 59, 89, DateTimeKind.Utc).AddTicks(6501), new Guid("30000000-0000-0000-0000-000000000001"), 0 },
                    { new Guid("fc8d6dae-d715-4b04-aec1-bfe6e2bdf993"), new DateTime(2026, 7, 22, 10, 32, 59, 89, DateTimeKind.Utc).AddTicks(6158), 1, new Guid("30000000-0000-0000-0000-000000000001"), new DateTime(2026, 7, 22, 10, 32, 59, 89, DateTimeKind.Utc).AddTicks(2548), true, true, false, new Guid("10000000-0000-0000-0000-000000000001"), null, null, null, new DateTime(2026, 7, 22, 10, 32, 59, 89, DateTimeKind.Utc).AddTicks(4346), new Guid("30000000-0000-0000-0000-000000000001"), 0 }
                });

            migrationBuilder.UpdateData(
                schema: "userManagement",
                table: "user_statuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "FromDate",
                value: new DateTime(2026, 7, 22, 10, 32, 59, 91, DateTimeKind.Utc).AddTicks(7905));

            migrationBuilder.UpdateData(
                schema: "userManagement",
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                columns: new[] { "ConcurrencyStamp", "EntryDate", "SecurityStamp" },
                values: new object[] { "e299ddf3-085d-4111-b195-8e89c8ff50cf", new DateTime(2026, 7, 22, 10, 32, 59, 82, DateTimeKind.Utc).AddTicks(3946), "8e791f37-f25b-4e9f-b0de-3698ab698802" });
        }
    }
}
