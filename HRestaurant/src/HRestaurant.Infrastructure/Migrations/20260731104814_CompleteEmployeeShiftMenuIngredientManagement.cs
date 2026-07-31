using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRestaurant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompleteEmployeeShiftMenuIngredientManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuCategories_Restaurants_ResdaranId",
                table: "MenuCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuCategories_Restaurants_RestaurantID",
                table: "MenuCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Menus_MenuCategories_CategoryId",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Menus_CategoryId",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_MenuCategories_ResdaranId",
                table: "MenuCategories");

            migrationBuilder.DropIndex(
                name: "IX_MenuCategories_RestaurantID",
                table: "MenuCategories");

            migrationBuilder.Sql(
                """
                UPDATE [MenuCategories]
                SET [ResdaranId] = [RestaurantID]
                WHERE [RestaurantID] IS NOT NULL
                  AND [ResdaranId] = '00000000-0000-0000-0000-000000000000';
                """);

            migrationBuilder.DropColumn(
                name: "RestaurantID",
                table: "MenuCategories");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "AppUserId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContact",
                table: "Users",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "HireDate",
                table: "Users",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "Users",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedPhone",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RestaurantId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                table: "Users",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Nutrition",
                table: "Menus",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ImageURL",
                table: "Menus",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Image",
                table: "Menus",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Desc",
                table: "Menus",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "Menus",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalPrice",
                table: "Menus",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Menus",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPopular",
                table: "Menus",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Menus",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Menus",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PreparationTimeMinutes",
                table: "Menus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "RestaurantId",
                table: "Menus",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "MenuCategories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "MenuCategories",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "MenuCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "MenuCategories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "MenuCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "MenuCategories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE [Users]
                SET [NormalizedEmail] = UPPER(LTRIM(RTRIM([Email]))),
                    [IsActive] = 1;

                ;WITH [CategoryNames] AS
                (
                    SELECT [ID],
                           UPPER(LTRIM(RTRIM([Name]))) AS [BaseName],
                           ROW_NUMBER() OVER
                           (
                               PARTITION BY [ResdaranId], UPPER(LTRIM(RTRIM([Name])))
                               ORDER BY [ID]
                           ) AS [RowNumber]
                    FROM [MenuCategories]
                )
                UPDATE [Category]
                SET [NormalizedName] = CASE
                    WHEN [Names].[RowNumber] = 1 THEN [Names].[BaseName]
                    ELSE LEFT([Names].[BaseName], 91) + '-' +
                         LEFT(REPLACE(CONVERT(varchar(36), [Category].[ID]), '-', ''), 8)
                END
                FROM [MenuCategories] AS [Category]
                INNER JOIN [CategoryNames] AS [Names]
                    ON [Category].[ID] = [Names].[ID];

                UPDATE [MenuCategories]
                SET [IsActive] = 1;

                UPDATE [Menus]
                SET [RestaurantId] = [Category].[ResdaranId],
                    [Name] = 'Menu ' + LEFT(REPLACE(CONVERT(varchar(36), [Menus].[ID]), '-', ''), 8),
                    [NormalizedName] = 'MENU ' + LEFT(REPLACE(CONVERT(varchar(36), [Menus].[ID]), '-', ''), 8),
                    [FinalPrice] = [Menus].[Price],
                    [IsAvailable] = 1
                FROM [Menus]
                INNER JOIN [MenuCategories] AS [Category]
                    ON [Menus].[CategoryId] = [Category].[ID];
                """);

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Ingredients_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Shifts_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shifts_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemIngredients",
                columns: table => new
                {
                    MenuItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemIngredients", x => new { x.MenuItemId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_MenuItemIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuItemIngredients_Menus_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "Menus",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeShifts",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeShifts", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EmployeeShifts_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeShifts_Users_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_AppUserId",
                table: "Users",
                column: "AppUserId",
                unique: true,
                filter: "[AppUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_BranchId",
                table: "Users",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true,
                filter: "[RestaurantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedPhone",
                table: "Users",
                column: "NormalizedPhone",
                unique: true,
                filter: "[RestaurantId] IS NOT NULL AND [NormalizedPhone] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RestaurantId_BranchId_IsDeleted_IsActive",
                table: "Users",
                columns: new[] { "RestaurantId", "BranchId", "IsDeleted", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_RestaurantId_Role_IsDeleted",
                table: "Users",
                columns: new[] { "RestaurantId", "Role", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Menus_CategoryId_NormalizedName",
                table: "Menus",
                columns: new[] { "CategoryId", "NormalizedName" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_RestaurantId_IsDeleted_IsAvailable_IsPopular",
                table: "Menus",
                columns: new[] { "RestaurantId", "IsDeleted", "IsAvailable", "IsPopular" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuCategories_ResdaranId_IsDeleted_IsActive_DisplayOrder",
                table: "MenuCategories",
                columns: new[] { "ResdaranId", "IsDeleted", "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuCategories_ResdaranId_NormalizedName",
                table: "MenuCategories",
                columns: new[] { "ResdaranId", "NormalizedName" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShifts_EmployeeId_ShiftId_WorkDate",
                table: "EmployeeShifts",
                columns: new[] { "EmployeeId", "ShiftId", "WorkDate" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShifts_EmployeeId_WorkDate_StartTime_EndTime",
                table: "EmployeeShifts",
                columns: new[] { "EmployeeId", "WorkDate", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShifts_ShiftId_WorkDate",
                table: "EmployeeShifts",
                columns: new[] { "ShiftId", "WorkDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_RestaurantId_IsDeleted_IsActive",
                table: "Ingredients",
                columns: new[] { "RestaurantId", "IsDeleted", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_RestaurantId_NormalizedName",
                table: "Ingredients",
                columns: new[] { "RestaurantId", "NormalizedName" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemIngredients_IngredientId",
                table: "MenuItemIngredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_BranchId",
                table: "Shifts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_RestaurantId_BranchId_IsDeleted_IsActive",
                table: "Shifts",
                columns: new[] { "RestaurantId", "BranchId", "IsDeleted", "IsActive" });

            migrationBuilder.AddForeignKey(
                name: "FK_MenuCategories_Restaurants_ResdaranId",
                table: "MenuCategories",
                column: "ResdaranId",
                principalTable: "Restaurants",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_MenuCategories_CategoryId",
                table: "Menus",
                column: "CategoryId",
                principalTable: "MenuCategories",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Restaurants_RestaurantId",
                table: "Menus",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_AspNetUsers_AppUserId",
                table: "Users",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Branches_BranchId",
                table: "Users",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Restaurants_RestaurantId",
                table: "Users",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuCategories_Restaurants_ResdaranId",
                table: "MenuCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Menus_MenuCategories_CategoryId",
                table: "Menus");

            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Restaurants_RestaurantId",
                table: "Menus");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_AspNetUsers_AppUserId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Branches_BranchId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Restaurants_RestaurantId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "EmployeeShifts");

            migrationBuilder.DropTable(
                name: "MenuItemIngredients");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropIndex(
                name: "IX_Users_AppUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_BranchId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_NormalizedPhone",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_RestaurantId_BranchId_IsDeleted_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_RestaurantId_Role_IsDeleted",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Menus_CategoryId_NormalizedName",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Menus_RestaurantId_IsDeleted_IsAvailable_IsPopular",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_MenuCategories_ResdaranId_IsDeleted_IsActive_DisplayOrder",
                table: "MenuCategories");

            migrationBuilder.DropIndex(
                name: "IX_MenuCategories_ResdaranId_NormalizedName",
                table: "MenuCategories");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmergencyContact",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedPhone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "FinalPrice",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "IsPopular",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "PreparationTimeMinutes",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "MenuCategories");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "MenuCategories");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "MenuCategories");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "MenuCategories");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "MenuCategories");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254);

            migrationBuilder.AlterColumn<string>(
                name: "Nutrition",
                table: "Menus",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "ImageURL",
                table: "Menus",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Image",
                table: "Menus",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Desc",
                table: "Menus",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "MenuCategories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "RestaurantID",
                table: "MenuCategories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Menus_CategoryId",
                table: "Menus",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuCategories_ResdaranId",
                table: "MenuCategories",
                column: "ResdaranId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuCategories_RestaurantID",
                table: "MenuCategories",
                column: "RestaurantID");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_MenuCategories_CategoryId",
                table: "Menus",
                column: "CategoryId",
                principalTable: "MenuCategories",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
