using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpdManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AccumulatedExaminationTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccumulatedExaminationSeconds",
                table: "QueueEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccumulatedExaminationSeconds",
                table: "QueueEntries");
        }
    }
}
