using Baitaptuan5.Models;
using Baitaptuan5.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Add ASP.NET Core Identity with ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Simple password options to make testing easy
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Register Repositories
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();

// 4. Add Google External Authentication (Optional / Kept for compatibility)
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        var clientId = builder.Configuration["Authentication:Google:ClientId"];
        var clientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

        options.ClientId = (string.IsNullOrEmpty(clientId) || clientId == "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com") 
            ? "placeholder-client-id.apps.googleusercontent.com" 
            : clientId;

        options.ClientSecret = (string.IsNullOrEmpty(clientSecret) || clientSecret == "YOUR_GOOGLE_CLIENT_SECRET") 
            ? "placeholder-client-secret" 
            : clientSecret;

        options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
        {
            OnRedirectToAuthorizationEndpoint = context =>
            {
                context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
                return System.Threading.Tasks.Task.CompletedTask;
            },
            OnRemoteFailure = context =>
            {
                context.Response.Redirect("/Account/Login?remoteError=" + System.Net.WebUtility.UrlEncode(context.Failure?.Message ?? "Lỗi đăng nhập bằng Google. Vui lòng cấu hình ClientSecret hợp lệ trong appsettings.json."));
                context.HandleResponse();
                return System.Threading.Tasks.Task.CompletedTask;
            }
        };
    });

// 5. Configure Application Cookie redirection paths (Lab 4, Page 79)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Register Session state & HttpContextAccessor services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// Add MVC controllers with views & Razor Pages for Identity
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// 6. Seed Roles, Admin and Default Data on startup (adapted for ApplicationUser if needed)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Có lỗi xảy ra trong quá trình seed dữ liệu.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Đặt trước UseRouting
app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Area routing mapping (Lab 4, Page 78)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

