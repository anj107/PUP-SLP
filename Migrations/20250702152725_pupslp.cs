using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pupslp_tickets.Migrations
{
    /// <inheritdoc />
    public partial class pupslp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accountLoginModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    RecoveryEmail = table.Column<string>(type: "TEXT", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "TEXT", nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accountLoginModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "attendanceDashboardModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    attendance = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendanceDashboardModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dashboardOverviewModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    attendeesPerShow = table.Column<int>(type: "INTEGER", nullable: false),
                    totalAttendees = table.Column<int>(type: "INTEGER", nullable: false),
                    soldTotalTickets = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dashboardOverviewModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "eventDetailsModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    eventTitle = table.Column<string>(type: "TEXT", nullable: false),
                    eventDescription = table.Column<string>(type: "TEXT", nullable: false),
                    eventVenue = table.Column<string>(type: "TEXT", nullable: false),
                    eventCapacity = table.Column<int>(type: "INTEGER", nullable: false),
                    eventDates = table.Column<DateTime>(type: "TEXT", nullable: false),
                    hmContactInfo = table.Column<string>(type: "TEXT", nullable: false),
                    hmName = table.Column<string>(type: "TEXT", nullable: false),
                    eventPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    eventPosterUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventDetailsModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "regPaymentModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    regPaymentMode = table.Column<string>(type: "TEXT", nullable: false),
                    regProofPayment = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regPaymentModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "regUserModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    dataPrivacyConsent = table.Column<bool>(type: "INTEGER", nullable: false),
                    regName = table.Column<string>(type: "TEXT", nullable: false),
                    regEmail = table.Column<string>(type: "TEXT", nullable: false),
                    regContactNumber = table.Column<string>(type: "TEXT", nullable: false),
                    regInvitedBy = table.Column<string>(type: "TEXT", nullable: false),
                    regUniv = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regUserModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "registrationFormModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RegUserModelId = table.Column<int>(type: "INTEGER", nullable: false),
                    RegPaymentModelId = table.Column<int>(type: "INTEGER", nullable: false),
                    EventDetailsModelId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registrationFormModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_registrationFormModel_eventDetailsModel_EventDetailsModelId",
                        column: x => x.EventDetailsModelId,
                        principalTable: "eventDetailsModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_registrationFormModel_regPaymentModels_RegPaymentModelId",
                        column: x => x.RegPaymentModelId,
                        principalTable: "regPaymentModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_registrationFormModel_regUserModels_RegUserModelId",
                        column: x => x.RegUserModelId,
                        principalTable: "regUserModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "regTicketModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ticketNumber = table.Column<string>(type: "TEXT", nullable: true),
                    regTicketQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    regVenue = table.Column<string>(type: "TEXT", nullable: false),
                    regTicketDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ticketSched = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    ticketType = table.Column<string>(type: "TEXT", nullable: false),
                    IsAttended = table.Column<bool>(type: "INTEGER", nullable: false),
                    RegistrationFormModelId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regTicketModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_regTicketModels_registrationFormModel_RegistrationFormModelId",
                        column: x => x.RegistrationFormModelId,
                        principalTable: "registrationFormModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_registrationFormModel_EventDetailsModelId",
                table: "registrationFormModel",
                column: "EventDetailsModelId");

            migrationBuilder.CreateIndex(
                name: "IX_registrationFormModel_RegPaymentModelId",
                table: "registrationFormModel",
                column: "RegPaymentModelId");

            migrationBuilder.CreateIndex(
                name: "IX_registrationFormModel_RegUserModelId",
                table: "registrationFormModel",
                column: "RegUserModelId");

            migrationBuilder.CreateIndex(
                name: "IX_regTicketModels_RegistrationFormModelId",
                table: "regTicketModels",
                column: "RegistrationFormModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accountLoginModel");

            migrationBuilder.DropTable(
                name: "attendanceDashboardModels");

            migrationBuilder.DropTable(
                name: "dashboardOverviewModel");

            migrationBuilder.DropTable(
                name: "regTicketModels");

            migrationBuilder.DropTable(
                name: "registrationFormModel");

            migrationBuilder.DropTable(
                name: "eventDetailsModel");

            migrationBuilder.DropTable(
                name: "regPaymentModels");

            migrationBuilder.DropTable(
                name: "regUserModels");
        }
    }
}
