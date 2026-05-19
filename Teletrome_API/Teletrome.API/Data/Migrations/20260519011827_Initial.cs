using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Teletrome.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    api_key = table.Column<string>(type: "char(64)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "builds",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    project_id = table.Column<int>(type: "int", nullable: false),
                    version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_builds", x => x.id);
                    table.ForeignKey(
                        name: "FK_builds_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "installs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    project_id = table.Column<int>(type: "int", nullable: false),
                    install_id = table.Column<string>(type: "char(36)", nullable: false),
                    first_seen_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    last_seen_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_installs", x => x.id);
                    table.ForeignKey(
                        name: "FK_installs_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "function_registry",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    build_id = table.Column<int>(type: "int", nullable: false),
                    function_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    first_seen_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_function_registry", x => x.id);
                    table.ForeignKey(
                        name: "FK_function_registry_builds_build_id",
                        column: x => x.build_id,
                        principalTable: "builds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    function_registry_id = table.Column<int>(type: "int", nullable: false),
                    install_id = table.Column<int>(type: "int", nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_events_function_registry_function_registry_id",
                        column: x => x.function_registry_id,
                        principalTable: "function_registry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_events_installs_install_id",
                        column: x => x.install_id,
                        principalTable: "installs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_builds_project_id",
                table: "builds",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "uq_build",
                table: "builds",
                columns: new[] { "project_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_events_function_registry_id",
                table: "events",
                column: "function_registry_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_install_id",
                table: "events",
                column: "install_id");

            migrationBuilder.CreateIndex(
                name: "ix_function_registry_build_id",
                table: "function_registry",
                column: "build_id");

            migrationBuilder.CreateIndex(
                name: "uq_function",
                table: "function_registry",
                columns: new[] { "build_id", "function_name", "file_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_installs_project_id",
                table: "installs",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "uq_install",
                table: "installs",
                columns: new[] { "project_id", "install_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_projects_api_key",
                table: "projects",
                column: "api_key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "function_registry");

            migrationBuilder.DropTable(
                name: "installs");

            migrationBuilder.DropTable(
                name: "builds");

            migrationBuilder.DropTable(
                name: "projects");
        }
    }
}
