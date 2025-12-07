using ADPD_code.Data;
using ADPD_code.Models;
using ADPD_code.Services;
using ADPD_code.Services.Export;
using ADPD_code.Services.Notification;
using ADPD_code.Patterns;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// ========== DATABASE CONFIGURATION ==========
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlServer(connectionString);

// ========== SINGLETON PATTERNS ==========

// Đăng ký Singleton DatabaseConnectionManager
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetService<ILogger<DatabaseConnectionManager>>();
    return DatabaseConnectionManager.GetInstance(optionsBuilder.Options, logger);
});

// ⭐ Đăng ký Singleton ClassManager
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetService<ILogger<ClassManager>>();
    return ClassManager.GetInstance(logger);
});

// ========== DATABASE CONTEXT ==========

// Đăng ký ApplicationDbContext sử dụng Singleton Manager
builder.Services.AddScoped<ApplicationDbContext>(sp =>
{
    var manager = sp.GetRequiredService<DatabaseConnectionManager>();
    return manager.CreateDbContext();
});

// Đăng ký DbContextFactory cho các service cần
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ========== SESSION CONFIGURATION ==========
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ========== NOTIFICATION SERVICES (Factory Pattern) ==========
builder.Services.AddScoped<INotificationFactory, NotificationFactory>();
builder.Services.AddScoped<INotificationManager, NotificationManager>();
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<SMSNotificationService>();
builder.Services.AddScoped<InAppNotificationService>();
builder.Services.AddScoped<PushNotificationService>();

// ========== EXPORT SERVICES (Adapter Pattern) ==========
builder.Services.AddScoped<IExportService, ExcelExportAdapter>();

// ========== LECTURER SERVICES ==========
builder.Services.AddScoped<ILecturerAnalyticsService, LecturerAnalyticsService>();

var app = builder.Build();

// ========== STARTUP TESTS ==========
using (var scope = app.Services.CreateScope())
{
    var dbManager = scope.ServiceProvider.GetRequiredService<DatabaseConnectionManager>();
    var classManager = scope.ServiceProvider.GetRequiredService<ClassManager>();

    Console.WriteLine("\n" + "=".PadRight(70, '='));
    Console.WriteLine("🔍 Testing Singleton Patterns...");
    Console.WriteLine("=".PadRight(70, '='));

    // Test Database Connection
    Console.WriteLine("\n📊 Database Connection Test:");
    var testResult = await dbManager.TestConnectionAsync();

    if (!testResult)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("❌ WARNING: Cannot connect to database!");
        Console.ResetColor();
        Console.WriteLine($"Connection String: {dbManager.GetConnectionString()}");
        Console.WriteLine("\n💡 Check:");
        Console.WriteLine("   - SQL Server is running");
        Console.WriteLine("   - Server name is correct: GNCYNO\\SQLEXPRESS");
        Console.WriteLine("   - Database QLSV exists");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ Database connection successful!");
        Console.ResetColor();
        var dbStats = dbManager.GetStats();
        Console.WriteLine($"   {dbStats}");
    }

    // Test ClassManager
    Console.WriteLine("\n🏫 ClassManager Test:");
    try
    {
        var classStats = classManager.GetStats();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ ClassManager initialized successfully!");
        Console.ResetColor();
        Console.WriteLine($"   Instance Hash: {classStats.InstanceHashCode}");
        Console.WriteLine($"   Total Operations: {classStats.TotalOperations}");
        Console.WriteLine($"   Uptime: {classStats.Uptime.TotalSeconds:F1}s");
        Console.WriteLine($"   Status: {(classStats.IsActive ? "Active ✓" : "Inactive ✗")}");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ ClassManager error: {ex.Message}");
        Console.ResetColor();
    }

    Console.WriteLine("\n" + "=".PadRight(70, '=') + "\n");
}

// ========== HTTP REQUEST PIPELINE ==========
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

// ========== ROUTING ==========
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ========== API ENDPOINTS FOR MONITORING ==========

