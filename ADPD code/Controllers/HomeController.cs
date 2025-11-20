using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ADPD_code.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;

        public HomeController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            string connectionString = _config.GetConnectionString("DefaultConnection");
            string message;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();  // Nếu mở được → kết nối thành công
                    message = "Kết nối CSDL thành công!";
                }
            }
            catch (Exception ex)
            {
                message = "Kết nối CSDL thất bại: " + ex.Message;
            }

            ViewBag.DbMessage = message;
            return View();
        }
    }
}
