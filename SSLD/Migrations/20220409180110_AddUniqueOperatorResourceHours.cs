using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSLD.Migrations
{
    public partial class AddUniqueOperatorResourceHours : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_operator_resource_hours_operator_resource_id",
                table: "operator_resource_hours");

            migrationBuilder.CreateIndex(
                name: "ix_operator_resource_hours_operator_resource_id_hour",
                table: "operator_resource_hours",
                columns: new[] { "operator_resource_id", "hour" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_operator_resource_hours_operator_resource_id_hour",
                table: "operator_resource_hours");

            migrationBuilder.CreateIndex(
                name: "ix_operator_resource_hours_operator_resource_id",
                table: "operator_resource_hours",
                column: "operator_resource_id");
        }
    }
}
