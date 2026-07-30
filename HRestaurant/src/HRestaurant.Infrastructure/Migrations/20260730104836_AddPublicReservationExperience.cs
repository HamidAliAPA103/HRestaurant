using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRestaurant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicReservationExperience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Tables_TableId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_CustomerId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Tables_Restaurants_RestaurantID",
                table: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_TableId",
                table: "Reservations");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Tables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Tables",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<double>(
                name: "Length",
                table: "Tables",
                type: "float",
                nullable: false,
                defaultValue: 1.8);

            migrationBuilder.AddColumn<double>(
                name: "PositionX",
                table: "Tables",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PositionY",
                table: "Tables",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PositionZ",
                table: "Tables",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RotationX",
                table: "Tables",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RotationY",
                table: "Tables",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RotationZ",
                table: "Tables",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Shape",
                table: "Tables",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TableNumber",
                table: "Tables",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Width",
                table: "Tables",
                type: "float",
                nullable: false,
                defaultValue: 1.8);

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "Restaurants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Restaurants",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Restaurants",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Restaurants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Restaurants",
                type: "varchar(120)",
                unicode: false,
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Reservations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Reservations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Reservations",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmationCode",
                table: "Reservations",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 120);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Reservations",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "Reservations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Reservations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNormalized",
                table: "Reservations",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PublicTrackingTokenHash",
                table: "Reservations",
                type: "varchar(64)",
                unicode: false,
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SpecialNotes",
                table: "Reservations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "varchar(120)", unicode: false, maxLength: 120, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    TimeZoneId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Branches_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReservationAuditLogs",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IpAddressHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    CreatAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationAuditLogs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ReservationAuditLogs_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchWorkingHours",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    OpensAt = table.Column<TimeOnly>(type: "time", nullable: true),
                    ClosesAt = table.Column<TimeOnly>(type: "time", nullable: true),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    CreatAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchWorkingHours", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BranchWorkingHours_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                UPDATE [Restaurants]
                SET [Slug] =
                    'restaurant-' +
                    LEFT(
                        REPLACE(
                            CONVERT(varchar(36), [ID]),
                            '-',
                            ''),
                        12)
                WHERE [Slug] = '';

                INSERT INTO [Branches]
                    ([ID], [RestaurantId], [Name], [Slug], [Address],
                     [Phone], [Email], [TimeZoneId], [IsActive],
                     [CreatAt], [UpdateAt], [DeletedAt], [IsDeleted])
                SELECT
                    NEWID(),
                    [restaurant].[ID],
                    [restaurant].[Name],
                    'main',
                    [restaurant].[Adres],
                    [restaurant].[Number],
                    [restaurant].[Email],
                    'Asia/Baku',
                    [restaurant].[IsActive],
                    SYSUTCDATETIME(),
                    NULL,
                    NULL,
                    CAST(0 AS bit)
                FROM [Restaurants] AS [restaurant];

                INSERT INTO [BranchWorkingHours]
                    ([ID], [BranchId], [DayOfWeek], [OpensAt],
                     [ClosesAt], [IsClosed], [CreatAt], [UpdateAt],
                     [DeletedAt], [IsDeleted])
                SELECT
                    NEWID(),
                    [branch].[ID],
                    [day].[DayOfWeek],
                    [restaurantHour].[OpensAt],
                    [restaurantHour].[ClosesAt],
                    CASE
                        WHEN [restaurantHour].[ID] IS NULL
                            THEN CAST(1 AS bit)
                        ELSE [restaurantHour].[IsClosed]
                    END,
                    SYSUTCDATETIME(),
                    NULL,
                    NULL,
                    CAST(0 AS bit)
                FROM [Branches] AS [branch]
                CROSS JOIN
                    (VALUES (0), (1), (2), (3), (4), (5), (6))
                    AS [day]([DayOfWeek])
                LEFT JOIN [RestaurantWorkingHours] AS [restaurantHour]
                    ON [restaurantHour].[RestaurantId] =
                        [branch].[RestaurantId]
                    AND [restaurantHour].[DayOfWeek] =
                        [day].[DayOfWeek]
                    AND [restaurantHour].[IsDeleted] = 0;

                ;WITH [NumberedTables] AS
                (
                    SELECT
                        [table].[ID],
                        [branch].[ID] AS [BranchId],
                        ROW_NUMBER() OVER
                        (
                            PARTITION BY [table].[RestaurantID]
                            ORDER BY [table].[CreatAt], [table].[ID]
                        ) AS [RowNumber]
                    FROM [Tables] AS [table]
                    INNER JOIN [Branches] AS [branch]
                        ON [branch].[RestaurantId] =
                            [table].[RestaurantID]
                        AND [branch].[Slug] = 'main'
                )
                UPDATE [table]
                SET
                    [table].[BranchId] = [numbered].[BranchId],
                    [table].[TableNumber] =
                        'T-' + CONVERT(varchar(10), [numbered].[RowNumber]),
                    [table].[IsActive] = CAST(1 AS bit),
                    [table].[Width] = 1.8,
                    [table].[Length] = 1.8,
                    [table].[PositionX] =
                        (([numbered].[RowNumber] - 1) % 4) * 3.0 - 4.5,
                    [table].[PositionY] = 0,
                    [table].[PositionZ] =
                        FLOOR(([numbered].[RowNumber] - 1) / 4.0)
                        * 3.0 - 3.0,
                    [table].[RotationX] = 0,
                    [table].[RotationY] = 0,
                    [table].[RotationZ] = 0
                FROM [Tables] AS [table]
                INNER JOIN [NumberedTables] AS [numbered]
                    ON [numbered].[ID] = [table].[ID];

                UPDATE [reservation]
                SET
                    [reservation].[BranchId] = [table].[BranchId],
                    [reservation].[DurationMinutes] = 120,
                    [reservation].[EndTime] =
                        DATEADD(
                            minute,
                            120,
                            [reservation].[ReservationTime]),
                    [reservation].[FullName] =
                        COALESCE([customer].[Name], 'Guest'),
                    [reservation].[PhoneNormalized] = '',
                    [reservation].[ConfirmationCode] =
                        'RSV-' +
                        UPPER(
                            LEFT(
                                REPLACE(
                                    CONVERT(
                                        varchar(36),
                                        [reservation].[ID]),
                                    '-',
                                    ''),
                                6)),
                    [reservation].[PublicTrackingTokenHash] =
                        LOWER(
                            CONVERT(
                                varchar(64),
                                HASHBYTES(
                                    'SHA2_256',
                                    CONCAT(
                                        'migrated-reservation:',
                                        CONVERT(
                                            varchar(36),
                                            [reservation].[ID]))),
                                2))
                FROM [Reservations] AS [reservation]
                INNER JOIN [Tables] AS [table]
                    ON [table].[ID] = [reservation].[TableId]
                LEFT JOIN [Users] AS [customer]
                    ON [customer].[ID] = [reservation].[CustomerId];

                INSERT INTO [ReservationAuditLogs]
                    ([ID], [ReservationId], [Action], [Reason],
                     [IpAddressHash], [CreatAt], [UpdateAt],
                     [DeletedAt], [IsDeleted])
                SELECT
                    NEWID(),
                    [reservation].[ID],
                    'Migrated',
                    NULL,
                    NULL,
                    SYSUTCDATETIME(),
                    NULL,
                    NULL,
                    CAST(0 AS bit)
                FROM [Reservations] AS [reservation];
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Tables_BranchId_TableNumber",
                table: "Tables",
                columns: new[] { "BranchId", "TableNumber" },
                unique: true,
                filter: "[BranchId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_Slug",
                table: "Restaurants",
                column: "Slug",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BranchId_ReservationTime",
                table: "Reservations",
                columns: new[] { "BranchId", "ReservationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ConfirmationCode",
                table: "Reservations",
                column: "ConfirmationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PublicTrackingTokenHash",
                table: "Reservations",
                column: "PublicTrackingTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_TableId_ReservationTime_EndTime",
                table: "Reservations",
                columns: new[] { "TableId", "ReservationTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_RestaurantId_Slug",
                table: "Branches",
                columns: new[] { "RestaurantId", "Slug" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_BranchWorkingHours_BranchId_DayOfWeek",
                table: "BranchWorkingHours",
                columns: new[] { "BranchId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservationAuditLogs_ReservationId",
                table: "ReservationAuditLogs",
                column: "ReservationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Branches_BranchId",
                table: "Reservations",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Tables_TableId",
                table: "Reservations",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_CustomerId",
                table: "Reservations",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_Branches_BranchId",
                table: "Tables",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_Restaurants_RestaurantID",
                table: "Tables",
                column: "RestaurantID",
                principalTable: "Restaurants",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Branches_BranchId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Tables_TableId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_CustomerId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Tables_Branches_BranchId",
                table: "Tables");

            migrationBuilder.DropForeignKey(
                name: "FK_Tables_Restaurants_RestaurantID",
                table: "Tables");

            migrationBuilder.DropTable(
                name: "BranchWorkingHours");

            migrationBuilder.DropTable(
                name: "ReservationAuditLogs");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Tables_BranchId_TableNumber",
                table: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_Slug",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_BranchId_ReservationTime",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ConfirmationCode",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_PublicTrackingTokenHash",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_TableId_ReservationTime_EndTime",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "PositionX",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "PositionY",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "PositionZ",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "RotationX",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "RotationY",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "RotationZ",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "Shape",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "TableNumber",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ConfirmationCode",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PhoneNormalized",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PublicTrackingTokenHash",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "SpecialNotes",
                table: "Reservations");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Reservations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_TableId",
                table: "Reservations",
                column: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Tables_TableId",
                table: "Reservations",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_CustomerId",
                table: "Reservations",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_Restaurants_RestaurantID",
                table: "Tables",
                column: "RestaurantID",
                principalTable: "Restaurants",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
