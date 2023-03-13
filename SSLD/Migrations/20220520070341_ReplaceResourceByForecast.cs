using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SSLD.Migrations
{
    public partial class ReplaceResourceByForecast : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gis_country_resources");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gis_country_resources",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gis_country_id = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<DateOnly>(type: "date", nullable: false),
                    value = table.Column<decimal>(type: "numeric(16,8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gis_country_resources", x => x.id);
                    table.ForeignKey(
                        name: "fk_gis_country_resources_gis_countries_gis_country_id",
                        column: x => x.gis_country_id,
                        principalTable: "gis_countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_resources_gis_country_id_month",
                table: "gis_country_resources",
                columns: new[] { "gis_country_id", "month" },
                unique: true);
        }
    }
}
