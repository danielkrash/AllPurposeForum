using AllPurposeForum.Data;
using AllPurposeForum.Data.Models;
using AllPurposeForum.Services;
using AllPurposeForum.Services.Implementation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace AllPurposeForum.Helpers;

public static class AppConfig
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdministratorRole",
                policy => policy.RequireRole("Administrator"));
        });
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ITopicService, TopicService>();
        services.AddScoped<IPostCommentService, PostCommentService>();
        services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
//builder.Services.AddControllersWithViews(config =>
//{
//    var policy = new AuthorizationPolicyBuilder()
//                     .RequireAuthenticatedUser()
//                     .Build();
//    config.Filters.Add(new AuthorizeFilter(policy));
//});
        services.AddControllersWithViews(option => { });

        services.AddHttpContextAccessor();

        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new CustomViewLocationExpander());
        });


        services.AddRouting(options => { options.LowercaseUrls = true; });

        services.AddEndpointsApiExplorer();
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Servers.Where(s => s.Url == "https://[::]:7128").ToList().ForEach(s =>
                {
                    s.Url = "https://localhost:7128";
                });
                return Task.CompletedTask;
            });
        });
    }

    public static void ConfigureWebHost(IWebHostBuilder webHost)
    {
        webHost.ConfigureKestrel((context, options) =>
        {
            options.ListenAnyIP(7128, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                listenOptions.UseHttps();
            });
        });
    }

    public static async Task SeedDatabase(WebApplication webapp)
    {
        //Seed Roles For Users
        using var scope = webapp.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[] { "Admin", "Manager" };
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        //Seed Admin User
        using var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        const string adminEmail = "admin@mail.com";
        const string password = "123456redQ!";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                Email = adminEmail,
                UserName = adminEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, password);
            await userManager.AddToRoleAsync(adminUser, "admin");
        }

        // Seed Manager User
        const string managerEmail = "manager@mail.com";
        const string managerPassword = "123456redQ!";
        if (await userManager.FindByEmailAsync(managerEmail) == null)
        {
            var managerUser = new ApplicationUser
            {
                Email = managerEmail,
                UserName = managerEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(managerUser, managerPassword);
            await userManager.AddToRoleAsync(managerUser, "manager");
        }
    }

    private static void ConfigurePolicies(IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy =>
                policy
                    .RequireRole("Admin"))
            .AddPolicy("UserOnly", policy =>
                policy
                    .RequireRole("User"))
            .AddPolicy("ManagerUser", policy =>
                policy
                    .RequireRole("Manager", "User"));
    }
}