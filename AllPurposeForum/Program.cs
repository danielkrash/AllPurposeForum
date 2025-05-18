using AllPurposeForum.Data;
using AllPurposeForum.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using AllPurposeForum.Services;
using AllPurposeForum.Services.Implementation;
using Microsoft.AspNetCore.Mvc.Razor;
using AllPurposeForum.Helpers; // Add this line

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole",
        policy => policy.RequireRole("Administrator"));
});
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<IPostCommentService, PostCommentService>();
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
//builder.Services.AddControllersWithViews(config =>
//{
//    var policy = new AuthorizationPolicyBuilder()
//                     .RequireAuthenticatedUser()
//                     .Build();
//    config.Filters.Add(new AuthorizeFilter(policy));
//});
builder.Services.AddControllersWithViews(option => { });

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new CustomViewLocationExpander());
});

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.ListenAnyIP(7128, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
        listenOptions.UseHttps();
    });
});

builder.Services.AddRouting(options => { options.LowercaseUrls = true; });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.MapOpenApi();
    app.MapScalarApiReference(options => { options.BaseServerUrl = "https://localhost:7128"; });
    //app.UseSwaggerUI(options =>
    //{
    //    options.SwaggerEndpoint("/openapi/v1.json", "v1");
    //    options.
    //});
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=home}/{action=index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();

public partial class Program
{
}