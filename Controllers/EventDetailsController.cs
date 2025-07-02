using Microsoft.AspNetCore.Mvc;
using pupslp_tickets.Models;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

namespace pupslp_tickets.Controllers
{
    public class EventDetailsController : Controller
    {
        private readonly ILogger<EventDetailsController> _logger;
        private readonly DbModel _context;

        public EventDetailsController(DbModel context, ILogger<EventDetailsController> logger)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public IActionResult EventForm()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var eventDetails = _context.eventDetailsModel.FirstOrDefault(e => e.Id == id);
            if (eventDetails == null)
            {
                return NotFound();
            }
            return View("EventForm", eventDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EventForm(EventDetailsModel eventDetails, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Upload image to Supabase if a new image was selected
                if (imageFile != null && imageFile.Length > 0)
                {
                    var supabaseUrl = "https://qlxddunpyaxzdgffuocr.supabase.co";
                    var bucketName = "event-posters";
                    var supabaseApiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InFseGRkdW5weWF4emRnZmZ1b2NyIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc0OTM3NDkxNCwiZXhwIjoyMDY0OTUwOTE0fQ.mrUzOld8ozXYfEzG_AT2SZ0aeAkm3tza9xGyrYJYpaQ"; 
                    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);

                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Add("apikey", supabaseApiKey);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseApiKey}");

                    using var content = new MultipartFormDataContent();
                    var streamContent = new StreamContent(imageFile.OpenReadStream());
                    content.Add(streamContent, "file", fileName);

                    var uploadUrl = $"{supabaseUrl}/storage/v1/object/{bucketName}/{fileName}";
                    var response = await httpClient.PostAsync(uploadUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var publicUrl = $"{supabaseUrl}/storage/v1/object/public/{bucketName}/{fileName}";
                        eventDetails.eventPosterUrl = publicUrl;
                    }
                    else
                    {
                        TempData["Error"] = "Image upload failed.";
                    }
                }

                if (eventDetails.Id == 0)
                {
                    _context.eventDetailsModel.Add(eventDetails);
                }
                else
                {
                    var existingEvent = _context.eventDetailsModel.FirstOrDefault(e => e.Id == eventDetails.Id);
                    if (existingEvent == null) return NotFound();

                    // Update fields
                    existingEvent.eventTitle = eventDetails.eventTitle;
                    existingEvent.eventDescription = eventDetails.eventDescription;
                    existingEvent.eventVenue = eventDetails.eventVenue;
                    existingEvent.eventCapacity = eventDetails.eventCapacity;
                    existingEvent.eventDates = eventDetails.eventDates;
                    existingEvent.hmName = eventDetails.hmName;
                    existingEvent.hmContactInfo = eventDetails.hmContactInfo;
                    existingEvent.eventPrice = eventDetails.eventPrice;

                    // Only update poster if newly uploaded
                    if (!string.IsNullOrEmpty(eventDetails.eventPosterUrl))
                        existingEvent.eventPosterUrl = eventDetails.eventPosterUrl;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Overview", "Dashboard", new { id = eventDetails.Id });
            }

            return View(eventDetails);
        }
    }
}
