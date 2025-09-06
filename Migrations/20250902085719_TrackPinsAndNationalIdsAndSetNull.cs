using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShipmentTrackingSystem.Migrations
{
    /// <inheritdoc />
    public partial class TrackPinsAndNationalIdsAndSetNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // أزل الـ FK مؤقتًا حتى نعيد إنشاؤه بـ SetNull
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Drivers_DriverId",
                table: "Shipments");

            // أعمدة قديمة من Driver
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Drivers");

            // TrackPin: نضيفه NOT NULL مع قيمة افتراضية للسجلات القديمة
            migrationBuilder.AddColumn<string>(
                name: "TrackPin",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // --------- أهم تعديل: NationalId على مرحلتين ---------

            // 1) أضِف NationalId كـ NULL (بدون default) حتى لا تتصادم القيم
            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "Drivers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // 2) عالج السجلات القديمة: املا الفارغ بقيم فريدة، وعالج أي تكرارات
            migrationBuilder.Sql(@"
                ;WITH norm AS (
	                SELECT Id,
		                   NId = LTRIM(RTRIM(ISNULL(NationalId, '')))
	                FROM Drivers
                ),
                dedup AS (
	                SELECT d.Id,
		                   d.NId,
		                   rn = ROW_NUMBER() OVER (PARTITION BY d.NId ORDER BY d.Id)
	                FROM norm d
                )
                UPDATE drv
                   SET NationalId = CASE 
		                                 WHEN d.NId = '' THEN CONCAT('TMP-', drv.Id)
		                                 WHEN d.rn = 1  THEN d.NId
		                                 ELSE CONCAT(d.NId, '-', drv.Id)
	                                 END
                FROM Drivers drv
                JOIN dedup d ON d.Id = drv.Id;
            ");

            // 3) حوّل العمود إلى NOT NULL
            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "Drivers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            // 4) أنشئ فهرسًا فريدًا
            migrationBuilder.CreateIndex(
                name: "IX_Drivers_NationalId",
                table: "Drivers",
                column: "NationalId",
                unique: true);

            // 5) أعد ربط الـ FK مع SetNull (تأكد أن DriverId nullable في Shipments)
            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Drivers_DriverId",
                table: "Shipments",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Drivers_DriverId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_NationalId",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "TrackPin",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "Drivers");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Drivers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Drivers",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Drivers_DriverId",
                table: "Shipments",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id"); // السلوك القديم
        }
    }
}
