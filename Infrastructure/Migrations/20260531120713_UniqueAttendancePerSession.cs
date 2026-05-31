using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UniqueAttendancePerSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attendances_SessionId",
                table: "Attendances");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_SessionId_UserId",
                table: "Attendances",
                columns: new[] { "SessionId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attendances_SessionId_UserId",
                table: "Attendances");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_SessionId",
                table: "Attendances",
                column: "SessionId");
        }
    }
}
