using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSLD.Migrations
{
    public partial class PageBalanceInit1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_forecast_years_year",
                table: "forecast_years",
                column: "year",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_forecast_years_year",
                table: "forecast_years");
        }
    }
}
