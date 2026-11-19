
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

// API Controllers
builder.Services.AddControllers();

// Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// CORS must execute before Authentication and Authorization
app.UseCors("MtssFrontend");

app.UseAuthentication();

app.UseAuthorization();

// Environment-specific pages
if (app.Environment.IsDevelopment())
{
    // Swagger - Development Only
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Production Welcome Page
    app.MapGet("/", () =>
        Results.Content(
            """
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>MTSS Backend API</title>
                <style>
                    * {
                        box-sizing: border-box;
                    }

                    body {
                        margin: 0;
                        min-height: 100vh;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        font-family: Arial, sans-serif;
                        background: #f4f6f9;
                        color: #1e293b;
                    }

                    .container {
                        max-width: 520px;
                        width: 90%;
                        padding: 45px 30px;
                        text-align: center;
                        background: white;
                        border-radius: 16px;
                        box-shadow: 0 8px 30px rgba(0,0,0,0.08);
                    }

                    h1 {
                        margin-bottom: 12px;
                        color: #2563eb;
                        font-size: 32px;
                    }

                    p {
                        color: #64748b;
                        line-height: 1.6;
                    }

                    .status {
                        display: inline-block;
                        margin-top: 18px;
                        padding: 10px 18px;
                        border-radius: 30px;
                        background: #dcfce7;
                        color: #15803d;
                        font-size: 14px;
                        font-weight: bold;
                    }

                    footer {
                        margin-top: 35px;
                        color: #94a3b8;
                        font-size: 12px;
                    }
                </style>
            </head>
            <body>
                <div class="container">
                    <h1>MTSS Backend API</h1>
                    <p>Multi-Tenant Society Management System</p>
                    <p>Backend service is running successfully.</p>
                    <div class="status">
                        Service Operational
                    </div>
                    <footer>
                        MTSS Backend Service
                    </footer>
                </div>
            </body>
            </html>
            """,
            "text/html"));
}

// API Attribute Routing
app.MapControllers();

// Seed Identity Roles
using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedRolesAsync(
        scope.ServiceProvider,
        builder.Configuration);
}

app.Run();
