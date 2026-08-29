using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectorMesh.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AttachNetworkServiceToDataPlane : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DataPlaneId",
                table: "NetworkServices",
                type: "integer",
                nullable: true);

            // Backfill: pin each existing service to a data plane already attached to its site.
            migrationBuilder.Sql(@"
                UPDATE ""NetworkServices"" ns
                SET ""DataPlaneId"" = sub.""DataPlaneId""
                FROM (
                    SELECT sdp.""SitesId"" AS ""SiteId"", MIN(sdp.""DataPlanesId"") AS ""DataPlaneId""
                    FROM ""SiteDataPlane"" sdp
                    GROUP BY sdp.""SitesId""
                ) sub
                WHERE ns.""SiteId"" = sub.""SiteId"" AND ns.""DataPlaneId"" IS NULL;
            ");

            // Fallback for services whose site had no data plane attached — pin to any
            // existing data plane so the column can be made required; fix up manually afterward.
            migrationBuilder.Sql(@"
                UPDATE ""NetworkServices""
                SET ""DataPlaneId"" = (SELECT MIN(""Id"") FROM ""DataPlanes"")
                WHERE ""DataPlaneId"" IS NULL;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "DataPlaneId",
                table: "NetworkServices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NetworkServices_DataPlaneId",
                table: "NetworkServices",
                column: "DataPlaneId");

            migrationBuilder.AddForeignKey(
                name: "FK_NetworkServices_DataPlanes_DataPlaneId",
                table: "NetworkServices",
                column: "DataPlaneId",
                principalTable: "DataPlanes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NetworkServices_DataPlanes_DataPlaneId",
                table: "NetworkServices");

            migrationBuilder.DropIndex(
                name: "IX_NetworkServices_DataPlaneId",
                table: "NetworkServices");

            migrationBuilder.DropColumn(
                name: "DataPlaneId",
                table: "NetworkServices");
        }
    }
}
