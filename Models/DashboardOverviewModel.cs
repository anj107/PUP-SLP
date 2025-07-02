namespace pupslp_tickets.Models
{
    public class DashboardOverviewModel
    {
        public int Id { get; set; }
        public int attendeesPerShow { get; set; }
        public int totalAttendees {  get; set; }
        public decimal soldTotalTickets { get; set; }
    }
}
