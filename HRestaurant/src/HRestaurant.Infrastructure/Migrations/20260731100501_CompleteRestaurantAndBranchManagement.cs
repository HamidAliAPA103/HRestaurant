using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRestaurant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompleteRestaurantAndBranchManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Branches",
                type: "decimal(8,6)",
                precision: 8,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Branches",
                type: "decimal(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "Branches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Branches",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE [Branches] "
                + "SET [NormalizedName] = UPPER(LTRIM(RTRIM([Name]))) "
                + "WHERE [NormalizedName] IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedName",
                table: "Branches",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_IsDeleted_CreatAt",
                table: "Restaurants",
                columns: new[] { "IsDeleted", "CreatAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_IsDeleted_IsActive_Name",
                table: "Restaurants",
                columns: new[] { "IsDeleted", "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ManagerId_IsDeleted",
                table: "Branches",
                columns: new[] { "ManagerId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_RestaurantId_IsDeleted_IsActive",
                table: "Branches",
                columns: new[] { "RestaurantId", "IsDeleted", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_RestaurantId_NormalizedName",
                table: "Branches",
                columns: new[] { "RestaurantId", "NormalizedName" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_AspNetUsers_ManagerId",
                table: "Branches",
                column: "ManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_AspNetUsers_ManagerId",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_IsDeleted_CreatAt",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_IsDeleted_IsActive_Name",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Branches_ManagerId_IsDeleted",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Branches_RestaurantId_IsDeleted_IsActive",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Branches_RestaurantId_NormalizedName",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Branches");
        }
    }
}
