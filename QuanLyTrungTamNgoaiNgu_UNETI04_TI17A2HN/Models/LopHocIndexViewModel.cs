// Họ và tên: Phạm Đức Duy - 23103100084

// Nội dung thực hiện: Module 2 - ViewModel phục vụ tìm kiếm, lọc, sắp xếp và phân trang lớp học

using System;
using System.Collections.Generic;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models
{
    public class LopHocIndexViewModel
    {
        
        public IEnumerable<LopHoc> DanhSachLop { get; set; } = new List<LopHoc>();

       
        public string? SearchKeyword { get; set; }     
        public int? MaKhoaHoc { get; set; }            
        public string? NgoaiNgu { get; set; }          
        public string? TrangThai { get; set; }         
        public decimal? GiaTu { get; set; }           
        public decimal? GiaDen { get; set; }           

       
        public string? SortOrder { get; set; }         

       
        public int CurrentPage { get; set; } = 1;      
        public int PageSize { get; set; } = 5;        
        public int TotalItems { get; set; }            
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); 
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
