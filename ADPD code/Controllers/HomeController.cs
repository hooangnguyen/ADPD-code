using ADPD_code.Data;
using ADPD_code.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ADPD_code.Controllers
{
    public class HomeController(ApplicationDbContext context) : Controller
    {
        private bool isConnected = false;
        private readonly ApplicationDbContext _context = context;

        public IActionResult Index()
        {
            try
            {
                this.isConnected = _context.Database.CanConnect();
                ViewBag.IsConnected = isConnected;
                ViewBag.ConnectionError = null;
            }
            catch (Exception ex)
            {
                this.isConnected = false;
                ViewBag.IsConnected = false;
                ViewBag.ConnectionError = ex.Message;
                // Log the error for debugging
                System.Diagnostics.Debug.WriteLine($"Database Connection Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
