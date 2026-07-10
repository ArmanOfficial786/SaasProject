//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Logging;
//using Shared.Application.Interfaces;
//using UserManagement.Domain.Entities;

//namespace Shared.Application.SeedData;

///// <summary>
///// DbInitializer seeds data that requires business logic at runtime.
///// This includes Users (password hashing via UserManager), UserRoles, AgentUser, and AgentRole.
///// 
///// Static reference data like Company, Roles, and ModulePermissions are seeded via HasData in configurations.
///// 
///// Call this after Migrate() in Program.cs to ensure idempotent seeding.
///// </summary>
//public class DbInitializer
//{
//    private readonly IUnitOfWork _unitOfWork;
//    private readonly UserManager<User> _userManager;
//    private readonly ILogger<DbInitializer> _logger;

//    public DbInitializer(IUnitOfWork unitOfWork, UserManager<User> userManager, ILogger<DbInitializer> logger)
//    {
//        _unitOfWork = unitOfWork;
//        _userManager = userManager;
//        _logger = logger;
//    }

//    /// <summary>
//    /// Seeds application data that requires business logic (password hashing, etc.).
//    /// Idempotent: checks if data already exists before inserting.
//    /// </summary>
//    public async Task SeedAsync()
//    {
//        try
//        {
//            _logger.LogInformation("Starting database seeding...");

//            // Check if users already exist (idempotency)
//            var adminUserExists = await _userManager.FindByNameAsync("admin");
//            if (adminUserExists != null)
//            {
//                _logger.LogInformation("Users already seeded. Skipping.");
//                return;
//            }

//            // Get the company (created via HasData in migration)
//            var company = await _unitOfWork.Repository<Company>().GetSingleOrDefaultAsync(c => c.Id == 1);
//            if (company == null)
//            {
//                _logger.LogError("Company with ID 1 not found. Cannot seed users.");
//                return;
//            }

//            // Get predefined roles (created via HasData in migration)
//            var adminRole = await _unitOfWork.Repository<Role>().GetSingleOrDefaultAsync(r => r.Name == "Admin");
//            var managerRole = await _unitOfWork.Repository<Role>().GetSingleOrDefaultAsync(r => r.Name == "Manager");
//            var userRole = await _unitOfWork.Repository<Role>().GetSingleOrDefaultAsync(r => r.Name == "User");

//            if (adminRole == null || managerRole == null || userRole == null)
//            {
//                _logger.LogError("Required roles not found. Cannot seed users.");
//                return;
//            }

//            // Seed users with password hashing
//            var adminUser = await SeedAdminUser(company, adminRole);
//            var managerUser = await SeedManagerUser(company, managerRole);
//            var testUser = await SeedTestUser(company, userRole);

//            // Seed agent (if needed for AgentUser/AgentRole associations)
//            var agent = await SeedAgent(company);

//            // Seed agent-user associations
//            if (agent != null)
//            {
//                await SeedAgentUsers(adminUser, managerUser, testUser, agent);
//            }

//            _logger.LogInformation("Database seeding completed successfully.");
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "An error occurred during database seeding.");
//            throw;
//        }
//    }

//    private async Task<User> SeedAdminUser(Company company, Role adminRole)
//    {
//        var adminUser = new User(
//           companyId: companyId,
//            companyId: company.Id,
//            userName: "admin",
//            firstName: "Admin",
//            middleName: "",
//            lastName: "User",
//            email: "admin@armansoftware.com",
//            contact: "+977-1-4000000",
//            entryByUserId: null
//        )
//        {
//            Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
//            EmailConfirmed = true
//        };

//        var result = await _userManager.CreateAsync(adminUser, "admin@123");
//        if (!result.Succeeded)
//        {
//            _logger.LogWarning("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
//            return adminUser;
//        }

//        // Assign admin role
//        await _userManager.AddToRoleAsync(adminUser, adminRole.Name!);
//        _logger.LogInformation("Seeded admin user: {Username}", adminUser.UserName);

//        return adminUser;
//    }

//    private async Task<User> SeedManagerUser(Company company, Role managerRole)
//    {
//        var managerUser = new User(
//            company: company,
//            companyId: company.Id,
//            userName: "manager",
//            firstName: "Manager",
//            middleName: "",
//            lastName: "User",
//            email: "manager@armansoftware.com",
//            contact: "+977-1-4000001",
//            entryByUserId: null
//        )
//        {
//            Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
//            EmailConfirmed = true
//        };

