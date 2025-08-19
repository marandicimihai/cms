using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Main.Migrations
{
    /// <inheritdoc />
    public partial class AddedPropertyIsRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "SchemaProperties",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "SchemaProperties");
        }
    }
}
