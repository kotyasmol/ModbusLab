using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModbusLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppUserIsEnabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "app_users",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "app_users");
        }
    }
}
