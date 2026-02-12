using Duende.IdentityServer.EntityFramework.DbContexts;
using Hooome.Identity.Data;
using Hooome.Identity.Models;
using Hooome.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var configuration = builder.Configuration;

var connectionString = configuration["DbConnections:auth"];
var serverVersion = new MySqlServerVersion(new Version(configuration["DatabaseSettings:ServerVersion"]
    ?? throw new NullReferenceException("Server version is null")));
var migrationsAssembly = typeof(Program).Assembly.GetName().Name;

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseMySql(connectionString, serverVersion, 
        sql => sql.MigrationsAssembly(migrationsAssembly));
});


var configConnectionString = configuration["DbConnections:auth"];

builder.Services.AddDbContext<ConfigurationDbContext>(options =>
{
    options.UseMySql(configConnectionString, serverVersion,
        sql => sql.MigrationsAssembly(migrationsAssembly));
});

builder.Services.AddDbContext<PersistedGrantDbContext>(options =>
{
    options.UseMySql(configConnectionString, serverVersion,
        sql => sql.MigrationsAssembly(migrationsAssembly));
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
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseMySql(configConnectionString, serverVersion,
                sql => sql.MigrationsAssembly(migrationsAssembly));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b => 
            b.UseMySql(configConnectionString, serverVersion, 
                sql => sql.MigrationsAssembly(migrationsAssembly));

        options.EnableTokenCleanup = true;
        options.TokenCleanupInterval = 3600;
        options.RemoveConsumedTokens = true;
    })
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

await IdentityDbInitializer.Initialize(scope.ServiceProvider);

app.UseRouting();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseIdentityServer();
app.MapControllers();

app.Run();

