using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CourseOfferingStaffAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CourseOfferingStaffs_CourseOfferingId",
                table: "CourseOfferingStaffs");

            migrationBuilder.AddColumn<int>(
                name: "AccessLevel",
                table: "CourseOfferingStaffs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "CourseOfferingStaffs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "CourseOfferingStaffs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferingStaffs_CourseOfferingId_UserId_Scope_SectionId",
                table: "CourseOfferingStaffs",
                columns: new[] { "CourseOfferingId", "UserId", "Scope", "SectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferingStaffs_SectionId",
                table: "CourseOfferingStaffs",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseOfferingStaffs_Sections_SectionId",
                table: "CourseOfferingStaffs",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseOfferingStaffs_Sections_SectionId",
                table: "CourseOfferingStaffs");

            migrationBuilder.DropIndex(
                name: "IX_CourseOfferingStaffs_CourseOfferingId_UserId_Scope_SectionId",
                table: "CourseOfferingStaffs");

            migrationBuilder.DropIndex(
                name: "IX_CourseOfferingStaffs_SectionId",
                table: "CourseOfferingStaffs");

            migrationBuilder.DropColumn(
                name: "AccessLevel",
                table: "CourseOfferingStaffs");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "CourseOfferingStaffs");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "CourseOfferingStaffs");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferingStaffs_CourseOfferingId",
                table: "CourseOfferingStaffs",
                column: "CourseOfferingId");
        }
    }
}
