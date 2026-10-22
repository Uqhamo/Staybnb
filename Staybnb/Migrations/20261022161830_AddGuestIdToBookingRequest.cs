using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Staybnb.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestIdToBookingRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuestId",
                table: "BookingRequests",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuestId",
                table: "BookingRequests");
        }
    }
}
