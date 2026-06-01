using DotNetEnv;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Hooome.Identity.Data;
using Hooome.Identity.Models;
using Hooome.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers(); 

var configuration = builder.Configuration;

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Auth"));
});

var configConnectionString = builder.Configuration.GetConnectionString("Auth");

builder.Services.AddDbContext<ConfigurationDbContext>(options =>
{
    options.UseNpgsql(configConnectionString, b => b.MigrationsAssembly("Hooome.Identity"));
});

builder.Services.AddDbContext<PersistedGrantDbContext>(options =>
{
    options.UseNpgsql(configConnectionString, b => b.MigrationsAssembly("Hooome.Identity"));
});

builder.Services.AddIdentity<AppUser, IdentityRole>(config =>
{
    config.Password.RequiredLength = 8;
    config.Password.RequireDigit = true;
    config.Password.RequireNonAlphanumeric = true;
    config.Password.RequireUppercase = true;
})
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddIdentityServer(options =>
{
    options.IssuerUri = "https://identity-production-69c1.up.railway.app";
})
    .AddAspNetIdentity<AppUser>()
    .AddProfileService<ProfileService>()
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseNpgsql(configConnectionString, b => b.MigrationsAssembly("Hooome.Identity"));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b =>
             b.UseNpgsql(configConnectionString, b => b.MigrationsAssembly("Hooome.Identity"));

        options.EnableTokenCleanup = true;
        options.TokenCleanupInterval = 3600;
        options.RemoveConsumedTokens = true;
    })
    .AddDeveloperSigningCredential();


builder.Services.AddCors(options => 
{
    options.AddPolicy("AllowReactApp", 
        policy => 
        policy.WithOrigins("https://hooome.web.app","http://localhost:3000", "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); 
});

builder.Services.AddScoped<IPassGenService, PassGenService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
await DbContextInitializer.Initialize(context, userManager, roleManager);

await IdentityDbInitializer.Initialize(scope.ServiceProvider);

app.UseRouting();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseIdentityServer();
app.MapControllers();

app.Run();

