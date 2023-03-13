using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSLD.Migrations
{
    public partial class PageBalanceInit2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_forecasts_forecast_years_forecast_year_id",
                table: "forecasts");

            migrationBuilder.AlterColumn<int>(
                name: "forecast_year_id",
                table: "forecasts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_forecasts_forecast_years_forecast_year_id",
                table: "forecasts",
                column: "forecast_year_id",
                principalTable: "forecast_years",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_forecasts_forecast_years_forecast_year_id",
                table: "forecasts");

            migrationBuilder.AlterColumn<int>(
                name: "forecast_year_id",
                table: "forecasts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "fk_forecasts_forecast_years_forecast_year_id",
                table: "forecasts",
                column: "forecast_year_id",
                principalTable: "forecast_years",
                principalColumn: "id");
        }
    }
}
