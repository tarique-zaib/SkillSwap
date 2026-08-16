using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSwap.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchInitiatedByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InitiatedByUserId",
                table: "matches",
                type: "uuid",
                nullable: true,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
    UPDATE matches m
    SET "InitiatedByUserId" = n."UserId"
    FROM skill_listings n
    WHERE n."Id" = m."NeedId";
""");

            migrationBuilder.AlterColumn<Guid>(
    name: "InitiatedByUserId",
    table: "matches",
    type: "uuid",
    nullable: false,
    oldClrType: typeof(Guid),
    oldType: "uuid",
    oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_matches_InitiatedByUserId",
                table: "matches",
                column: "InitiatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_matches_users_InitiatedByUserId",
                table: "matches",
                column: "InitiatedByUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_matches_users_InitiatedByUserId",
                table: "matches");

            migrationBuilder.DropIndex(
                name: "IX_matches_InitiatedByUserId",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "InitiatedByUserId",
                table: "matches");
        }
    }
}
