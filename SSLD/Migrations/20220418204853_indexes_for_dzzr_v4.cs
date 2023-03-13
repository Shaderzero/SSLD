using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSLD.Migrations
{
    public partial class indexes_for_dzzr_v4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_operator_resources_operator_gis_id",
                table: "operator_resources",
                column: "operator_gis_id");

            migrationBuilder.CreateIndex(
                name: "ix_operator_resources_report_date",
                table: "operator_resources",
                column: "report_date");

            migrationBuilder.CreateIndex(
                name: "ix_operator_resources_supply_date",
                table: "operator_resources",
                column: "supply_date");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_operator_resources_operator_gis_id",
                table: "operator_resources");

            migrationBuilder.DropIndex(
                name: "ix_operator_resources_report_date",
                table: "operator_resources");

            migrationBuilder.DropIndex(
                name: "ix_operator_resources_supply_date",
                table: "operator_resources");
        }
    }
}
