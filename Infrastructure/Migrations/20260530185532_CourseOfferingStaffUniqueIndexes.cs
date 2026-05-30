using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CourseOfferingStaffUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CourseOfferingStaffs_CourseOfferingId_UserId_Scope_SectionId",
                table: "CourseOfferingStaffs");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferingStaffs_CourseOfferingId_UserId",
                table: "CourseOfferingStaffs",
                columns: new[] { "CourseOfferingId", "UserId" },
                unique: true,
                filter: "\"Scope\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferingStaffs_CourseOfferingId_UserId_SectionId",
                table: "CourseOfferingStaffs",
                columns: new[] { "CourseOfferingId", "UserId", "SectionId" },
                unique: true,
                filter: "\"Scope\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CourseOfferingStaffs_CourseOfferingId_UserId",
                table: "CourseOfferingStaffs");

            migrationBuilder.DropIndex(
                name: "IX_CourseOfferingStaffs_CourseOfferingId_UserId_SectionId",
                table: "CourseOfferingStaffs");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferingStaffs_CourseOfferingId_UserId_Scope_SectionId",
                table: "CourseOfferingStaffs",
                columns: new[] { "CourseOfferingId", "UserId", "Scope", "SectionId" },
                unique: true);
        }
    }
}
