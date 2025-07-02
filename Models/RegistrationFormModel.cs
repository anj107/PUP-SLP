using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace pupslp_tickets.Models
{
    public class RegistrationFormModel
    {
        public int Id { get; set; }
        public int RegUserModelId { get; set; }
        public int RegPaymentModelId { get; set; }
        public int EventDetailsModelId { get; set; }

        // Navigation Properties (EF Core uses these to link related objects/tables)
        public virtual RegUserModel RegUserModel { get; set; }
        public virtual RegPaymentModel RegPaymentModel { get; set; }
        public virtual EventDetailsModel EventDetailsModel { get; set; }

  
        // This allows to handle cases where a user orders more than one ticket in a single registration
        public virtual List<RegTicketModel> RegTicketModels { get; set; } = new List<RegTicketModel>();
        //contain all tickets generated for this registration
    }

   // Stores user information for a registration
    public class RegUserModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "You must consent to data privacy.")]
        public bool dataPrivacyConsent { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        public string regName { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string regEmail { get; set; }
        [Required(ErrorMessage = "Contact number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string regContactNumber { get; set; }
        [Required(ErrorMessage = "Inviter's name is required.")]
        public string regInvitedBy { get; set; }
        [Required(ErrorMessage = "University is required.")]
        public string regUniv { get; set; }

        //public string? regComments { get; set; }
    }

    public class RegTicketModel
    {
        public int Id { get; set; }
        public string? ticketNumber { get; set; } // Example: TCKT-00001
        public int regTicketQuantity { get; set; } = 1; //default by 1 ito yung migrated
        [Required(ErrorMessage = "Venue is required.")]
        public string regVenue { get; set; }
        [Required(ErrorMessage = "Date is required.")]
        public DateOnly regTicketDate { get; set; }
        [Required(ErrorMessage = "Show schedule is required.")]
        public TimeSpan ticketSched { get; set; }
        [Required(ErrorMessage = "Ticket type is required.")]
        public string ticketType { get; set; }

        public bool IsAttended { get; set; } = false; // Default is Absent


        // Foreign Key to link back to parent registration
        public int RegistrationFormModelId { get; set; }
        public virtual RegistrationFormModel? RegistrationFormModel { get; set; }
    }
}

// Payment info for a registration
public class RegPaymentModel
{
    public int Id { get; set; }
    public string regPaymentMode { get; set; }
    public string? regProofPayment { get; set; }
}