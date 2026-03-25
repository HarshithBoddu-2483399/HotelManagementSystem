using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Controllers
{
    public class HousekeepingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
