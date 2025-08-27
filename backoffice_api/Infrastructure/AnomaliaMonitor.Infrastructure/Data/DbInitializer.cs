using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AnomaliaMonitor.Domain.Entities;

namespace AnomaliaMonitor.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context, UserManager<User> userManager)
    {
        await context.Database.MigrateAsync();

        if (!await context.Users.AnyAsync())
        {
            var user = new User
            {
                UserName = "teste@teste.com",
                Email = "teste@teste.com",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(user, "Teste123");
        }

        if (!await context.SiteCategories.AnyAsync())
        {
            var categories = new[]
            {
                new SiteCategory
                {
                    Name = "Vídeos Pornográficos",
                    Description = "Plataformas e portais com conteúdo audiovisual de natureza sexual explícita",
                    Color = "#10B981",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteCategory
                {
                    Name = "Contos Eróticos",
                    Description = "Sites e blogs com histórias e textos de conteúdo sexual explícito",
                    Color = "#8B5CF6",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.SiteCategories.AddRangeAsync(categories);
        }

        if (!await context.SubjectsToResearch.AnyAsync())
        {
            var subjects = new[]
            {
                new SubjectToResearch
                {
                    Name = "Pedofilia",
                    Description = "Casos e investigações relacionados a abuso e exploração sexual de menores",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new SubjectToResearch
                {
                    Name = "Estupro",
                    Description = "Relatos e análises de crimes de violência sexual contra indivíduos",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new SubjectToResearch
                {
                    Name = "Racismo",
                    Description = "Incidentes e estudos sobre discriminação racial e preconceito étnico",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }

            };

            await context.SubjectsToResearch.AddRangeAsync(subjects);
        }

        await context.SaveChangesAsync();
    }
}