using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SSLD.Migrations
{
    public partial class reload : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "operator_resource",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gis_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    supply_date = table.Column<DateOnly>(type: "date", nullable: false),
                    report_date = table.Column<DateOnly>(type: "date", nullable: false),
                    time_only = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operator_resource", x => x.id);
                    table.ForeignKey(
                        name: "fk_operator_resource_gises_gis_id",
                        column: x => x.gis_id,
                        principalTable: "gises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "operator_resource_hour",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operator_resource_id = table.Column<long>(type: "bigint", nullable: false),
                    hour = table.Column<int>(type: "integer", nullable: false),
                    volume = table.Column<decimal>(type: "numeric(16,8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operator_resource_hour", x => x.id);
                    table.ForeignKey(
                        name: "fk_operator_resource_hour_operator_resource_operator_resource_",
                        column: x => x.operator_resource_id,
                        principalTable: "operator_resource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_operator_resource_gis_id",
                table: "operator_resource",
                column: "gis_id");

            migrationBuilder.CreateIndex(
                name: "ix_operator_resource_hour_operator_resource_id",
                table: "operator_resource_hour",
                column: "operator_resource_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "operator_resource_hour");

            migrationBuilder.DropTable(
                name: "operator_resource");
        }
    }
}
