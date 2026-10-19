using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Migrations
{
    /// <inheritdoc />
    public partial class AddGiangVienAndPhanCong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaGiangVien",
                table: "LopHoc",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GiangVien",
                columns: table => new
                {
                    MaGiangVien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GioiTinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TrinhDo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChuyenMon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiangVien", x => x.MaGiangVien);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LopHoc_MaGiangVien",
                table: "LopHoc",
                column: "MaGiangVien");

            migrationBuilder.AddForeignKey(
                name: "FK_LopHoc_GiangVien_MaGiangVien",
                table: "LopHoc",
                column: "MaGiangVien",
                principalTable: "GiangVien",
                principalColumn: "MaGiangVien",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LopHoc_GiangVien_MaGiangVien",
                table: "LopHoc");

            migrationBuilder.DropTable(
                name: "GiangVien");

            migrationBuilder.DropIndex(
                name: "IX_LopHoc_MaGiangVien",
                table: "LopHoc");

            migrationBuilder.DropColumn(
                name: "MaGiangVien",
                table: "LopHoc");
        }
    }
}
