using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Migrations
{
    /// <inheritdoc />
    public partial class AddTaiKhoanToGiangVien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaTaiKhoan",
                table: "GiangVien",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiangVien_MaTaiKhoan",
                table: "GiangVien",
                column: "MaTaiKhoan",
                unique: true,
                filter: "[MaTaiKhoan] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_GiangVien_TaiKhoan_MaTaiKhoan",
                table: "GiangVien",
                column: "MaTaiKhoan",
                principalTable: "TaiKhoan",
                principalColumn: "MaTaiKhoan",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GiangVien_TaiKhoan_MaTaiKhoan",
                table: "GiangVien");

            migrationBuilder.DropIndex(
                name: "IX_GiangVien_MaTaiKhoan",
                table: "GiangVien");

            migrationBuilder.DropColumn(
                name: "MaTaiKhoan",
                table: "GiangVien");
        }
    }
}
