using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectorMesh.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNetBirdSetupKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NetBirdSetupKey",
                table: "NetworkServices",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NetBirdSetupKey",
                table: "NetworkServices");
        }
    }
}
