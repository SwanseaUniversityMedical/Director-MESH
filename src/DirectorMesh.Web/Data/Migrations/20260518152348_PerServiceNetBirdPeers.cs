using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectorMesh.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class PerServiceNetBirdPeers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "NetBirdDnsName",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "NetBirdPeerId",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "NetBirdPeerName",
                table: "Sites");

            migrationBuilder.AddColumn<string>(
                name: "NetBirdIpAddress",
                table: "NetworkServices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NetBirdPeerId",
                table: "NetworkServices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NetBirdPeerName",
                table: "NetworkServices",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NetBirdIpAddress",
                table: "NetworkServices");

            migrationBuilder.DropColumn(
                name: "NetBirdPeerId",
                table: "NetworkServices");

            migrationBuilder.DropColumn(
                name: "NetBirdPeerName",
                table: "NetworkServices");

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Sites",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NetBirdDnsName",
                table: "Sites",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NetBirdPeerId",
                table: "Sites",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NetBirdPeerName",
                table: "Sites",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
