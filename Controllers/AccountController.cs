using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using pupslp_tickets.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using pupslp_tickets.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks; 
using Microsoft.AspNetCore.Http;
using System;




namespace pupslp_tickets.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly DbModel _context;

        public AccountController(DbModel context, IEmailSender emailSender, ILogger<AccountController> logger)
        {
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        // Helper: Hash password for security.
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        // added by ten para maaccess yung forgot password page...
        public IActionResult ForgotPassword()
        {
            return View();
        }


        //added for forgot password flow.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string recoveryEmail)
        {
            if (string.IsNullOrEmpty(recoveryEmail))
            {
                ViewBag.ErrorMessage = "Please enter your recovery email.";
                return View();
            }

            // Find the user by recovery email
            var user = await _context.accountLoginModel
                                     .FirstOrDefaultAsync(u => u.RecoveryEmail == recoveryEmail);

            if (user == null)
            {
            //viewbag for them to check their email/or tell them account don't exist
                ViewBag.Message = "Please check your email, a password reset link has been sent to your account.";
                _logger.LogWarning($"Forgot Password attempt for non-existent or unlinked email: {recoveryEmail}");
                return View();
            }

            // Generate Token 
            string token = GeneratePasswordResetToken(user.Username); // Use username
            DateTime expiry = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour 

            // Save Token and Expiry to Database
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = expiry;
            await _context.SaveChangesAsync();

            // Generate Reset Link
            string resetLink = GeneratePasswordResetLink(user.Username, token);

            // Send Email
            try
            {
                await _emailSender.SendEmailAsync(
                    user.RecoveryEmail, 
                    "PUP SLP - Password Reset Request", // Subject 
                    $"<p>Dear {user.Username},</p>" +
                    "<p>We received a request to reset the password for your PUP Sining-Lahi account.</p>" +
                    $"<p>Please click the following link to reset your password: <a href='{resetLink}'>Reset My Password</a></p>" +
                    "<p>This link will expire in 1 hour for security reasons. If you did not request a password reset, please ignore this email.</p>" +
                    "<br><p>Thank you,</p>" +
                    "<p>PUP Sining-Lahi</p>"
                );
                ViewBag.Message = "Please check your email, a password reset link has been sent to your account.";
                _logger.LogInformation($"Password reset link sent to {user.RecoveryEmail} for user {user.Username}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset email to {user.RecoveryEmail}.");
                ViewBag.ErrorMessage = "Error sending password reset email. Please try again later or contact support.";
            }

            return View();
        }

     
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string username, string token)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(token))
            {
                ViewBag.ErrorMessage = "Invalid password reset link. Missing username or token.";
                return View("Login"); // Redirect to login or to error page
            }

            // Look up the user and validate the token from the database
            var user = await _context.accountLoginModel
                                     .FirstOrDefaultAsync(u => u.Username == username && u.PasswordResetToken == token);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Invalid password reset link. User or token mismatch.";
                _logger.LogWarning($"ResetPassword GET: User '{username}' or token mismatch.");
                return View("Login");
            }

            if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                ViewBag.ErrorMessage = "Password reset link has expired. Please request a new one.";
                _logger.LogWarning($"ResetPassword GET: Token for user '{username}' has expired.");
                return View("Login");
            }

            // If valid, show the reset password form populated with hidden fields
            var model = new ResetPasswordModel
            {
                Username = username,
                Token = token
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                // If model validation fails (e.g., passwords don't match, or too short)
                return View(model);
            }

            // Re-validate token and user to prevent  re-use of expired tokens
            var user = await _context.accountLoginModel
                                     .FirstOrDefaultAsync(u => u.Username == model.Username && u.PasswordResetToken == model.Token);

            if (user == null || user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                // Invalid or expired token 
                ViewBag.ErrorMessage = "Invalid or expired password reset link. Please request a new one.";
                _logger.LogWarning($"ResetPassword POST: Token for user '{model.Username}' is invalid or expired on submission.");
                return View("Login");
            }
            user.Password = HashPassword(model.Password);

            // Clear the token and expiry 
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            ViewBag.Message = "Your password has been reset successfully. You can now log in with your new password.";
            _logger.LogInformation($"Password successfully reset for user {user.Username}.");
            return View("Login"); 
        }
       

        // Helper for password reset flow
        private string GeneratePasswordResetToken(string username)
        {
            byte[] randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            string combinedString = username + Convert.ToBase64String(randomBytes) + DateTime.UtcNow.Ticks.ToString();

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedString));
                return Convert.ToBase64String(bytes).Replace("/", "_").Replace("+", "-").Replace("=", "");
            }
        }

        // Helper for password reset flow (updated to use Url.Action)
        private string GeneratePasswordResetLink(string username, string token)
        {
           
            string scheme = HttpContext.Request.Scheme;
            string host = HttpContext.Request.Host.ToUriComponent();

            string callbackUrl = Url.Action(
                "ResetPassword",      
                "Account",            
                new { username = username, token = token }, //(parameters)
                scheme,               // Scheme (http/https)
                host                  // Host (domain)
            );
            return callbackUrl;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(AccountLoginModel model)
        {
            model.Password = HashPassword(model.Password);
            _logger.LogInformation("pw: " + model.Password);


            ModelState.Remove("AccountLoginModel.RecoveryEmail");
            if (ModelState.IsValid)
            {
                var user = _context.accountLoginModel
                    .FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);

                _logger.LogInformation("user: " + user);

                var eventDetails = _context.eventDetailsModel
                    .OrderByDescending(e => e.Id)
                    .FirstOrDefault();

                if (eventDetails == null)
                {
                    // Create default event detail if none exists
                    eventDetails = new EventDetailsModel
                    {
                        eventTitle = "Default Event Title",
                        eventDescription = "Default Event Description",
                        eventVenue = "Default Venue",
                        eventCapacity = 100,
                        eventDates = DateTime.Now,
                        hmContactInfo = "Default Contact Info",
                        hmName = "Default Manager Name",
                        eventPrice = 25.00m
                    };
                    _context.eventDetailsModel.Add(eventDetails);
                    _context.SaveChanges();
                }

                // THE ROUTING TO THE DASHBOARD AFTER CORRECT LOGIN
                if (user != null)
                {
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("Username", user.Username);
                    return RedirectToAction("Overview", "Dashboard", new { id = eventDetails.Id });
                }
                else
                {
                    _logger.LogWarning("Login failed. No user found.");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(AccountLoginModel model)
        {
            if (ModelState.IsValid)
            {
                if (_context.accountLoginModel.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    return View(model);
                }
                if (_context.accountLoginModel.Any(u => u.RecoveryEmail == model.RecoveryEmail))
                {
                    ModelState.AddModelError("RecoveryEmail", "Email already registered.");
                    return View(model);
                }

                model.Password = HashPassword(model.Password);

                _context.accountLoginModel.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}