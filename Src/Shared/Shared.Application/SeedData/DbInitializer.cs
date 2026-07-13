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
/// DbInitializer creates the ONE bootstrap "system" company, its default Admin/
/// Manager/User roles, its Head Office agent, and its admin/manager/testuser
/// accounts — entirely at runtime, entirely idempotent. No migration-time HasData
/// is involved anywhere; this is the only source of that first company's data.
///
/// Every other company (created by superadmin) goes through
/// CreateCompanyCommandHandler instead, which follows the identical
/// Company -> Roles -> Agent -> User sequence per tenant.
///
/// Call after Migrate() in Program.cs. Safe to run every startup.
/// </summary>
public class DbInitializer
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DbInitializer> _logger;

    private static readonly Guid BootstrapAdminId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid BootstrapManagerId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    private static readonly Guid BootstrapTestUserId = Guid.Parse("20000000-0000-0000-0000-000000000003");
    private static readonly Guid BootstrapAgentId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private const string BootstrapRegNo = "SYSTEM-DEFAULT";

    public DbInitializer(IUnitOfWork unitOfWork, UserManager<User> userManager, ILogger<DbInitializer> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seeding...");

        var company = await GetOrCreateBootstrapCompany();
        if (company == null) return;

        var (adminRole, managerRole, userRole) = await GetOrCreateRoles(company.Id);

        var adminUser = await SeedUserSafe(company.Id, adminRole, "admin",
            BootstrapAdminId, "Admin", "User",
            "admin@armansoftware.com", "+977-1-4000000", "admin@123");

        var managerUser = await SeedUserSafe(company.Id, managerRole, "manager",
            BootstrapManagerId, "Manager", "User",
            "manager@armansoftware.com", "+977-1-4000001", "manager@123");

        var testUser = await SeedUserSafe(company.Id, userRole, "testuser",
            BootstrapTestUserId, "Test", "User",
            "testuser@armansoftware.com", "+977-1-4000002", "testuser@123");

        var agent = await GetOrCreateAgent(company.Id);

        if (agent != null && adminUser != null && managerUser != null && testUser != null)
        {
            await LinkAgentUsers(adminUser, managerUser, testUser, agent);
        }

        _logger.LogInformation("Database seeding completed.");
    }

    private async Task<Company?> GetOrCreateBootstrapCompany()
    {
        var companyRepo = _unitOfWork.Repository<Company>();

        var existing = await companyRepo.GetSingleOrDefaultAsync(c => c.RegNo == BootstrapRegNo);
        if (existing != null) return existing;

        var company = new Company(
            productCode: "SYS",
            name: "Arman Software Solutions",
            email: "info@armansoftware.com",
            address: "Kathmandu, Nepal",
            phoneNo: "+977-1-4000000",
            pan: "600000000",
            regNo: BootstrapRegNo,
            url: "https://armansoftware.com"
        );

        await companyRepo.InsertAsync(company);
        await _unitOfWork.SaveChangesAsync(); // flush now — need real company.Id below

        _logger.LogInformation("Seeded bootstrap company with Id {CompanyId}", company.Id);
        return company;
    }

    private async Task<(Role Admin, Role Manager, Role User)> GetOrCreateRoles(int companyId)
    {
        var roleRepo = _unitOfWork.Repository<Role>();

        var existingAdmin = await roleRepo.GetSingleOrDefaultAsync(r => r.CompanyId == companyId && r.NormalizedName == "ADMIN");
        var existingManager = await roleRepo.GetSingleOrDefaultAsync(r => r.CompanyId == companyId && r.NormalizedName == "MANAGER");
        var existingUser = await roleRepo.GetSingleOrDefaultAsync(r => r.CompanyId == companyId && r.NormalizedName == "USER");

        if (existingAdmin != null && existingManager != null && existingUser != null)
        {
            return (existingAdmin, existingManager, existingUser);
        }

        var (admin, manager, user) = DefaultRoleFactory.CreateForCompany(companyId);

        if (existingAdmin == null) await roleRepo.InsertAsync(admin); else admin = existingAdmin;
        if (existingManager == null) await roleRepo.InsertAsync(manager); else manager = existingManager;
        if (existingUser == null) await roleRepo.InsertAsync(user); else user = existingUser;

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Seeded default roles for company {CompanyId}", companyId);

        return (admin, manager, user);
    }

    private async Task<User?> SeedUserSafe(int companyId, Role role, string userName, Guid id,
        string firstName, string lastName, string email, string contact, string password)
    {
        var existingById = await _unitOfWork.Repository<User>()
            .GetSingleOrDefaultAsync(u => u.Id == id, disableTracking: true);
        if (existingById != null)
        {
            _logger.LogInformation("User {UserName} already exists. Skipping.", userName);
            return existingById;
        }

        var existingByName = await _userManager.FindByNameAsync(userName);
        if (existingByName != null)
        {
            _logger.LogInformation("Username {UserName} already taken. Reusing existing.", userName);
            return existingByName;
        }

        var user = new User(
            companyId: companyId,
            userName: userName,
            firstName: firstName,
            middleName: "",
            lastName: lastName,
            email: email,
            contact: contact,
            entryByUserId: null
        )
        {
            Id = id,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed to create {UserName}: {Errors}", userName,
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return null;
        }

        await _userManager.AddToRoleAsync(user, role.Name!);
        _logger.LogInformation("Seeded user: {UserName}", userName);
        return user;
    }

    private async Task<Agent?> GetOrCreateAgent(int companyId)
    {
        var existing = await _unitOfWork.Repository<Agent>()
            .GetSingleOrDefaultAsync(a => a.CompanyId == companyId, disableTracking: true);
        if (existing != null) return existing;

        var agent = new Agent(
            name: "Head Office",
            address: "Kathmandu, Nepal",
            pan: "999999999",
            regNo: $"{BootstrapRegNo}-HO",
            isParent: true,
            referralCode: $"{BootstrapRegNo}-{companyId}",
            companyId: companyId
        )
        {
            Id = BootstrapAgentId
        };

        await _unitOfWork.Repository<Agent>().InsertAsync(agent);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Seeded agent {Name} for company {CompanyId}", agent.Name, companyId);
        return agent;
    }

    private async Task LinkAgentUsers(User adminUser, User managerUser, User testUser, Agent agent)
    {
        var existing = await _unitOfWork.Repository<AgentUser>()
            .GetSingleOrDefaultAsync(au => au.AgentId == agent.Id && au.ToDate == null, disableTracking: true);
        if (existing != null)
        {
            _logger.LogInformation("Agent-user links already exist. Skipping.");
            return;
        }

        await _unitOfWork.Repository<AgentUser>().InsertAsync(new AgentUser(adminUser.Id, agent.Id));
        await _unitOfWork.Repository<AgentUser>().InsertAsync(new AgentUser(managerUser.Id, agent.Id));
        await _unitOfWork.Repository<AgentUser>().InsertAsync(new AgentUser(testUser.Id, agent.Id));
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Seeded 3 agent-user links for agent {AgentId}", agent.Id);
    }
}
