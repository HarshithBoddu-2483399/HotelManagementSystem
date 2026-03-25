using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Controllers
{
    public class ReportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
