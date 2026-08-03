using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRestaurant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuItem3DExperience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Is3DEnabled",
                table: "Menus",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Model3DUrl",
                table: "Menus",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModelPosterUrl",
                table: "Menus",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ModelRotationX",
                table: "Menus",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ModelRotationY",
                table: "Menus",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ModelRotationZ",
                table: "Menus",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ModelScale",
                table: "Menus",
                type: "decimal(8,4)",
                precision: 8,
                scale: 4,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "MenuItemIngredients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ExplodedPositionX",
                table: "MenuItemIngredients",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExplodedPositionY",
                table: "MenuItemIngredients",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExplodedPositionZ",
                table: "MenuItemIngredients",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExplodedRotationX",
                table: "MenuItemIngredients",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExplodedRotationY",
                table: "MenuItemIngredients",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExplodedRotationZ",
                table: "MenuItemIngredients",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisibleIn3D",
                table: "MenuItemIngredients",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "AllergenInformation",
                table: "Ingredients",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Calories",
                table: "Ingredients",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Carbohydrates",
                table: "Ingredients",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Ingredients",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Fat",
                table: "Ingredients",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Ingredients",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model3DUrl",
                table: "Ingredients",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Origin",
                table: "Ingredients",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Protein",
                table: "Ingredients",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Is3DEnabled",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "Model3DUrl",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "ModelPosterUrl",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "ModelRotationX",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "ModelRotationY",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "ModelRotationZ",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "ModelScale",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "MenuItemIngredients");

            migrationBuilder.DropColumn(
                name: "ExplodedPositionX",
                table: "MenuItemIngredients");

            migrationBuilder.DropColumn(
                name: "ExplodedPositionY",
                table: "MenuItemIngredients");

            migrationBuilder.DropColumn(
                name: "ExplodedPositionZ",
                table: "MenuItemIngredients");

            migrationBuilder.DropColumn(
                name: "ExplodedRotationX",
                table: "MenuItemIngredients");

            migrationBuilder.DropColumn(
                name: "ExplodedRotationY",
                table: "MenuItemIngredients");

            migrationBuilder.DropColumn(
                name: "ExplodedRotationZ",
                table: "MenuItemIngredients");

            migrationBuilder.DropColumn(
                name: "IsVisibleIn3D",
                table: "MenuItemIngredients");

            migrationBuilder.DropColumn(
                name: "AllergenInformation",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Calories",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Carbohydrates",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Fat",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Model3DUrl",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Origin",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Protein",
                table: "Ingredients");
        }
    }
}
