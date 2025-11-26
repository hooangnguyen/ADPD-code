using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // <- thêm dòng này

namespace ADPD_code.Controllers
{
    public class StudentController : Controller
    {
        // Action này t??ng ?ng v?i ???ng d?n /Student/Dashboard
        public IActionResult Dashboard()
        {
            // Ki?m tra session l?i cho ch?c ch?n
            if (HttpContext.Session.GetString("Role") != "Student")
            {
                return RedirectToAction("Index", "Login");
            }

            // Tr? v? View: Views/Student/Dashboard.cshtml
            return View();
        }
    }
}