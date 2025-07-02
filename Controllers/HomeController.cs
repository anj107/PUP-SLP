using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pupslp_tickets.Models;

namespace pupslp_tickets.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DbModel _context;

    public HomeController(ILogger<HomeController> logger, DbModel context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(RegistrationFormModel reg)
    {
        if (ModelState.IsValid)
        {
            // Save user
            _context.Add(reg.RegUserModel);
            _context.SaveChanges();

            int userId = reg.RegUserModel.Id;

            // Set foreign keys if ne
            reg.RegPaymentModel.Id = userId;

            // Save ticket and payment
            _context.Add(reg.RegPaymentModel);
            _context.SaveChanges();

            // Optionally redirect to a confirmation or login page
            return RedirectToAction("Index");
        }

        return View(reg); // Return with validation errors if any
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}