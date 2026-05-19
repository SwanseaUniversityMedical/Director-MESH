using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectorMesh.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class PerDataPlaneConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "Sites",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentHash",
                table: "SiteConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DataPlaneId",
                table: "SiteConfigurations",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteConfigurations_DataPlaneId",
                table: "SiteConfigurations",
                column: "DataPlaneId");

            migrationBuilder.AddForeignKey(
                name: "FK_SiteConfigurations_DataPlanes_DataPlaneId",
                table: "SiteConfigurations",
                column: "DataPlaneId",
                principalTable: "DataPlanes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SiteConfigurations_DataPlanes_DataPlaneId",
                table: "SiteConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_SiteConfigurations_DataPlaneId",
                table: "SiteConfigurations");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "ContentHash",
                table: "SiteConfigurations");

            migrationBuilder.DropColumn(
                name: "DataPlaneId",
                table: "SiteConfigurations");
        }
    }
}
