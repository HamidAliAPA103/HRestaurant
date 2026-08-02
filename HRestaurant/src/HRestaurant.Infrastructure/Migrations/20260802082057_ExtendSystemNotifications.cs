using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRestaurant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendSystemNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryNotifications_InventoryItemId_Type",
                table: "InventoryNotifications");

            migrationBuilder.AlterColumn<Guid>(
                name: "InventoryItemId",
                table: "InventoryNotifications",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedEntityId",
                table: "InventoryNotifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetUrl",
                table: "InventoryNotifications",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryNotifications_InventoryItemId_Type",
                table: "InventoryNotifications",
                columns: new[] { "InventoryItemId", "Type" },
                unique: true,
                filter: "[InventoryItemId] IS NOT NULL AND [IsDeleted] = 0 AND [IsRead] = 0 AND [IsResolved] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryNotifications_InventoryItemId_Type",
                table: "InventoryNotifications");

            migrationBuilder.DropColumn(
                name: "RelatedEntityId",
                table: "InventoryNotifications");

            migrationBuilder.DropColumn(
                name: "TargetUrl",
                table: "InventoryNotifications");

            migrationBuilder.AlterColumn<Guid>(
                name: "InventoryItemId",
                table: "InventoryNotifications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryNotifications_InventoryItemId_Type",
                table: "InventoryNotifications",
                columns: new[] { "InventoryItemId", "Type" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [IsRead] = 0 AND [IsResolved] = 0");
        }
    }
}
