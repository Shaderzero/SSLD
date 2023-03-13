using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSLD.Migrations
{
    public partial class indexes_for_daily_review : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_input_files_logs_file_date",
                table: "input_files_logs",
                column: "file_date");

            migrationBuilder.CreateIndex(
                name: "ix_input_files_logs_file_time",
                table: "input_files_logs",
                column: "file_time");

            migrationBuilder.CreateIndex(
                name: "ix_input_files_logs_filename",
                table: "input_files_logs",
                column: "filename");

            migrationBuilder.CreateIndex(
                name: "ix_input_files_logs_input_time",
                table: "input_files_logs",
                column: "input_time");

            migrationBuilder.CreateIndex(
                name: "ix_gis_output_values_report_date",
                table: "gis_output_values",
                column: "report_date");

            migrationBuilder.CreateIndex(
                name: "ix_gis_input_values_report_date",
                table: "gis_input_values",
                column: "report_date");

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_values_report_date",
                table: "gis_country_values",
                column: "report_date");

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_addon_values_report_date",
                table: "gis_country_addon_values",
                column: "report_date");

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_addon_types_start_date",
                table: "gis_country_addon_types",
                column: "start_date");

            migrationBuilder.CreateIndex(
                name: "ix_gis_addon_values_report_date",
                table: "gis_addon_values",
                column: "report_date");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_input_files_logs_file_date",
                table: "input_files_logs");

            migrationBuilder.DropIndex(
                name: "ix_input_files_logs_file_time",
                table: "input_files_logs");

            migrationBuilder.DropIndex(
                name: "ix_input_files_logs_filename",
                table: "input_files_logs");

            migrationBuilder.DropIndex(
                name: "ix_input_files_logs_input_time",
                table: "input_files_logs");

            migrationBuilder.DropIndex(
                name: "ix_gis_output_values_report_date",
                table: "gis_output_values");

            migrationBuilder.DropIndex(
                name: "ix_gis_input_values_report_date",
                table: "gis_input_values");

            migrationBuilder.DropIndex(
                name: "ix_gis_country_values_report_date",
                table: "gis_country_values");

            migrationBuilder.DropIndex(
                name: "ix_gis_country_addon_values_report_date",
                table: "gis_country_addon_values");

            migrationBuilder.DropIndex(
                name: "ix_gis_country_addon_types_start_date",
                table: "gis_country_addon_types");

            migrationBuilder.DropIndex(
                name: "ix_gis_addon_values_report_date",
                table: "gis_addon_values");
        }
    }
}
