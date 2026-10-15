using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartWarehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoRfidTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Devices",
                columns: new[] { "Id", "Code", "LastSeenAt", "Name", "Status", "Type" },
                values: new object[] { new Guid("70000000-0000-0000-0000-000000000004"), "RFID-OUT-01", null, "Cổng RFID xuất", 2, 1 });

            migrationBuilder.InsertData(
                table: "RfidTags",
                columns: new[] { "Id", "Epc", "LastSeenAt", "ProductId", "Status" },
                values: new object[] { new Guid("80000000-0000-0000-0000-000000000001"), "E2806894A000000000000001", null, new Guid("20000000-0000-0000-0000-000000000001"), 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "RfidTags",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000001"));
        }
    }
}
