using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarDamageClaims.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationOutboxSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "notification_outbox",
                type: "TEXT",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Subject", table: "notification_outbox");
        }
    }
}
