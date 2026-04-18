using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace demomvc.Services.GA
{
    /// <summary>
    /// Kiểm tra môn học có bị xếp SAI ca hay không
    /// CaHoc lấy trực tiếp từ bảng LopHoc ("SANG", "CHIEU", null)
    /// </summary>
    public static class CaHocHelper
    {
        /// <param name="monHocId">ID môn học</param>
        /// <param name="tiet">Tiết học</param>
        /// <param name="caHoc">Ca chính của lớp ("SANG" / "CHIEU")</param>
        public static bool IsSaiCa(int monHocId, int tiet, string caHoc)
        {
            // ✅ Chỉ áp dụng cho Tin & Thể dục
            bool laTinHoacTD = monHocId == 18 || monHocId == 14;
            if (!laTinHoacTD) return false;

            if (string.IsNullOrEmpty(caHoc))
                return false; // chưa cấu hình ca → không xét

            caHoc = caHoc.ToUpper().Trim();

            bool tietBuoiSang = tiet <= 5;   // 1–5: sáng
            bool tietBuoiChieu = tiet > 5;   // 6–10: chiều

            // ✅ Lớp ca SÁNG → Tin/TD học CHIỀU
            if (caHoc == "SANG" && tietBuoiSang)
                return true;

            // ✅ Lớp ca CHIỀU → Tin/TD học SÁNG
            if (caHoc == "CHIEU" && tietBuoiChieu)
                return true;

            return false;
        }
    }
}
