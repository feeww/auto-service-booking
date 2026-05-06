using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoServiceBooking.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingEstimates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationMinutes",
                table: "Bookings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedPrice",
                table: "Bookings",
                type: "numeric(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedDurationMinutes",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "EstimatedPrice",
                table: "Bookings");
        }
    }
}
