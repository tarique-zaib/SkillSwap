using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSwap.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    NeedId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_matches_skill_listings_NeedId",
                        column: x => x.NeedId,
                        principalTable: "skill_listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_matches_skill_listings_OfferId",
                        column: x => x.OfferId,
                        principalTable: "skill_listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_matches_NeedId",
                table: "matches",
                column: "NeedId");

            migrationBuilder.CreateIndex(
                name: "IX_matches_OfferId_NeedId",
                table: "matches",
                columns: new[] { "OfferId", "NeedId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "matches");
        }
    }
}
