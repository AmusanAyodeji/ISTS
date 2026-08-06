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
        await SeedTicketsAsync(context, logger);
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

    private static async Task SeedTicketsAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Tickets.AnyAsync())
            return;

        var staff = await context.Users.FirstOrDefaultAsync(u => u.Email == "staff@ists.local");
        var agent = await context.Users.FirstOrDefaultAsync(u => u.Email == "agent@ists.local");

        if (staff is null || agent is null)
        {
            logger.LogWarning("Staff or agent user not found; skipping ticket seeding.");
            return;
        }

        var now = DateTime.UtcNow;

        var ticketData = new[]
        {
            ("Laptop not powering on after update", "My laptop shut down during the update and now it will not turn on.", TicketPriority.High, TicketStatus.InProgress, true, now.AddHours(-3)),
            ("Cannot connect to VPN", "I am unable to connect to the corporate VPN from my home office.", TicketPriority.Medium, TicketStatus.Open, false, now.AddHours(-5)),
            ("Printer issue in HR", "The HR department printer is showing a paper jam error with no paper jammed.", TicketPriority.Low, TicketStatus.Resolved, true, now.AddDays(-2)),
            ("Password reset request", "I forgot my password and need it reset urgently to finish a report.", TicketPriority.Urgent, TicketStatus.Closed, true, now.AddHours(-1)),
            ("Blue screen showing", "My computer shows a blue screen every time I open Excel.", TicketPriority.High, TicketStatus.Open, false, now.AddHours(-6)),
            ("Email not syncing", "Outlook on my phone is not syncing with the server.", TicketPriority.Medium, TicketStatus.InProgress, true, now.AddHours(-4)),
            ("Payroll discrepancy", "My payslip shows missing overtime hours for last month.", TicketPriority.High, TicketStatus.Open, false, now.AddHours(-2)),
            ("Invoice approval stuck", "A client invoice has been pending approval for three days.", TicketPriority.Medium, TicketStatus.Resolved, false, now.AddDays(-1)),
            ("Screen flickering", "My monitor flickers continuously and causes eye strain.", TicketPriority.Low, TicketStatus.Open, true, now.AddHours(-8)),
            ("New hire laptop request", "Please prepare a laptop for the new developer starting next week.", TicketPriority.Medium, TicketStatus.InProgress, false, now.AddHours(-10)),
            ("File server access denied", "I cannot access the shared file server this morning.", TicketPriority.Urgent, TicketStatus.Open, true, now.AddMinutes(-30)),
            ("Expense reimbursement", "My expense claim from last month has not been reimbursed yet.", TicketPriority.Low, TicketStatus.Closed, false, now.AddDays(-5)),
            ("Software license expired", "My design tool license expired and I cannot renew it.", TicketPriority.High, TicketStatus.InProgress, true, now.AddHours(-7)),
            ("Meeting room projector", "Projector in conference room B is not displaying HDMI input.", TicketPriority.Medium, TicketStatus.Resolved, true, now.AddDays(-3)),
            ("Security alert response", "Respond to the phishing alert reported by three staff members.", TicketPriority.Urgent, TicketStatus.InProgress, true, now.AddMinutes(-45))
        };

        var categories = await context.Categories.ToListAsync();
        var random = new Random(42);

        var tickets = new List<Ticket>();

        foreach (var (title, description, priority, status, assigned, createdAt) in ticketData)
        {
            var category = categories[random.Next(categories.Count)];
            var slaMinutes = GetDefaultSlaMinutes(priority);
            var slaDueAt = createdAt.AddMinutes(slaMinutes);

            // Resolved tickets: 50/50 within or over SLA for variety
            DateTime? resolvedAt = status is TicketStatus.Resolved or TicketStatus.Closed
                ? createdAt.AddMinutes(random.Next(2) == 0 ? slaMinutes - 10 : slaMinutes + 20)
                : null;

            tickets.Add(new Ticket
            {
                Title = title,
                Description = description,
                Priority = priority,
                Status = status,
                DepartmentId = category.DepartmentId,
                CategoryId = category.Id,
                CreatedById = staff.Id,
                CreatedAt = createdAt,
                AssignedToId = assigned ? agent.Id : null,
                SlaDueAt = slaDueAt,
                ResolvedAt = resolvedAt
            });
        }

        context.Tickets.AddRange(tickets);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {TicketCount} sample tickets.", tickets.Count);
    }

    private static int GetDefaultSlaMinutes(TicketPriority priority)
    {
        return priority switch
        {
            TicketPriority.Low => 24 * 60,
            TicketPriority.Medium => 8 * 60,
            TicketPriority.High => 4 * 60,
            TicketPriority.Urgent => 60,
            _ => 8 * 60
        };
    }
}
