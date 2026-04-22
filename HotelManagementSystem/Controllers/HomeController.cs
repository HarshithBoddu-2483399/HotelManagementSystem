using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Controllers { 
    public class HomeController : Controller
    {
        [Route("Home/Error404")]
        public IActionResult Error404()
        {
            return View("NotFound");
        }
    }
}