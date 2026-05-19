using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DirectorMesh.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataPlanes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    NetBirdNetworkId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataPlanes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    NetBirdPeerName = table.Column<string>(type: "text", nullable: false),
                    NetBirdDnsName = table.Column<string>(type: "text", nullable: true),
                    NetBirdPeerId = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataPlaneId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sites_DataPlanes_DataPlaneId",
                        column: x => x.DataPlaneId,
                        principalTable: "DataPlanes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Protocol = table.Column<int>(type: "integer", nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    LocalEndpoint = table.Column<string>(type: "text", nullable: true),
                    RemoteNetBirdDnsName = table.Column<string>(type: "text", nullable: true),
                    NetBirdDnsName = table.Column<string>(type: "text", nullable: true),
                    NetBirdGroupId = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NetworkServices_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ConfigurationJson = table.Column<string>(type: "text", nullable: true),
                    HaProxyConfig = table.Column<string>(type: "text", nullable: true),
                    K8sManifestYaml = table.Column<string>(type: "text", nullable: true),
                    ApprovalToken = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteConfigurations_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AclRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SourceServiceId = table.Column<int>(type: "integer", nullable: false),
                    DestinationServiceId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DataPlaneId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AclRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AclRules_DataPlanes_DataPlaneId",
                        column: x => x.DataPlaneId,
                        principalTable: "DataPlanes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AclRules_NetworkServices_DestinationServiceId",
                        column: x => x.DestinationServiceId,
                        principalTable: "NetworkServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AclRules_NetworkServices_SourceServiceId",
                        column: x => x.SourceServiceId,
                        principalTable: "NetworkServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AclRules_DataPlaneId",
                table: "AclRules",
                column: "DataPlaneId");

            migrationBuilder.CreateIndex(
                name: "IX_AclRules_DestinationServiceId",
                table: "AclRules",
                column: "DestinationServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_AclRules_SourceServiceId",
                table: "AclRules",
                column: "SourceServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkServices_SiteId",
                table: "NetworkServices",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteConfigurations_SiteId",
                table: "SiteConfigurations",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_DataPlaneId",
                table: "Sites",
                column: "DataPlaneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AclRules");

            migrationBuilder.DropTable(
                name: "SiteConfigurations");

            migrationBuilder.DropTable(
                name: "NetworkServices");

            migrationBuilder.DropTable(
                name: "Sites");

            migrationBuilder.DropTable(
                name: "DataPlanes");
        }
    }
}
