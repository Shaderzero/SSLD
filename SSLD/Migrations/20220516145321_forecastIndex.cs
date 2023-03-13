using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSLD.Migrations
{
    public partial class forecastIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_forecast_gis_countries_forecast_id",
                table: "forecast_gis_countries");

            migrationBuilder.CreateIndex(
                name: "ix_forecast_gis_countries_forecast_id_gis_country_id_month",
                table: "forecast_gis_countries",
                columns: new[] { "forecast_id", "gis_country_id", "month" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_forecast_gis_countries_forecast_id_gis_country_id_month",
                table: "forecast_gis_countries");

            migrationBuilder.CreateIndex(
                name: "ix_forecast_gis_countries_forecast_id",
                table: "forecast_gis_countries",
                column: "forecast_id");
        }
    }
}
