
using demonvc.Services.GA;
using System.Collections.Generic;
using System.Linq;

namespace demomvc.Services.GA
{
    public class FitnessCalculator
    {
        public double Calculate(Chromosome c)
        {
            double fitness = 100000;

            var lopSlot = new HashSet<string>();
            var gvSlot = new HashSet<string>();
            var phongSlot = new HashSet<string>();

            var demDonMon = new Dictionary<string, int>();

            foreach (var g in c.Genes)
            {
                // ===== 1. TRÙNG LỚP =====
                string keyLop = $"{g.LopHocID}-{g.Tuan}-{g.Thu}-{g.Tiet}";
                if (!lopSlot.Add(keyLop))
                    fitness -= 20000;

                // ===== 2. TRÙNG GIÁO VIÊN =====
                string keyGV = $"{g.GiaoVienID}-{g.Tuan}-{g.Thu}-{g.Tiet}";
                if (!gvSlot.Add(keyGV))
                    fitness -= 20000;

                // ===== 3. TRÙNG PHÒNG =====
                string keyPhong = $"{g.PhongHocID}-{g.Tuan}-{g.Thu}-{g.Tiet}";
                if (!phongSlot.Add(keyPhong))
                    fitness -= 20000;

                // ===== 4. QUÁ 2 TIẾT / NGÀY / MÔN =====
                string keyDon = $"{g.LopHocID}-{g.MonHocID}-{g.Tuan}-{g.Thu}";
                if (!demDonMon.ContainsKey(keyDon))
                    demDonMon[keyDon] = 1;
                else
                {
                    demDonMon[keyDon]++;
                    if (demDonMon[keyDon] > 2)
                        fitness -= 50;
                }

                // ===== 5. TIN / TD SAI CA =====
                if (CaHocHelper.IsSaiCa(g.MonHocID, g.Tiet, g.CaHoc))
                    fitness -= 200;
            }

            //// ===== 6. ƯU TIÊN TOÁN – VĂN – ANH HỌC 2 TIẾT LIỀN =====
            var uuTienMon = new HashSet<int> { 5, 2, 13 }; // Toán, Văn, Anh
            var checkLienTiet = new HashSet<string>();

            foreach (var g in c.Genes)
            {
                if (!uuTienMon.Contains(g.MonHocID)) continue;

                string nextTiet = $"{g.LopHocID}-{g.MonHocID}-{g.Tuan}-{g.Thu}-{g.Tiet + 1}";
                if (checkLienTiet.Contains(nextTiet))
                    fitness += 5000;

                string curTiet = $"{g.LopHocID}-{g.MonHocID}-{g.Tuan}-{g.Thu}-{g.Tiet}";
                checkLienTiet.Add(curTiet);
            }




            // ===== 7. PHẠT TIẾT LẺ (KHÔNG LIỀN MẠCH) =====
            var nhomTheoLopNgayCa = c.Genes
                .GroupBy(x => new { x.LopHocID, x.Tuan, x.Thu, x.CaHoc });

            foreach (var group in nhomTheoLopNgayCa)
            {
                var tiets = group
                    .Select(x => x.Tiet)
                    .OrderBy(t => t)
                    .ToList();

                if (tiets.Count <= 1)
                    continue;

                for (int i = 1; i < tiets.Count; i++)
                {
                    if (tiets[i] != tiets[i - 1] + 1)
                    {
                        fitness -= 500; // ❌ PHẠT NẶNG TIẾT LẺ
                    }
                }
            }

            // ===== 9. ƯU TIÊN XẾP VÀO THỨ 2,7,3,6,4,5 =====
            var thuUuTien = new List<int> { 2, 7, 3, 6, 4, 5 };

            foreach (var g in c.Genes)
            {
                int idx = thuUuTien.IndexOf(g.Thu);

                if (idx >= 0)
                {
                    // ✅ càng ưu tiên → thưởng càng nhiều
                    // Thứ 2 idx=0 → +50
                    // Thứ 7 idx=1 → +45 ...
                    fitness += (6 - idx) * 100;
                }
                else
                {
                    // ❌ thứ không mong muốn (nếu có)
                    fitness -= 10;
                }
            }

            // ===== 10. BẮT BUỘC PHẢI CÓ THỨ 7 =====
            var lopIds = c.Genes
                .Select(x => x.LopHocID)
                .Distinct()
                .ToList();

            foreach (var lopId in lopIds)
            {
                bool coThu7 = c.Genes.Any(x =>
                    x.LopHocID == lopId &&
                    x.Thu == 7);

                if (!coThu7)
                {
                    fitness -= 500; // PHẠT CỰC NẶNG
                }
            }

            return fitness;
        }
    }
}