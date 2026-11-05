// Họ và tên: Phạm Đức Duy - 23103100084 & Nhóm UNETI04
// Nội dung thực hiện: Module 2 & Tiện ích hệ thống - Kiểm tra trùng phòng học & Ca học (Schedule & Room Conflict Checker)

using System.Text.RegularExpressions;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Helpers
{
    public static class LichHocHelper
    {
        // Danh sách các ca học phổ biến để hiển thị gợi ý (Datalist / Dropdown)
        public static readonly string[] CaHocPhoBien = new[]
        {
            "T2 - T4 - T6 (18:00 - 20:00)",
            "T2 - T4 - T6 (19:30 - 21:30)",
            "T3 - T5 - T7 (18:00 - 20:00)",
            "T3 - T5 - T7 (19:30 - 21:30)",
            "T7 - CN (08:30 - 11:30)",
            "T7 - CN (14:00 - 17:00)",
            "T7 - CN (18:00 - 21:00)"
        };

        // Danh sách các phòng học thường dùng của Trung tâm Ngoại ngữ UNETI
        public static readonly string[] PhongHocPhoBien = new[]
        {
            "Phòng A101",
            "Phòng A102",
            "Phòng A103",
            "Phòng B201",
            "Phòng B202",
            "Phòng C301",
            "Phòng C302",
            "Phòng C303",
            "Phòng D401",
            "Phòng D402",
            "Phòng E501",
            "Phòng E502"
        };

        /// <summary>
        /// Chuẩn hóa tên phòng học (bỏ chữ 'phòng', khoảng trắng, dấu gạch, viết thường)
        /// VD: "Phòng A101", "A-101", "phong a101" -> "a101"
        /// </summary>
        public static string NormalizeRoom(string? room)
        {
            if (string.IsNullOrWhiteSpace(room)) return string.Empty;
            string s = room.Trim().ToLower();
            s = Regex.Replace(s, @"^(phòng|phong|room)\s*", "");
            s = Regex.Replace(s, @"[\s\-_.]+", "");
            return s;
        }

        /// <summary>
        /// Kiểm tra hai phòng học có phải là cùng một phòng không
        /// </summary>
        public static bool IsSameRoom(string? room1, string? room2)
        {
            var r1 = NormalizeRoom(room1);
            var r2 = NormalizeRoom(room2);
            return !string.IsNullOrEmpty(r1) && !string.IsNullOrEmpty(r2) && r1 == r2;
        }

        /// <summary>
        /// Bóc tách các thứ trong tuần từ chuỗi lịch học.
        /// Quy ước: 2 = Thứ 2, 3 = Thứ 3, ..., 7 = Thứ 7, 8 = Chủ nhật
        /// </summary>
        public static HashSet<int> ParseDaysOfWeek(string? schedule)
        {
            var days = new HashSet<int>();
            if (string.IsNullOrWhiteSpace(schedule)) return days;

            string s = schedule.ToLower();

            // Thứ 2
            if (Regex.IsMatch(s, @"\b(t2|thứ\s*2|thu\s*2|thứ\s*hai|thu\s*hai|t\.2|mon)\b")) days.Add(2);
            // Thứ 3
            if (Regex.IsMatch(s, @"\b(t3|thứ\s*3|thu\s*3|thứ\s*ba|thu\s*ba|t\.3|tue)\b")) days.Add(3);
            // Thứ 4
            if (Regex.IsMatch(s, @"\b(t4|thứ\s*4|thu\s*4|thứ\s*tư|thu\s*tu|t\.4|wed)\b")) days.Add(4);
            // Thứ 5
            if (Regex.IsMatch(s, @"\b(t5|thứ\s*5|thu\s*5|thứ\s*năm|thu\s*nam|t\.5|thu)\b")) days.Add(5);
            // Thứ 6
            if (Regex.IsMatch(s, @"\b(t6|thứ\s*6|thu\s*6|thứ\s*sáu|thu\s*sau|t\.6|fri)\b")) days.Add(6);
            // Thứ 7
            if (Regex.IsMatch(s, @"\b(t7|thứ\s*7|thu\s*7|thứ\s*bảy|thu\s*bay|t\.7|sat)\b")) days.Add(7);
            // Chủ nhật
            if (Regex.IsMatch(s, @"\b(cn|chủ\s*nhật|chu\s*nhat|t8|sun)\b")) days.Add(8);

            return days;
        }

        /// <summary>
        /// Bóc tách khoảng thời gian (giờ bắt đầu, giờ kết thúc) từ chuỗi lịch học
        /// Hỗ trợ định dạng: "18:00 - 20:00", "18h30 - 20h30", "18h - 20h", "8:30 - 11:30"
        /// </summary>
        public static (TimeSpan Start, TimeSpan End)? ParseTimeRange(string? schedule)
        {
            if (string.IsNullOrWhiteSpace(schedule)) return null;

            // Pattern: (\d{1,2})[:h](\d{2})?\s*[-–—đếnto~]+\s*(\d{1,2})[:h](\d{2})?
            var match = Regex.Match(schedule, @"(\d{1,2})(?:[:h](\d{2}))?\s*[-–—đếnto~]+\s*(\d{1,2})(?:[:h](\d{2}))?", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                int h1 = int.Parse(match.Groups[1].Value);
                int m1 = match.Groups[2].Success && !string.IsNullOrEmpty(match.Groups[2].Value) ? int.Parse(match.Groups[2].Value) : 0;
                int h2 = int.Parse(match.Groups[3].Value);
                int m2 = match.Groups[4].Success && !string.IsNullOrEmpty(match.Groups[4].Value) ? int.Parse(match.Groups[4].Value) : 0;

                if (h1 >= 0 && h1 < 24 && m1 >= 0 && m1 < 60 && h2 >= 0 && h2 < 24 && m2 >= 0 && m2 < 60)
                {
                    var start = new TimeSpan(h1, m1, 0);
                    var end = new TimeSpan(h2, m2, 0);
                    if (end > start)
                    {
                        return (start, end);
                    }
                }
            }

            // Fallback ca học tên thông dụng nếu không ghi giờ cụ thể
            string s = schedule.ToLower();
            if (s.Contains("ca sáng") || s.Contains("buổi sáng")) return (new TimeSpan(8, 0, 0), new TimeSpan(11, 0, 0));
            if (s.Contains("ca chiều") || s.Contains("buổi chiều")) return (new TimeSpan(14, 0, 0), new TimeSpan(17, 0, 0));
            if (s.Contains("ca tối 1") || s.Contains("ca 1")) return (new TimeSpan(18, 0, 0), new TimeSpan(20, 0, 0));
            if (s.Contains("ca tối 2") || s.Contains("ca 2")) return (new TimeSpan(19, 30, 0), new TimeSpan(21, 30, 0));

            return null;
        }

        /// <summary>
        /// Kiểm tra 2 ca học/lịch học có bị giao nhau (trùng nhau) hay không
        /// </summary>
        public static bool KiemTraTrungLich(string? schedule1, string? schedule2)
        {
            if (string.IsNullOrWhiteSpace(schedule1) || string.IsNullOrWhiteSpace(schedule2))
                return false;

            var days1 = ParseDaysOfWeek(schedule1);
            var days2 = ParseDaysOfWeek(schedule2);

            // Nếu cả hai đều phân tích được các thứ trong tuần
            if (days1.Any() && days2.Any())
            {
                // Nếu không có ngày nào trùng nhau trong tuần -> Không trùng!
                if (!days1.Overlaps(days2))
                {
                    return false;
                }
            }

            // Phân tích khoảng giờ
            var time1 = ParseTimeRange(schedule1);
            var time2 = ParseTimeRange(schedule2);

            if (time1.HasValue && time2.HasValue)
            {
                // Công thức giao khoảng thời gian: StartA < EndB && StartB < EndA
                bool timeOverlap = (time1.Value.Start < time2.Value.End) && (time2.Value.Start < time1.Value.End);
                return timeOverlap;
            }

            // Fallback: Nếu không bóc tách được giờ chuẩn xác, so sánh chuỗi sau khi làm sạch
            string clean1 = Regex.Replace(schedule1.Trim().ToLower(), @"\s+", " ");
            string clean2 = Regex.Replace(schedule2.Trim().ToLower(), @"\s+", " ");
            return clean1 == clean2 || clean1.Contains(clean2) || clean2.Contains(clean1);
        }

        /// <summary>
        /// Kiểm tra trùng phòng học và ca học đồng thời
        /// </summary>
        public static bool KiemTraTrungPhongVaCaHoc(string? room1, string? schedule1, string? room2, string? schedule2)
        {
            if (!IsSameRoom(room1, room2))
            {
                return false;
            }

            return KiemTraTrungLich(schedule1, schedule2);
        }
    }
}
