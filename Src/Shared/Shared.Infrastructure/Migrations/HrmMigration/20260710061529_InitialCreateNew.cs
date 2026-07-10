using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Shared.Infrastructure.Migrations.HrmMigration
{
    /// <inheritdoc />
    public partial class InitialCreateNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "userManagement");

            migrationBuilder.CreateTable(
                name: "applications",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhoneNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Pan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RegNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "menus",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ToolTip = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    OrderNo = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menus_menus_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "userManagement",
                        principalTable: "menus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "agents",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pan = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    RegNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsParent = table.Column<bool>(type: "bit", nullable: false),
                    ReferralCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    EntryByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerificationStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agents_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    MiddleName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Contact = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsEmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EntryByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_AspNetUsers_EntryByUserId",
                        column: x => x.EntryByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "modules",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MenuId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_modules_menus_MenuId",
                        column: x => x.MenuId,
                        principalSchema: "userManagement",
                        principalTable: "menus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "agent_users",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agent_users_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_agent_users_agents_AgentId",
                        column: x => x.AgentId,
                        principalSchema: "userManagement",
                        principalTable: "agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EntryByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoles_AspNetUsers_EntryByUserId",
                        column: x => x.EntryByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetRoles_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "login_logs",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    MacAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ClientAgent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OS = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LoginDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_login_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_login_logs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_statuses",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_statuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_statuses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "module_permissions",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permission = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_module_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_module_permissions_modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "userManagement",
                        principalTable: "modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agent_roles",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    EntryByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerificationStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agent_roles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_agent_roles_agents_AgentId",
                        column: x => x.AgentId,
                        principalSchema: "userManagement",
                        principalTable: "agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    EntryByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerificationStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_module_permissions",
                schema: "userManagement",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModulePermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_module_permissions", x => new { x.RoleId, x.ModulePermissionId });
                    table.ForeignKey(
                        name: "FK_role_module_permissions_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_module_permissions_module_permissions_ModulePermissionId",
                        column: x => x.ModulePermissionId,
                        principalSchema: "userManagement",
                        principalTable: "module_permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_module_permissions",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModulePermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_module_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_module_permissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_module_permissions_module_permissions_ModulePermissionId",
                        column: x => x.ModulePermissionId,
                        principalSchema: "userManagement",
                        principalTable: "module_permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "applications",
                columns: new[] { "Id", "Code", "Desc", "Name" },
                values: new object[] { new Guid("89de1083-5d8b-401c-8914-7f6cc1363fdf"), 0, "Usermanagement", "Usermanagement" });

            migrationBuilder.InsertData(
                table: "companies",
                columns: new[] { "Id", "Address", "Email", "Name", "Pan", "PhoneNo", "ProductCode", "RegNo", "Url" },
                values: new object[] { 1, "Kathmandu, Nepal", "info@armansoftware.com", "Arman Software Solutions", "600000000", "+977-1-4000000", null, "120000", "https://armansoftware.com" });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "menus",
                columns: new[] { "Id", "Active", "Color", "Icon", "MenuText", "OrderNo", "ParentId", "ToolTip", "Url" },
                values: new object[] { new Guid("9a71e39c-1e80-423e-9d87-16586687575f"), true, "red", "FaShieldHalved", "UserManagement", 1, null, "UserManagement", null });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "PermissionId", "Code", "CompanyId", "Description", "Module" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), "user.view", 1, "View users", "UserManagement" },
                    { new Guid("30000000-0000-0000-0000-000000000002"), "user.create", 1, "Create new users", "UserManagement" },
                    { new Guid("30000000-0000-0000-0000-000000000003"), "user.edit", 1, "Edit existing users", "UserManagement" },
                    { new Guid("30000000-0000-0000-0000-000000000004"), "user.delete", 1, "Delete users", "UserManagement" },
                    { new Guid("30000000-0000-0000-0000-000000000005"), "role.view", 1, "View roles", "UserManagement" },
                    { new Guid("30000000-0000-0000-0000-000000000006"), "role.create", 1, "Create new roles", "UserManagement" },
                    { new Guid("30000000-0000-0000-0000-000000000007"), "role.edit", 1, "Edit existing roles", "UserManagement" },
                    { new Guid("30000000-0000-0000-0000-000000000008"), "role.delete", 1, "Delete roles", "UserManagement" }
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "CompanyId", "ConcurrencyStamp", "Desc", "EntryByUserId", "EntryDate", "FromDate", "Name", "NormalizedName", "ToDate" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), 1, "56784e48-be89-425f-b37c-8007dded6f64", "Administrator with full access", null, new DateTime(2026, 7, 10, 6, 15, 27, 577, DateTimeKind.Utc).AddTicks(3492), new DateTime(2026, 7, 10, 6, 15, 27, 577, DateTimeKind.Utc).AddTicks(3496), "Admin", "ADMIN", null },
                    { new Guid("10000000-0000-0000-0000-000000000002"), 1, "92bdbfd5-3c37-4d70-9a80-6302838cb5ca", "Manager with operational access", null, new DateTime(2026, 7, 10, 6, 15, 27, 578, DateTimeKind.Utc).AddTicks(1719), new DateTime(2026, 7, 10, 6, 15, 27, 578, DateTimeKind.Utc).AddTicks(1721), "Manager", "MANAGER", null },
                    { new Guid("10000000-0000-0000-0000-000000000003"), 1, "e835a216-6cfd-460a-b896-226a6044e67b", "Regular user with limited access", null, new DateTime(2026, 7, 10, 6, 15, 27, 578, DateTimeKind.Utc).AddTicks(1744), new DateTime(2026, 7, 10, 6, 15, 27, 578, DateTimeKind.Utc).AddTicks(1744), "User", "USER", null }
                });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "menus",
                columns: new[] { "Id", "Active", "Color", "Icon", "MenuText", "OrderNo", "ParentId", "ToolTip", "Url" },
                values: new object[,]
                {
                    { new Guid("37878e39-c706-427e-bc86-0e7d13c76665"), true, "blue", "FaUserGear", "User Role", 2, new Guid("9a71e39c-1e80-423e-9d87-16586687575f"), "Role for User Management", "/UserManagement/user-role" },
                    { new Guid("45bda341-5e70-495c-aecd-075efef1885b"), true, "blue", "FaUsersGear", "Collection Center Role", 1, new Guid("9a71e39c-1e80-423e-9d87-16586687575f"), "Role for Collection and Distribution Center Management", "/UserManagement/agent-role" },
                    { new Guid("5f35399e-05b3-42f1-8548-ab31b8cb731c"), true, "blue", "FaUser", "User", 3, new Guid("9a71e39c-1e80-423e-9d87-16586687575f"), "User Management", "/UserManagement/user" }
                });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "modules",
                columns: new[] { "Id", "ApplicationId", "Code", "Description", "FromDate", "MenuId", "Name", "ToDate" },
                values: new object[,]
                {
                    { new Guid("65d5de5a-3b73-4e45-8775-1b3d6f144268"), new Guid("89de1083-5d8b-401c-8914-7f6cc1363fdf"), 2, "User", new DateTime(2024, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("5f35399e-05b3-42f1-8548-ab31b8cb731c"), "User", null },
                    { new Guid("ba51d83f-8c02-4fb5-922f-650b945b79b2"), new Guid("89de1083-5d8b-401c-8914-7f6cc1363fdf"), 1, "User Role", new DateTime(2024, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("37878e39-c706-427e-bc86-0e7d13c76665"), "UserRole", null },
                    { new Guid("e3c916fb-608f-42b3-87db-1c46ae5b5148"), new Guid("89de1083-5d8b-401c-8914-7f6cc1363fdf"), 0, "Collection Center Role", new DateTime(2024, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("45bda341-5e70-495c-aecd-075efef1885b"), "AgentRole", null }
                });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "module_permissions",
                columns: new[] { "Id", "ModuleId", "Permission" },
                values: new object[,]
                {
                    { new Guid("50000000-0000-0000-0000-000000000001"), new Guid("65d5de5a-3b73-4e45-8775-1b3d6f144268"), "Read" },
                    { new Guid("50000000-0000-0000-0000-000000000002"), new Guid("65d5de5a-3b73-4e45-8775-1b3d6f144268"), "Write" },
                    { new Guid("50000000-0000-0000-0000-000000000003"), new Guid("65d5de5a-3b73-4e45-8775-1b3d6f144268"), "Update" },
                    { new Guid("50000000-0000-0000-0000-000000000004"), new Guid("65d5de5a-3b73-4e45-8775-1b3d6f144268"), "Delete" },
                    { new Guid("50000000-0000-0000-0000-000000000005"), new Guid("ba51d83f-8c02-4fb5-922f-650b945b79b2"), "Read" },
                    { new Guid("50000000-0000-0000-0000-000000000006"), new Guid("ba51d83f-8c02-4fb5-922f-650b945b79b2"), "Write" },
                    { new Guid("50000000-0000-0000-0000-000000000007"), new Guid("ba51d83f-8c02-4fb5-922f-650b945b79b2"), "Update" },
                    { new Guid("50000000-0000-0000-0000-000000000008"), new Guid("ba51d83f-8c02-4fb5-922f-650b945b79b2"), "Delete" },
                    { new Guid("50000000-0000-0000-0000-000000000009"), new Guid("e3c916fb-608f-42b3-87db-1c46ae5b5148"), "Read" },
                    { new Guid("50000000-0000-0000-0000-00000000000a"), new Guid("e3c916fb-608f-42b3-87db-1c46ae5b5148"), "Write" },
                    { new Guid("50000000-0000-0000-0000-00000000000b"), new Guid("e3c916fb-608f-42b3-87db-1c46ae5b5148"), "Update" },
                    { new Guid("50000000-0000-0000-0000-00000000000c"), new Guid("e3c916fb-608f-42b3-87db-1c46ae5b5148"), "Delete" }
                });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "role_module_permissions",
                columns: new[] { "ModulePermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("50000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("50000000-0000-0000-0000-000000000002"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("50000000-0000-0000-0000-000000000003"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("50000000-0000-0000-0000-000000000004"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("50000000-0000-0000-0000-000000000005"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("50000000-0000-0000-0000-000000000006"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("50000000-0000-0000-0000-000000000007"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("50000000-0000-0000-0000-000000000008"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("50000000-0000-0000-0000-000000000009"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("50000000-0000-0000-0000-00000000000a"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("50000000-0000-0000-0000-00000000000b"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("50000000-0000-0000-0000-00000000000c"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("50000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("50000000-0000-0000-0000-000000000002"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("50000000-0000-0000-0000-000000000005"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("50000000-0000-0000-0000-000000000006"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("50000000-0000-0000-0000-000000000009"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("50000000-0000-0000-0000-00000000000a"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("50000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000003") },
                    { new Guid("50000000-0000-0000-0000-000000000005"), new Guid("10000000-0000-0000-0000-000000000003") },
                    { new Guid("50000000-0000-0000-0000-000000000009"), new Guid("10000000-0000-0000-0000-000000000003") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_agent_roles_AgentId",
                schema: "userManagement",
                table: "agent_roles",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_roles_RoleId",
                schema: "userManagement",
                table: "agent_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_users_AgentId",
                schema: "userManagement",
                table: "agent_users",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_users_UserId",
                schema: "userManagement",
                table: "agent_users",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_agents_CompanyId",
                schema: "userManagement",
                table: "agents",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_agents_ReferralCode",
                schema: "userManagement",
                table: "agents",
                column: "ReferralCode",
                unique: true,
                filter: "[ReferralCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_CompanyId_NormalizedName",
                table: "AspNetRoles",
                columns: new[] { "CompanyId", "NormalizedName" },
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_EntryByUserId",
                table: "AspNetRoles",
                column: "EntryByUserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CompanyId_NormalizedEmail",
                table: "AspNetUsers",
                columns: new[] { "CompanyId", "NormalizedEmail" },
                unique: true,
                filter: "[NormalizedEmail] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_EntryByUserId",
                table: "AspNetUsers",
                column: "EntryByUserId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_companies_Pan",
                table: "companies",
                column: "Pan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_companies_RegNo",
                table: "companies",
                column: "RegNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_login_logs_UserId",
                schema: "userManagement",
                table: "login_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_menus_ParentId",
                schema: "userManagement",
                table: "menus",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_module_permissions_ModuleId_Permission",
                schema: "userManagement",
                table: "module_permissions",
                columns: new[] { "ModuleId", "Permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_modules_MenuId",
                schema: "userManagement",
                table: "modules",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_CompanyId_Code",
                table: "permissions",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_role_module_permissions_ModulePermissionId",
                schema: "userManagement",
                table: "role_module_permissions",
                column: "ModulePermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_user_module_permissions_ModulePermissionId",
                schema: "userManagement",
                table: "user_module_permissions",
                column: "ModulePermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_user_module_permissions_UserId_ModulePermissionId",
                schema: "userManagement",
                table: "user_module_permissions",
                columns: new[] { "UserId", "ModulePermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_statuses_UserId",
                schema: "userManagement",
                table: "user_statuses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                schema: "userManagement",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                schema: "userManagement",
                table: "UserRoles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_roles",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "agent_users",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "applications",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "login_logs",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "role_module_permissions",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "user_module_permissions",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "user_statuses",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "agents",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "module_permissions",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "modules",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "menus",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "companies");
        }
    }
}
