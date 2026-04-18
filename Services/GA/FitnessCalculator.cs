//using demonvc.Services.GA;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

//namespace demomvc.Services.GA
//{
//	public class FitnessCalculator
//	{
//		public double Calculate(Chromosome c)
//		{
//			double fitness = 100000;

//			//trung lop
//			if (c.Genes.GroupBy(g => new { g.LopHocID, g.Tuan, g.Thu, g.Tiet }).Any(g => g.Count() > 1))
//				fitness -= 20000;

//			//trung GV
//			if (c.Genes.GroupBy(g => new { g.GiaoVienID, g.Tuan, g.Thu, g.Tiet }).Any(g => g.Count() > 1))
//				fitness -= 20000;

//			//trung phong
//			if (c.Genes.GroupBy(g => new { g.PhongHocID, g.Tuan, g.Thu, g.Tiet }).Any(g => g.Count() > 1))
//				fitness -= 20000;

//			//khong don 2 tiet 1 ngay
//			var donMon = c.Genes
//				.GroupBy(g => new { g.LopHocID, g.MonHocID, g.Tuan, g.Thu })
//				.Where(g => g.Count() > 2);
//			fitness -= donMon.Count() * 50;

//			//uu tien toan id =5, van id=2 ,anh id =13, hoc 2 tiet lien tiep 
//			foreach(var g in c.Genes.Where(x => x.MonHocID==5|| x.MonHocID==2 || x.MonHocID == 13))
//			{
//				bool lienTiet = c.Genes.Any(x =>
//					x.LopHocID == g.LopHocID &&
//					x.MonHocID == g.MonHocID &&
//					x.Tuan == g.Tuan &&
//					x.Thu == g.Thu &&
//					x.Tiet == g.Tiet + 1
//				);
//				if (lienTiet) fitness += 20;
//			}

//            // ❌ Tin / Thể dục học sai ca
//            foreach (var g in c.Genes)
//            {
//                if (CaHocHelper.IsSaiCa(g.MonHocID, g.Tiet, g.CaHoc))
//                    fitness -= 200; // phạt nhẹ (soft constraint)
//            }

//            return fitness;

//		}
//	}
//}


using demonvc.Services.GA;
using System.Collections.Generic;

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

            // ===== 6. ƯU TIÊN TOÁN – VĂN – ANH HỌC 2 TIẾT LIỀN =====
            var uuTienMon = new HashSet<int> { 5, 2, 13 }; // Toán, Văn, Anh
            var checkLienTiet = new HashSet<string>();

            foreach (var g in c.Genes)
            {
                if (!uuTienMon.Contains(g.MonHocID)) continue;

                string nextTiet = $"{g.LopHocID}-{g.MonHocID}-{g.Tuan}-{g.Thu}-{g.Tiet + 1}";
                if (checkLienTiet.Contains(nextTiet))
                    fitness += 20;

                string curTiet = $"{g.LopHocID}-{g.MonHocID}-{g.Tuan}-{g.Thu}-{g.Tiet}";
                checkLienTiet.Add(curTiet);
            }

            return fitness;
        }
    }
}