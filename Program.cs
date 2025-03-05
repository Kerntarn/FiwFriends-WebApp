using Microsoft.EntityFrameworkCore;
using FiwFriends.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using FiwFriends.Models;
using FiwFriends.Services;
using DotNetEnv;    

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var apiKey = Environment.GetEnvironmentVariable("API_KEY") ?? throw new Exception("Where's ur api key?????");

builder.Services.AddSingleton(apiKey);

// Enable console logging for debugging
builder.Logging.AddConsole();

// Database connection
builder.Services.AddDbContext<ApplicationDBContext>(options =>
      options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Identity services with additional password policies
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Lockout.AllowedForNewUsers = true;
    options.SignIn.RequireConfirmedEmail = false; // Change to true if email confirmation is needed
})
.AddEntityFrameworkStores<ApplicationDBContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<MapperService>();
// Add memory cache
builder.Services.AddMemoryCache();

// Add session with proper configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1440); // Set session timeout to 1440 minutes
    options.Cookie.HttpOnly = true; // Prevents JavaScript access for security
    options.Cookie.IsEssential = true; // Makes the cookie essential
});

// Configure cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Set timeout to 30 minutes
        options.SlidingExpiration = true; // Reset expiration on activity
        options.LoginPath = "/Auth/Login"; // Redirect to login if not authenticated
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("https://yourfrontendurl.com")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials(); // Allows cookies for authentication
    });
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";  // Redirect to login page
    options.AccessDeniedPath = "/Auth/Login";  // Redirect if no permission
});

// Add controllers with views
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });

// Add HTTP context accessor for services like CurrentUserService
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.UseCors();

app.UseSession(); 
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Post}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
