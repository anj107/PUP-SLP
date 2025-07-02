using System.ComponentModel.DataAnnotations;

namespace pupslp_tickets.Models
{
    public class AccountLoginModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a username.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please enter a password.")]
        public string Password { get; set; } // Store hashed password

        [EmailAddress]
        public string? RecoveryEmail { get; set; }
        public string? PasswordResetToken { get; set; } //For reset
        public DateTime? PasswordResetTokenExpiry { get; set; } //For reset
    }
}