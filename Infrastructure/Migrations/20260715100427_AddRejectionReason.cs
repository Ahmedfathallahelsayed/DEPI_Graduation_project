using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LessonProgresses_EnrollmentId",
                table: "LessonProgresses");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonProgresses_EnrollmentId_LessonId",
                table: "LessonProgresses",
                columns: new[] { "EnrollmentId", "LessonId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LessonProgresses_EnrollmentId_LessonId",
                table: "LessonProgresses");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Courses");

            migrationBuilder.CreateIndex(
                name: "IX_LessonProgresses_EnrollmentId",
                table: "LessonProgresses",
                column: "EnrollmentId");
        }
    }
}
