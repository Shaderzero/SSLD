using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSLD.Migrations
{
    public partial class removeLogToDzZr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_operator_resources_input_files_logs_input_file_log_id",
                table: "operator_resources");

            migrationBuilder.DropIndex(
                name: "ix_operator_resources_input_file_log_id",
                table: "operator_resources");

            migrationBuilder.DropColumn(
                name: "input_file_log_id",
                table: "operator_resources");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "input_file_log_id",
                table: "operator_resources",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_operator_resources_input_file_log_id",
                table: "operator_resources",
                column: "input_file_log_id");

            migrationBuilder.AddForeignKey(
                name: "fk_operator_resources_input_files_logs_input_file_log_id",
                table: "operator_resources",
                column: "input_file_log_id",
                principalTable: "input_files_logs",
                principalColumn: "id");
        }
    }
}
