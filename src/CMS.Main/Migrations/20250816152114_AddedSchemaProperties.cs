using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Main.Migrations
{
    /// <inheritdoc />
    public partial class AddedSchemaProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchemaProperties",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    SchemaId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Options = table.Column<string[]>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemaProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchemaProperties_Schemas_SchemaId",
                        column: x => x.SchemaId,
                        principalTable: "Schemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchemaProperties_SchemaId",
                table: "SchemaProperties",
                column: "SchemaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchemaProperties");
        }
    }
}
