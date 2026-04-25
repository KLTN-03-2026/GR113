

using demomvc.App_Start;
using demomvc.Models;
using demomvc.Services.GA;
using demonvc.Services.GA;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using demomvc.Services.Hubs;
using System;

namespace demomvc.Controllers
{
    [RoleAuthorize(RolesRequired = "HieuTruong")]
    public class QuanLyThoiKhoaBieuController : Controller
    {
        private readonly QuanLyTruongHocEntities1 db = new QuanLyTruongHocEntities1();
        private readonly GeneticAlgorithmService gaService = new GeneticAlgorithmService();



        [HttpGet]
        public ActionResult Index()
        {
            int? namHocId = null;
            int? hocKyId = null;
            int? tuan = null;
            string caHoc = "Sang";

            if (Request.QueryString["NamHocID"] != null)
                namHocId = int.Parse(Request.QueryString["NamHocID"]);

            if (Request.QueryString["HocKyID"] != null)
                hocKyId = int.Parse(Request.QueryString["HocKyID"]);

            if (Request.QueryString["tuan"] != null)
                tuan = int.Parse(Request.QueryString["tuan"]);

            if (Request.QueryString["caHoc"] != null)
                caHoc = Request.QueryString["caHoc"];

            // nếu chưa truyền năm + học kỳ thì lấy bộ đã có TKB
            if (!namHocId.HasValue || !hocKyId.HasValue)
            {
                var last = db.ThoiKhoaBieu
                    .OrderByDescending(x => x.NamHocID)
                    .ThenByDescending(x => x.HocKyID)
                    .FirstOrDefault();

                if (last != null)
                {
                    namHocId = last.NamHocID;
                    hocKyId = last.HocKyID;
                }
            }

            // ✅ CHỈ TẠO 1 MODEL
            var model = new TimLopViewModel
            {
                NamHocID = namHocId ?? 0,
                HocKyID = hocKyId ?? 0,
                CaHoc = caHoc,

                ListNamHoc = db.NamHoc
                    .AsEnumerable()
                    .OrderBy(x => x.TenNamHoc)
                    .Select(x => new SelectListItem
                    {
                        Value = x.NamHocID.ToString(),
                        Text = x.TenNamHoc
                    }).ToList(),

                ListHocKy = db.HocKy
                    .AsEnumerable()
                    .OrderBy(x => x.TenHocKy)
                    .Select(x => new SelectListItem
                    {
                        Value = x.HocKyID.ToString(),
                        Text = x.TenHocKy
                    }).ToList()
            };

            if (namHocId.HasValue && hocKyId.HasValue)
            {
                int tuanHienThi = tuan ??
                    db.ThoiKhoaBieu
                        .Where(x => x.NamHocID == namHocId && x.HocKyID == hocKyId)
                        .Min(x => (int?)x.Tuan) ?? 1;

                model.Tuan = tuanHienThi;

                //model.ThoiKhoaBieus = db.ThoiKhoaBieu
                //    .Where(x =>
                //        x.NamHocID == namHocId &&
                //        x.HocKyID == hocKyId &&
                //        x.Tuan == tuanHienThi)
                //    .OrderBy(x => x.Thu)
                //    .ThenBy(x => x.TietHoc)
                //    .ThenBy(x => x.TietHoc)
                //    .ToList();

                var ds = db.ThoiKhoaBieu
                    .Where(x =>
                        x.NamHocID == namHocId &&
                        x.HocKyID == hocKyId &&
                        x.Tuan == tuanHienThi)
                    .ToList();

                ////  LỌC THEO CA ĐANG XEM
                //if (model.CaHoc == "Sang")
                //{
                //    ds = ds.Where(x =>
                //        (x.LopHoc.CaHoc == "SANG" && x.TietHoc <= 5) ||
                //        (x.LopHoc.CaHoc == "CHIEU" && x.TietHoc >= 6)
                //    ).ToList();
                //}
                //else // Chieu
                //{
                //    ds = ds.Where(x =>
                //        (x.LopHoc.CaHoc == "CHIEU" && x.TietHoc <= 5) ||
                //        (x.LopHoc.CaHoc == "SANG" && x.TietHoc >= 6)
                //    ).ToList();
                //}

                //model.ThoiKhoaBieus = ds;

                //cai moi hon
                if (model.CaHoc == "Sang")
                {
                    ds = ds.Where(x =>
                        x.LopHoc.CaHoc == "SANG" && (
                            // tiết chính buổi sáng
                            (x.TietHoc >= 1 && x.TietHoc <= 5 &&
                             x.MonHoc.TenMonHoc != "Thể dục" &&
                             x.MonHoc.TenMonHoc != "Tin học")
                            ||
                            // thể dục + tin buổi chiều
                            (x.TietHoc >= 6 && x.TietHoc <= 10 &&
                             (x.MonHoc.TenMonHoc == "Thể dục" ||
                              x.MonHoc.TenMonHoc == "Tin học"))
                        )
                    ).ToList();
                }
                else // Chieu
                {
                    ds = ds.Where(x =>
                        x.LopHoc.CaHoc == "CHIEU" && (
                            // tiết chính buổi chiều
                            (x.TietHoc >= 6 && x.TietHoc <= 10 &&
                             x.MonHoc.TenMonHoc != "Thể dục" &&
                             x.MonHoc.TenMonHoc != "Tin học")
                            ||
                            // thể dục + tin buổi sáng
                            (x.TietHoc >= 1 && x.TietHoc <= 5 &&
                             (x.MonHoc.TenMonHoc == "Thể dục" ||
                              x.MonHoc.TenMonHoc == "Tin học"))
                        )
                    ).ToList();
                }
                model.ThoiKhoaBieus = ds;
            }

            return View(model);
        }



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


