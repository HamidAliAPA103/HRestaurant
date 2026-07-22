using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRestaurant.Migrations
{
    /// <inheritdoc />
    public partial class FixRestaurantForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuCategories_Restaurants_RestaurantID",
                table: "MenuCategories");

            migrationBuilder.AlterColumn<Guid>(
                name: "RestaurantID",
                table: "MenuCategories",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_MenuCategories_ResdaranId",
                table: "MenuCategories",
                column: "ResdaranId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuCategories_Restaurants_ResdaranId",
                table: "MenuCategories",
                column: "ResdaranId",
                principalTable: "Restaurants",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuCategories_Restaurants_RestaurantID",
                table: "MenuCategories",
                column: "RestaurantID",
                principalTable: "Restaurants",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuCategories_Restaurants_ResdaranId",
                table: "MenuCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuCategories_Restaurants_RestaurantID",
                table: "MenuCategories");

            migrationBuilder.DropIndex(
                name: "IX_MenuCategories_ResdaranId",
                table: "MenuCategories");

            migrationBuilder.AlterColumn<Guid>(
                name: "RestaurantID",
                table: "MenuCategories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuCategories_Restaurants_RestaurantID",
                table: "MenuCategories",
                column: "RestaurantID",
                principalTable: "Restaurants",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
