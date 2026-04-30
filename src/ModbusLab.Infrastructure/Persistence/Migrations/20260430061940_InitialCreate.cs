using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModbusLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "device_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "modbus_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SlaveDeviceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SlaveAddress = table.Column<int>(type: "integer", nullable: false),
                    FunctionCode = table.Column<int>(type: "integer", nullable: false),
                    RegisterAddress = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modbus_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "register_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AccessMode = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    MinValue = table.Column<int>(type: "integer", nullable: true),
                    MaxValue = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_register_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "register_values",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SlaveDeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegisterDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_register_values", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "slave_devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SlaveAddress = table.Column<int>(type: "integer", nullable: false),
                    DeviceTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_slave_devices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_modbus_logs_SlaveAddress",
                table: "modbus_logs",
                column: "SlaveAddress");

            migrationBuilder.CreateIndex(
                name: "IX_modbus_logs_TimestampUtc",
                table: "modbus_logs",
                column: "TimestampUtc");

            migrationBuilder.CreateIndex(
                name: "IX_register_definitions_DeviceTypeId_Address",
                table: "register_definitions",
                columns: new[] { "DeviceTypeId", "Address" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_register_values_SlaveDeviceId_RegisterDefinitionId",
                table: "register_values",
                columns: new[] { "SlaveDeviceId", "RegisterDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_slave_devices_SlaveAddress",
                table: "slave_devices",
                column: "SlaveAddress",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_types");

            migrationBuilder.DropTable(
                name: "modbus_logs");

            migrationBuilder.DropTable(
                name: "register_definitions");

            migrationBuilder.DropTable(
                name: "register_values");

            migrationBuilder.DropTable(
                name: "slave_devices");
        }
    }
}
