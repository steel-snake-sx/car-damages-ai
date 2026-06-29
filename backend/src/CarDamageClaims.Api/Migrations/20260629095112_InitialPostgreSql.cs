using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarDamageClaims.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    MiddleName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "damage_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    MiddleName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    CarBrand = table.Column<string>(type: "text", nullable: false),
                    CarModel = table.Column<string>(type: "text", nullable: false),
                    CarYear = table.Column<int>(type: "integer", nullable: false),
                    AiIsCar = table.Column<bool>(type: "boolean", nullable: false),
                    AiSummary = table.Column<string>(type: "text", nullable: false),
                    AiEstimatedTotalCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AdminDecisionComment = table.Column<string>(type: "text", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_damage_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_damage_requests_users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "damage_estimate_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DamageRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartName = table.Column<string>(type: "text", nullable: false),
                    DamageDescription = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_damage_estimate_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_damage_estimate_items_damage_requests_DamageRequestId",
                        column: x => x.DamageRequestId,
                        principalTable: "damage_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "damage_request_photos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DamageRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_damage_request_photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_damage_request_photos_damage_requests_DamageRequestId",
                        column: x => x.DamageRequestId,
                        principalTable: "damage_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_outbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DamageRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientEmail = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: true),
                    NotificationType = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_outbox", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notification_outbox_damage_requests_DamageRequestId",
                        column: x => x.DamageRequestId,
                        principalTable: "damage_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_damage_estimate_items_DamageRequestId",
                table: "damage_estimate_items",
                column: "DamageRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_damage_request_photos_DamageRequestId",
                table: "damage_request_photos",
                column: "DamageRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_damage_requests_ApprovedByUserId",
                table: "damage_requests",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_damage_requests_CreatedAt",
                table: "damage_requests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_damage_requests_Email",
                table: "damage_requests",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_damage_requests_Status",
                table: "damage_requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_notification_outbox_CreatedAt",
                table: "notification_outbox",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_notification_outbox_DamageRequestId",
                table: "notification_outbox",
                column: "DamageRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_notification_outbox_RecipientEmail",
                table: "notification_outbox",
                column: "RecipientEmail");

            migrationBuilder.CreateIndex(
                name: "IX_notification_outbox_Status",
                table: "notification_outbox",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "damage_estimate_items");

            migrationBuilder.DropTable(
                name: "damage_request_photos");

            migrationBuilder.DropTable(
                name: "notification_outbox");

            migrationBuilder.DropTable(
                name: "damage_requests");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
