using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SSLD.Migrations
{
    public partial class PageBalanceInit3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropColumn(
                name: "year",
                table: "forecasts");

            migrationBuilder.AddColumn<string[]>(
                name: "china_txts",
                table: "forecast_years",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<double[]>(
                name: "china_values",
                table: "forecast_years",
                type: "double precision[]",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "gas_phg_txts",
                table: "forecast_years",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<double[]>(
                name: "gas_phg_values",
                table: "forecast_years",
                type: "double precision[]",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "gas_tov_txts",
                table: "forecast_years",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<double[]>(
                name: "gas_tov_values",
                table: "forecast_years",
                type: "double precision[]",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "gps_txts",
                table: "forecast_years",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<double[]>(
                name: "gps_values",
                table: "forecast_years",
                type: "double precision[]",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "repo_txts",
                table: "forecast_years",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<double[]>(
                name: "repo_values",
                table: "forecast_years",
                type: "double precision[]",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "china_txts",
                table: "forecast_years");

            migrationBuilder.DropColumn(
                name: "china_values",
                table: "forecast_years");

            migrationBuilder.DropColumn(
                name: "gas_phg_txts",
                table: "forecast_years");

            migrationBuilder.DropColumn(
                name: "gas_phg_values",
                table: "forecast_years");

            migrationBuilder.DropColumn(
                name: "gas_tov_txts",
                table: "forecast_years");

            migrationBuilder.DropColumn(
                name: "gas_tov_values",
                table: "forecast_years");

            migrationBuilder.DropColumn(
                name: "gps_txts",
                table: "forecast_years");

            migrationBuilder.DropColumn(
                name: "gps_values",
                table: "forecast_years");

            migrationBuilder.DropColumn(
                name: "repo_txts",
                table: "forecast_years");

            migrationBuilder.DropColumn(
                name: "repo_values",
                table: "forecast_years");

            migrationBuilder.AddColumn<int>(
                name: "year",
                table: "forecasts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "forecast_year_china_value",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    forecast_year_id = table.Column<int>(type: "integer", nullable: false),
                    info = table.Column<string>(type: "text", nullable: true),
                    month = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false)
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
                    info = table.Column<string>(type: "text", nullable: true),
                    month = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false)
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
                    info = table.Column<string>(type: "text", nullable: true),
                    month = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false)
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
                    info = table.Column<string>(type: "text", nullable: true),
                    month = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false)
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
                    info = table.Column<string>(type: "text", nullable: true),
                    month = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false)
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
        }
    }
}
