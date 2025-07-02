using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using pupslp_tickets.Models;

namespace pupslp_tickets.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly DbModel _context;

        public DashboardController(ILogger<DashboardController> logger, DbModel context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Overview(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            _logger.LogInformation($"UserId in session: {userId}");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // TO SHOW INFO ON THE OVERVIEW
            var eventDetails = _context.eventDetailsModel.FirstOrDefault(e => e.Id == id);

            if (eventDetails == null)
            {
                return NotFound();
            }

            ViewBag.latestEventId = eventDetails.Id;
            return View(eventDetails);
        }
    }
}
