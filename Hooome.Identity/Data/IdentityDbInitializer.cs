using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Hooome.Identity.Data;

public class IdentityDbInitializer
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        // Миграции для ConfigurationDbContext
        var configContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        await configContext.Database.MigrateAsync();

        // Миграции для PersistedGrantDbContext
        var grantContext = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
        await grantContext.Database.MigrateAsync();

        // Заполняем начальными данными из Config.cs (если таблицы пусты)
        if (!configContext.Clients.Any())
        {
            foreach (var client in Config.Clients)
            {
                await configContext.Clients.AddAsync(client.ToEntity());
            }
            await configContext.SaveChangesAsync();
        }

        if (!configContext.IdentityResources.Any())
        {
            foreach (var resource in Config.IdentityResources)
            {
                await configContext.IdentityResources.AddAsync(resource.ToEntity());
            }
            await configContext.SaveChangesAsync();
        }

        if (!configContext.ApiScopes.Any())
        {
            foreach (var apiScope in Config.ApiScopes)
            {
                await configContext.ApiScopes.AddAsync(apiScope.ToEntity());
            }
            await configContext.SaveChangesAsync();
        }

        if (!configContext.ApiResources.Any())
        {
            foreach (var resource in Config.ApiResources)
            {
                await configContext.ApiResources.AddAsync(resource.ToEntity());
            }
            await configContext.SaveChangesAsync();
        }
    }
}