//        var result = await _userManager.CreateAsync(managerUser, "manager@123");
//        if (!result.Succeeded)
//        {
//            _logger.LogWarning("Failed to create manager user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
//            return managerUser;
//        }

//        // Assign manager role
//        await _userManager.AddToRoleAsync(managerUser, managerRole.Name!);
//        _logger.LogInformation("Seeded manager user: {Username}", managerUser.UserName);

//        return managerUser;
//    }

//    private async Task<User> SeedTestUser(Company company, Role userRole)
//    {
//        var testUser = new User(
//            company: company,
//            companyId: company.Id,
//            userName: "testuser",
//            firstName: "Test",
//            middleName: "",
//            lastName: "User",
//            email: "testuser@armansoftware.com",
//            contact: "+977-1-4000002",
//            entryByUserId: null
//        )
//        {
//            Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
//            EmailConfirmed = true
//        };

//        var result = await _userManager.CreateAsync(testUser, "testuser@123");
//        if (!result.Succeeded)
//        {
//            _logger.LogWarning("Failed to create test user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
//            return testUser;
//        }

//        // Assign user role
//        await _userManager.AddToRoleAsync(testUser, userRole.Name!);
//        _logger.LogInformation("Seeded test user: {Username}", testUser.UserName);

//        return testUser;
//    }

//    private async Task<Agent?> SeedAgent(Company company)
//    {
//        // Check if agent already exists
//        var existingAgent = await _unitOfWork.Repository<Agent>().GetSingleOrDefaultAsync(a => a.CompanyId == company.Id);
//        if (existingAgent != null)
//        {
//            return existingAgent;
//        }

//        var agent = new Agent(
//            name: "Head Office",
//            address: "Kathmandu, Nepal",
//            pan: "123456789",
//            regNo: "ORG-2024-001",
//            isParent: true,
//            referralCode: "HO-2024",
//            company: company,
//            companyId: company.Id
//        )
//        {
//            Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
//            Company = company
//        };

//        await _unitOfWork.Repository<Agent>().InsertAsync(agent);
//        await _unitOfWork.SaveChangesAsync();
//        _logger.LogInformation("Seeded agent: {Name} for company {CompanyId}", agent.Name, company.Id);

//        return agent;
//    }

//    private async Task SeedAgentUsers(User adminUser, User managerUser, User testUser, Agent agent)
//    {
//        // Check if agent users already exist
//        var existingAgentUsers = await _unitOfWork.Repository<AgentUser>().GetSingleOrDefaultAsync(au => au.AgentId == agent.Id && au.ToDate == null);

//        if (existingAgentUsers != null)
//        {
//            _logger.LogInformation("Agent users already exist. Skipping seeding.");
//            return;
//        }

//        // Seed agent-user associations
//        var adminAgentUser = new AgentUser(adminUser.Id, agent.Id);
//        var managerAgentUser = new AgentUser(managerUser.Id, agent.Id);
//        var testAgentUser = new AgentUser(testUser.Id, agent.Id);

//        await _unitOfWork.Repository<AgentUser>().InsertAsync(adminAgentUser);
//        await _unitOfWork.Repository<AgentUser>().InsertAsync(managerAgentUser);
//        await _unitOfWork.Repository<AgentUser>().InsertAsync(testAgentUser);
//        await _unitOfWork.SaveChangesAsync();

//        _logger.LogInformation("Seeded {Count} agent-user associations for agent {AgentId}", 3, agent.Id);
//    }
//}



using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using UserManagement.Domain.Entities;

namespace Shared.Application.SeedData;

