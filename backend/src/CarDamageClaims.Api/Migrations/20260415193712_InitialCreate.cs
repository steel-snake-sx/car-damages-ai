using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarDamageClaims.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    MiddleName = table.Column<string>(type: "TEXT", nullable: true),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "damage_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    MiddleName = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    CarBrand = table.Column<string>(type: "TEXT", nullable: false),
                    CarModel = table.Column<string>(type: "TEXT", nullable: false),
                    CarYear = table.Column<int>(type: "INTEGER", nullable: false),
                    AiIsCar = table.Column<bool>(type: "INTEGER", nullable: false),
                    AiSummary = table.Column<string>(type: "TEXT", nullable: false),
                    AiEstimatedTotalCost = table.Column<decimal>(
                        type: "TEXT",
                        precision: 18,
                        scale: 2,
                        nullable: false
                    ),
                    AdminDecisionComment = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_damage_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_damage_requests_users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "damage_estimate_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DamageRequestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PartName = table.Column<string>(type: "TEXT", nullable: false),
                    DamageDescription = table.Column<string>(type: "TEXT", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", nullable: false),
                    EstimatedCost = table.Column<decimal>(
                        type: "TEXT",
                        precision: 18,
                        scale: 2,
                        nullable: false
                    ),
                    Confidence = table.Column<double>(type: "REAL", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_damage_estimate_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_damage_estimate_items_damage_requests_DamageRequestId",
                        column: x => x.DamageRequestId,
                        principalTable: "damage_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "damage_request_photos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DamageRequestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_damage_request_photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_damage_request_photos_damage_requests_DamageRequestId",
                        column: x => x.DamageRequestId,
                        principalTable: "damage_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_damage_estimate_items_DamageRequestId",
                table: "damage_estimate_items",
                column: "DamageRequestId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_damage_request_photos_DamageRequestId",
                table: "damage_request_photos",
                column: "DamageRequestId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_damage_requests_ApprovedByUserId",
                table: "damage_requests",
                column: "ApprovedByUserId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_damage_requests_CreatedAt",
                table: "damage_requests",
                column: "CreatedAt"
            );

            migrationBuilder.CreateIndex(
                name: "IX_damage_requests_Email",
                table: "damage_requests",
                column: "Email"
            );

            migrationBuilder.CreateIndex(
                name: "IX_damage_requests_Status",
                table: "damage_requests",
                column: "Status"
            );

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "damage_estimate_items");

            migrationBuilder.DropTable(name: "damage_request_photos");

            migrationBuilder.DropTable(name: "damage_requests");

            migrationBuilder.DropTable(name: "users");
        }
    }
}
