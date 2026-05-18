using demomvc.App_Start;
using demomvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace demomvc.Controllers
{
    [RoleAuthorize(RolesRequired = "HieuTruong")]
    public class QuanLyLopHocController : Controller
    {
        QuanLyTruongHocEntities2 db = new QuanLyTruongHocEntities2();

        // GET: QuanLyLopHoc
        public ActionResult Index()
        {
            var namHocList = db.NamHoc
                .OrderBy(x => x.TenNamHoc)
                .ToList(); //  CẮT LINQ TO ENTITIES

            var model = new TimLopViewModel
            {
                ListNamHoc = namHocList

                    .Select(x => new SelectListItem
                    {
                        Value = x.NamHocID.ToString(), //  OK (LINQ to Objects)
                        Text = x.TenNamHoc
                    })
                    .ToList(),

                ListLop = new List<LopHocViewModel>()
            };

            return View(model);
        }


        // Khi bấm XEM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XemLop(TimLopViewModel model)
        {
            var vm = new TimLopViewModel();

            // FIX LỖI ToString()
            vm.ListNamHoc = db.NamHoc
                .OrderBy(x => x.TenNamHoc)
                .ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.NamHocID + "",   // FIX ✔
                    Text = x.TenNamHoc
                })
                .ToList();

            // Nếu chưa chọn năm học
            if (!model.NamHocID.HasValue || model.NamHocID == 0)
            {
                ModelState.AddModelError("NamHocID", "Vui lòng chọn năm học");
                vm.ListLop = new List<LopHocViewModel>();
                return View("Index", vm);
            }
            // Nếu có chọn năm học
            if (model.NamHocID > 0)
            {
                vm.ListLop = db.LopHoc
                    .Where(l => l.NienKhoa == model.NamHocID)
                    .Select(l => new
                    {
                        Lop = l,
                        Khoi = db.KhoiLop.FirstOrDefault(k => k.KhoiLopID == l.KhoiLopID)
                    })
                    .OrderBy(x => x.Khoi.KhoiLopID) // Sắp xếp tăng dần theo khối
                    .Select(x => new LopHocViewModel
                    {
                        LopHocID = x.Lop.LopHocID,
                        TenLop = x.Lop.TenLop,
                        SiSo = db.HocSinh.Count(h => h.LopHocID == x.Lop.LopHocID),
                        GiaoVienChuNhiem = db.GiaoVien
                            .Where(g => g.GiaoVienID == x.Lop.GiaoVienChuNhiem)
                            .Select(g => g.NguoiDung.HoTen)
                            .FirstOrDefault() ?? "Không có GVCN",
                        TenKhoi = x.Khoi.TenKhoi,
                        CaHoc = x.Lop.CaHoc,
                        TrangThaiNamHoc = x.Khoi.TrangThai // trạng thái năm học
                    })
                    .ToList();
            }
            else
            {
                vm.ListLop = new List<LopHocViewModel>();
            }

            vm.NamHocID = model.NamHocID;

            return View("Index", vm);

        }
        //xóa loppw
        public ActionResult Delete(int id)
        {
            var lop = db.LopHoc.FirstOrDefault(p => p.LopHocID == id);
            if (lop == null)
            {
                TempData["Error"] = "Lớp không tồn tại!";
                return RedirectToAction("Index");
            }

            bool isUsed = db.ThoiKhoaBieu.Any(t => t.LopHocID == id);

            if (isUsed)
            {
                TempData["Error"] = "Lớp được sử dụng trong thời khóa biểu. Vui lòng đổi sang lớp khác trước khi xóa!";
                return RedirectToAction("Index");
            }


            db.LopHoc.Remove(lop);
            db.SaveChanges();

            TempData["Success"] = "Xóa Lớp thành công!";
            return RedirectToAction("Index");
        }
        public ActionResult ThemMoiLop()
        {
            var model = new ThemLopViewModel();

            model.ListGiaoVien = db.GiaoVien
                .ToList()
                .Select(g => new SelectListItem
                {
                    Value = g.GiaoVienID.ToString(),
                    Text = g.NguoiDung.HoTen
                }).ToList();


            model.ListNamHoc = db.NamHoc
                .ToList()
                .Select(n => new SelectListItem
                {
                    Value = n.NamHocID.ToString(),
                    Text = n.TenNamHoc
                }).ToList();


            model.ListKhoiLop = db.KhoiLop
                .ToList()
                .Select(k => new SelectListItem
                {
                    Value = k.KhoiLopID.ToString(),
                    Text = k.TenKhoi
                }).ToList();


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemMoiLop(ThemLopViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ListGiaoVien = db.GiaoVien.ToList()
       .Select(g => new SelectListItem
       {
           Value = g.GiaoVienID.ToString(),
           Text = g.NguoiDung.HoTen
       }).ToList();

                model.ListNamHoc = db.NamHoc.ToList()
                    .Select(n => new SelectListItem
                    {
                        Value = n.NamHocID.ToString(),
                        Text = n.TenNamHoc
                    }).ToList();

                model.ListKhoiLop = db.KhoiLop.ToList()
                    .Select(k => new SelectListItem
                    {
                        Value = k.KhoiLopID.ToString(),
                        Text = k.TenKhoi
                    }).ToList();
                //TempData["Error"] = "Dữ liệu không hợp lệ!";
                return View(model);


            }
            //trung ten lop
            var ktr = db.LopHoc.Any(x => x.TenLop.ToLower() == model.TenLop.ToLower() && x.NienKhoa == model.NamHocID);
            if (ktr)
            {
                LoadDropdown(model);
                TempData["Error"] = "Tên lớp đã tồn tại trong năm học này!";
               
                return View(model);
            }

            LopHoc lop = new LopHoc
            {
                TenLop = model.TenLop,
                GiaoVienChuNhiem = model.GiaoVienID.HasValue ? model.GiaoVienID.Value : (int?)null, // nếu không chọn thì null
                NienKhoa = model.NamHocID,
                KhoiLopID = model.KhoiLopID,
                CaHoc = model.CaHoc

            };

            db.LopHoc.Add(lop);
            db.SaveChanges();

            TempData["Success"] = "Thêm lớp thành công!";
            return RedirectToAction("Index");
        }
        private void LoadDropdown(ThemLopViewModel model)
        {
            model.ListGiaoVien = db.GiaoVien
                .ToList()
                .Select(g => new SelectListItem
                {
                    Value = g.GiaoVienID.ToString(),
                    Text = g.NguoiDung.HoTen
                }).ToList();

            model.ListNamHoc = db.NamHoc
                .ToList()
                .Select(n => new SelectListItem
                {
                    Value = n.NamHocID.ToString(),
                    Text = n.TenNamHoc
                }).ToList();

            model.ListKhoiLop = db.KhoiLop
                .ToList()
                .Select(k => new SelectListItem
                {
                    Value = k.KhoiLopID.ToString(),
                    Text = k.TenKhoi
                }).ToList();
        }
        public ActionResult PhanCongChuNhiem()
        {
            // 1. Lấy tất cả lớp ra list 
            var lopList = db.LopHoc.ToList();
            var khoiList = db.KhoiLop.ToList();
            // 2. Lấy danh sách giáo viên ra list 
            var gvList = db.GiaoVien
                .Join(db.NguoiDung,
                      gv => gv.NguoiDungID,
                      nd => nd.NguoiDungID,
                      (gv, nd) => new { gv.GiaoVienID, nd.HoTen })
                .ToList();

            // 3. Tạo ViewModel
            var model = lopList.Select(l => new PhanCongChuNhiemVM
            {
                LopHocID = l.LopHocID,
                TenLop = l.TenLop,
                TenKhoi = khoiList.FirstOrDefault(k => k.KhoiLopID == l.KhoiLopID)?.TenKhoi ?? "Chưa có khối",
                GiaoVienChuNhiemID = l.GiaoVienChuNhiem,
                TenGiaoVien = l.GiaoVienChuNhiem != null
                                ? gvList.FirstOrDefault(g => g.GiaoVienID == l.GiaoVienChuNhiem)?.HoTen
                                : "Chưa có GVCN",
                // Tạo dropdown từ list thuần
                ListGiaoVien = gvList
                    .Select(g => new SelectListItem
                    {
                        Value = g.GiaoVienID.ToString(),
                        Text = g.HoTen
                    })
                    .ToList()

            }).ToList();

            return View(model);
        }






        [HttpPost]
        public ActionResult CapNhatChuNhiem(int lopHocID, int giaoVienID)
        {
            var lop = db.LopHoc.FirstOrDefault(x => x.LopHocID == lopHocID);
            if (lop == null) return Json(new { success = false, message = "Lớp không tồn tại." });

            // Nếu chọn để trống, vẫn cho phép
            if (giaoVienID == 0)
            {
                lop.GiaoVienChuNhiem = null;
                db.SaveChanges();
                return Json(new { success = true });
            }

            // Lấy năm học của lớp hiện tại
            var namHoc = lop.NienKhoa;

            // Kiểm tra xem giáo viên này đã làm chủ nhiệm lớp khác trong cùng năm học chưa
            bool daCoLop = db.LopHoc.Any(l =>
                l.GiaoVienChuNhiem == giaoVienID &&
                l.NienKhoa == namHoc &&
                l.LopHocID != lopHocID); // bỏ qua lớp hiện tại

            if (daCoLop)
            {
                return Json(new { success = false, message = "Giáo viên đã có lớp chủ nhiệm trong năm học này!" });
            }

            // Cập nhật bình thường
            lop.GiaoVienChuNhiem = giaoVienID;
            db.SaveChanges();

            return Json(new { success = true });
        }


        public ActionResult CapNhatLop(int id)
        {
            var lop = db.LopHoc.FirstOrDefault(x => x.LopHocID == id);
            if (lop == null) return HttpNotFound();

            var khoi = db.KhoiLop.FirstOrDefault(k => k.KhoiLopID == lop.KhoiLopID);
            var namhoc = db.NamHoc.FirstOrDefault(n => n.NamHocID == lop.NienKhoa);


            var model = new LopHocViewModel
            {
                LopHocID = lop.LopHocID,
                TenLop = lop.TenLop,
                GiaoVienChuNhiem = lop.GiaoVienChuNhiem + "",
                TenKhoi = khoi?.TenKhoi,

            };

            var listGV = db.GiaoVien.ToList();    // 1. Load hoàn toàn vào bộ nhớ

            ViewBag.ListGiaoVien = listGV         // 2. Lúc này LINQ-to-Objects, an toàn
                .Select(g => new SelectListItem
                {
                    Value = g.GiaoVienID.ToString(),
                    Text = g.NguoiDung.HoTen
                })
                .ToList();


            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatLop(LopHocViewModel model)
        {
            var lop = db.LopHoc.FirstOrDefault(x => x.LopHocID == model.LopHocID);
            if (lop == null)
                return HttpNotFound();

            // Ép kiểu giáo viên
            int giaoVienID = string.IsNullOrEmpty(model.GiaoVienChuNhiem)
                ? 0
                : int.Parse(model.GiaoVienChuNhiem);

            int namHoc = lop.NienKhoa ?? 0;
            int lopHocID = lop.LopHocID;
            int khoiID = lop.KhoiLopID;   // dùng để kiểm tra trùng tên trong cùng khối


            bool tenLopTrung = db.LopHoc.Any(l =>
                l.TenLop.Trim().ToLower() == model.TenLop.Trim().ToLower() &&
                l.KhoiLopID == khoiID &&
                l.NienKhoa == namHoc &&
                l.LopHocID != lopHocID            // bỏ qua chính nó
            );


            if (tenLopTrung)
            {

                TempData["Error"] = "Tên lớp đã tồn tại trong năm học này!";
                return RedirectToAction("CapNhatLop", new { id = lopHocID });
            }


            if (giaoVienID != 0)
            {
                bool daCoLop = db.LopHoc.Any(l =>
                    l.GiaoVienChuNhiem == giaoVienID &&
                    l.NienKhoa == namHoc &&
                    l.LopHocID != lopHocID
                );

                if (daCoLop)
                {
                    TempData["Error"] = "Giáo viên đã có lớp chủ nhiệm trong năm học này!";
                    return RedirectToAction("CapNhatLop", new { id = lopHocID });
                }
            }


            lop.TenLop = model.TenLop;
            lop.GiaoVienChuNhiem = (giaoVienID == 0 ? (int?)null : giaoVienID);

            db.SaveChanges();
            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletelop(int id)
        {
            try
            {
                var lop = db.LopHoc.Find(id);
                if (lop == null)
                {
                    return Json(new { ok = false, msg = "Không có lớp này" });
                }

                if (db.ThoiKhoaBieu.Any(x => x.LopHocID == id))
                {
                    return Json(new { ok = false, msg = "Đã có thời khóa biểu" });
                }

                if (db.PhanCongGiangDay.Any(x => x.LopHocID == id))
                {
                    return Json(new { ok = false, msg = "Đã có phân công" });
                }

                if (db.HocSinh.Any(x => x.LopHocID == id))
                {
                    return Json(new { ok = false, msg = "Đã có học sinh" });
                }

                db.LopHoc.Remove(lop);
                db.SaveChanges();

                return Json(new { ok = true, msg = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = ex.Message });
            }
        }

        //[HttpPost]
        //public JsonResult LenLop(int namHocCu, int NamHocMoi)
        //{
        //    try
        //    {
        //        // ✅ 1. CHECK NĂM HỌC MỚI
        //        bool coNamHocMoi = db.NamHoc.Any(n => n.NamHocID == NamHocMoi);
        //        if (!coNamHocMoi)
        //        {
        //            return Json(new
        //            {
        //                success = false,
        //                message = "❌ Chưa có năm học mới!"
        //            });
        //        }

        //        // ✅ 2. LẤY LỚP NĂM CŨ
        //        var dsLopCu = db.LopHoc
        //            .Where(l => l.NienKhoa == namHocCu)
        //            .ToList();

        //        if (!dsLopCu.Any())
        //        {
        //            return Json(new
        //            {
        //                success = false,
        //                message = "❌ Không có lớp ở năm học cũ!"
        //            });
        //        }

        //        // ✅ 3. CHECK LỚP NĂM MỚI
        //        var dsLopMoi = db.LopHoc
        //            .Where(l => l.NienKhoa == NamHocMoi)
        //            .ToList();

        //        if (!dsLopMoi.Any())
        //        {
        //            return Json(new
        //            {
        //                success = false,
        //                message = "❌ Chưa tạo lớp cho năm học mới!"
        //            });
        //        }

        //        // ===== XỬ LÝ LÊN LỚP =====
        //        foreach (var lopCu in dsLopCu)
        //        {
        //            int khoiCu = lopCu.KhoiLopID;

        //            //  LỚP 9 → TỐT NGHIỆP
        //            if (khoiCu == 9)
        //            {
        //                var hs9 = db.HocSinh
        //                    .Where(h => h.LopHocID == lopCu.LopHocID)
        //                    .ToList();

        //                foreach (var hs in hs9)
        //                {
        //                    hs.TrangThaiHocTap = "Đã tốt nghiệp"; // 
        //                }

        //                continue;
        //            }

        //            int khoiMoi = khoiCu + 1;

        //            //  lấy đuôi tên lớp (VD: 6A -> A)
        //            string duoiTen = lopCu.TenLop.Substring(1);

        //            var lopMoi = db.LopHoc.FirstOrDefault(
        //                l => l.NienKhoa == NamHocMoi &&
        //                     l.KhoiLopID == khoiMoi &&
        //                     l.TenLop.EndsWith(duoiTen)
        //            );

        //            if (lopMoi == null)
        //            {
        //                continue;
        //            }

        //            //  lấy học sinh lớp cũ
        //            var dsHS = db.HocSinh
        //                .Where(h => h.LopHocID == lopCu.LopHocID)
        //                .ToList();

        //            foreach (var hs in dsHS)
        //            {
        //                hs.LopHocID = lopMoi.LopHocID;
        //            }
        //        }

        //        db.SaveChanges();

        //        return Json(new
        //        {
        //            success = true,
        //            message = "✅ Lên lớp thành công!"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = "❌ Lỗi: " + ex.Message
        //        });
        //    }
        //}
        [HttpPost]
        public JsonResult LenLop(int namHocCu, int NamHocMoi)
        {
            try
            {
                var namCuObj = db.NamHoc.FirstOrDefault(n => n.NamHocID == namHocCu);
                var namMoiObj = db.NamHoc.FirstOrDefault(n => n.NamHocID == NamHocMoi);

                if (namCuObj == null || namMoiObj == null)
                {
                    return Json(new { success = false, message = "❌ Năm học không hợp lệ!" });
                }

                int namBatDauCu = int.Parse(namCuObj.TenNamHoc.Substring(0, 4));
                int namBatDauMoi = int.Parse(namMoiObj.TenNamHoc.Substring(0, 4));

                if (namBatDauMoi <= namBatDauCu)
                {
                    return Json(new { success = false, message = "❌ Năm học mới phải lớn hơn năm học cũ!" });
                }

                var dsLopCu = db.LopHoc.Where(l => l.NienKhoa == namHocCu).ToList();
                if (!dsLopCu.Any())
                    return Json(new { success = false, message = "❌ Không có lớp ở năm học cũ!" });

                var dsLopMoi = db.LopHoc.Where(l => l.NienKhoa == NamHocMoi).ToList();
                if (!dsLopMoi.Any())
                    return Json(new { success = false, message = "❌ Chưa tạo lớp cho năm học mới!" });

                foreach (var lopCu in dsLopCu)
                {
                    int khoiCu = lopCu.KhoiLopID;

                    // ✅ Khối 9 → tốt nghiệp
                    if (khoiCu == 9)
                    {
                        var hs9 = db.HocSinh.Where(h => h.LopHocID == lopCu.LopHocID).ToList();
                        foreach (var hs in hs9)
                        {
                            hs.TrangThaiHocTap = "Đã tốt nghiệp";
                        }
                        continue;
                    }

                    int khoiMoi = khoiCu + 1;
                    string duoiTen = lopCu.TenLop.Substring(1);

                    var lopMoi = db.LopHoc.FirstOrDefault(l =>
                        l.NienKhoa == NamHocMoi &&
                        l.KhoiLopID == khoiMoi &&
                        l.TenLop.EndsWith(duoiTen)
                    );

                    if (lopMoi == null) continue;

                    var dsHS = db.HocSinh.Where(h => h.LopHocID == lopCu.LopHocID).ToList();
                    foreach (var hs in dsHS)
                    {
                        hs.LopHocID = lopMoi.LopHocID;
                    }
                }

                // ✅ cập nhật trạng thái năm học
                namCuObj.TrangThai = "Đã kết thúc";
                namMoiObj.TrangThai = "Đang học";

                db.SaveChanges();

                return Json(new { success = true, message = "✅ Lên lớp thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "❌ Lỗi: " + ex.Message });
            }
        }
    }
}