            //tao cho 1 tuan dda
            // ✅ 6. LƯU THỜI KHÓA BIỂU TUẦN 1 (TUẦN MẪU)
            foreach (var g in best.Genes)
            {
                db.ThoiKhoaBieu.Add(new ThoiKhoaBieu
                {
                    LopHocID = g.LopHocID,
                    MonHocID = g.MonHocID,
                    GiaoVienID = g.GiaoVienID,
                    PhongHocID = g.PhongHocID,
                    HocKyID = g.HocKyID,
                    NamHocID = NamHocID,
                    Tuan = 1,                  // ✅ LUÔN LÀ TUẦN 1
                    Thu = g.Thu,
                    TietHoc = g.Tiet
                });
            }

            db.SaveChanges();
            db.SaveChanges();

            TempData["Success"] = "✅ Tạo thời khóa biểu thành công!";
            return RedirectToAction("Index");
        }

        private List<Gene> TaoBaseGenes(List<PhanCongGiangDay> phanCong, int hocKyId)
        {
            var genes = new List<Gene>();

            if (!phanCong.Any())
                return genes;

            var firstPC = phanCong.First();
            if (!firstPC.LopHoc.NienKhoa.HasValue)
                return genes;

            int namHocId = firstPC.LopHoc.NienKhoa.Value;

            int tongTuanNam = db.HocKy
                .Where(h => h.NamHocID == namHocId)
                .Sum(h => h.SoTuanThucHoc);

            if (tongTuanNam <= 0)
                return genes;

            foreach (var pc in phanCong)
            {
                if (!pc.GiaoVien.MonHocID.HasValue)
                    continue;

                int soTietNam = db.MonHocKhoi
                    .Where(m =>
                        m.KhoiLopID == pc.LopHoc.KhoiLopID &&
                        m.MonHocID == pc.GiaoVien.MonHocID.Value)
                    .Select(m => m.SoTietNam)
                    .FirstOrDefault();

                if (soTietNam <= 0)
                    continue;

                // ✅ ĐẾM SỐ TIẾT TUẦN 1 (LỊCH GỐC)
                int soTietTuan1 = (int)Math.Round((double)soTietNam / tongTuanNam);

                if (soTietTuan1 < 1) soTietTuan1 = 1;
                if (soTietTuan1 > 5) soTietTuan1 = 5;

                // ✅ TẠO ĐỦ GENE CHO TUẦN 1
                for (int i = 0; i < soTietTuan1; i++)
                {
                    genes.Add(new Gene
                    {
                        LopHocID = pc.LopHocID,
                        KhoiLopID = pc.LopHoc.KhoiLopID,
                        MonHocID = pc.GiaoVien.MonHocID.Value,
                        GiaoVienID = pc.GiaoVienID,
                        PhongHocID = pc.LopHoc.PhongHocID,
                        HocKyID = hocKyId,
                        Tuan = 1,
                        CaHoc = pc.LopHoc.CaHoc
                    });
                }
            }

            return genes;
        }


        private int LamTronSoTietTuan(double tietTuan)
        {
            if (tietTuan < 0.75) return 0;
            if (tietTuan < 1.5) return 1;
            if (tietTuan < 2.5) return 2;
            if (tietTuan < 3.5) return 3;
            if (tietTuan < 4.5) return 4;
            return 5;

        }

    }
}


