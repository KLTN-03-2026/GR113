//using demomvc.App_Start;
//using demomvc.Models;
//using demomvc.Services.GA;
//using demonvc.Services.GA;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web.Mvc;

//namespace demomvc.Controllers
//{
//    [RoleAuthorize(RolesRequired = "HieuTruong")]
//    public class QuanLyThoiKhoaBieuController : Controller
//    {
//        private QuanLyTruongHocEntities1 db = new QuanLyTruongHocEntities1();
//        private GeneticAlgorithmService gaService = new GeneticAlgorithmService();

//        // =============================
//        // 1. TRANG CHỌN NĂM HỌC – HỌC KỲ
//        // =============================
//        public ActionResult Index()
//        {
//            var model = new TimLopViewModel
//            {


//                ListNamHoc = db.NamHoc
//                 .AsEnumerable()//luw vaof ram rooif qua tostrting
//                    .OrderBy(x => x.TenNamHoc)
//                    .Select(x => new SelectListItem
//                    {
//                        Value = x.NamHocID.ToString(),
//                        Text = x.TenNamHoc
//                    })
//                    .ToList(),

//                ListLop = new List<LopHocViewModel>()
//            };

//            return View(model);
//        }

//        // =============================
//        // 2. XEM DANH SÁCH LỚP THEO NĂM
//        // =============================
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult XemLop(TimLopViewModel model)
//        {
//            var vm = new TimLopViewModel
//            {
//                HocKyID = model.HocKyID,
//                NamHocID = model.NamHocID,

//                ListHocKy = db.HocKy
//                    .OrderBy(x => x.TenHocKy)
//                    .ToList()
//                    .Select(x => new SelectListItem
//                    {
//                        Value = x.HocKyID.ToString(),
//                        Text = x.TenHocKy
//                    })
//                    .ToList(),

//                ListNamHoc = db.NamHoc
//                    .OrderBy(x => x.TenNamHoc)
//                    .ToList()
//                    .Select(x => new SelectListItem
//                    {
//                        Value = x.NamHocID.ToString(),
//                        Text = x.TenNamHoc
//                    })
//                    .ToList()
//            };

//            if (model.NamHocID > 0)
//            {
//                vm.ListLop = db.LopHoc
//                    .Where(x => x.NienKhoa == model.NamHocID)
//                    .Select(x => new LopHocViewModel
//                    {
//                        LopHocID = x.LopHocID,
//                        TenLop = x.TenLop
//                    })
//                    .ToList();
//            }
//            else
//            {
//                vm.ListLop = new List<LopHocViewModel>();
//            }

//            return View("Index", vm);
//        }

//        // =================================================
//        // 3. TẠO THỜI KHÓA BIỂU BẰNG GA (ĐÚNG CHUẨN)
//        // =================================================
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult TaoThoiKhoaBieu(int NamHocID, int HocKyID)
//        {
//            //  Lấy số tuần học của học kỳ
//            int soTuanHoc = db.HocKy
//                .Where(h => h.HocKyID == HocKyID)
//                .Select(h => h.SoTuanThucHoc)
//                .FirstOrDefault();

//            if (soTuanHoc <= 0)
//            {
//                TempData["Error"] = "Học kỳ chưa được cấu hình số tuần học!";
//                return RedirectToAction("Index");
//            }

//            // Lấy phân công giảng dạy HỢP LỆ

//            //var phanCong = db.PhanCongGiangDay
//            //    .Where(x =>
//            //        x.HocKyID == HocKyID &&

//            //        x.LopHoc.NamHoc.NamHocID == NamHocID &&

//            //        x.GiaoVien.TrangThaiGiangDay.Contains("Dang")
//            //    )
//            //    .ToList();

//            var phanCong = db.PhanCongGiangDay
//     .Where(pc =>
//         pc.HocKyID == HocKyID &&
//         pc.HocKy.NamHocID == NamHocID &&
//         pc.GiaoVien.TrangThaiGiangDay != null //&&
//         //pc.GiaoVien.TrangThaiGiangDay
//         //    .Trim()//
//         //    .ToLower()
//         //    .Contains("dang giảng dạy")
//     )
//     .ToList();




//            //if (!phanCong.Any())
//            //{
//            //    TempData["Error"] = "Chưa có phân công giảng dạy hợp lệ!";
//            //    return RedirectToAction("Index");
//            //}

//            // Sinh baseGenes
//            List<Gene> baseGenes = new List<Gene>();

