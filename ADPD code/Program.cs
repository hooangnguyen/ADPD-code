using ADPD_code.Data;
using ADPD_code.Models;
using ADPD_code.Services;
using ADPD_code.Services.Export;
using ADPD_code.Services.Notification;
using ADPD_code.Patterns; // ⭐ THÊM NAMESPACE MỚI
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// ========== SINGLETON DATABASE CONNECTION MANAGER ⭐ ==========
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlServer(connectionString);

// Đăng ký Singleton DatabaseConnectionManager
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetService<ILogger<DatabaseConnectionManager>>();
    return DatabaseConnectionManager.GetInstance(optionsBuilder.Options, logger);
});

// Đăng ký ApplicationDbContext sử dụng Singleton Manager
builder.Services.AddScoped<ApplicationDbContext>(sp =>
{
    var manager = sp.GetRequiredService<DatabaseConnectionManager>();
    return manager.CreateDbContext();
});

// Giữ nguyên DbContextFactory cho các service cần
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add distributed memory cache và session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Notification Services
builder.Services.AddScoped<INotificationFactory, NotificationFactory>();
builder.Services.AddScoped<INotificationManager, NotificationManager>();
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<SMSNotificationService>();
builder.Services.AddScoped<InAppNotificationService>();
builder.Services.AddScoped<PushNotificationService>();

// Export Services
builder.Services.AddScoped<IExportService, ExcelExportAdapter>();

// Lecturer Services
builder.Services.AddScoped<ILecturerAnalyticsService, LecturerAnalyticsService>();

var app = builder.Build();

// ========== TEST DATABASE CONNECTION AT STARTUP ⭐ ==========
using (var scope = app.Services.CreateScope())
{
    var manager = scope.ServiceProvider.GetRequiredService<DatabaseConnectionManager>();

    Console.WriteLine("=".PadRight(60, '='));
    Console.WriteLine("🔍 Testing Database Connection...");
    Console.WriteLine("=".PadRight(60, '='));

    var testResult = await manager.TestConnectionAsync();

    if (!testResult)
    {
        Console.WriteLine("❌ WARNING: Cannot connect to database!");
        Console.WriteLine($"Connection String: {manager.GetConnectionString()}");
    }
    else
    {
        Console.WriteLine("✅ Database connection successful!");
        var stats = manager.GetStats();
        Console.WriteLine($"📊 Stats: {stats}");
    }

    Console.WriteLine("=".PadRight(60, '='));
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ========== ENDPOINT ĐỂ XEM THỐNG KÊ CONNECTION ⭐ ==========
app.MapGet("/api/db-stats", (DatabaseConnectionManager manager) =>
{
    var stats = manager.GetStats();
    var activeConnections = manager.GetActiveConnections();

    return Results.Ok(new
    {
        stats = new
        {
            stats.TotalConnections,
            stats.ActiveConnections,
            stats.LastConnectionTime,
            stats.ManagerCreatedTime,
            UptimeMinutes = stats.Uptime.TotalMinutes,
            stats.IsActive,
            stats.ConnectionPoolSize
        },
        activeConnections = activeConnections.Select(c => new
        {
            c.Id,
            c.CreatedAt,
            AgeSeconds = c.Age.TotalSeconds
        })
    });
});

app.Run();