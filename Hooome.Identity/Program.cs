using Hooome.Identity;
using Hooome.Identity.Data;
using Hooome.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var configuration = builder.Configuration;

var connectionString = configuration["DbConnection"];
var serverVersion = new MySqlServerVersion(new Version(configuration["DatabaseSettings:ServerVersion"]
    ?? throw new NullReferenceException("Server version is null")));

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseMySql(connectionString, serverVersion);
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

builder.Services.ConfigureApplicationCookie(config => {
    config.Cookie.Name = "Hooome.Identity.Cookie";
    config.Cookie.HttpOnly = true;
    config.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
    config.Cookie.SameSite = SameSiteMode.None; 
    config.LoginPath = "/auth/login";
    config.LogoutPath = "/auth/logout";
});

builder.Services.AddIdentityServer()
    .AddAspNetIdentity<AppUser>()
    .AddInMemoryClients(Config.Clients)
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddInMemoryApiResources(Config.ApiResources)
    .AddDeveloperSigningCredential();


builder.Services.AddCors(options => 
{
    options.AddPolicy("AllowReactApp", 
        policy => 
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); 
}); 

var app = builder.Build();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
await DbContextInitializer.Initialize(context, roleManager);

app.UseRouting();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseIdentityServer();
app.MapControllers();

app.Run();
