using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Shared.Infrastructure.Migrations.HrmMigration
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                schema: "userManagement",
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        principalSchema: "userManagement",
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "userManagement",
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_users_users_EntryByUserId",
                        column: x => x.EntryByUserId,
                        principalSchema: "userManagement",
                        principalTable: "users",
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        name: "FK_agent_users_agents_AgentId",
                        column: x => x.AgentId,
                        principalSchema: "userManagement",
                        principalTable: "agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_agent_users_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "userManagement",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        name: "FK_AspNetUserClaims_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "userManagement",
                        principalTable: "users",
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
                        name: "FK_AspNetUserLogins_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "userManagement",
                        principalTable: "users",
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
                        name: "FK_AspNetUserTokens_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "userManagement",
                        principalTable: "users",
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
                        name: "FK_login_logs_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "userManagement",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EntryByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_roles_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "userManagement",
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_roles_users_EntryByUserId",
                        column: x => x.EntryByUserId,
                        principalSchema: "userManagement",
                        principalTable: "users",
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
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_statuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_statuses_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "userManagement",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "module_permissions",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permission = table.Column<int>(type: "int", nullable: false),
                    ModuleId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_module_permissions_modules_ModuleId1",
                        column: x => x.ModuleId1,
                        principalSchema: "userManagement",
                        principalTable: "modules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "agent_roles",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
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
                        name: "FK_agent_roles_agents_AgentId",
                        column: x => x.AgentId,
                        principalSchema: "userManagement",
                        principalTable: "agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_agent_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "userManagement",
                        principalTable: "roles",
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
                        name: "FK_AspNetRoleClaims_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "userManagement",
                        principalTable: "roles",
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
                        name: "FK_AspNetUserRoles_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "userManagement",
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "userManagement",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "company_roles",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    EntryByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerificationStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_company_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "userManagement",
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "userManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RoleId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    EntryByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerificationStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "userManagement",
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_roles_RoleId1",
                        column: x => x.RoleId1,
                        principalSchema: "userManagement",
                        principalTable: "roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_user_roles_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "userManagement",
                        principalTable: "users",
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
                        name: "FK_role_module_permissions_module_permissions_ModulePermissionId",
                        column: x => x.ModulePermissionId,
                        principalSchema: "userManagement",
                        principalTable: "module_permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_module_permissions_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "userManagement",
                        principalTable: "roles",
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
                    ModulePermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModulePermissionId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_module_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_module_permissions_module_permissions_ModulePermissionId",
                        column: x => x.ModulePermissionId,
                        principalSchema: "userManagement",
                        principalTable: "module_permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_module_permissions_module_permissions_ModulePermissionId1",
                        column: x => x.ModulePermissionId1,
                        principalSchema: "userManagement",
                        principalTable: "module_permissions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_user_module_permissions_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "userManagement",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "applications",
                columns: new[] { "Id", "Code", "Desc", "Name" },
                values: new object[] { new Guid("89de1083-5d8b-401c-8914-7f6cc1363fdf"), 0, "Identity & Access Management for the SaaS platform", "User Management" });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "companies",
                columns: new[] { "Id", "Address", "Email", "Name", "Pan", "PhoneNo", "ProductCode", "RegNo", "Url" },
                values: new object[] { 1, "Kathmandu, Nepal", "info@arsuhrm.com", "ArsuHrm Solutions Pvt. Ltd.", "123456789", "9829967841", "HRM", "REG-001", "https://arsuhrm.com" });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "menus",
                columns: new[] { "Id", "Active", "Color", "Icon", "MenuText", "OrderNo", "ParentId", "ToolTip", "Url" },
                values: new object[] { new Guid("9a71e39c-1e80-423e-9d87-16586687575f"), true, "red", "FaShieldHalved", "User Management", 1, null, "User Management", null });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "agents",
                columns: new[] { "Id", "Address", "CompanyId", "EntryByUserId", "EntryDate", "IsParent", "Name", "Pan", "ReferralCode", "RegNo", "ToDate", "UpdatedByUserId", "UpdatedDate", "VerificationStatus" },
                values: new object[] { new Guid("20000000-0000-0000-0000-000000000001"), "Kathmandu, Nepal", 1, null, new DateTime(2026, 7, 22, 10, 32, 59, 37, DateTimeKind.Utc).AddTicks(1560), true, "Head Office", "123456789", "REG-001--1", "REG-001", null, null, null, 0 });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "menus",
                columns: new[] { "Id", "Active", "Color", "Icon", "MenuText", "OrderNo", "ParentId", "ToolTip", "Url" },
                values: new object[,]
                {
                    { new Guid("37878e39-c706-427e-bc86-0e7d13c76665"), true, "blue", "FaUserGear", "User Role", 3, new Guid("9a71e39c-1e80-423e-9d87-16586687575f"), "Role for User Management", "/UserManagement/user-role" },
                    { new Guid("45bda341-5e70-495c-aecd-075efef1885b"), true, "blue", "FaUsersGear", "Collection Center Role", 2, new Guid("9a71e39c-1e80-423e-9d87-16586687575f"), "Role for Collection and Distribution Center Management", "/UserManagement/agent-role" },
                    { new Guid("5f35399e-05b3-42f1-8548-ab31b8cb731c"), true, "blue", "FaUser", "User", 4, new Guid("9a71e39c-1e80-423e-9d87-16586687575f"), "User Management", "/UserManagement/user" },
                    { new Guid("6a7b8c9d-0e1f-4a2b-8c9d-0e1f2a3b4c5d"), true, "purple", "FaBuilding", "Company Role", 1, new Guid("9a71e39c-1e80-423e-9d87-16586687575f"), "Company Role Management", "/UserManagement/company-role" }
                });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "roles",
                columns: new[] { "Id", "CompanyId", "concurrency_stamp", "Desc", "EntryByUserId", "EntryDate", "FromDate", "Name", "NormalizedName", "ToDate" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), 1, "1b1e4565-e32f-4510-a6e7-7d54796540c5", "Company owner with full permissions", null, new DateTime(2026, 7, 22, 10, 32, 59, 73, DateTimeKind.Utc).AddTicks(9092), new DateTime(2026, 7, 22, 10, 32, 59, 73, DateTimeKind.Utc).AddTicks(9095), "Owner", "OWNER", null },
                    { new Guid("10000000-0000-0000-0000-000000000002"), 1, "8c05e73f-2afd-4259-a9e5-caba554d9825", "Administrator with full access", null, new DateTime(2026, 7, 22, 10, 32, 59, 74, DateTimeKind.Utc).AddTicks(7476), new DateTime(2026, 7, 22, 10, 32, 59, 74, DateTimeKind.Utc).AddTicks(7478), "Admin", "ADMIN", null },
                    { new Guid("10000000-0000-0000-0000-000000000003"), 1, "28f15bfd-69df-4831-a998-3cd30f4157e7", "Manager with operational access", null, new DateTime(2026, 7, 22, 10, 32, 59, 74, DateTimeKind.Utc).AddTicks(7548), new DateTime(2026, 7, 22, 10, 32, 59, 74, DateTimeKind.Utc).AddTicks(7548), "Manager", "MANAGER", null },
                    { new Guid("10000000-0000-0000-0000-000000000004"), 1, "53bbb331-e396-4e22-9e20-edffd36cea52", "Regular user with limited access", null, new DateTime(2026, 7, 22, 10, 32, 59, 74, DateTimeKind.Utc).AddTicks(7553), new DateTime(2026, 7, 22, 10, 32, 59, 74, DateTimeKind.Utc).AddTicks(7553), "User", "USER", null }
                });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "users",
                columns: new[] { "Id", "AccessFailedCount", "CompanyId", "ConcurrencyStamp", "Contact", "Email", "EmailConfirmed", "EntryByUserId", "EntryDate", "FailedLoginAttempts", "FirstName", "IsEmailConfirmed", "LastName", "LockedUntil", "LockoutEnabled", "LockoutEnd", "MiddleName", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("30000000-0000-0000-0000-000000000001"), 0, 1, "e299ddf3-085d-4111-b195-8e89c8ff50cf", "9800000001", "admin@arsuhrm.com", false, null, new DateTime(2026, 7, 22, 10, 32, 59, 82, DateTimeKind.Utc).AddTicks(3946), 0, "Arman", false, "Shrestha", null, false, null, null, "ADMIN@ARSUHRM.COM", "ADMIN.ARSUHRM", null, null, false, "8e791f37-f25b-4e9f-b0de-3698ab698802", false, "admin.arsuhrm" });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "modules",
                columns: new[] { "Id", "ApplicationId", "Code", "Description", "FromDate", "MenuId", "Name", "ToDate" },
                values: new object[,]
                {
                    { new Guid("65d5de5a-3b73-4e45-8775-1b3d6f144268"), new Guid("89de1083-5d8b-401c-8914-7f6cc1363fdf"), 4, "User Management", new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("5f35399e-05b3-42f1-8548-ab31b8cb731c"), "User", null },
                    { new Guid("ba51d83f-8c02-4fb5-922f-650b945b79b2"), new Guid("89de1083-5d8b-401c-8914-7f6cc1363fdf"), 3, "User Role Management", new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("37878e39-c706-427e-bc86-0e7d13c76665"), "UserRole", null },
                    { new Guid("e3c916fb-608f-42b3-87db-1c46ae5b5148"), new Guid("89de1083-5d8b-401c-8914-7f6cc1363fdf"), 2, "Collection Center Role Management", new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("45bda341-5e70-495c-aecd-075efef1885b"), "AgentRole", null },
                    { new Guid("f7a8b9c0-d1e2-4f3a-8b9c-0d1e2f3a4b5c"), new Guid("89de1083-5d8b-401c-8914-7f6cc1363fdf"), 1, "Company Role Management", new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("6a7b8c9d-0e1f-4a2b-8c9d-0e1f2a3b4c5d"), "CompanyRole", null }
                });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "user_roles",
                columns: new[] { "Id", "ApprovedDate", "CompanyId", "EntryByUserId", "EntryDate", "IsActive", "IsApproved", "IsDeleted", "RoleId", "RoleId1", "ToDate", "UpdatedByUserId", "UpdatedDate", "UserId", "VerificationStatus" },
                values: new object[,]
                {
                    { new Guid("ac5bc193-a466-4ee7-855d-e2bbbb0c5fc1"), new DateTime(2026, 7, 22, 10, 32, 59, 89, DateTimeKind.Utc).AddTicks(6502), 1, new Guid("30000000-0000-0000-0000-000000000001"), new DateTime(2026, 7, 22, 10, 32, 59, 89, DateTimeKind.Utc).AddTicks(6489), true, true, false, new Guid("10000000-0000-0000-0000-000000000002"), null, null, null, new DateTime(2026, 7, 22, 10, 32, 59, 89, DateTimeKind.Utc).AddTicks(6501), new Guid("30000000-0000-0000-0000-000000000001"), 0 },
                    { new Guid("fc8d6dae-d715-4b04-aec1-bfe6e2bdf993"), new DateTime(2026, 7, 22, 10, 32, 59, 89, DateTimeKind.Utc).AddTicks(6158), 1, new Guid("30000000-0000-0000-0000-000000000001"), new DateTime(2026, 7, 22, 10, 32, 59, 89, DateTimeKind.Utc).AddTicks(2548), true, true, false, new Guid("10000000-0000-0000-0000-000000000001"), null, null, null, new DateTime(2026, 7, 22, 10, 32, 59, 89, DateTimeKind.Utc).AddTicks(4346), new Guid("30000000-0000-0000-0000-000000000001"), 0 }
                });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "user_statuses",
                columns: new[] { "Id", "FromDate", "Remarks", "ToDate", "UserId" },
                values: new object[] { 1, new DateTime(2026, 7, 22, 10, 32, 59, 91, DateTimeKind.Utc).AddTicks(7905), "Default owner user created", null, new Guid("30000000-0000-0000-0000-000000000001") });

            migrationBuilder.InsertData(
                schema: "userManagement",
                table: "module_permissions",
                columns: new[] { "Id", "ModuleId", "ModuleId1", "Permission" },
                values: new object[,]
                {
                    { new Guid("50000000-0000-0000-0000-000000000001"), new Guid("65d5de5a-3b73-4e45-8775-1b3d6f144268"), null, 0 },
                    { new Guid("50000000-0000-0000-0000-000000000002"), new Guid("65d5de5a-3b73-4e45-8775-1b3d6f144268"), null, 1 },
                    { new Guid("50000000-0000-0000-0000-000000000003"), new Guid("65d5de5a-3b73-4e45-8775-1b3d6f144268"), null, 2 },
                    { new Guid("50000000-0000-0000-0000-000000000004"), new Guid("65d5de5a-3b73-4e45-8775-1b3d6f144268"), null, 3 },
                    { new Guid("50000000-0000-0000-0000-000000000005"), new Guid("ba51d83f-8c02-4fb5-922f-650b945b79b2"), null, 0 },
                    { new Guid("50000000-0000-0000-0000-000000000006"), new Guid("ba51d83f-8c02-4fb5-922f-650b945b79b2"), null, 1 },
                    { new Guid("50000000-0000-0000-0000-000000000007"), new Guid("ba51d83f-8c02-4fb5-922f-650b945b79b2"), null, 2 },
                    { new Guid("50000000-0000-0000-0000-000000000008"), new Guid("ba51d83f-8c02-4fb5-922f-650b945b79b2"), null, 3 },
                    { new Guid("50000000-0000-0000-0000-000000000009"), new Guid("e3c916fb-608f-42b3-87db-1c46ae5b5148"), null, 0 },
                    { new Guid("50000000-0000-0000-0000-00000000000a"), new Guid("e3c916fb-608f-42b3-87db-1c46ae5b5148"), null, 1 },
                    { new Guid("50000000-0000-0000-0000-00000000000b"), new Guid("e3c916fb-608f-42b3-87db-1c46ae5b5148"), null, 2 },
                    { new Guid("50000000-0000-0000-0000-00000000000c"), new Guid("e3c916fb-608f-42b3-87db-1c46ae5b5148"), null, 3 },
                    { new Guid("60000000-0000-0000-0000-000000000001"), new Guid("f7a8b9c0-d1e2-4f3a-8b9c-0d1e2f3a4b5c"), null, 0 },
                    { new Guid("60000000-0000-0000-0000-000000000002"), new Guid("f7a8b9c0-d1e2-4f3a-8b9c-0d1e2f3a4b5c"), null, 1 },
                    { new Guid("60000000-0000-0000-0000-000000000003"), new Guid("f7a8b9c0-d1e2-4f3a-8b9c-0d1e2f3a4b5c"), null, 2 },
                    { new Guid("60000000-0000-0000-0000-000000000004"), new Guid("f7a8b9c0-d1e2-4f3a-8b9c-0d1e2f3a4b5c"), null, 3 }
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
                    { new Guid("60000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("60000000-0000-0000-0000-000000000002"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("60000000-0000-0000-0000-000000000003"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("60000000-0000-0000-0000-000000000004"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("50000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("50000000-0000-0000-0000-000000000002"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("50000000-0000-0000-0000-000000000005"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("50000000-0000-0000-0000-000000000006"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("50000000-0000-0000-0000-000000000009"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("50000000-0000-0000-0000-00000000000a"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("60000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("60000000-0000-0000-0000-000000000002"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("50000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000003") },
                    { new Guid("50000000-0000-0000-0000-000000000005"), new Guid("10000000-0000-0000-0000-000000000003") },
                    { new Guid("50000000-0000-0000-0000-000000000009"), new Guid("10000000-0000-0000-0000-000000000003") },
                    { new Guid("60000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000003") }
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
                name: "IX_companies_Pan",
                schema: "userManagement",
                table: "companies",
                column: "Pan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_companies_RegNo",
                schema: "userManagement",
                table: "companies",
                column: "RegNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_company_roles_RoleId",
                schema: "userManagement",
                table: "company_roles",
                column: "RoleId");

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
                name: "IX_module_permissions_ModuleId1",
                schema: "userManagement",
                table: "module_permissions",
                column: "ModuleId1");

            migrationBuilder.CreateIndex(
                name: "IX_modules_MenuId",
                schema: "userManagement",
                table: "modules",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_role_module_permissions_ModulePermissionId",
                schema: "userManagement",
                table: "role_module_permissions",
                column: "ModulePermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_roles_CompanyId_NormalizedName",
                schema: "userManagement",
                table: "roles",
                columns: new[] { "CompanyId", "NormalizedName" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_roles_EntryByUserId",
                schema: "userManagement",
                table: "roles",
                column: "EntryByUserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "userManagement",
                table: "roles",
                column: "NormalizedName",
                unique: true,
                filter: "[CompanyId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_user_module_permissions_ModulePermissionId",
                schema: "userManagement",
                table: "user_module_permissions",
                column: "ModulePermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_user_module_permissions_ModulePermissionId1",
                schema: "userManagement",
                table: "user_module_permissions",
                column: "ModulePermissionId1");

            migrationBuilder.CreateIndex(
                name: "IX_user_module_permissions_UserId_ModulePermissionId",
                schema: "userManagement",
                table: "user_module_permissions",
                columns: new[] { "UserId", "ModulePermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_RoleId",
                schema: "userManagement",
                table: "user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_RoleId1",
                schema: "userManagement",
                table: "user_roles",
                column: "RoleId1");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_UserId_CompanyId",
                schema: "userManagement",
                table: "user_roles",
                columns: new[] { "UserId", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_UserId_RoleId",
                schema: "userManagement",
                table: "user_roles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_UserId_RoleId_CompanyId",
                schema: "userManagement",
                table: "user_roles",
                columns: new[] { "UserId", "RoleId", "CompanyId" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_user_statuses_UserId",
                schema: "userManagement",
                table: "user_statuses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_statuses_UserId_FromDate",
                schema: "userManagement",
                table: "user_statuses",
                columns: new[] { "UserId", "FromDate" });

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "userManagement",
                table: "users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_users_CompanyId_NormalizedEmail",
                schema: "userManagement",
                table: "users",
                columns: new[] { "CompanyId", "NormalizedEmail" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL AND [NormalizedEmail] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_EntryByUserId",
                schema: "userManagement",
                table: "users",
                column: "EntryByUserId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "userManagement",
                table: "users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
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
                name: "company_roles",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "login_logs",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "role_module_permissions",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "user_module_permissions",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "user_statuses",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "agents",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "module_permissions",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "modules",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "users",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "menus",
                schema: "userManagement");

            migrationBuilder.DropTable(
                name: "companies",
                schema: "userManagement");
        }
    }
}
