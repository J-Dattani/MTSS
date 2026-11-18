
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MTSS.Data;
using MTSS.Models;
using MTSS.Services;

var builder = WebApplication.CreateBuilder(args);

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MTSSConnection")));

// ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("MtssFrontend", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7113",
                "http://localhost:5124"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Cookie Configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// MVC and API Controllers
builder.Services.AddControllersWithViews();

// Register Application Services
builder.Services.AddScoped<SubscriptionPlanService>();
builder.Services.AddScoped<SocietyService>();
builder.Services.AddScoped<SocietySubscriptionService>();
builder.Services.AddScoped<SocietyAdminService>();
builder.Services.AddScoped<WingService>();
builder.Services.AddScoped<FloorService>();
builder.Services.AddScoped<FlatService>();
builder.Services.AddScoped<ResidentService>();
builder.Services.AddScoped<AmenityService>();
builder.Services.AddScoped<GuestEntryService>();
builder.Services.AddScoped<ParcelService>();
builder.Services.AddScoped<ComplaintService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<AmenityAccessPinService>();
builder.Services.AddScoped<ReportService>();

var app = builder.Build();

// HTTP Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// CORS must execute before Authentication and Authorization
app.UseCors("MtssFrontend");

app.UseAuthentication();

app.UseAuthorization();

// API Attribute Routing
app.MapControllers();

// Default MVC Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed Identity Roles
using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedRolesAsync(
        scope.ServiceProvider,
        builder.Configuration);
}

app.Run();
