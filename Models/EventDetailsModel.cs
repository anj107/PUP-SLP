using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace pupslp_tickets.Models
{
    public class EventDetailsModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter an event title.")]
        public string eventTitle { get; set; }

        [Required(ErrorMessage = "Enter an event description.")]
        public string eventDescription { get; set; }

        [Required(ErrorMessage = "Enter an event venue.")]
        public string eventVenue { get; set; }

        [Required(ErrorMessage = "Enter an event capacity.")]
        public int eventCapacity { get; set; }
        public DateTime eventDates { get; set; }

        [Required(ErrorMessage = "Enter house manager contact information.")]
        public string hmContactInfo { get; set; }
        public string hmName { get; set; }

        [Precision(18, 2)]
        public decimal eventPrice { get; set; }
        public string? eventPosterUrl { get; set; }
    }
}
