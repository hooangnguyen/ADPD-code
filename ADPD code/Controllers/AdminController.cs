using Microsoft.AspNetCore.Mvc;

namespace ADPD_code.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }
    }
}