using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pupslp_tickets.Models;

namespace pupslp_tickets.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ILogger<RegFormController> _logger;
        private readonly DbModel _context;

        public AttendanceController(ILogger<RegFormController> logger, DbModel context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult AudienceSchedules()
        {
            var tickets = _context.regTicketModels
                .Include(t => t.RegistrationFormModel)
                    .ThenInclude(r => r.RegUserModel)
                .Include(t => t.RegistrationFormModel)
                    .ThenInclude(r => r.RegPaymentModel)
                .Include(t => t.RegistrationFormModel)
                    .ThenInclude(r => r.EventDetailsModel)
                .OrderBy(t => t.Id)
                .ToList();

            // Total Attended Tickets
            int totalAttended = _context.regTicketModels
                .Where(t => t.IsAttended && !string.IsNullOrEmpty(t.ticketNumber))
                .Count();

            ViewBag.TotalAttended = totalAttended;


            // Total Tickets 
            var totalTicketsGenerated = _context.regTicketModels
                                            .Where(t => !string.IsNullOrEmpty(t.ticketNumber))
                                            .Sum(t => t.regTicketQuantity);

            ViewBag.TotalTicketsGenerated = totalTicketsGenerated;


            // Total Tickets per Type :"Regular", "Invited", "Sponsor", "Walk-in"

            var predefinedTicketTypes = new List<string> { "Regular", "Invited", "Sponsor", "Walk-In" };

            // Get counts from the database for existing tickets
            var actualTicketTypeCounts = _context.regTicketModels
                .Where(t => !string.IsNullOrEmpty(t.ticketNumber))
                .GroupBy(t => t.ticketType)
                .Select(g => new
                {
                    TicketType = g.Key,
                    TotalTickets = g.Sum(t => t.regTicketQuantity)
                })
                .ToDictionary(x => x.TicketType, x => x.TotalTickets); 

           
            var ticketTypeSummaryForView = new List<object>(); 

            foreach (var type in predefinedTicketTypes)
            {
                int count = 0;
                if (actualTicketTypeCounts.TryGetValue(type, out int actualCount))
                {
                    count = actualCount;
                }
                ticketTypeSummaryForView.Add(new { TicketType = type, TotalTickets = count });
            }

            ViewBag.TicketTypeSummary = ticketTypeSummaryForView;

            // Calculate Total Seats Remaining
            var eventDetails = _context.eventDetailsModel
                .OrderByDescending(e => e.Id)
                .FirstOrDefault();
            int currentEventCapacity = 0; // Default to 0


            if (eventDetails != null)
            {
                currentEventCapacity = eventDetails.eventCapacity; 
            }


            var totalRemainingSeats = currentEventCapacity - totalTicketsGenerated;
            ViewBag.TotalRemainingSeats = totalRemainingSeats;
            ViewBag.TotalEventCapacity = currentEventCapacity;

            return View(tickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public IActionResult DeleteTicketById(int id) 
        {
            var ticketToDelete = _context.regTicketModels
                                         .FirstOrDefault(t => t.Id == id); 

            if (ticketToDelete == null)
            {
                _logger.LogWarning($"Attempted to delete ticket with Id '{id}' but it was not found.");
                TempData["ErrorMessage"] = $"Ticket with Id '{id}' not found.";
                return NotFound(); 
            }

            if (ticketToDelete.RegistrationFormModel != null)
            {
                _context.registrationFormModel.Remove(ticketToDelete.RegistrationFormModel);
            }

            _context.regTicketModels.Remove(ticketToDelete);
            _context.SaveChanges();

            _logger.LogInformation($"Successfully deleted ticket with Id '{id}' (Ticket Number: {ticketToDelete.ticketNumber}).");
            TempData["SuccessMessage"] = $"Ticket '{ticketToDelete.ticketNumber}' (Id: {id}) successfully deleted.";

            return RedirectToAction("AudienceSchedules");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleAttendance(int id)
        {
            var ticket = _context.regTicketModels.FirstOrDefault(t => t.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            ticket.IsAttended = !ticket.IsAttended;
            _context.SaveChanges();

            return RedirectToAction("AudienceSchedules");
        }

    }

}
