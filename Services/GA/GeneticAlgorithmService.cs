using demonvc.Services.GA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace demomvc.Services.GA
{
    public class GeneticAlgorithmService
    {
        private readonly Random _rnd = new Random();
        private readonly FitnessCalculator _fitness = new FitnessCalculator();

        public Chromosome Generate(
            List<Gene> baseGenes,
            int soTuanHoc,
            Action<string> report,
            int populationSize = 15,
            int generations = 40)
        {
            List<Chromosome> population = InitPopulation(baseGenes, populationSize, soTuanHoc);
            // moi them


            // ✅ CHẶN TRƯỜNG HỢP KHÔNG TẠO ĐƯỢC CÁ THỂ HỢP LỆ
            if (population.Count == 0)
                throw new Exception("❌ Không tạo được TKB hợp lệ cho tuần 1. Ràng buộc quá chặt hoặc dữ liệu phân công sai.");

            if (population.Count == 1)
            {
                // ✅ dup để GA không crash
                population.Add(population[0].Clone());
            }

            //////



            for (int gen = 0; gen < generations; gen++)
            {
                foreach (var c in population)
                    c.Fitness = _fitness.Calculate(c);

                population = population
                    .OrderByDescending(x => x.Fitness)
                    .Take(populationSize / 2)
                    .ToList();

                while (population.Count < populationSize)
                {
                    //var p1 = population[_rnd.Next(population.Count)];
                    //var p2 = population[_rnd.Next(population.Count)];

                    //var child = Crossover(p1, p2);
                    //Mutate(child);

                    //population.Add(child);

                    //new
                    if (population.Count < 2)
                    {
                        population.Add(population[0].Clone());
                        continue;
                    }

                    var p1 = population[_rnd.Next(population.Count)];
                    var p2 = population[_rnd.Next(population.Count)];

                    var child = Crossover(p1, p2);
                    Mutate(child);

                    population.Add(child);
                    //  new//
                }
                //bao cao tien trinh
                report?.Invoke($"đang tối ưu thế hệ{gen}/{generations}...");
            }
            report?.Invoke("Hoàn tất xếp thời khóa biểu");
            return population.OrderByDescending(x => x.Fitness).First();
        }





        //new nhat
        //private List<Chromosome> InitPopulation(List<Gene> genes, int size, int soTuanHoc)
        //{
        //    var population = new List<Chromosome>();

        //    for (int i = 0; i < size; i++)
        //    {
        //        var chromo = new Chromosome();

        //        // ✅ Slot đã dùng
        //        var usedLopSlot = new HashSet<string>();
        //        var usedGVSlot = new HashSet<string>();
        //        var usedPhongSlot = new HashSet<string>();

        //        foreach (var g in genes)
        //        {
        //            bool found = false;

        //            bool laTinHoacTD = (g.MonHocID == 18 || g.MonHocID == 14);

        //            for (int attempt = 0; attempt < 80; attempt++)
        //            {
        //                int[] thuUuTien = { 2, 7, 3, 6, 4, 5 };
        //                int thu = thuUuTien[_rnd.Next(thuUuTien.Length)];

        //                int tiet;

        //                // ✅ Tin + TD học trái ca
        //                if (laTinHoacTD)
        //                {
        //                    tiet = g.CaHoc == "SANG"
        //                        ? _rnd.Next(7, 11)
        //                        : _rnd.Next(1, 5);
        //                }
        //                else // môn còn lại học đúng ca
        //                {
        //                    tiet = g.CaHoc == "SANG"
        //                        ? _rnd.Next(1, 6)
        //                        : _rnd.Next(6, 11);
        //                }

        //                // ❌ Tiết cố định
        //                if (FixedSlotHelper.IsFixedSlot(thu, tiet, g.CaHoc))
        //                    continue;

        //                string lopSlot = $"{g.LopHocID}-{g.Tuan}-{thu}-{tiet}";
        //                string gvSlot = $"{g.GiaoVienID}-{g.Tuan}-{thu}-{tiet}";
        //                string phongSlot = $"{g.PhongHocID}-{g.Tuan}-{thu}-{tiet}";

        //                // ❌ Trùng lớp
        //                if (usedLopSlot.Contains(lopSlot)) continue;

        //                // ❌ Trùng giáo viên
        //                if (usedGVSlot.Contains(gvSlot)) continue;

        //                // ❌ Trùng phòng
        //                if (g.PhongHocID.HasValue && usedPhongSlot.Contains(phongSlot))
        //                    continue;

        //                // ✅ KHÔNG CHO HỌC RỜI RẠC TRONG NGÀY
        //                var tietTrongNgay = chromo.Genes
        //                    .Where(x =>
        //                        x.LopHocID == g.LopHocID &&
        //                        x.Tuan == g.Tuan &&
        //                        x.Thu == thu)
        //                    .Select(x => x.Tiet)
        //                    .ToList();

        //                if (tietTrongNgay.Any())
        //                {
        //                    bool lienTiet = tietTrongNgay.Any(t => Math.Abs(t - tiet) == 1);
        //                    if (!lienTiet && !laTinHoacTD)
        //                        continue; // ❌ rời rạc
        //                }

        //                // ✅ NHẬN SLOT
        //                var newGene = new Gene
        //                {
        //                    LopHocID = g.LopHocID,
        //                    KhoiLopID = g.KhoiLopID,
        //                    MonHocID = g.MonHocID,
        //                    GiaoVienID = g.GiaoVienID,
        //                    PhongHocID = g.PhongHocID,
        //                    HocKyID = g.HocKyID,
        //                    CaHoc = g.CaHoc,
        //                    Tuan = g.Tuan,   // ✅ tuần mẫu (1)
        //                    Thu = thu,
        //                    Tiet = tiet
        //                };

        //                chromo.Genes.Add(newGene);
        //                usedLopSlot.Add(lopSlot);
        //                usedGVSlot.Add(gvSlot);
        //                if (g.PhongHocID.HasValue)
        //                    usedPhongSlot.Add(phongSlot);
        //                found = true;
        //                break;
        //            }

        //            // ⚠️ nếu sau 80 lần vẫn không xếp được → bỏ gene này
        //            if (!found)
        //            {
        //                // Debug để bạn biết môn nào khó
        //                System.Diagnostics.Debug.WriteLine(
        //                    $"❌ Không xếp được: Lop {g.LopHocID}, Mon {g.MonHocID}"
        //                );
        //            }
        //        }

        //        // ✅ CHỈ NHẬN CÁ THỂ CÓ GENE
        //        if (chromo.Genes.Any())
        //            population.Add(chromo);
        //    }

        //    return population;
        //}

        private static readonly int[] CA_SANG = { 1, 2, 3, 4, 5 };
        private static readonly int[] CA_CHIEU = { 6, 7, 8, 9, 10 };
        private bool IsValidBlock(IEnumerable<int> existing, int newTiet)
        {
            var list = existing.Append(newTiet).Distinct().OrderBy(x => x).ToList();
            int min = list.First();
            int max = list.Last();

            for (int t = min; t <= max; t++)
                if (!list.Contains(t))
                    return false;

            return true;
        }

        private List<Chromosome> InitPopulation(
      List<Gene> genes,
      int size,
      int soTuanHoc)
        {
            var population = new List<Chromosome>();

            int[] thus = { 2, 7, 3, 6, 4, 5 };
            int[] tietSang = { 1, 2, 3, 4, 5 };
            int[] tietChieu = { 6, 7, 8, 9, 10 };

            var lopIds = genes
                .Select(g => g.LopHocID)
                .Distinct()
                .ToList();

            for (int i = 0; i < size; i++)
            {
                // ✅ CLONE GENE POOL RIÊNG CHO CHROMOSOME
                var genePool = genes
                    .Select(g => new Gene
                    {
                        LopHocID = g.LopHocID,
                        KhoiLopID = g.KhoiLopID,
                        MonHocID = g.MonHocID,
                        GiaoVienID = g.GiaoVienID,
                        PhongHocID = g.PhongHocID,
                        HocKyID = g.HocKyID,
                        CaHoc = g.CaHoc,
                        Tuan = g.Tuan
                    })
                    .ToList();

                var chromo = new Chromosome();

                var usedLop = new HashSet<string>();
                var usedGV = new HashSet<string>();
                var usedPhong = new HashSet<string>();

                foreach (var thu in thus)
                {
                    foreach (var ca in new[] { "SANG", "CHIEU" })
                    {
                        var tietTrongCa = ca == "SANG" ? tietSang : tietChieu;

                        foreach (var tiet in tietTrongCa)
                        {
                            // ❌ tiết cố định
                            if (FixedSlotHelper.IsFixedSlot(thu, tiet, ca))
                                continue;

                            // ✅ DUYỆT TẤT CẢ LỚP CHO SLOT NÀY
                            foreach (var lopId in lopIds)
                            {
                                var candidates = genePool
                                    .Where(g =>
                                    {
                                        if (g.LopHocID != lopId)
                                            return false;

                                        bool laTinHoacTD = g.MonHocID == 18 || g.MonHocID == 14;

                                        bool hopLeCa = laTinHoacTD
                                            ? g.CaHoc != ca       // ✅ Tin + TD học trái ca
                                            : g.CaHoc == ca;      // ✅ môn thường học đúng ca

                                        if (!hopLeCa)
                                            return false;


                                        if (laTinHoacTD)
                                        {
                                            if (ca == "SANG")
                                            {
                                                // ✅ chỉ cho tiết 1 → 4
                                                if (tiet < 1 || tiet > 4)
                                                    return false;
                                            }
                                            else // CHIEU
                                            {
                                                // ✅ chỉ cho tiết 7 → 10
                                                if (tiet < 7 || tiet > 10)
                                                    return false;
                                            }
                                        }


                                        if (CaHocHelper.IsSaiCa(g.MonHocID, tiet, g.CaHoc))
                                            return false;

                                        return true;
                                    })
                                    .OrderBy(x => _rnd.Next())
                                    .ToList();

                                foreach (var g in candidates)
                                {
                                    string lopKey = $"{lopId}-{thu}-{tiet}";
                                    string gvKey = $"{g.GiaoVienID}-{thu}-{tiet}";
                                    string phongKey = $"{g.PhongHocID}-{thu}-{tiet}";

                                    if (usedLop.Contains(lopKey)) continue;
                                    if (usedGV.Contains(gvKey)) continue;
                                    if (g.PhongHocID.HasValue && usedPhong.Contains(phongKey))
                                        continue;

                                    chromo.Genes.Add(new Gene
                                    {
                                        LopHocID = g.LopHocID,
                                        KhoiLopID = g.KhoiLopID,
                                        MonHocID = g.MonHocID,
                                        GiaoVienID = g.GiaoVienID,
                                        PhongHocID = g.PhongHocID,
                                        HocKyID = g.HocKyID,
                                        Tuan = 1,
                                        Thu = thu,
                                        Tiet = tiet,
                                        CaHoc = ca
                                    });

                                    usedLop.Add(lopKey);
                                    usedGV.Add(gvKey);
                                    if (g.PhongHocID.HasValue)
                                        usedPhong.Add(phongKey);

                                    // ✅ REMOVE TIẾT ĐÃ DÙNG (ĐẾM TIẾT)
                                    genePool.Remove(g);
                                    break; // ✅ mỗi lớp đúng 1 tiết / slot
                                }
                            }
                        }
                    }
                }

                if (chromo.Genes.Any())
                    population.Add(chromo);
            }

            return population;
        }
        private Chromosome Crossover(Chromosome a, Chromosome b)
        {
            int cut = _rnd.Next(a.Genes.Count);

            var genes = a.Genes.Take(cut)
                .Concat(b.Genes.Skip(cut))
                .Select(g => new Gene
                {
                    LopHocID = g.LopHocID,
                    KhoiLopID = g.KhoiLopID,
                    MonHocID = g.MonHocID,
                    GiaoVienID = g.GiaoVienID,
                    PhongHocID = g.PhongHocID,
                    HocKyID = g.HocKyID,
                    CaHoc = g.CaHoc,
                    Tuan = g.Tuan,
                    Thu = g.Thu,
                    Tiet = g.Tiet
                }).ToList();

            return new Chromosome { Genes = genes };
        }

        private void Mutate(Chromosome c)
        {
            if (_rnd.NextDouble() < 0.1)
            {
                var g = c.Genes[_rnd.Next(c.Genes.Count)];
                int thu, tiet;
                bool lopcasang = g.CaHoc == "SANG";
                do
                {
                    thu = _rnd.Next(2, 8);
                    if (lopcasang)
                        tiet = _rnd.Next(1, 6);
                    else
                        tiet = _rnd.Next(6, 11);
                }
                while (FixedSlotHelper.IsFixedSlot(thu, tiet, g.CaHoc));

                //g.Thu = thu;
                //g.Tiet = tiet;

                if (!c.Genes.Any(x =>
                    x != g &&
                    x.LopHocID == g.LopHocID &&
                    x.Tuan == g.Tuan &&
                    x.Thu == thu &&
                    x.Tiet == tiet))
                {
                    g.Thu = thu;
                    g.Tiet = tiet;
                }
            }
        }
        // CHECK lop đo co trùng tiết hay ko 

        private bool IsLopSlotTrung(Gene g, List<Gene> genes)
        {
            return genes.Any(x =>
                x.LopHocID == g.LopHocID &&
                x.Tuan == g.Tuan &&
                x.Thu == g.Thu &&
                x.Tiet == g.Tiet
            );
        }


    }
}