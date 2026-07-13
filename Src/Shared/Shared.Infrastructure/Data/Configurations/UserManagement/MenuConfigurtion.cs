namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        _ = builder.ToTable("menus", Schemas.UserManagement);

        //var seedMenu = new List<Menu>
        //    {
        //        SeedMenu.UserManagement,
        //        SeedMenu.AgentRole,
        //        SeedMenu.UserRole,
        //        SeedMenu.User,

        //    };

        //_ = builder.HasData(seedMenu);
    }
}

public class SeedMenu
{
    private static DateTime LastUpdatedTime = DateTime.Parse("2024-06-06");
    #region UserManagement - 1
    public static Menu UserManagement = new(Guid.Parse("9a71e39c-1e80-423e-9d87-16586687575f"), "UserManagement", "UserManagement", 1, null, null, "FaShieldHalved", "red", true);
    public static Menu AgentRole = new(Guid.Parse("45bda341-5e70-495c-aecd-075efef1885b"), "Collection Center Role", "Role for Collection and Distribution Center Management", 1, "/UserManagement/agent-role", UserManagement.Id, "FaUsersGear", "blue", true);
    public static Menu UserRole = new(Guid.Parse("37878e39-c706-427e-bc86-0e7d13c76665"), "User Role", "Role for User Management", 2, "/UserManagement/user-role", UserManagement.Id, "FaUserGear", "blue", true);
    public static Menu User = new(Guid.Parse("5f35399e-05b3-42f1-8548-ab31b8cb731c"), "User", "User Management", 3, "/UserManagement/user", UserManagement.Id, "FaUser", "blue", true);
    #endregion

}