/// <summary>
/// DbInitializer seeds data that requires business logic at runtime.
/// This includes Users (password hashing via UserManager), UserRoles, AgentUser, and AgentRole.
/// 
/// Static reference data like Company and ModulePermissions is seeded via HasData in configurations.
/// Roles are now ALSO seeded here at runtime (see note on RoleConfiguration below) since UserRole
/// FK resolution against seed Users requires runtime User IDs to already exist.
/// 
/// Call this after Migrate() in Program.cs to ensure idempotent seeding.
/// </summary>
public class DbInitializer
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(IUnitOfWork unitOfWork, UserManager<User> userManager, ILogger<DbInitializer> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            var adminUserExists = await _userManager.FindByNameAsync("admin");
            if (adminUserExists != null)
            {
                _logger.LogInformation("Users already seeded. Skipping.");
                return;
            }

            var company = await _unitOfWork.Repository<Company>().GetSingleOrDefaultAsync(c => c.Id == 1);
            if (company == null)
            {
                _logger.LogError("Company with ID 1 not found. Cannot seed users.");
                return;
            }

            var adminRole = await _unitOfWork.Repository<Role>().GetSingleOrDefaultAsync(r => r.Name == "Admin");
            var managerRole = await _unitOfWork.Repository<Role>().GetSingleOrDefaultAsync(r => r.Name == "Manager");
            var userRole = await _unitOfWork.Repository<Role>().GetSingleOrDefaultAsync(r => r.Name == "User");

            if (adminRole == null || managerRole == null || userRole == null)
            {
                _logger.LogError("Required roles not found. Cannot seed users.");
                return;
            }

            var adminUser = await SeedAdminUser(company.Id, adminRole);
            var managerUser = await SeedManagerUser(company.Id, managerRole);
            var testUser = await SeedTestUser(company.Id, userRole);

            var agent = await SeedAgent(company.Id);

            if (agent != null)
            {
                await SeedAgentUsers(adminUser, managerUser, testUser, agent);
            }

            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database seeding.");
            throw;
        }
    }

    private async Task<User> SeedAdminUser(int companyId, Role adminRole)
    {
        var adminUser = new User(
            companyId: companyId,
            userName: "admin",
            firstName: "Admin",
            middleName: "",
            lastName: "User",
            email: "admin@armansoftware.com",
            contact: "+977-1-4000000",
            entryByUserId: null
        )
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(adminUser, "admin@123");
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return adminUser;
        }

        await _userManager.AddToRoleAsync(adminUser, adminRole.Name!);
        _logger.LogInformation("Seeded admin user: {Username}", adminUser.UserName);

        return adminUser;
    }

    private async Task<User> SeedManagerUser(int companyId, Role managerRole)
    {
        var managerUser = new User(
            companyId: companyId,
            userName: "manager",
            firstName: "Manager",
            middleName: "",
            lastName: "User",
            email: "manager@armansoftware.com",
            contact: "+977-1-4000001",
            entryByUserId: null
        )
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(managerUser, "manager@123");
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed to create manager user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return managerUser;
        }

        await _userManager.AddToRoleAsync(managerUser, managerRole.Name!);
        _logger.LogInformation("Seeded manager user: {Username}", managerUser.UserName);

        return managerUser;
    }

    private async Task<User> SeedTestUser(int companyId, Role userRole)
    {
        var testUser = new User(
            companyId: companyId,
            userName: "testuser",
            firstName: "Test",
            middleName: "",
            lastName: "User",
            email: "testuser@armansoftware.com",
            contact: "+977-1-4000002",
            entryByUserId: null
        )
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(testUser, "testuser@123");
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed to create test user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return testUser;
        }

        await _userManager.AddToRoleAsync(testUser, userRole.Name!);
        _logger.LogInformation("Seeded test user: {Username}", testUser.UserName);

        return testUser;
    }

    private async Task<Agent?> SeedAgent(int companyId)
    {
        var existingAgent = await _unitOfWork.Repository<Agent>().GetSingleOrDefaultAsync(a => a.CompanyId == companyId);
        if (existingAgent != null)
        {
            return existingAgent;
        }

        var agent = new Agent(
            name: "Head Office",
            address: "Kathmandu, Nepal",
            pan: "123456789",
            regNo: "ORG-2024-001",
            isParent: true,
            referralCode: "HO-2024",
            companyId: companyId
        )
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
        };

        await _unitOfWork.Repository<Agent>().InsertAsync(agent);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Seeded agent: {Name} for company {CompanyId}", agent.Name, companyId);

        return agent;
    }

    private async Task SeedAgentUsers(User adminUser, User managerUser, User testUser, Agent agent)
    {
        var existingAgentUsers = await _unitOfWork.Repository<AgentUser>().GetSingleOrDefaultAsync(au => au.AgentId == agent.Id && au.ToDate == null);

        if (existingAgentUsers != null)
        {
            _logger.LogInformation("Agent users already exist. Skipping seeding.");
            return;
        }

        var adminAgentUser = new AgentUser(adminUser.Id, agent.Id);
        var managerAgentUser = new AgentUser(managerUser.Id, agent.Id);
        var testAgentUser = new AgentUser(testUser.Id, agent.Id);

        await _unitOfWork.Repository<AgentUser>().InsertAsync(adminAgentUser);
        await _unitOfWork.Repository<AgentUser>().InsertAsync(managerAgentUser);
        await _unitOfWork.Repository<AgentUser>().InsertAsync(testAgentUser);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} agent-user associations for agent {AgentId}", 3, agent.Id);
    }
}



