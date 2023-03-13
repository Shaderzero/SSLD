using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSLD.Migrations
{
    public partial class add_operator_resource_log : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_operator_resource_gises_gis_id",
                table: "operator_resource");

            migrationBuilder.DropForeignKey(
                name: "fk_operator_resource_hour_operator_resource_operator_resource_",
                table: "operator_resource_hour");

            migrationBuilder.DropPrimaryKey(
                name: "pk_operator_resource_hour",
                table: "operator_resource_hour");

            migrationBuilder.DropPrimaryKey(
                name: "pk_operator_resource",
                table: "operator_resource");

            migrationBuilder.RenameTable(
                name: "operator_resource_hour",
                newName: "operator_resource_hours");

            migrationBuilder.RenameTable(
                name: "operator_resource",
                newName: "operator_resources");

            migrationBuilder.RenameIndex(
                name: "ix_operator_resource_hour_operator_resource_id",
                table: "operator_resource_hours",
                newName: "ix_operator_resource_hours_operator_resource_id");

            migrationBuilder.RenameIndex(
                name: "ix_operator_resource_gis_id",
                table: "operator_resources",
                newName: "ix_operator_resources_gis_id");

            migrationBuilder.AddColumn<long>(
                name: "log_id",
                table: "operator_resources",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "pk_operator_resource_hours",
                table: "operator_resource_hours",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_operator_resources",
                table: "operator_resources",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_operator_resources_log_id",
                table: "operator_resources",
                column: "log_id");

            migrationBuilder.AddForeignKey(
                name: "fk_operator_resource_hours_operator_resources_operator_resourc",
                table: "operator_resource_hours",
                column: "operator_resource_id",
                principalTable: "operator_resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_operator_resources_gises_gis_id",
                table: "operator_resources",
                column: "gis_id",
                principalTable: "gises",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_operator_resources_input_files_logs_log_id",
                table: "operator_resources",
                column: "log_id",
                principalTable: "input_files_logs",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_operator_resource_hours_operator_resources_operator_resourc",
                table: "operator_resource_hours");

            migrationBuilder.DropForeignKey(
                name: "fk_operator_resources_gises_gis_id",
                table: "operator_resources");

            migrationBuilder.DropForeignKey(
                name: "fk_operator_resources_input_files_logs_log_id",
                table: "operator_resources");

            migrationBuilder.DropPrimaryKey(
                name: "pk_operator_resources",
                table: "operator_resources");

            migrationBuilder.DropIndex(
                name: "ix_operator_resources_log_id",
                table: "operator_resources");

            migrationBuilder.DropPrimaryKey(
                name: "pk_operator_resource_hours",
                table: "operator_resource_hours");

            migrationBuilder.DropColumn(
                name: "log_id",
                table: "operator_resources");

            migrationBuilder.RenameTable(
                name: "operator_resources",
                newName: "operator_resource");

            migrationBuilder.RenameTable(
                name: "operator_resource_hours",
                newName: "operator_resource_hour");

            migrationBuilder.RenameIndex(
                name: "ix_operator_resources_gis_id",
                table: "operator_resource",
                newName: "ix_operator_resource_gis_id");

            migrationBuilder.RenameIndex(
                name: "ix_operator_resource_hours_operator_resource_id",
                table: "operator_resource_hour",
                newName: "ix_operator_resource_hour_operator_resource_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_operator_resource",
                table: "operator_resource",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_operator_resource_hour",
                table: "operator_resource_hour",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_operator_resource_gises_gis_id",
                table: "operator_resource",
                column: "gis_id",
                principalTable: "gises",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_operator_resource_hour_operator_resource_operator_resource_",
                table: "operator_resource_hour",
                column: "operator_resource_id",
                principalTable: "operator_resource",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
