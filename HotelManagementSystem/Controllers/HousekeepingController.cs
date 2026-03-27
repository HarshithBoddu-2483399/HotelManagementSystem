using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.Services;

namespace HotelManagementSystem.Controllers
{
    public class HousekeepingController : Controller
    {
        private readonly IHousekeepingService _service;
        public HousekeepingController(IHousekeepingService service) { _service = service; }

        public IActionResult Index() => View(_service.GetPendingTasks());

        [HttpPost]
        public IActionResult MarkClean(int taskId)
        {
            _service.MarkClean(taskId);
            return RedirectToAction("Index");
        }
    }
}