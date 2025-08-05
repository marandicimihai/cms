using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QueueID = table.Column<string>(type: "text", nullable: false),
                    TrackingID = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecuteAfter = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpireOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false),
                    CommandJson = table.Column<string>(type: "text", nullable: false),
                    ResultJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jobs");
        }
    }
}
