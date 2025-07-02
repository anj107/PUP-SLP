using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pupslp_tickets.Models;
using System.Diagnostics;
using System;

namespace pupslp_tickets.Controllers
{
    public class RegFormController : Controller
    {
        private readonly ILogger<RegFormController> _logger;
        private readonly DbModel _context;

        public RegFormController(ILogger<RegFormController> logger, DbModel context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public IActionResult RegForm()
        {
            var eventDetails = _context.eventDetailsModel
                .OrderByDescending(e => e.Id)
                .FirstOrDefault();

            var model = new RegistrationFormModel
            {
                EventDetailsModel = eventDetails ?? new EventDetailsModel(),
                RegTicketModels = new List<RegTicketModel> { new RegTicketModel() } 
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegForm(RegistrationFormModel reg, IFormFile? proofImage)
        {
            var eventDetails = _context.eventDetailsModel
                .OrderByDescending(e => e.Id)
                .FirstOrDefault();

            if (eventDetails != null)
                reg.EventDetailsModel = eventDetails;

            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("EventDetailsModel")).ToList())
            {
                ModelState.Remove(key);
            }

            if (ModelState.IsValid)
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // Validate proof of payment only if payment mode is GCash
                        if (reg.RegPaymentModel.regPaymentMode == "GCash" && proofImage == null)
                        {
                            ModelState.AddModelError("RegPaymentModel.regProofPayment", "Please upload a proof of payment image for GCash.");
                            reg.EventDetailsModel = eventDetails ?? new EventDetailsModel();
                            return View(reg);
                        }


                        // ADDED PROOF IMAGE UPLOAD
                        if (proofImage != null && proofImage.Length > 0)
                        {
                            var supabaseUrl = "https://qlxddunpyaxzdgffuocr.supabase.co";
                            var bucketName = "payment-proof";
                            var supabaseApiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InFseGRkdW5weWF4emRnZmZ1b2NyIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc0OTM3NDkxNCwiZXhwIjoyMDY0OTUwOTE0fQ.mrUzOld8ozXYfEzG_AT2SZ0aeAkm3tza9xGyrYJYpaQ"; 

                            var fileName = Guid.NewGuid() + Path.GetExtension(proofImage.FileName);

                            using var httpClient = new HttpClient();
                            httpClient.DefaultRequestHeaders.Add("apikey", supabaseApiKey);
                            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseApiKey}");

                            using var content = new MultipartFormDataContent();
                            var streamContent = new StreamContent(proofImage.OpenReadStream());
                            content.Add(streamContent, "file", fileName);

                            var uploadUrl = $"{supabaseUrl}/storage/v1/object/{bucketName}/{fileName}";
                            var response = await httpClient.PostAsync(uploadUrl, content);

                            if (response.IsSuccessStatusCode)
                            {
                                var publicUrl = $"{supabaseUrl}/storage/v1/object/public/{bucketName}/{fileName}";
                                reg.RegPaymentModel.regProofPayment = publicUrl;
                            }
                            else
                            {
                                TempData["Error"] = "Failed to upload proof of payment.";
                            }
                        }

                        // Save user and payment
                        _context.regUserModels.Add(reg.RegUserModel);
                        _context.regPaymentModels.Add(reg.RegPaymentModel);
                        _context.SaveChanges();

                        if (eventDetails == null)
                        {
                            ModelState.AddModelError("", "No event is currently active or found.");
                            return View(reg);
                        }

                        reg.EventDetailsModel = eventDetails;
                        reg.RegTicketModels = new List<RegTicketModel>();

                        // Save registration form (parent)
                        _context.registrationFormModel.Add(reg);
                        _context.SaveChanges();

                        // generate ticket list
                        int ticketQty = Convert.ToInt32(Request.Form["RegTicketModels[0].regTicketQuantity"]);

                        if (ticketQty <= 0)
                        {
                            return RedirectToAction("RegForm"); // invalid qty
                        }

                        // Read the form values
                        var ticket = new RegTicketModel
                        {
                            regTicketDate = DateOnly.FromDateTime(Convert.ToDateTime(Request.Form["RegTicketModels[0].regTicketDate"])),
                            regVenue = Request.Form["RegTicketModels[0].regVenue"],
                            ticketSched = TimeSpan.Parse(Request.Form["RegTicketModels[0].ticketSched"]),
                            ticketType = Request.Form["RegTicketModels[0].ticketType"],
                            RegistrationFormModelId = reg.Id
                        };

                        var tickets = new List<RegTicketModel>();
                        for (int i = 0; i < ticketQty; i++)
                        {
                            tickets.Add(new RegTicketModel
                            {
                                regTicketDate = ticket.regTicketDate,
                                regVenue = ticket.regVenue,
                                ticketSched = ticket.ticketSched,
                                ticketType = ticket.ticketType,
                                RegistrationFormModelId = ticket.RegistrationFormModelId,
                                regTicketQuantity = 1
                            });
                        }

                        _context.regTicketModels.AddRange(tickets);
                        _context.SaveChanges();

                        // Ticket Number Assignment
                        foreach (var t in tickets)
                        {
                            t.ticketNumber = $"TCKT-{t.Id:D5}"; // Format Id as 5 digits (TCKT-00001)
                        }

                        _context.SaveChanges();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                return RedirectToAction("RegForm");
            }

            if (reg.EventDetailsModel == null)
                reg.EventDetailsModel = eventDetails ?? new EventDetailsModel();

            return View(reg);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
