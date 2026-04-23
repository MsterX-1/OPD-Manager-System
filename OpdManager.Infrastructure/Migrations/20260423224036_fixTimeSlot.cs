using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpdManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixTimeSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeSlot",
                table: "Appointments");

            migrationBuilder.AddColumn<long>(
                name: "TimeSlotTicks",
                table: "Appointments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeSlotTicks",
                table: "Appointments");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeSlot",
                table: "Appointments",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