//            foreach (var pc in phanCong)
//            {
//                if (!pc.GiaoVien.MonHocID.HasValue)
//                    continue;

//                int monHocId = pc.GiaoVien.MonHocID.Value;

//                int soTiet = db.MonHocKhoi
//                    .Where(mhk =>
//                        mhk.KhoiLopID == pc.LopHoc.KhoiLopID &&
//                        mhk.MonHocID == monHocId)
//                    .Select(mhk => mhk.SoTietNam)
//                    .FirstOrDefault();

//                if (soTiet <= 0)
//                    continue;

//                for (int i = 0; i < soTiet; i++)
//                {
//                    baseGenes.Add(new Gene
//                    {
//                        LopHocID = pc.LopHocID,
//                        KhoiLopID = pc.LopHoc.KhoiLopID,
//                        MonHocID = monHocId,
//                        GiaoVienID = pc.GiaoVienID,
//                        PhongHocID = pc.LopHoc.PhongHocID,
//                        HocKyID = HocKyID,
//                        CaHoc = pc.LopHoc.CaHoc   // ✅ RẤT QUAN TRỌNG
//                    });
//                }
//            }

//            // ✅ 4. CHẠY GA (TRUYỀN soTuanHoc)
//            Chromosome best = gaService.Generate(baseGenes, soTuanHoc);

//            // ✅ 5. LƯU THỜI KHÓA BIỂU
//            foreach (var g in best.Genes)
//            {
//                var tkb = new ThoiKhoaBieu
//                {
//                    LopHocID = g.LopHocID,
//                    MonHocID = g.MonHocID,
//                    GiaoVienID = g.GiaoVienID,
//                    PhongHocID = g.LopHocID,
//                    HocKyID = g.HocKyID,
//                    Tuan = g.Tuan,
//                    Thu = g.Thu,
//                    TietHoc = g.Tiet
//                };

//                db.ThoiKhoaBieu.Add(tkb);
//            }

//            db.SaveChanges();

//            TempData["Success"] = "✅ Tạo thời khóa biểu thành công!";

//            var model = LoadIndexData(NamHocID, HocKyID);
//            return RedirectToAction("Index", model);
//        }

//        //private TimLopViewModel LoadIndexData(int namHocId, int hocKyId)
//        //{
//        //    return new TimLopViewModel
//        //    {
//        //        NamHocID = namHocId,
//        //        HocKyID = hocKyId,

//        //        ListNamHoc = db.NamHoc.Select(x =>
//        //            new SelectListItem
//        //            {
//        //                Value = x.NamHocID.ToString(),
//        //                Text = x.TenNamHoc
//        //            }).ToList(),

//        //        ListHocKy = db.HocKy.Select(x =>
//        //            new SelectListItem
//        //            {
//        //                Value = x.HocKyID.ToString(),
//        //                Text = x.TenHocKy
//        //            }).ToList(),

//        //        ThoiKhoaBieus = db.ThoiKhoaBieu
//        //    .Where(x => x.HocKyID == hocKyId &&
//        //                x.LopHoc.NienKhoa == namHocId)
//        //    .OrderBy(x => x.LopHocID)
//        //    .ThenBy(x => x.Tuan)
//        //    .ThenBy(x => x.Thu)
//        //    .ThenBy(x => x.TietHoc)
//        //    .ToList()
//        //    }
//        //    ;

//        //}
//        private TimLopViewModel LoadIndexData(int namHocId, int hocKyId)
//        {
//            return new TimLopViewModel
//            {
//                NamHocID = namHocId,
//                HocKyID = hocKyId,

//                ListNamHoc = db.NamHoc
//                    .AsEnumerable()   // ✅ CẮT EF
//                    .Select(x => new SelectListItem
//                    {
//                        Value = x.NamHocID.ToString(),
//                        Text = x.TenNamHoc
//                    })
//                    .ToList(),

//                ListHocKy = db.HocKy
//                    .AsEnumerable()   // ✅ CẮT EF
//                    .Select(x => new SelectListItem
//                    {
//                        Value = x.HocKyID.ToString(),
//                        Text = x.TenHocKy
//                    })
//                    .ToList(),