// Database Statistics Endpoint
app.MapGet("/api/db-stats", (DatabaseConnectionManager manager) =>
{
    var stats = manager.GetStats();
    var activeConnections = manager.GetActiveConnections();

    return Results.Ok(new
    {
        pattern = "Singleton Pattern",
        component = "DatabaseConnectionManager",
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

// ClassManager Statistics Endpoint
app.MapGet("/api/class-manager-stats", (ClassManager manager) =>
{
    var stats = manager.GetStats();

    return Results.Ok(new
    {
        pattern = "Singleton Pattern",
        component = "ClassManager",
        instance = new
        {
            hashCode = stats.InstanceHashCode,
            createdTime = stats.CreatedTime,
            uptime = new
            {
                minutes = stats.Uptime.TotalMinutes,
                seconds = stats.Uptime.TotalSeconds,
                formatted = $"{stats.Uptime.TotalMinutes:F1}m"
            }
        },
        operations = new
        {
            total = stats.TotalOperations,
            breakdown = "CREATE, READ, UPDATE, DELETE"
        },
        status = stats.IsActive ? "Active ✓" : "Inactive ✗"
    });
});

// Combined Singleton Stats Endpoint
app.MapGet("/api/singleton-stats", (DatabaseConnectionManager dbManager, ClassManager classManager) =>
{
    var dbStats = dbManager.GetStats();
    var classStats = classManager.GetStats();

    return Results.Ok(new
    {
        pattern = "Singleton Pattern Demo",
        timestamp = DateTime.Now,
        singletons = new
        {
            databaseConnectionManager = new
            {
                status = "Active",
                hashCode = dbManager.GetHashCode(),
                totalConnections = dbStats.TotalConnections,
                activeConnections = dbStats.ActiveConnections,
                uptime = $"{dbStats.Uptime.TotalMinutes:F1} minutes"
            },
            classManager = new
            {
                status = classStats.IsActive ? "Active" : "Inactive",
                hashCode = classStats.InstanceHashCode,
                totalOperations = classStats.TotalOperations,
                uptime = $"{classStats.Uptime.TotalMinutes:F1} minutes"
            }
        }
    });
});

// Health Check Endpoint
app.MapGet("/api/health", async (DatabaseConnectionManager dbManager, ClassManager classManager) =>
{
    var dbHealthy = await dbManager.TestConnectionAsync();
    var dbStats = dbManager.GetStats();
    var classStats = classManager.GetStats();

    var overallHealthy = dbHealthy && classStats.IsActive;

    return Results.Ok(new
    {
        status = overallHealthy ? "Healthy ✓" : "Unhealthy ✗",
        timestamp = DateTime.Now,
        components = new
        {
            database = new
            {
                healthy = dbHealthy,
                connections = dbStats.ActiveConnections,
                uptime = $"{dbStats.Uptime.TotalMinutes:F1}m"
            },
            classManager = new
            {
                healthy = classStats.IsActive,
                operations = classStats.TotalOperations,
                uptime = $"{classStats.Uptime.TotalMinutes:F1}m",
                hashCode = classStats.InstanceHashCode
            }
        }
    });
});

// Verify Singleton Endpoint
app.MapGet("/api/verify-singleton", (ClassManager manager) =>
{
    var instance1 = manager;
    var instance2 = ClassManager.Instance;
    var areSame = ReferenceEquals(instance1, instance2);

    return Results.Ok(new
    {
        test = "Singleton Verification",
        result = areSame ? "PASSED ✓" : "FAILED ✗",
        details = new
        {
            injectedInstance = instance1.GetHashCode(),
            staticInstance = instance2.GetHashCode(),
            areSameReference = areSame,
            message = areSame
                ? "Both references point to the same instance - Singleton working correctly!"
                : "Different instances detected - Singleton NOT working!"
        }
    });
});

Console.WriteLine("\n🚀 Application started successfully!");
Console.WriteLine("📊 Monitoring endpoints:");
Console.WriteLine("   - GET /api/db-stats              (Database stats)");
Console.WriteLine("   - GET /api/class-manager-stats   (ClassManager stats)");
Console.WriteLine("   - GET /api/singleton-stats       (Combined stats)");
Console.WriteLine("   - GET /api/health                (Health check)");
Console.WriteLine("   - GET /api/verify-singleton      (Verify singleton)");
Console.WriteLine();

app.Run();