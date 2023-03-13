using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SSLD.Migrations
{
    public partial class operator_gis : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_operator_resources_gises_gis_id",
                table: "operator_resources");

            migrationBuilder.DropForeignKey(
                name: "fk_operator_resource_hours_operator_resources_operator_resourc",
                table: "operator_resource_hours");

            migrationBuilder.DropPrimaryKey(
                name: "pk_operator_resource_hours",
                table: "operator_resource_hours");

            migrationBuilder.DropPrimaryKey(
                name: "pk_operator_resources",
                table: "operator_resources");

            migrationBuilder.DropColumn(
                name: "time_only",
                table: "operator_resources");

            migrationBuilder.RenameTable(
                name: "operator_resource_hours",
                newName: "operator_resource_hours");

            migrationBuilder.RenameTable(
                name: "operator_resources",
                newName: "operator_resources");

            migrationBuilder.RenameIndex(
                name: "ix_operator_resource_hours_operator_resource_id",
                table: "operator_resource_hours",
                newName: "ix_operator_resource_hours_operator_resource_id");

            migrationBuilder.RenameColumn(
                name: "gis_id",
                table: "operator_resources",
                newName: "operator_gis_id");

            migrationBuilder.RenameIndex(
                name: "ix_operator_resources_gis_id",
                table: "operator_resources",
                newName: "ix_operator_resources_operator_gis_id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "report_date",
                table: "operator_resources",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddPrimaryKey(
                name: "pk_operator_resource_hours",
                table: "operator_resource_hours",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_operator_resources",
                table: "operator_resources",
                column: "id");

            migrationBuilder.CreateTable(
                name: "operator_gises",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operator_gises", x => x.id);
                });

            migrationBuilder.AddForeignKey(
                name: "fk_operator_resource_hours_operator_resources_operator_resourc",
                table: "operator_resource_hours",
                column: "operator_resource_id",
                principalTable: "operator_resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_operator_resources_operator_gises_operator_gis_id",
                table: "operator_resources",
                column: "operator_gis_id",
                principalTable: "operator_gises",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_operator_resource_hours_operator_resources_operator_resourc",
                table: "operator_resource_hours");

            migrationBuilder.DropForeignKey(
                name: "fk_operator_resources_operator_gises_operator_gis_id",
                table: "operator_resources");

            migrationBuilder.DropTable(
                name: "operator_gises");

            migrationBuilder.DropPrimaryKey(
                name: "pk_operator_resources",
                table: "operator_resources");

            migrationBuilder.DropPrimaryKey(
                name: "pk_operator_resource_hours",
                table: "operator_resource_hours");

            migrationBuilder.RenameColumn(
                name: "operator_gis_id",
                table: "operator_resource",
                newName: "gis_id");

            migrationBuilder.RenameIndex(
                name: "ix_operator_resources_operator_gis_id",
                table: "operator_resources",
                newName: "ix_operator_resource_gis_id");

            migrationBuilder.RenameIndex(
                name: "ix_operator_resource_hours_operator_resource_id",
                table: "operator_resource_hour",
                newName: "ix_operator_resource_hour_operator_resource_id");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "report_date",
                table: "operator_resource",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "time_only",
                table: "operator_resource",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

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
