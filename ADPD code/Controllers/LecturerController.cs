using Microsoft.AspNetCore.Mvc;

namespace ADPD_code.Controllers
{
    public class LecturerController : Controller
    {
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
    }
}