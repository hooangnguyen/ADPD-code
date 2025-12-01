using ADPD_code.Data;
using ADPD_code.Models;
using ADPD_code.Services;
using ADPD_code.Services.Export;
using ADPD_code.Services.Notification;
using Microsoft.EntityFrameworkCore;

// ==========================================================

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();

// Register ApplicationDbContext and its factory.
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add distributed memory cache required by session and configure session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// ========== NOTIFICATION SERVICES REGISTRATION ==========
builder.Services.AddScoped<INotificationFactory, NotificationFactory>();
builder.Services.AddScoped<INotificationManager, NotificationManager>();

// Register notification implementations (optional)
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<SMSNotificationService>();
builder.Services.AddScoped<InAppNotificationService>();
builder.Services.AddScoped<PushNotificationService>();

// ========== EXPORT SERVICES REGISTRATION ==========
// Đăng ký dịch vụ Export của bạn
builder.Services.AddScoped<IExportService, ExcelExportAdapter>();


// ========== LECTURER SERVICES REGISTRATION ==========
builder.Services.AddScoped<ILecturerAnalyticsService, LecturerAnalyticsService>();

var app = builder.Build();

// Configure the HTTP request pipeline. 
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session after routing but before endpoints
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
  name: "default",
  pattern: "{controller=Home}/{action=Index}/{id?}");

// TEMP: minimal test endpoint... (giữ nguyên)
app.MapGet("/test-notification", async (
  int recipientId,
  string title,
  string message,
  string type,
  string? email,
  INotificationManager notificationManager) =>
{
    if (!Enum.TryParse<NotificationType>(type, true, out var ntype))
        return Results.BadRequest($"Invalid notification type '{type}'. Valid: Email,SMS,InApp,Push");

    var success = await notificationManager.SendNotificationAsync(
      recipientId,
      title,
      message,
      ntype,
      email: email,
      phone: null,
      priority: "Medium");

    return success ? Results.Ok("Notification queued/sent") : Results.Problem("Failed to send notification");
});

app.Run();