//                ThoiKhoaBieus = db.ThoiKhoaBieu
//                    .Where(x => x.HocKyID == hocKyId &&
//                                x.LopHoc.NienKhoa == namHocId)
//                    .OrderBy(x => x.LopHocID)
//                    .ThenBy(x => x.Tuan)
//                    .ThenBy(x => x.Thu)
//                    .ThenBy(x => x.TietHoc)
//                    .ToList()
//            };
//        }
//    }
//}



using demomvc.App_Start;
using demomvc.Models;
using demomvc.Services.GA;
using demonvc.Services.GA;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using demomvc.Services.Hubs;

namespace demomvc.Controllers
{
    [RoleAuthorize(RolesRequired = "HieuTruong")]
    public class QuanLyThoiKhoaBieuController : Controller
    {
        private readonly QuanLyTruongHocEntities1 db = new QuanLyTruongHocEntities1();
        private readonly GeneticAlgorithmService gaService = new GeneticAlgorithmService();

        // =============================
        // 1. TRANG CHỌN NĂM + HỌC KỲ
        // =============================
        [HttpGet]
        public ActionResult Index()
        {
            var model = new TimLopViewModel
            {
                ListNamHoc = db.NamHoc
                    .AsEnumerable() // ✅ cắt LINQ to Entities
                    .OrderBy(x => x.TenNamHoc)
                    .Select(x => new SelectListItem
                    {
                        Value = x.NamHocID.ToString(),
                        Text = x.TenNamHoc
                    })
                    .ToList(),

                ListHocKy = db.HocKy
                    .AsEnumerable()
                    .OrderBy(x => x.TenHocKy)
                    .Select(x => new SelectListItem
                    {
                        Value = x.HocKyID.ToString(),
                        Text = x.TenHocKy
                    })
                    .ToList()
            };

            return View(model);
        }

        // =================================================
        // 2. TẠO THỜI KHÓA BIỂU (GA)
        // =================================================
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult TaoThoiKhoaBieu(int NamHocID, int HocKyID)
        //{
        //    // ====== kiểm tra học kỳ ======
        //    int soTuanHoc = db.HocKy
        //        .Where(h => h.HocKyID == HocKyID && h.NamHocID == NamHocID)
        //        .Select(h => h.SoTuanThucHoc)
        //        .FirstOrDefault();

        //    if (soTuanHoc <= 0)
        //    {
        //        TempData["Error"] = "Học kỳ chưa được cấu hình số tuần học!";
        //        return RedirectToAction("Index");
        //    }

        //    // ====== LẤY PHÂN CÔNG (ĐÃ TEST SQL OK) ======
        //    var phanCong = db.PhanCongGiangDay
        //        .Where(pc =>
        //            pc.HocKyID == HocKyID &&
        //            pc.HocKy.NamHocID == NamHocID //&&
        //           // pc.GiaoVien.TrangThaiGiangDay != null// &&
        //           // pc.GiaoVien.TrangThaiGiangDay.Contains("Dang giảng dạy")
        //        )
        //        .ToList();

        //    //if (!phanCong.Any())
        //    //{
        //    //    TempData["Error"] = "Không có phân công giảng dạy hợp lệ!";
        //    //    return RedirectToAction("Index");
        //    //}

        //    // ====== SINH GENE ======
        //    List<Gene> baseGenes = new List<Gene>();

        //    foreach (var pc in phanCong)
        //    {
        //        if (!pc.GiaoVien.MonHocID.HasValue)
        //            continue;

        //        int monHocId = pc.GiaoVien.MonHocID.Value;

        //        int soTiet = db.MonHocKhoi
        //            .Where(m =>
        //                m.KhoiLopID == pc.LopHoc.KhoiLopID &&
        //                m.MonHocID == monHocId)
        //            .Select(m => m.SoTietNam)
        //            .FirstOrDefault();

        //        if (soTiet <= 0)
        //            continue;

        //        for (int i = 0; i < soTiet; i++)
        //        {
        //            baseGenes.Add(new Gene
        //            {
        //                LopHocID = pc.LopHocID,
        //                KhoiLopID = pc.LopHoc.KhoiLopID,
        //                MonHocID = monHocId,
        //                GiaoVienID = pc.GiaoVienID,
        //                PhongHocID = pc.LopHoc.PhongHocID,
        //                HocKyID = HocKyID,
        //                CaHoc = pc.LopHoc.CaHoc
        //            });
        //        }
        //    }

        //    // ====== CHẠY GA ======
        //    Chromosome best = gaService.Generate(baseGenes, soTuanHoc);

