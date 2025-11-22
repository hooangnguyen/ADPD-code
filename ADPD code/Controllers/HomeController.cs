using ADPD_code.Data;
using ADPD_code.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ADPD_code.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private bool isConnected = false;
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            this.isConnected = _context.Database.CanConnect();
            ViewBag.IsConnected = isConnected;
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
