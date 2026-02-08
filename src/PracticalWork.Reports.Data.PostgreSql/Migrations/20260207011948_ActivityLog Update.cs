using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PracticalWork.Reports.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class ActivityLogUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ❗ Удаляем старую string-колонку
            migrationBuilder.DropColumn(
                name: "EventType",
                table: "activity_logs");

            // ✅ Создаём новую enum(int)
            migrationBuilder.AddColumn<int>(
                name: "EventType",
                table: "activity_logs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "activity_logs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodTo = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_EventDate",
                table: "activity_logs",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_EventType",
                table: "activity_logs",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_reports_GeneratedAt",
                table: "reports",
                column: "GeneratedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reports");

            migrationBuilder.DropIndex(
                name: "IX_activity_logs_EventDate",
                table: "activity_logs");

            migrationBuilder.DropIndex(
                name: "IX_activity_logs_EventType",
                table: "activity_logs");

            // ⬅️ Возвращаем string
            migrationBuilder.DropColumn(
                name: "EventType",
                table: "activity_logs");

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "activity_logs",
                type: "text",
                nullable: false,
                defaultValue: "Unknown");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "activity_logs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");
        }
    }
}
