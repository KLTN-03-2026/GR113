using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace demomvc.Services.GA
{
    /// <summary>
    /// Xác định các tiết CỐ ĐỊNH theo CA học
    /// - Ca sáng:
    ///   + Thứ 2 tiết 1: Chào cờ
    ///   + Thứ 7 tiết 5: Sinh hoạt
    /// - Ca chiều:
    ///   + Thứ 2 tiết 5: Chào cờ
    ///   + Thứ 7 tiết 10: Sinh hoạt
    /// </summary>
    public static class FixedSlotHelper
    {
        /// <param name="thu">Thứ (2 → 7)</param>
        /// <param name="tiet">Tiết</param>
        /// <param name="caHoc">1 = ca sáng, 2 = ca chiều</param>
        public static bool IsFixedSlot(int thu, int tiet, string caHoc)
        {
            if (caHoc == "SANG") //
            {
                // Thứ 2 tiết 1: Chào cờ
                if (thu == 2 && tiet == 1) return true;

                // Thứ 7 tiết 5: Sinh hoạt
                if (thu == 7 && tiet == 5) return true;
            }
            else if (caHoc == "CHIEU") // 
            {
                // Thứ 2 tiết 5: Chào cờ
                if (thu == 2 && tiet == 10) return true;

                // Thứ 7 tiết 10: Sinh hoạt
                if (thu == 7 && tiet == 10) return true;
            }

            return false;
        }
    }
}