using Hooome.Identity;
using Hooome.Identity.Data;
using Hooome.Identity.Models;
using Hooome.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = "https://localhost:5001"; 
    options.Audience = "HooomeWebApi";            
    options.RequireHttpsMetadata = false;
});


builder.Services.AddIdentityServer()
    .AddAspNetIdentity<AppUser>()
    .AddProfileService<ProfileService>()
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
