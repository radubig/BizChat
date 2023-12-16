using ArticlesApp.Models;
using BizChat.Data;
using BizChat.Hubs;
using BizChat.MiddlewareExtensions;
using BizChat.Models;
using BizChat.SubscribeTableDependencies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Chestie pentru real-time messaging
builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors = true;
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString), ServiceLifetime.Singleton);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    ;

builder.Services.AddControllersWithViews()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddSingleton<SignalRHub>();
builder.Services.AddSingleton<SubscribeMessageTableDependency>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Servers}/{action=Index}/{serverId?}/{channelId?}");
app.MapHub<SignalRHub>("/SignalRHub");
app.UseSqlTableDependency<SubscribeMessageTableDependency>(connectionString);

app.MapRazorPages();

app.Run();
