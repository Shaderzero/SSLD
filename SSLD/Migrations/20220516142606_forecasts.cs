using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SSLD.Migrations
{
    public partial class forecasts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "forecasts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    fullname = table.Column<string>(type: "text", nullable: true),
                    report_date = table.Column<DateOnly>(type: "date", nullable: false),
                    input_file_log_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forecasts", x => x.id);
                    table.ForeignKey(
                        name: "fk_forecasts_input_files_logs_input_file_log_id",
                        column: x => x.input_file_log_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "forecast_gis_countries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    forecast_id = table.Column<int>(type: "integer", nullable: false),
                    gis_country_id = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<decimal>(type: "numeric(16,8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forecast_gis_countries", x => x.id);
                    table.ForeignKey(
                        name: "fk_forecast_gis_countries_forecasts_forecast_id",
                        column: x => x.forecast_id,
                        principalTable: "forecasts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_forecast_gis_countries_gis_countries_gis_country_id",
                        column: x => x.gis_country_id,
                        principalTable: "gis_countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_forecast_gis_countries_forecast_id",
                table: "forecast_gis_countries",
                column: "forecast_id");

            migrationBuilder.CreateIndex(
                name: "ix_forecast_gis_countries_gis_country_id",
                table: "forecast_gis_countries",
                column: "gis_country_id");

            migrationBuilder.CreateIndex(
                name: "ix_forecasts_input_file_log_id",
                table: "forecasts",
                column: "input_file_log_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "forecast_gis_countries");

            migrationBuilder.DropTable(
                name: "forecasts");
        }
    }
}
