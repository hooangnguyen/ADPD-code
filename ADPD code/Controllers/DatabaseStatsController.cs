using ADPD_code.Patterns;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ADPD_code.Controllers
{
    /// <summary>
    /// Controller demo Singleton Pattern - DatabaseConnectionManager
    /// </summary>
    public class DatabaseStatsController : Controller
    {
        private readonly DatabaseConnectionManager _dbManager;

        public DatabaseStatsController(DatabaseConnectionManager dbManager)
        {
            _dbManager = dbManager;
        }

        /// <summary>
        /// Trang chính hiển thị thống kê
        /// GET: /DatabaseStats
        /// </summary>
        public IActionResult Index()
        {
            var stats = _dbManager.GetStats();
            var activeConnections = _dbManager.GetActiveConnections();

            ViewBag.Stats = stats;
            ViewBag.ActiveConnections = activeConnections;
            ViewBag.ConnectionString = MaskConnectionString(_dbManager.GetConnectionString());

            return View();
        }

        /// <summary>
        /// Test connection
        /// GET: /DatabaseStats/TestConnection
        /// </summary>
        public async Task<IActionResult> TestConnection()
        {
            var result = await _dbManager.TestConnectionAsync();
            var stats = _dbManager.GetStats();

            return Json(new
            {
                success = result,
                message = result ? "✅ Connection successful!" : "❌ Connection failed!",
                stats = new
                {
                    stats.TotalConnections,
                    stats.ActiveConnections,
                    stats.LastConnectionTime,
                    UptimeMinutes = stats.Uptime.TotalMinutes,
                    stats.ConnectionPoolSize
                }
            });
        }

        /// <summary>
        /// Lấy thống kê JSON
        /// GET: /DatabaseStats/GetStats
        /// </summary>
        public IActionResult GetStats()
        {
            var stats = _dbManager.GetStats();
            var activeConnections = _dbManager.GetActiveConnections();

            return Json(new
            {
                stats = new
                {
                    stats.TotalConnections,
                    stats.ActiveConnections,
                    stats.LastConnectionTime,
                    stats.ManagerCreatedTime,
                    UptimeSeconds = stats.Uptime.TotalSeconds,
                    stats.IsActive,
                    stats.ConnectionPoolSize
                },
                activeConnections = activeConnections.Select(c => new
                {
                    id = c.Id.ToString(),
                    createdAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    ageSeconds = c.Age.TotalSeconds
                })
            });
        }

        /// <summary>
        /// Demo tạo nhiều DbContext
        /// GET: /DatabaseStats/CreateMultipleContexts?count=5
        /// </summary>
        public async Task<IActionResult> CreateMultipleContexts(int count = 5)
        {
            if (count < 1 || count > 20)
            {
                return BadRequest("Count must be between 1 and 20");
            }

            var tasks = new List<Task>();
            var results = new List<string>();

            for (int i = 0; i < count; i++)
            {
                var index = i + 1;
                tasks.Add(Task.Run(async () =>
                {
                    using var context = _dbManager.CreateDbContext();
                    var canConnect = await context.Database.CanConnectAsync();

                    var studentCount = await context.Students.CountAsync();

                    results.Add($"Context #{index}: Connected={canConnect}, Students={studentCount}");

                    // Simulate work
                    await Task.Delay(1000);
                }));
            }

            await Task.WhenAll(tasks);

            var stats = _dbManager.GetStats();

            return Json(new
            {
                message = $"Created {count} DbContext instances",
                results,
                stats = new
                {
                    stats.TotalConnections,
                    stats.ActiveConnections,
                    stats.ConnectionPoolSize
                }
            });
        }

        /// <summary>
        /// Demo singleton instance
        /// GET: /DatabaseStats/VerifySingleton
        /// </summary>
        public IActionResult VerifySingleton()
        {
            var instance1 = _dbManager;
            var instance2 = DatabaseConnectionManager.Instance;
            var areSame = ReferenceEquals(instance1, instance2);

            return Json(new
            {
                areSameInstance = areSame,
                message = areSame
                    ? "✅ Both references point to the same Singleton instance!"
                    : "❌ Different instances detected (should not happen)",
                instance1HashCode = instance1.GetHashCode(),
                instance2HashCode = instance2.GetHashCode()
            });
        }

        /// <summary>
        /// Mask connection string để bảo mật
        /// </summary>
        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return "N/A";

            // Mask password
            var parts = connectionString.Split(';');
            var masked = parts.Select(part =>
            {
                if (part.ToLower().Contains("password"))
                {
                    return "Password=****";
                }
                return part;
            });

            return string.Join("; ", masked);
        }
    }
}