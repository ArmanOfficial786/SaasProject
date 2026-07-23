namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        _ = builder.ToTable("menus", Schemas.UserManagement);

        // Self-referencing relationship for hierarchical menus
        _ = builder.HasOne(m => m.Parent)
                   .WithMany(p => p.Children)
                   .HasForeignKey(m => m.ParentId)
                   .OnDelete(DeleteBehavior.Restrict);

        var seedMenu = new List<Menu>
            {
                SeedMenu.UserManagement,
                SeedMenu.CompanyRole,
                SeedMenu.AgentRole,
                SeedMenu.UserRole,
                SeedMenu.User,

            };

        _ = builder.HasData(seedMenu);
    }
}
public static class SeedMenu
{
    private static readonly DateTime _lastUpdated = DateTime.Parse("2024-06-06");

    #region UserManagement Menus

    public static Menu UserManagement = new(
        id: Guid.Parse("9a71e39c-1e80-423e-9d87-16586687575f"),
        menuText: "User Management",
        toolTip: "User Management",
        orderNo: 1,
        url: null,
        parentId: null,
        icon: "FaShieldHalved",
        color: "red",
        active: true
    );

    /// <summary>
    /// CompanyRole Menu - Sub-menu of UserManagement
    /// </summary>
    public static Menu CompanyRole = new(
        id: Guid.Parse("6a7b8c9d-0e1f-4a2b-8c9d-0e1f2a3b4c5d"),
        menuText: "Company Role",
        toolTip: "Company Role Management",
        orderNo: 1,
        url: "/UserManagement/company-role",
        parentId: UserManagement.Id,
        icon: "FaBuilding",
        color: "purple",
        active: true
    );

    public static Menu AgentRole = new(
        id: Guid.Parse("45bda341-5e70-495c-aecd-075efef1885b"),
        menuText: "Collection Center Role",
        toolTip: "Role for Collection and Distribution Center Management",
        orderNo: 2,
        url: "/UserManagement/agent-role",
        parentId: UserManagement.Id,
        icon: "FaUsersGear",
        color: "blue",
        active: true
    );

    public static Menu UserRole = new(
        id: Guid.Parse("37878e39-c706-427e-bc86-0e7d13c76665"),
        menuText: "User Role",
        toolTip: "Role for User Management",
        orderNo: 3,
        url: "/UserManagement/user-role",
        parentId: UserManagement.Id,
        icon: "FaUserGear",
        color: "blue",
        active: true
    );

    public static Menu User = new(
        id: Guid.Parse("5f35399e-05b3-42f1-8548-ab31b8cb731c"),
        menuText: "User",
        toolTip: "User Management",
        orderNo: 4,
        url: "/UserManagement/user",
        parentId: UserManagement.Id,
        icon: "FaUser",
        color: "blue",
        active: true
    );

    #endregion

    public static List<Menu> GetAll() => new()
    {
        UserManagement,
        CompanyRole,
        AgentRole,
        UserRole,
        User,
    };
}
