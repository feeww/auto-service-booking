using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoServiceBooking.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestBookingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_ClientUserId",
                table: "Bookings");

            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "Bookings",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "ClientUserId",
                table: "Bookings",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "Bookings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "Bookings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                table: "Bookings",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GuestVehicleFuelType",
                table: "Bookings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestVehicleLicensePlate",
                table: "Bookings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestVehicleMake",
                table: "Bookings",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuestVehicleMileage",
                table: "Bookings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestVehicleModel",
                table: "Bookings",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuestVehicleYear",
                table: "Bookings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_ClientUserId",
                table: "Bookings",
                column: "ClientUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_ClientUserId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CustomerPhone",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GuestVehicleFuelType",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GuestVehicleLicensePlate",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GuestVehicleMake",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GuestVehicleMileage",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GuestVehicleModel",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GuestVehicleYear",
                table: "Bookings");

            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ClientUserId",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_ClientUserId",
                table: "Bookings",
                column: "ClientUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
