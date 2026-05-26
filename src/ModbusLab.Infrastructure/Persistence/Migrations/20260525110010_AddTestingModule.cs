using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModbusLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTestingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "test_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "test_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TestProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "test_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TestProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    SlaveAddress = table.Column<int>(type: "integer", nullable: true),
                    RegisterAddress = table.Column<int>(type: "integer", nullable: true),
                    Value = table.Column<int>(type: "integer", nullable: true),
                    MinValue = table.Column<int>(type: "integer", nullable: true),
                    MaxValue = table.Column<int>(type: "integer", nullable: true),
                    DelayMs = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_test_steps_test_profiles_TestProfileId",
                        column: x => x.TestProfileId,
                        principalTable: "test_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "test_step_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TestRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    TestStepId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    StepName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StepType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ExpectedValue = table.Column<int>(type: "integer", nullable: true),
                    ActualValue = table.Column<int>(type: "integer", nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_step_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_test_step_results_test_runs_TestRunId",
                        column: x => x.TestRunId,
                        principalTable: "test_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_test_profiles_Name",
                table: "test_profiles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_test_runs_StartedAtUtc",
                table: "test_runs",
                column: "StartedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_test_runs_TestProfileId",
                table: "test_runs",
                column: "TestProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_test_step_results_TestRunId_OrderIndex",
                table: "test_step_results",
                columns: new[] { "TestRunId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_test_steps_TestProfileId_OrderIndex",
                table: "test_steps",
                columns: new[] { "TestProfileId", "OrderIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "test_step_results");

            migrationBuilder.DropTable(
                name: "test_steps");

            migrationBuilder.DropTable(
                name: "test_runs");

            migrationBuilder.DropTable(
                name: "test_profiles");
        }
    }
}
