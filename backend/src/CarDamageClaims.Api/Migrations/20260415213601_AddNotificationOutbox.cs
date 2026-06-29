using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarDamageClaims.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notification_outbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DamageRequestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipientEmail = table.Column<string>(type: "TEXT", nullable: false),
                    NotificationType = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_outbox", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notification_outbox_damage_requests_DamageRequestId",
                        column: x => x.DamageRequestId,
                        principalTable: "damage_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_notification_outbox_CreatedAt",
                table: "notification_outbox",
                column: "CreatedAt"
            );

            migrationBuilder.CreateIndex(
                name: "IX_notification_outbox_DamageRequestId",
                table: "notification_outbox",
                column: "DamageRequestId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_notification_outbox_RecipientEmail",
                table: "notification_outbox",
                column: "RecipientEmail"
            );

            migrationBuilder.CreateIndex(
                name: "IX_notification_outbox_Status",
                table: "notification_outbox",
                column: "Status"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "notification_outbox");
        }
    }
}
