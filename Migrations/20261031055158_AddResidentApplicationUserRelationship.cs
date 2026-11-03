using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTSS.Migrations
{
    /// <inheritdoc />
    public partial class AddResidentApplicationUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SocietySubscriptions_Societies_SocietyId",
                table: "SocietySubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_SocietySubscriptions_SocietyId",
                table: "SocietySubscriptions");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "ResidentProfiles",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResidentProfiles_ApplicationUserId",
                table: "ResidentProfiles",
                column: "ApplicationUserId",
                unique: true,
                filter: "[ApplicationUserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ResidentProfiles_AspNetUsers_ApplicationUserId",
                table: "ResidentProfiles",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResidentProfiles_AspNetUsers_ApplicationUserId",
                table: "ResidentProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ResidentProfiles_ApplicationUserId",
                table: "ResidentProfiles");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "ResidentProfiles");

            migrationBuilder.CreateIndex(
                name: "IX_SocietySubscriptions_SocietyId",
                table: "SocietySubscriptions",
                column: "SocietyId");

            migrationBuilder.AddForeignKey(
                name: "FK_SocietySubscriptions_Societies_SocietyId",
                table: "SocietySubscriptions",
                column: "SocietyId",
                principalTable: "Societies",
                principalColumn: "SocietyId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
