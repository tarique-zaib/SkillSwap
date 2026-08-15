using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSwap.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVouches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vouches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FavorId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StoryText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vouches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vouches_favors_FavorId",
                        column: x => x.FavorId,
                        principalTable: "favors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vouches_users_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vouches_users_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vouches_FavorId_FromUserId_ToUserId",
                table: "vouches",
                columns: new[] { "FavorId", "FromUserId", "ToUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vouches_FromUserId",
                table: "vouches",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_vouches_ToUserId",
                table: "vouches",
                column: "ToUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vouches");
        }
    }
}
