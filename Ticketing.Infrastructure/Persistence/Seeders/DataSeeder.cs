using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ticketing.Domain.Constants;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;
using Ticketing.Infrastructure.Persistence.Configurations;
using Ticketing.Infrastructure.Persistence.Context;

namespace Ticketing.Infrastructure.Persistence.Seeders;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher<User> passwordHasher, ILogger logger)
    {
        // Seed departments and categories via HasData in EF configurations
        await context.SaveChangesAsync();

        var roles = await context.Roles.ToListAsync();
        var roleByName = roles.ToDictionary(r => r.Name, r => r);

        if (!roleByName.ContainsKey(SystemRoles.Admin))
        {
            logger.LogWarning("Required roles not found. Ensure migrations have been applied.");
            return;
        }

        var itDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Id == DepartmentConfiguration.ItDepartmentId);
        var hrDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Id == DepartmentConfiguration.HrDepartmentId);
        var financeDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Id == DepartmentConfiguration.FinanceDepartmentId);

        var existingEmails = await context.Users.Select(u => u.Email).ToListAsync();
        var existingEmailSet = existingEmails.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var admin = CreateUserIfMissing(existingEmailSet, "System", "Admin", "admin@ists.local", [roleByName[SystemRoles.Admin]], itDepartment?.Id);
        var staff = CreateUserIfMissing(existingEmailSet, "Staff", "User", "staff@ists.local", [roleByName[SystemRoles.Staff]], itDepartment?.Id);
        var agent = CreateUserIfMissing(existingEmailSet, "Support", "Agent", "agent@ists.local", [roleByName[SystemRoles.Agent]], itDepartment?.Id);
        var manager = CreateUserIfMissing(existingEmailSet, "Team", "Manager", "manager@ists.local", [roleByName[SystemRoles.Manager]], itDepartment?.Id);

        var usersToAdd = new List<User>();

        if (admin != null)
        {
            admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin@1234!");
            usersToAdd.Add(admin);
        }

        if (staff != null)
        {
            staff.PasswordHash = passwordHasher.HashPassword(staff, "Staff@1234!");
            usersToAdd.Add(staff);
        }

        if (agent != null)
        {
            agent.PasswordHash = passwordHasher.HashPassword(agent, "Agent@1234!");
            usersToAdd.Add(agent);
        }

        if (manager != null)
        {
            manager.PasswordHash = passwordHasher.HashPassword(manager, "Manager@1234!");
            usersToAdd.Add(manager);
        }

        if (usersToAdd.Count > 0)
        {
            context.Users.AddRange(usersToAdd);
            await context.SaveChangesAsync();
            logger.LogInformation(
                "Seeded {Count} default user(s): admin@ists.local, staff@ists.local, agent@ists.local, manager@ists.local",
                usersToAdd.Count);
        }

        await SeedSLAsAsync(context, itDepartment, hrDepartment, financeDepartment, logger);
    }

    private static User? CreateUserIfMissing(
        HashSet<string> existingEmails,
        string firstName,
        string lastName,
        string email,
        List<Role> roles,
        Guid? departmentId)
    {
        if (existingEmails.Contains(email))
            return null;

        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            IsActive = true,
            DepartmentId = departmentId,
            Roles = roles
        };
    }

    private static async Task SeedSLAsAsync(
        AppDbContext context,
        Department? itDepartment,
        Department? hrDepartment,
        Department? financeDepartment,
        ILogger logger)
    {
        if (itDepartment is null || hrDepartment is null || financeDepartment is null)
        {
            logger.LogWarning("Departments not found; skipping SLA seeding.");
            return;
        }

        if (await context.SLAs.AnyAsync())
            return;

        var slaEntries = new List<SLA>
        {
            new() { DepartmentId = itDepartment.Id, Priority = TicketPriority.Low, ResponseTimeMinutes = 240, ResolutionTimeMinutes = 1440 },
            new() { DepartmentId = itDepartment.Id, Priority = TicketPriority.Medium, ResponseTimeMinutes = 120, ResolutionTimeMinutes = 480 },
            new() { DepartmentId = itDepartment.Id, Priority = TicketPriority.High, ResponseTimeMinutes = 60, ResolutionTimeMinutes = 240 },
            new() { DepartmentId = itDepartment.Id, Priority = TicketPriority.Urgent, ResponseTimeMinutes = 15, ResolutionTimeMinutes = 60 },
            new() { DepartmentId = hrDepartment.Id, Priority = TicketPriority.Low, ResponseTimeMinutes = 240, ResolutionTimeMinutes = 1440 },
            new() { DepartmentId = hrDepartment.Id, Priority = TicketPriority.Medium, ResponseTimeMinutes = 120, ResolutionTimeMinutes = 480 },
            new() { DepartmentId = hrDepartment.Id, Priority = TicketPriority.High, ResponseTimeMinutes = 60, ResolutionTimeMinutes = 240 },
            new() { DepartmentId = hrDepartment.Id, Priority = TicketPriority.Urgent, ResponseTimeMinutes = 15, ResolutionTimeMinutes = 60 },
            new() { DepartmentId = financeDepartment.Id, Priority = TicketPriority.Low, ResponseTimeMinutes = 240, ResolutionTimeMinutes = 1440 },
            new() { DepartmentId = financeDepartment.Id, Priority = TicketPriority.Medium, ResponseTimeMinutes = 120, ResolutionTimeMinutes = 480 },
            new() { DepartmentId = financeDepartment.Id, Priority = TicketPriority.High, ResponseTimeMinutes = 60, ResolutionTimeMinutes = 240 },
            new() { DepartmentId = financeDepartment.Id, Priority = TicketPriority.Urgent, ResponseTimeMinutes = 15, ResolutionTimeMinutes = 60 }
        };

        context.SLAs.AddRange(slaEntries);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded default SLA entries for all departments.");
    }
}
