using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRestaurant.Infrastructure.Migrations;

public partial class AddPublicVideoAndBranchMedia : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>("IsVideoEnabled", "Menus", "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<int>("VideoDisplayOrder", "Menus", "int", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>("VideoDurationSeconds", "Menus", "int", nullable: true);
        migrationBuilder.AddColumn<string>("VideoPosterUrl", "Menus", "nvarchar(500)", maxLength: 500, nullable: true);
        migrationBuilder.AddColumn<string>("VideoUrl", "Menus", "nvarchar(500)", maxLength: 500, nullable: true);

        migrationBuilder.AddColumn<string>("CoverImageUrl", "Branches", "nvarchar(500)", maxLength: 500, nullable: true);
        migrationBuilder.AddColumn<string>("FrontImageUrl", "Branches", "nvarchar(500)", maxLength: 500, nullable: true);
        migrationBuilder.AddColumn<string>("GoogleMapsUrl", "Branches", "nvarchar(500)", maxLength: 500, nullable: true);
        migrationBuilder.AddColumn<bool>("IsPubliclyVisible", "Branches", "bit", nullable: false, defaultValue: true);
        migrationBuilder.AddColumn<string>("Landmark", "Branches", "nvarchar(250)", maxLength: 250, nullable: true);
        migrationBuilder.AddColumn<string>("ParkingInfo", "Branches", "nvarchar(500)", maxLength: 500, nullable: true);
        migrationBuilder.AddColumn<string>("ShortDescription", "Branches", "nvarchar(500)", maxLength: 500, nullable: true);
        migrationBuilder.AddColumn<string>("VirtualTourUrl", "Branches", "nvarchar(500)", maxLength: 500, nullable: true);

        migrationBuilder.CreateIndex("IX_Menus_RestaurantId_IsVideoEnabled_VideoDisplayOrder", "Menus", new[] { "RestaurantId", "IsVideoEnabled", "VideoDisplayOrder" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_Menus_RestaurantId_IsVideoEnabled_VideoDisplayOrder", "Menus");
        foreach (var column in new[] { "IsVideoEnabled", "VideoDisplayOrder", "VideoDurationSeconds", "VideoPosterUrl", "VideoUrl" })
            migrationBuilder.DropColumn(column, "Menus");
        foreach (var column in new[] { "CoverImageUrl", "FrontImageUrl", "GoogleMapsUrl", "IsPubliclyVisible", "Landmark", "ParkingInfo", "ShortDescription", "VirtualTourUrl" })
            migrationBuilder.DropColumn(column, "Branches");
    }
}
