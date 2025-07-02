using Microsoft.EntityFrameworkCore;

namespace pupslp_tickets.Models
{
    public class DbModel : DbContext
    {
        public DbModel(DbContextOptions<DbModel> options) : base(options) { }
        public DbSet<AccountLoginModel> accountLoginModel {  get; set; }
        public DbSet<RegistrationFormModel> registrationFormModel { get; set; }
        public DbSet<DashboardOverviewModel> dashboardOverviewModel { get; set; }
        public DbSet<EventDetailsModel> eventDetailsModel { get; set; }

        public DbSet<AttendanceDashboardModel> attendanceDashboardModels { get; set; }

        //SAMPLE

        // nested classes
        public DbSet<RegUserModel> regUserModels { get; set; }
        public DbSet<RegTicketModel> regTicketModels { get; set; }
        public DbSet<RegPaymentModel> regPaymentModels { get; set; }


        // create database
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(@"Data Source=C:\pupslp-tickets\pupslpDatabase.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RegistrationFormModel>()
                .HasOne(r => r.RegUserModel)
                .WithMany()
                .HasForeignKey(r => r.RegUserModelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RegistrationFormModel>()
                .HasOne(r => r.RegPaymentModel)
                .WithMany()
                .HasForeignKey(r => r.RegPaymentModelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RegistrationFormModel>()
                .HasOne(r => r.EventDetailsModel)
                .WithMany()
                .HasForeignKey(r => r.EventDetailsModelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RegTicketModel>()
                .HasOne(t => t.RegistrationFormModel)
                .WithMany(r => r.RegTicketModels)
                .HasForeignKey(t => t.RegistrationFormModelId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
