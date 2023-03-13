using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SSLD.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    daily_review_name = table.Column<string>(type: "text", nullable: true),
                    name_en = table.Column<string>(type: "text", nullable: true),
                    short_name = table.Column<string>(type: "text", nullable: true),
                    names = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_countries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "file_type_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    type_name = table.Column<string>(type: "text", nullable: true),
                    must_have = table.Column<List<string>>(type: "text[]", nullable: true),
                    not_have = table.Column<List<string>>(type: "text[]", nullable: true),
                    country_entry = table.Column<List<string>>(type: "text[]", nullable: true),
                    gis_entry = table.Column<List<string>>(type: "text[]", nullable: true),
                    requested_value_entry = table.Column<List<string>>(type: "text[]", nullable: true),
                    allocated_value_entry = table.Column<List<string>>(type: "text[]", nullable: true),
                    estimated_value_entry = table.Column<List<string>>(type: "text[]", nullable: true),
                    fact_value_entry = table.Column<List<string>>(type: "text[]", nullable: true),
                    data_entry = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_type_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "gises",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    daily_review_name = table.Column<string>(type: "text", nullable: true),
                    daily_review_order = table.Column<int>(type: "integer", nullable: false),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false),
                    is_not_calculated = table.Column<bool>(type: "boolean", nullable: false),
                    is_ukraine_transport = table.Column<bool>(type: "boolean", nullable: false),
                    is_top = table.Column<bool>(type: "boolean", nullable: false),
                    is_bottom = table.Column<bool>(type: "boolean", nullable: false),
                    is_one_row = table.Column<bool>(type: "boolean", nullable: false),
                    is_no_phg = table.Column<bool>(type: "boolean", nullable: false),
                    names = table.Column<List<string>>(type: "text[]", nullable: true),
                    gis_input_names = table.Column<List<string>>(type: "text[]", nullable: true),
                    gis_output_names = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gises", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "AspNetRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_user_claims_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_asp_net_user_logins_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "AspNetRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "input_files_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    filename = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    input_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    file_date = table.Column<DateOnly>(type: "date", nullable: false),
                    file_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_input_files_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_input_files_logs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "gis_addons",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gis_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    daily_review_name = table.Column<string>(type: "text", nullable: true),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false),
                    is_input = table.Column<bool>(type: "boolean", nullable: false),
                    is_output = table.Column<bool>(type: "boolean", nullable: false),
                    names = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gis_addons", x => x.id);
                    table.ForeignKey(
                        name: "fk_gis_addons_gises_gis_id",
                        column: x => x.gis_id,
                        principalTable: "gises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gis_countries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    country_id = table.Column<int>(type: "integer", nullable: false),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false),
                    is_not_calculated = table.Column<bool>(type: "boolean", nullable: false),
                    gis_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gis_countries", x => x.id);
                    table.ForeignKey(
                        name: "fk_gis_countries_countries_country_id",
                        column: x => x.country_id,
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_gis_countries_gises_gis_id",
                        column: x => x.gis_id,
                        principalTable: "gises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gis_input_values",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gis_id = table.Column<int>(type: "integer", nullable: false),
                    report_date = table.Column<DateOnly>(type: "date", nullable: false),
                    requested_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    requested_value_time_id = table.Column<long>(type: "bigint", nullable: true),
                    allocated_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    allocated_value_time_id = table.Column<long>(type: "bigint", nullable: true),
                    estimated_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    estimated_value_time_id = table.Column<long>(type: "bigint", nullable: true),
                    fact_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    fact_value_time_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gis_input_values", x => x.id);
                    table.ForeignKey(
                        name: "fk_gis_input_values_gises_gis_id",
                        column: x => x.gis_id,
                        principalTable: "gises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_gis_input_values_input_files_logs_allocated_value_time_id",
                        column: x => x.allocated_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_gis_input_values_input_files_logs_estimated_value_time_id",
                        column: x => x.estimated_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_gis_input_values_input_files_logs_fact_value_time_id",
                        column: x => x.fact_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_gis_input_values_input_files_logs_requested_value_time_id",
                        column: x => x.requested_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "gis_output_values",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gis_id = table.Column<int>(type: "integer", nullable: false),
                    report_date = table.Column<DateOnly>(type: "date", nullable: false),
                    requested_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    requested_value_time_id = table.Column<long>(type: "bigint", nullable: true),
                    allocated_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    allocated_value_time_id = table.Column<long>(type: "bigint", nullable: true),
                    estimated_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    estimated_value_time_id = table.Column<long>(type: "bigint", nullable: true),
                    fact_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    fact_value_time_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gis_output_values", x => x.id);
                    table.ForeignKey(
                        name: "fk_gis_output_values_gises_gis_id",
                        column: x => x.gis_id,
                        principalTable: "gises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_gis_output_values_input_files_logs_allocated_value_time_id",
                        column: x => x.allocated_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_gis_output_values_input_files_logs_estimated_value_time_id",
                        column: x => x.estimated_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_gis_output_values_input_files_logs_fact_value_time_id",
                        column: x => x.fact_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_gis_output_values_input_files_logs_requested_value_time_id",
                        column: x => x.requested_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "gis_addon_values",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gis_addon_id = table.Column<int>(type: "integer", nullable: false),
                    report_date = table.Column<DateOnly>(type: "date", nullable: false),
                    requested_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    requested_value_time_id = table.Column<long>(type: "bigint", nullable: true),
                    allocated_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    allocated_value_time_id = table.Column<long>(type: "bigint", nullable: true),
                    estimated_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    estimated_value_time_id = table.Column<long>(type: "bigint", nullable: true),
                    fact_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    fact_value_time_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gis_addon_values", x => x.id);
                    table.ForeignKey(
                        name: "fk_gis_addon_values_gis_addons_gis_addon_id",
                        column: x => x.gis_addon_id,
                        principalTable: "gis_addons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_gis_addon_values_input_files_logs_allocated_value_time_id",
                        column: x => x.allocated_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_gis_addon_values_input_files_logs_estimated_value_time_id",
                        column: x => x.estimated_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_gis_addon_values_input_files_logs_fact_value_time_id",
                        column: x => x.fact_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_gis_addon_values_input_files_logs_requested_value_time_id",
                        column: x => x.requested_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "gis_country_addons",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gis_country_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    daily_review_name = table.Column<string>(type: "text", nullable: true),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false),
                    is_calculated = table.Column<bool>(type: "boolean", nullable: false),
                    names = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gis_country_addons", x => x.id);
                    table.ForeignKey(
                        name: "fk_gis_country_addons_gis_countries_gis_country_id",
                        column: x => x.gis_country_id,
                        principalTable: "gis_countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gis_country_resources",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gis_country_id = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<DateOnly>(type: "date", nullable: false),
                    value = table.Column<decimal>(type: "numeric(16,8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gis_country_resources", x => x.id);
                    table.ForeignKey(
                        name: "fk_gis_country_resources_gis_countries_gis_country_id",
                        column: x => x.gis_country_id,
                        principalTable: "gis_countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gis_country_values",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gis_country_id = table.Column<int>(type: "integer", nullable: false),
                    report_date = table.Column<DateOnly>(type: "date", nullable: false),
                    requested_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    requested_value_time_id = table.Column<long>(type: "bigint", nullable: true),
                    allocated_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    allocated_value_time_id = table.Column<long>(type: "bigint", nullable: true),
                    estimated_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    estimated_value_time_id = table.Column<long>(type: "bigint", nullable: true),
                    fact_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    fact_value_time_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gis_country_values", x => x.id);
                    table.ForeignKey(
                        name: "fk_gis_country_values_gis_countries_gis_country_id",
                        column: x => x.gis_country_id,
                        principalTable: "gis_countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_gis_country_values_input_files_logs_allocated_value_time_id",
                        column: x => x.allocated_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_gis_country_values_input_files_logs_estimated_value_time_id",
                        column: x => x.estimated_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_gis_country_values_input_files_logs_fact_value_time_id",
                        column: x => x.fact_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_gis_country_values_input_files_logs_requested_value_time_id",
                        column: x => x.requested_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "gis_country_addon_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gis_country_addon_id = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    is_comm_gas = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gis_country_addon_types", x => x.id);
                    table.ForeignKey(
                        name: "fk_gis_country_addon_types_gis_country_addons_gis_country_addo",
                        column: x => x.gis_country_addon_id,
                        principalTable: "gis_country_addons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gis_country_addon_values",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gis_country_addon_id = table.Column<int>(type: "integer", nullable: false),
                    report_date = table.Column<DateOnly>(type: "date", nullable: false),
                    requested_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    requested_value_time_id = table.Column<long>(type: "bigint", nullable: true),
                    allocated_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    allocated_value_time_id = table.Column<long>(type: "bigint", nullable: true),
                    estimated_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    estimated_value_time_id = table.Column<long>(type: "bigint", nullable: true),
                    fact_value = table.Column<decimal>(type: "numeric(16,8)", nullable: false),
                    fact_value_time_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gis_country_addon_values", x => x.id);
                    table.ForeignKey(
                        name: "fk_gis_country_addon_values_gis_country_addons_gis_country_add",
                        column: x => x.gis_country_addon_id,
                        principalTable: "gis_country_addons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_gis_country_addon_values_input_files_logs_allocated_value_t",
                        column: x => x.allocated_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_gis_country_addon_values_input_files_logs_estimated_value_t",
                        column: x => x.estimated_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_gis_country_addon_values_input_files_logs_fact_value_time_id",
                        column: x => x.fact_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_gis_country_addon_values_input_files_logs_requested_value_t",
                        column: x => x.requested_value_time_id,
                        principalTable: "input_files_logs",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_role_claims_role_id",
                table: "AspNetRoleClaims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_claims_user_id",
                table: "AspNetUserClaims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_logins_user_id",
                table: "AspNetUserLogins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_roles_role_id",
                table: "AspNetUserRoles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_file_type_settings_name",
                table: "file_type_settings",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_gis_addon_values_allocated_value_time_id",
                table: "gis_addon_values",
                column: "allocated_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_addon_values_estimated_value_time_id",
                table: "gis_addon_values",
                column: "estimated_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_addon_values_fact_value_time_id",
                table: "gis_addon_values",
                column: "fact_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_addon_values_gis_addon_id_report_date",
                table: "gis_addon_values",
                columns: new[] { "gis_addon_id", "report_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_gis_addon_values_requested_value_time_id",
                table: "gis_addon_values",
                column: "requested_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_addons_gis_id",
                table: "gis_addons",
                column: "gis_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_countries_country_id",
                table: "gis_countries",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_countries_gis_id_country_id",
                table: "gis_countries",
                columns: new[] { "gis_id", "country_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_addon_types_gis_country_addon_id",
                table: "gis_country_addon_types",
                column: "gis_country_addon_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_addon_values_allocated_value_time_id",
                table: "gis_country_addon_values",
                column: "allocated_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_addon_values_estimated_value_time_id",
                table: "gis_country_addon_values",
                column: "estimated_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_addon_values_fact_value_time_id",
                table: "gis_country_addon_values",
                column: "fact_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_addon_values_gis_country_addon_id",
                table: "gis_country_addon_values",
                column: "gis_country_addon_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_addon_values_requested_value_time_id",
                table: "gis_country_addon_values",
                column: "requested_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_addons_gis_country_id",
                table: "gis_country_addons",
                column: "gis_country_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_resources_gis_country_id_month",
                table: "gis_country_resources",
                columns: new[] { "gis_country_id", "month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_values_allocated_value_time_id",
                table: "gis_country_values",
                column: "allocated_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_values_estimated_value_time_id",
                table: "gis_country_values",
                column: "estimated_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_values_fact_value_time_id",
                table: "gis_country_values",
                column: "fact_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_values_gis_country_id_report_date",
                table: "gis_country_values",
                columns: new[] { "gis_country_id", "report_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_gis_country_values_requested_value_time_id",
                table: "gis_country_values",
                column: "requested_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_input_values_allocated_value_time_id",
                table: "gis_input_values",
                column: "allocated_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_input_values_estimated_value_time_id",
                table: "gis_input_values",
                column: "estimated_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_input_values_fact_value_time_id",
                table: "gis_input_values",
                column: "fact_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_input_values_gis_id_report_date",
                table: "gis_input_values",
                columns: new[] { "gis_id", "report_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_gis_input_values_requested_value_time_id",
                table: "gis_input_values",
                column: "requested_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_output_values_allocated_value_time_id",
                table: "gis_output_values",
                column: "allocated_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_output_values_estimated_value_time_id",
                table: "gis_output_values",
                column: "estimated_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_output_values_fact_value_time_id",
                table: "gis_output_values",
                column: "fact_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_gis_output_values_gis_id_report_date",
                table: "gis_output_values",
                columns: new[] { "gis_id", "report_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_gis_output_values_requested_value_time_id",
                table: "gis_output_values",
                column: "requested_value_time_id");

            migrationBuilder.CreateIndex(
                name: "ix_input_files_logs_user_id",
                table: "input_files_logs",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "file_type_settings");

            migrationBuilder.DropTable(
                name: "gis_addon_values");

            migrationBuilder.DropTable(
                name: "gis_country_addon_types");

            migrationBuilder.DropTable(
                name: "gis_country_addon_values");

            migrationBuilder.DropTable(
                name: "gis_country_resources");

            migrationBuilder.DropTable(
                name: "gis_country_values");

            migrationBuilder.DropTable(
                name: "gis_input_values");

            migrationBuilder.DropTable(
                name: "gis_output_values");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "gis_addons");

            migrationBuilder.DropTable(
                name: "gis_country_addons");

            migrationBuilder.DropTable(
                name: "input_files_logs");

            migrationBuilder.DropTable(
                name: "gis_countries");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "countries");

            migrationBuilder.DropTable(
                name: "gises");
        }
    }
}
