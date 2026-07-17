// File: Shared.Application/SeedData/DbInitializer.cs

using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using UserManagement.Domain.Entities;

namespace Shared.Application.SeedData;

/// <summary>
/// Seeds bootstrap data straight from four JSON files, read directly with
/// System.Text.Json — no SeedXOptions classes, no IConfiguration/DI binding.
/// Files are linked by CompanyRegNo since the real int CompanyId only exists
/// after the company row is inserted at runtime.
///
/// Role assignment goes through user.AddRole(role) + UnitOfWork, NOT
/// UserManager.AddToRoleAsync — that writes to Identity's own default
/// AspNetUserRoles junction table, separate from the custom UserRole entity
/// everything else in this codebase reads from.
/// </summary>
public class DbInitializer
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DbInitializer> _logger;
    private readonly string _seedDataPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Inline DTOs — just deserialization targets, not shared/reused anywhere else.
    private record CompanySeed(string ProductCode, string Name, string Email, string Address,
        string PhoneNo, string Pan, string RegNo, string Url);

    private record RoleSeed(string Name, string Description, bool IsSystemRole, string CompanyRegNo);

    private record UserSeed(Guid Id, string UserName, string FirstName, string LastName,
        string Email, string Contact, string Password, string RoleName, string CompanyRegNo);

    private record AgentSeed(Guid Id, string Name, string Address, string Pan,
        string RegNoSuffix, bool IsParent, string CompanyRegNo);

    public DbInitializer(
        IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        ILogger<DbInitializer> logger,
        string seedDataPath = "SeedData")
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
        _seedDataPath = seedDataPath;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seeding...");

        var companies = await ReadSeedFile<CompanySeed>("companies.json");
        var roleSeeds = await ReadSeedFile<RoleSeed>("roles.json");
        var userSeeds = await ReadSeedFile<UserSeed>("users.json");
        var agentSeeds = await ReadSeedFile<AgentSeed>("agents.json");

        foreach (var companySeed in companies)
        {
            var company = await GetOrCreateCompany(companySeed);

            var roles = await GetOrCreateRoles(company.Id,
                roleSeeds.Where(r => r.CompanyRegNo == companySeed.RegNo));

            var seededUsers = new List<User>();
            foreach (var userSeed in userSeeds.Where(u => u.CompanyRegNo == companySeed.RegNo))
            {
                if (!roles.TryGetValue(userSeed.RoleName, out var role))
                {
                    _logger.LogWarning("Seed user {UserName} references unknown role {RoleName}. Skipping.",
                        userSeed.UserName, userSeed.RoleName);
                    continue;
                }

                var user = await SeedUserSafe(company.Id, role, userSeed);
                if (user != null) seededUsers.Add(user);
            }

            var agentSeed = agentSeeds.FirstOrDefault(a => a.CompanyRegNo == companySeed.RegNo);
            if (agentSeed != null)
            {
                var agent = await GetOrCreateAgent(company.Id, companySeed.RegNo, agentSeed);
                if (agent != null && seededUsers.Count > 0)
                    await LinkAgentUsers(seededUsers, agent);
            }
        }

        _logger.LogInformation("Database seeding completed.");
    }

    private async Task<List<T>> ReadSeedFile<T>(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, _seedDataPath, fileName);

        if (!File.Exists(path))
        {
            _logger.LogWarning("Seed file {Path} not found. Skipping.", path);
            return [];
        }

        await using var stream = File.OpenRead(path);
        var items = await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonOptions);
        return items ?? [];
    }

    private async Task<Company> GetOrCreateCompany(CompanySeed cfg)
    {
        var companyRepo = _unitOfWork.Repository<Company>();

        var existing = await companyRepo.GetSingleOrDefaultAsync(c => c.RegNo == cfg.RegNo);
        if (existing != null) return existing;

        var company = new Company(
            productCode: cfg.ProductCode,
            name: cfg.Name,
            email: cfg.Email,
            address: cfg.Address,
            phoneNo: cfg.PhoneNo,
            pan: cfg.Pan,
            regNo: cfg.RegNo,
            url: cfg.Url
        );

        await companyRepo.InsertAsync(company);
        await _unitOfWork.SaveChangesAsync(); // flush now — real company.Id needed below

        _logger.LogInformation("Seeded company {Name} with Id {CompanyId}", company.Name, company.Id);
        return company;
    }

    private async Task<Dictionary<string, Role>> GetOrCreateRoles(int companyId, IEnumerable<RoleSeed> roleSeeds)
    {
        var roleRepo = _unitOfWork.Repository<Role>();
        var result = new Dictionary<string, Role>(StringComparer.OrdinalIgnoreCase);
        var anyInserted = false;

        foreach (var roleSeed in roleSeeds)
        {
            var normalizedName = roleSeed.Name.ToUpperInvariant();

            var existing = await roleRepo.GetSingleOrDefaultAsync(
                r => r.CompanyId == companyId && r.NormalizedName == normalizedName);

            if (existing != null)
            {
                result[roleSeed.Name] = existing;
                continue;
            }

            var role = new Role(
                name: roleSeed.Name,
                desc: roleSeed.Description,
                companyId: companyId
            );

            await roleRepo.InsertAsync(role);
            result[roleSeed.Name] = role;
            anyInserted = true;
        }

        if (anyInserted)
        {
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Seeded roles for company {CompanyId}", companyId);
        }

        return result;
    }

    private async Task<User?> SeedUserSafe(int companyId, Role role, UserSeed userSeed)
    {
        var existingById = await _unitOfWork.Repository<User>()
            .GetSingleOrDefaultAsync(u => u.Id == userSeed.Id, disableTracking: true);
        if (existingById != null)
        {
            _logger.LogInformation("User {UserName} already exists. Skipping.", userSeed.UserName);
            return existingById;
        }

        var existingByName = await _userManager.FindByNameAsync(userSeed.UserName);
        if (existingByName != null)
        {
            _logger.LogInformation("Username {UserName} already taken. Reusing existing.", userSeed.UserName);
            return existingByName;
        }

        var user = new User(
            userName: userSeed.UserName,
            firstName: userSeed.FirstName,
            middleName: null,
            lastName: userSeed.LastName,
            email: userSeed.Email,
            contact: userSeed.Contact,
            entryByUserId: null,
            companyId: companyId
        )
        {
            Id = userSeed.Id,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, userSeed.Password);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed to create {UserName}: {Errors}", userSeed.UserName,
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return null;
        }

        // Goes through the domain, not UserManager.AddToRoleAsync — see class summary.
        user.AddRole(role);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Seeded user: {UserName}", userSeed.UserName);
        return user;
    }

    private async Task<Agent?> GetOrCreateAgent(int companyId, string companyRegNo, AgentSeed agentSeed)
    {
        var existing = await _unitOfWork.Repository<Agent>()
            .GetSingleOrDefaultAsync(a => a.CompanyId == companyId, disableTracking: true);
        if (existing != null) return existing;

        var agent = new Agent(
            name: agentSeed.Name,
            address: agentSeed.Address,
            pan: agentSeed.Pan,
            regNo: $"{companyRegNo}{agentSeed.RegNoSuffix}",
            isParent: agentSeed.IsParent,
            referralCode: $"{companyRegNo}-{companyId}",
            companyId: companyId
        )
        {
            Id = agentSeed.Id
        };

        await _unitOfWork.Repository<Agent>().InsertAsync(agent);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Seeded agent {Name} for company {CompanyId}", agent.Name, companyId);
        return agent;
    }

    private async Task LinkAgentUsers(List<User> users, Agent agent)
    {
        var existing = await _unitOfWork.Repository<AgentUser>()
            .GetSingleOrDefaultAsync(au => au.AgentId == agent.Id && au.ToDate == null, disableTracking: true);
        if (existing != null)
        {
            _logger.LogInformation("Agent-user links already exist. Skipping.");
            return;
        }

        foreach (var user in users)
            await _unitOfWork.Repository<AgentUser>().InsertAsync(new AgentUser(user.Id, agent.Id));

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} agent-user links for agent {AgentId}", users.Count, agent.Id);
    }
}
