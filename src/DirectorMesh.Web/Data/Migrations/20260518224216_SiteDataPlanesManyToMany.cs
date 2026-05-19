using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectorMesh.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SiteDataPlanesManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sites_DataPlanes_DataPlaneId",
                table: "Sites");

            migrationBuilder.DropIndex(
                name: "IX_Sites_DataPlaneId",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "DataPlaneId",
                table: "Sites");

            migrationBuilder.CreateTable(
                name: "SiteDataPlane",
                columns: table => new
                {
                    DataPlanesId = table.Column<int>(type: "integer", nullable: false),
                    SitesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteDataPlane", x => new { x.DataPlanesId, x.SitesId });
                    table.ForeignKey(
                        name: "FK_SiteDataPlane_DataPlanes_DataPlanesId",
                        column: x => x.DataPlanesId,
                        principalTable: "DataPlanes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SiteDataPlane_Sites_SitesId",
                        column: x => x.SitesId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteDataPlane_SitesId",
                table: "SiteDataPlane",
                column: "SitesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteDataPlane");

            migrationBuilder.AddColumn<int>(
                name: "DataPlaneId",
                table: "Sites",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sites_DataPlaneId",
                table: "Sites",
                column: "DataPlaneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_DataPlanes_DataPlaneId",
                table: "Sites",
                column: "DataPlaneId",
                principalTable: "DataPlanes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