        //    // ====== LƯU TKB ======
        //    foreach (var g in best.Genes)
        //    {
        //        db.ThoiKhoaBieu.Add(new ThoiKhoaBieu
        //        {
        //            LopHocID = g.LopHocID,
        //            MonHocID = g.MonHocID,
        //            GiaoVienID = g.GiaoVienID,
        //            PhongHocID = g.PhongHocID,
        //            HocKyID = g.HocKyID,
        //            Tuan = g.Tuan,
        //            Thu = g.Thu,
        //            TietHoc = g.Tiet
        //        });
        //    }

        //    db.SaveChanges();
        //    TempData["Success"] = "✅ Tạo thời khóa biểu thành công!";
        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TaoThoiKhoaBieu(int NamHocID, int HocKyID)
        {
            // 1. Lấy số tuần học
            int soTuanHoc = db.HocKy
                .Where(h => h.HocKyID == HocKyID && h.NamHocID == NamHocID)
                .Select(h => h.SoTuanThucHoc)
                .FirstOrDefault();

            if (soTuanHoc <= 0)
            {
                TempData["Error"] = "Học kỳ chưa cấu hình số tuần học!";
                return RedirectToAction("Index");
            }

            // 2. Lấy phân công giảng dạy hợp lệ
            var phanCong = db.PhanCongGiangDay
                .Where(pc =>
                    pc.HocKyID == HocKyID &&
                    pc.HocKy.NamHocID == NamHocID &&
                    pc.GiaoVien.TrangThaiGiangDay != null)
                .ToList();

            // 3. Sinh danh sách gene
            var baseGenes = TaoBaseGenes(phanCong, HocKyID);

            // 4. Chạy GA
            //  var best = gaService.Generate(baseGenes, soTuanHoc);

            //bug so gene
            System.Diagnostics.Debug.WriteLine("TOTAL GENES = " + baseGenes.Count);


            // 4. Chạy GA (có report realtime)
            var best = gaService.Generate(
                baseGenes,
                soTuanHoc,
                msg => GaProgressHub.Send(msg)
            );



            // 5. Xóa TKB cũ (nếu có)
            var old = db.ThoiKhoaBieu
                .Where(x => x.HocKyID == HocKyID && x.LopHoc.NienKhoa == NamHocID);

            foreach (var item in old.ToList())
            {
                db.ThoiKhoaBieu.Remove(item);
            }


            // 6. Lưu TKB mới
            foreach (var g in best.Genes)
            {
                db.ThoiKhoaBieu.Add(new ThoiKhoaBieu
                {
                    LopHocID = g.LopHocID,
                    MonHocID = g.MonHocID,
                    GiaoVienID = g.GiaoVienID,
                    PhongHocID = g.PhongHocID, //  sửa đúng
                    HocKyID = g.HocKyID,
                    Tuan = g.Tuan,
                    Thu = g.Thu,
                    NamHocID=NamHocID,////////////nên xem  lại ở đây
                    TietHoc = g.Tiet
                });
            }

            db.SaveChanges();
            TempData["Success"] = "✅ Tạo thời khóa biểu thành công!";
            return RedirectToAction("Index");
        }


        private List<Gene> TaoBaseGenes(List<PhanCongGiangDay> phanCong, int hocKyId)
        {
            var genes = new List<Gene>();

            foreach (var pc in phanCong)
            {
                if (!pc.GiaoVien.MonHocID.HasValue) continue;

                int soTietNam = db.MonHocKhoi
                    .Where(m => m.KhoiLopID == pc.LopHoc.KhoiLopID
                             && m.MonHocID == pc.GiaoVien.MonHocID.Value)
                    .Select(m => m.SoTietNam)
                    .FirstOrDefault();

                int soTietTuan = soTietNam / db.HocKy
                        .Where(h => h.HocKyID == hocKyId)
                        .Select(h => h.SoTuanThucHoc)
                        .FirstOrDefault();

                for (int i = 0; i < soTietTuan; i++)
                {
                    genes.Add(new Gene
                    {
                        LopHocID = pc.LopHocID,
                        KhoiLopID = pc.LopHoc.KhoiLopID,
                        MonHocID = pc.GiaoVien.MonHocID.Value,
                        GiaoVienID = pc.GiaoVienID,
                        PhongHocID = pc.LopHoc.PhongHocID,
                        HocKyID = hocKyId,
                        CaHoc = pc.LopHoc.CaHoc
                    });
                }
            }

            return genes;
        }

    }
}


