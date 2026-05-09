

using demomvc.App_Start;
using demomvc.Models;
using demomvc.Services.GA;
using demonvc.Services.GA;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using demomvc.Services.Hubs;
using System;
using System.Data.Entity.Infrastructure;
using DocumentFormat.OpenXml.Office2010.Excel;

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

            int? lopHocId = null;
            if (Request.QueryString["lopHocId"] != null)
            {
                lopHocId = int.Parse(Request.QueryString["lopHocId"]);
            }

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
            // new
            DateTime? ngayBatDauHoc = null;

            if (namHocId.HasValue && hocKyId.HasValue)
            {
                ngayBatDauHoc = db.HocKy
                    .Where(h => h.HocKyID == hocKyId && h.NamHocID == namHocId)
                    .Select(h => h.NgayBatDauHoc)
                    .FirstOrDefault();
            }
            // ✅ CHỈ TẠO 1 MODEL
            var model = new TimLopViewModel
            {
                NamHocID = namHocId ?? 0,
                HocKyID = hocKyId ?? 0,
                CaHoc = caHoc,
                //new
                NgayBatDauHoc = ngayBatDauHoc,

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
               
                int tuanHienTai = 1;

                if (ngayBatDauHoc.HasValue)
                {
                    DateTime start = ngayBatDauHoc.Value.Date;
                    DateTime today = DateTime.Now.Date;

                    int soNgay = (today - start).Days;

                    if (soNgay >= 0)
                    {
                        tuanHienTai = soNgay / 7 + 1;
                    }
                }

                int maxTuan = db.HocKy
                    .Where(h => h.HocKyID == hocKyId && h.NamHocID == namHocId)
                    .Select(h => h.SoTuanThucHoc)
                    .FirstOrDefault();

                if (tuanHienTai > maxTuan)
                {
                    tuanHienTai = maxTuan;
                }

                model.Tuan = tuan ?? tuanHienTai;

                //them moi ơ day --------------------------
                model.DsNgayNghi = db.NgayHoc
                       .Where(n =>
                           n.NamHocID == model.NamHocID &&
                           n.HocKyID == model.HocKyID &&
                           n.Tuan == model.Tuan &&
                           n.TrangThai == "NGHI"
                       )
                       .ToList();
                // -------------------------------------------

                //hien thi cho hoj bu
                model.DsHocBu = db.HocBu
                     .Where(h =>
                         db.NgayHoc.Any(n =>
                             n.NgayHocID == h.NgayHocBuID &&
                             n.NamHocID == model.NamHocID &&
                             n.HocKyID == model.HocKyID
                         )
                     )
                     .ToList();
                model.DsNgayHoc = db.NgayHoc
                    .Where(x =>
                        x.NamHocID == model.NamHocID &&
                        x.HocKyID == model.HocKyID
                    ).ToList();
                //-----------------------------------------

                var ds = db.ThoiKhoaBieu
                    .Where(x =>
                        x.NamHocID == namHocId &&
                        x.HocKyID == hocKyId &&
                        x.Tuan == //tuanHienThi
                                  model.Tuan
                                  )
                    .ToList();
                if (lopHocId.HasValue)
                {
                    ds = ds.Where(x => x.LopHocID == lopHocId.Value).ToList();
                }



                model.ListLopHoc = ds
                  .Select(x => x.LopHoc)
                  .Distinct()
                  .Select(l => new SelectListItem
                  {
                      Value = l.LopHocID.ToString(),
                      Text = l.TenLop
                  })
                  .OrderBy(x => x.Text)
                  .ToList();
                // srow giao vien

                model.ListGiaoVien = ds.
                    Select(x => x.GiaoVien)
                    .Distinct()
                    .Select(g => new SelectListItem
                    {
                        Value = g.GiaoVienID.ToString(),
                        Text = g.NguoiDung.HoTen
                    })
                    .OrderBy(x => x.Text)
                    .ToList();




                //neu
                // ✅ LỌC THEO CA LỚP – KHÔNG PHÂN BIỆT MÔN / TIẾT
                if (model.CaHoc == "Sang")
                {
                    ds = ds.Where(x => x.LopHoc.CaHoc == "SANG").ToList();
                }
                else // Chieu
                {
                    ds = ds.Where(x => x.LopHoc.CaHoc == "CHIEU").ToList();
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

            //chuyenr tuần 1 chuaane => các tuần còn lại của kh đó
            var tkbTuan1 = db.ThoiKhoaBieu.Where(x =>
            x.NamHocID == NamHocID &&
            x.HocKyID == HocKyID &&
            x.Tuan == 1).ToList();

            for (int tuan = 2; tuan <= soTuanHoc; tuan++)
            {
                foreach (var t in tkbTuan1)
                {
                    db.ThoiKhoaBieu.Add(new ThoiKhoaBieu
                    {
                        NamHocID = t.NamHocID,
                        HocKyID = t.HocKyID,
                        LopHocID = t.LopHocID,
                        MonHocID = t.MonHocID,
                        GiaoVienID = t.GiaoVienID,
                        PhongHocID = t.PhongHocID,
                        Thu = t.Thu,
                        TietHoc = t.TietHoc,
                        Tuan = tuan

                    });
                }
            }

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


        [HttpPost]
        public ActionResult UpdateTKB(XemTKB dto)
        {
            var tkb = db.ThoiKhoaBieu.Find(dto.TKBID);
            if (tkb == null)
            {
                return Json(new { oke = false, msg = "Không có tiết này" });
            }

            //chẹc rỗngc
            if (dto.LopHocId == 0 || dto.PhongHocID == 0 || dto.GiaoVienId == 0 || dto.MonHocId == 0)
            {
                return Json(new
                {
                    oke = false,
                    msg = "Vui lòng chọn đầy đủ thông tin"
                });
            }


            //check trung giao vien

            bool trungGV = db.ThoiKhoaBieu.Any(x =>
                   x.TKBID != tkb.TKBID &&
                   x.GiaoVienID == dto.GiaoVienId &&
                   x.Thu == tkb.Thu &&
                   x.TietHoc == tkb.TietHoc &&
                   x.Tuan == tkb.Tuan &&
                   x.NamHocID == tkb.NamHocID &&
                   x.HocKyID == tkb.HocKyID
               );


            if (trungGV)
                return Json(new { oke = false, msg = "Giáo viên bị trùng tiết" });

            //check trungf phong
            bool trungPhong = db.ThoiKhoaBieu.Any(
                x => x.TKBID != tkb.TKBID &&
                x.PhongHocID == dto.PhongHocID &&
                x.Thu == tkb.Thu &&
                x.TietHoc == tkb.TietHoc &&
                x.Tuan == tkb.Tuan &&
                x.NamHocID == tkb.NamHocID &&
                x.HocKyID == tkb.HocKyID
                );

            if (trungPhong)
            {
                return Json(new { oke = false, msg = "Phòng học đã bị trùng" });
            }

            //trung lop
            bool trungLop = db.ThoiKhoaBieu.Any(
                x => x.TKBID != tkb.TKBID &&
                x.LopHocID == dto.LopHocId &&
                x.Thu == tkb.Thu &&
                x.TietHoc == tkb.TietHoc &&
                x.Tuan == tkb.Tuan &&
                x.NamHocID == tkb.NamHocID &&
                x.HocKyID == tkb.HocKyID);

            if (trungLop)
            {
                return Json(new { oke = false, msg = "Lớp đã bị trùng tiết" });

            }

            if (tkb.GiaoVienID != dto.GiaoVienId || tkb.MonHocID != dto.MonHocId)
            {
                bool gvHopLe = db.GiaoVien.Any(g =>
                    g.GiaoVienID == dto.GiaoVienId &&
                    g.MonHocID == dto.MonHocId
                );

                if (!gvHopLe)
                {
                    return Json(new { oke = false, msg = "Giáo viên không dạy môn này" });
                }
            }


            //cap nhat
            tkb.LopHocID = dto.LopHocId;
            tkb.PhongHocID = dto.PhongHocID;
            tkb.MonHocID = dto.MonHocId;
            tkb.GiaoVienID = dto.GiaoVienId;

            db.SaveChanges();
            return Json(new { oke = true, msg = "Cập nhật thành công" });
        }

        [HttpGet]
        public ActionResult GetEditData(int namHocId, int hocKyId, int lopHocId, int? monHocId)
        {
            var lops = db.LopHoc
                .Where(x => x.NienKhoa == namHocId)
                .Select(x => new
                {
                    id = x.LopHocID,
                    text = x.TenLop
                }).ToList();

            var mons = db.MonHoc
                .Select(x => new
                {
                    id = x.MonHocID,
                    text = x.TenMonHoc
                }).ToList();


            var gvs = db.GiaoVien
                .Where(x => x.TrangThaiGiangDay != null && (!monHocId.HasValue || x.MonHocID == monHocId))
                .Select(x => new
                {
                    id = x.GiaoVienID,
                    text = x.NguoiDung.HoTen
                }).ToList();



            var phongs = db.PhongHoc
                .Select(x => new
                {
                    id = x.PhongHocID,
                    text = x.TenPhong
                }).ToList();



            return Json(new
            {
                lops,
                mons,
                gvs,
                phongs
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteTKB(int id)
        {
            var tkb = db.ThoiKhoaBieu.Find(id);
            if (tkb == null)
            {
                return Json(new { oke = false, msg = "Không có tiết này" });
            }

            db.ThoiKhoaBieu.Remove(tkb);
            db.SaveChanges();
            return Json(new { oke = true, msg = "Xóa Thành công" });
        }



        [HttpGet]
        public ActionResult addGetData(int namHocId, int hocKyId, int tuan, int thu, int tiet)
        {
            var lopHoc = db.LopHoc
                .Where(x => x.NienKhoa == namHocId)
                .Select(x => new
                {
                    id = x.LopHocID,
                    text = x.TenLop
                }).ToList();

            var monHoc = db.MonHoc
                        .Select(x => new
                        {
                            id = x.MonHocID,
                            text = x.TenMonHoc
                        }).ToList();

            var giaoVien = db.GiaoVien
                .Where(x => x.TrangThaiGiangDay != null)
                .Select(x => new
                {
                    id = x.GiaoVienID,
                    text = x.NguoiDung.HoTen
                }).ToList();

            var phongHoc = db.PhongHoc
               .Where(p => !db.ThoiKhoaBieu.Any(t =>
                   t.PhongHocID == p.PhongHocID &&
                   t.Thu == thu &&
                   t.TietHoc == tiet &&
                   t.Tuan == tuan &&
                   t.NamHocID == namHocId &&
                   t.HocKyID == hocKyId
               ))
               .Select(p => new
               {
                   id = p.PhongHocID,
                   text = p.TenPhong
               }).ToList();

            return Json(new { lopHoc, monHoc, giaoVien, phongHoc }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetGVByMon(int monHocId)
        {
            var gvs = db.GiaoVien
                .Where(x => x.MonHocID == monHocId && x.TrangThaiGiangDay != null)
                .Select(x => new
                {
                    id = x.GiaoVienID,
                    text = x.NguoiDung.HoTen
                }).ToList();

            return Json(gvs, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddTKB(XemTKB dtoAdd)
        {
            if (dtoAdd.LopHocId == 0
                 || dtoAdd.MonHocId == 0 ||
                 dtoAdd.GiaoVienId == 0 ||
                 dtoAdd.PhongHocID == 0)
            {
                return Json(new { oke = false, msg = "Vui lòng chọn đầy đủ Lớp, Môn, Giáo viên và Phòng học" });
            }
            //check trung giv
            bool trungGV = db.ThoiKhoaBieu.Any(x => x.GiaoVienID == dtoAdd.GiaoVienId &&
                                                 x.Thu == dtoAdd.Thu &&
                                                 x.TietHoc == dtoAdd.TietHoc &&
                                                 x.Tuan == dtoAdd.Tuan &&
                                                 x.NamHocID == dtoAdd.NamHocID &&
                                                 x.HocKyID == dtoAdd.HocKyID);
            if (trungGV)
            {
                return Json(new { oke = false, msg = "Giáo viên bị trùng tiết" });
            }

            //check trung phong
            bool trungPhong = db.ThoiKhoaBieu.Any(x =>
                                x.PhongHocID == dtoAdd.PhongHocID &&
                                x.Thu == dtoAdd.Thu &&
                                x.TietHoc == dtoAdd.TietHoc &&
                                x.Tuan == dtoAdd.Tuan &&
                                x.NamHocID == dtoAdd.NamHocID &&
                                x.HocKyID == dtoAdd.HocKyID);
            if (trungPhong)
            {
                return Json(new { oke = false, msg = "Phòng bị trùng tiết" });
            }

            //check trung lop
            bool trungLop = db.ThoiKhoaBieu.Any(x =>
                            x.LopHocID == dtoAdd.LopHocId &&
                            x.Tuan == dtoAdd.Tuan &&
                            x.Thu == dtoAdd.Thu &&
                            x.TietHoc == dtoAdd.TietHoc &&
                            x.NamHocID == dtoAdd.NamHocID &&
                            x.HocKyID == dtoAdd.HocKyID);
            if (trungLop)
            {
                return Json(new { oke = false, msg = "Lớp học bị trùng tiết" });
            }

            //check trung với học bù------------------------------------
            var hocBuList = db.HocBu.Where(h =>
                            db.NgayHoc.Any(n => n.NgayHocID == h.NgayHocBuID &&
                            n.NamHocID == dtoAdd.NamHocID &&
                            n.HocKyID == dtoAdd.HocKyID &&
                            n.Tuan == dtoAdd.Tuan &&
                            n.Thu == dtoAdd.Thu)
                            ).ToList();
            foreach(var hb in hocBuList)
            {
                var ngayNghi = db.NgayHoc.FirstOrDefault(n => n.NgayHocID == hb.NgayNghiID);
                if (ngayNghi == null) continue;

                bool trungHocBu = db.ThoiKhoaBieu.Any(x =>
                        x.NamHocID == dtoAdd.NamHocID &&
                        x.HocKyID == dtoAdd.HocKyID &&
                        x.Tuan == ngayNghi.Tuan &&   // tuần gốc
                        x.Thu == ngayNghi.Thu &&
                        x.TietHoc == dtoAdd.TietHoc &&
                        (
                            x.GiaoVienID == dtoAdd.GiaoVienId ||
                            x.PhongHocID == dtoAdd.PhongHocID ||
                            x.LopHocID == dtoAdd.LopHocId
                        )
                    );
                if (trungHocBu)
                {
                    return Json(new { oke = false, msg = "Trùng với lịch học bù" });
                }
            }

            //check giao vien dạy dung môn
            bool gvMon = db.GiaoVien.Any(x =>
                        x.GiaoVienID == dtoAdd.GiaoVienId &&
                        x.MonHocID == dtoAdd.MonHocId);
            if (!gvMon)
            {
                return Json(new { oke = false, msg = "Giáo viên không dạy môn này" });
            }

            var tkb = new ThoiKhoaBieu
            {
                LopHocID = dtoAdd.LopHocId,
                MonHocID = dtoAdd.MonHocId,
                GiaoVienID = dtoAdd.GiaoVienId,
                PhongHocID = dtoAdd.PhongHocID,
                Thu = dtoAdd.Thu,
                TietHoc = dtoAdd.TietHoc,
                Tuan = dtoAdd.Tuan,
                NamHocID = dtoAdd.NamHocID,
                HocKyID = dtoAdd.HocKyID
            };
            db.ThoiKhoaBieu.Add(tkb);
            db.SaveChanges();

            return Json(new { oke = true, msg = "thêm tiết thành công" });
        }

        [HttpPost]
        public JsonResult SetNgayBatDauHoc(int hocKyId, int namHocId, DateTime ngayBatDauHoc)
        {

            var hocKy = db.HocKy
                    .FirstOrDefault(h =>
                        h.HocKyID == hocKyId &&
                        h.NamHocID == namHocId
                    );
            if (hocKy == null)
            {
                return Json(new
                {
                    oke = false,
                    msg = "Không thấy học kì , năm học"
                });
            }
            hocKy.NgayBatDauHoc = ngayBatDauHoc;
            db.SaveChanges();

            // update HocKy.NgayBatDauHoc
            return Json(new { oke = true, msg = "Đã thiết lập" });
        }


        

        //new
        [HttpPost]
        public JsonResult DatNgayNghi(int namHocId, int hocKyId, DateTime ngayNghi)
        {
            var hocKy = db.HocKy
                .FirstOrDefault(h => h.HocKyID == hocKyId && h.NamHocID == namHocId);

            if (hocKy == null || !hocKy.NgayBatDauHoc.HasValue)
            {
                return Json(new { oke = false, msg = "Chưa thiết lập ngày học" });
            }

            DateTime start = hocKy.NgayBatDauHoc.Value.Date;
            int soNgay = (ngayNghi.Date - start).Days;

            if (soNgay < 0)
            {
                return Json(new { oke = false, msg = "Ngày nghỉ không hợp lệ" });
            }

            int tuan = soNgay / 7 + 1;
            int thu = ngayNghi.DayOfWeek == DayOfWeek.Sunday ? 8 : (int)ngayNghi.DayOfWeek + 1;

            bool coLichHoc = db.ThoiKhoaBieu.Any(x =>
                x.NamHocID == namHocId &&
                x.HocKyID == hocKyId &&
                x.Tuan == tuan &&
                x.Thu == thu &&
                x.TietHoc >= 1 && x.TietHoc <= 10
            );

            if (!coLichHoc)
            {
                return Json(new { oke = false, msg = "Ngày này không có lịch học" });
            }

            bool daCo = db.NgayHoc.Any(x =>
                x.Tuan == tuan &&
                x.Thu == thu &&
                x.HocKyID == hocKyId &&
                x.NamHocID == namHocId &&
                x.TrangThai == "NGHI"
            );

            if (daCo)
            {
                return Json(new { oke = false, msg = "Ngày này đã được đánh dấu nghỉ" });
            }

            // ✅ INSERT NGÀY NGHỈ
            var ngayNghiEntity = new NgayHoc
            {
                Ngay = ngayNghi,
                Tuan = tuan,
                Thu = thu,
                TrangThai = "NGHI",
                HocKyID = hocKyId,
                NamHocID = namHocId
            };

            db.NgayHoc.Add(ngayNghiEntity);
            db.SaveChanges();



            // ✅ KHÔNG query lại — dùng luôn object vừa insert
            var NgayNghiObj = ngayNghiEntity;

            // =======================
            // ✅ TÌM NGÀY HỌC BÙ
            // =======================

            NgayHoc ngayHocBu = null;

            int maxTuan = db.HocKy
                .Where(h => h.HocKyID == hocKyId && h.NamHocID == namHocId)
                .Select(h => h.SoTuanThucHoc)
                .FirstOrDefault();

            int maxtuanbonus = maxTuan + 5;

            for (int t = tuan + 1; t <= maxtuanbonus; t++)
            {
                var dsThu = new List<int> { thu }
                    .Concat(Enumerable.Range(2, 6).Where(x => x != thu));

                foreach (int th in dsThu)
                {
                    bool daCoTKB = db.ThoiKhoaBieu.Any(x =>
                        x.NamHocID == namHocId &&
                        x.HocKyID == hocKyId &&
                        x.Tuan == t &&
                        x.Thu == th
                    );

                    bool laNgayNghiKhac = db.NgayHoc.Any(x =>
                        x.NamHocID == namHocId &&
                        x.HocKyID == hocKyId &&
                        x.Tuan == t &&
                        x.Thu == th &&
                        x.TrangThai == "NGHI"
                    );

                    // ✅ FIX CHÍNH: tách exist ra trước
                    var existNgayHoc = db.NgayHoc.FirstOrDefault(x =>
                        x.Tuan == t &&
                        x.Thu == th &&
                        x.NamHocID == namHocId &&
                        x.HocKyID == hocKyId
                    );

                    bool daDuocDungLamHocBu = existNgayHoc != null &&
                        db.HocBu.Any(h => h.NgayHocBuID == existNgayHoc.NgayHocID);

                    bool laNgayNghi = existNgayHoc != null &&
                                      existNgayHoc.TrangThai == "NGHI";

                    // ✅ điều kiện đầy đủ
                    if (!daCoTKB &&
                        !laNgayNghiKhac &&
                        !daDuocDungLamHocBu &&
                        !laNgayNghi)
                    {
                        ngayHocBu = existNgayHoc;

                        if (ngayHocBu == null)
                        {
                            ngayHocBu = new NgayHoc
                            {
                                Tuan = t,
                                Thu = th,
                                NamHocID = namHocId,
                                HocKyID = hocKyId,
                                TrangThai = "HOC",

                                // ✅ tính ngày thật
                                Ngay = start.AddDays((t - 1) * 7 + (th - 2))
                            };

                            db.NgayHoc.Add(ngayHocBu);

                            db.SaveChanges();
                        }

                        break;
                    }
                }

                if (ngayHocBu != null)
                    break;
            }

            // ✅ nếu không tìm được slot
            if (ngayHocBu == null)
            {
                return Json(new
                {
                    oke = false,
                    msg = "Không tìm được ngày học bù phù hợp"
                });
            }

            // =========================
            // INSERT HỌC BÙ
            // =========================

            db.HocBu.Add(new HocBu
            {
                NgayHocBuID = ngayHocBu.NgayHocID,
                NgayNghiID = NgayNghiObj.NgayHocID,
                GhiChu = $"Học bù từ tuần {tuan}"
            });

            db.SaveChanges();


            return Json(new { oke = true, msg = "Đã đánh dấu nghỉ học" });
        }
        [HttpPost]
        public JsonResult BoNgayNghi(int namHocId, int hocKyId, int tuan, int thu)
        {
            var ngayNghi = db.NgayHoc.FirstOrDefault(x =>
                            x.NamHocID == namHocId &&
                            x.HocKyID == hocKyId &&
                            x.Tuan == tuan &&
                            x.Thu == thu &&
                            x.TrangThai == "NGHI");
            if (ngayNghi == null)
            {
                return Json(new { oke = false, msg = "Không có ngày nghỉ" });
            }
            var hocBuList = db.HocBu.Where(h => h.NgayNghiID == ngayNghi.NgayHocID).ToList();
            foreach(var hb in hocBuList)
            {
                var ngayBu = db.NgayHoc.FirstOrDefault(n => n.NgayHocID == hb.NgayHocBuID);
                if (ngayBu != null)
                {
                    db.NgayHoc.Remove(ngayBu);
                }
                db.HocBu.Remove(hb);
            }

            db.NgayHoc.Remove(ngayNghi);
            db.SaveChanges();
            return Json(new { oke = true, msg = "Đã hủy ngày nghỉ" });
        }

    }
}


