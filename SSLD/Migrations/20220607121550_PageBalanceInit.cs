using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SSLD.Migrations
{
    public partial class PageBalanceInit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_calculated",
                table: "gis_country_addons");

            migrationBuilder.DropColumn(
                name: "is_hidden",
                table: "gis_country_addons");

            migrationBuilder.AddColumn<int>(
                name: "forecast_year_id",
                table: "forecasts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "in_main",
                table: "forecasts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double[]>(
                name: "values",
                table: "forecasts",
                type: "double precision[]",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "forecast_years",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forecast_years", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "forecast_year_china_value",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    forecast_year_id = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false),
                    info = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forecast_year_china_value", x => x.id);
                    table.ForeignKey(
                        name: "fk_forecast_year_china_value_forecast_years_forecast_year_id",
                        column: x => x.forecast_year_id,
                        principalTable: "forecast_years",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "forecast_year_gas_tov_value",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    forecast_year_id = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false),
                    info = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forecast_year_gas_tov_value", x => x.id);
                    table.ForeignKey(
                        name: "fk_forecast_year_gas_tov_value_forecast_years_forecast_year_id",
                        column: x => x.forecast_year_id,
                        principalTable: "forecast_years",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "forecast_year_gps_value",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    forecast_year_id = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false),
                    info = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forecast_year_gps_value", x => x.id);
                    table.ForeignKey(
                        name: "fk_forecast_year_gps_value_forecast_years_forecast_year_id",
                        column: x => x.forecast_year_id,
                        principalTable: "forecast_years",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "forecast_year_phg_value",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    forecast_year_id = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false),
                    info = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forecast_year_phg_value", x => x.id);
                    table.ForeignKey(
                        name: "fk_forecast_year_phg_value_forecast_years_forecast_year_id",
                        column: x => x.forecast_year_id,
                        principalTable: "forecast_years",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "forecast_year_repo_value",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    forecast_year_id = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false),
                    info = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forecast_year_repo_value", x => x.id);
                    table.ForeignKey(
                        name: "fk_forecast_year_repo_value_forecast_years_forecast_year_id",
                        column: x => x.forecast_year_id,
                        principalTable: "forecast_years",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_forecasts_forecast_year_id",
                table: "forecasts",
                column: "forecast_year_id");

            migrationBuilder.CreateIndex(
                name: "ix_forecast_year_china_value_forecast_year_id_month",
                table: "forecast_year_china_value",
                columns: new[] { "forecast_year_id", "month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_forecast_year_gas_tov_value_forecast_year_id_month",
                table: "forecast_year_gas_tov_value",
                columns: new[] { "forecast_year_id", "month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_forecast_year_gps_value_forecast_year_id_month",
                table: "forecast_year_gps_value",
                columns: new[] { "forecast_year_id", "month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_forecast_year_phg_value_forecast_year_id_month",
                table: "forecast_year_phg_value",
                columns: new[] { "forecast_year_id", "month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_forecast_year_repo_value_forecast_year_id_month",
                table: "forecast_year_repo_value",
                columns: new[] { "forecast_year_id", "month" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_forecasts_forecast_years_forecast_year_id",
                table: "forecasts",
                column: "forecast_year_id",
                principalTable: "forecast_years",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_forecasts_forecast_years_forecast_year_id",
                table: "forecasts");

            migrationBuilder.DropTable(
                name: "forecast_year_china_value");

            migrationBuilder.DropTable(
                name: "forecast_year_gas_tov_value");

            migrationBuilder.DropTable(
                name: "forecast_year_gps_value");

            migrationBuilder.DropTable(
                name: "forecast_year_phg_value");

            migrationBuilder.DropTable(
                name: "forecast_year_repo_value");

            migrationBuilder.DropTable(
                name: "forecast_years");

            migrationBuilder.DropIndex(
                name: "ix_forecasts_forecast_year_id",
                table: "forecasts");

            migrationBuilder.DropColumn(
                name: "forecast_year_id",
                table: "forecasts");

            migrationBuilder.DropColumn(
                name: "in_main",
                table: "forecasts");

            migrationBuilder.DropColumn(
                name: "values",
                table: "forecasts");

            migrationBuilder.AddColumn<bool>(
                name: "is_calculated",
                table: "gis_country_addons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_hidden",
                table: "gis_country_addons",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
