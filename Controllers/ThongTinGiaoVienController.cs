using demomvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using demomvc.App_Start;

namespace demomvc.Controllers
{
    [RoleAuthorize(RolesRequired = "HieuTruong,GiaoVien,HieuPho,BiThu")]
    public class ThongTinGiaoVienController : Controller
    {
        QuanLyTruongHocEntities2 db = new QuanLyTruongHocEntities2();

        // ================================
        //       TRANG THÔNG TIN GIÁO VIÊN
        // ================================
        public ActionResult Index()
        {
            int nguoidungID = (int)Session["UserID"];

            // Lấy thông tin giáo viên + user
            var model = db.GiaoVien
                .Include(g => g.NguoiDung)
                .Include(g => g.MonHoc)      // GiaoVien → MonHoc
                .Include(g => g.MonHoc.BoMon) // MonHoc → BoMon
                .FirstOrDefault(g => g.NguoiDungID == nguoidungID);

            if (model == null) return HttpNotFound("Không tìm thấy giáo viên.");

            // =============================
            //  Lấy danh sách lớp giáo viên chủ nhiệm
            // =============================
            var lopChuNhiem = db.LopHoc
                .Include(l => l.NamHoc)
                .Where(l => l.GiaoVienChuNhiem == model.GiaoVienID)
                .OrderByDescending(l => l.NamHoc.TenNamHoc)
                .ToList();

            ViewBag.LopChuNhiem = lopChuNhiem;

            // =============================
            //        LẤY MÔN GIÁO VIÊN DẠY
            // =============================
            // GiaoVien.MonHocID → MonHoc → BoMon
            var monDay = db.MonHoc
                .Include(m => m.BoMon)
                .Where(m => m.MonHocID == model.MonHocID)
                .ToList();

            ViewBag.MonDay = monDay;

            return View("TTgiaovien", model);
        }

    
        public ActionResult CapNhatGV(int id)
        {
            var giaovien = db.GiaoVien
                .Include(g => g.NguoiDung)
                .Include(g => g.MonHoc)
                .Include(g => g.MonHoc.BoMon)
                .FirstOrDefault(g => g.GiaoVienID == id);

            if (giaovien == null) return HttpNotFound();

            ViewBag.NguoiDung = db.NguoiDung.ToList();
            ViewBag.MonHoc = db.MonHoc.ToList(); // vì giáo viên thuộc MonHoc, không phải BoMon
            return View(giaovien);
        }

        [HttpPost]
        public ActionResult CapNhatGV(GiaoVien gv)
        {
            // VALIDATION
            if (string.IsNullOrWhiteSpace(gv.NguoiDung?.HoTen))
                ModelState.AddModelError("NguoiDung.HoTen", "Không được để trống họ tên");

            if (string.IsNullOrWhiteSpace(gv.NguoiDung.Email))
                ModelState.AddModelError("NguoiDung.Email", "Không được để trống Email");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(gv.NguoiDung.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                ModelState.AddModelError("NguoiDung.Email", "Email không hợp lệ");

            if (string.IsNullOrWhiteSpace(gv.NguoiDung.SDT))
                ModelState.AddModelError("NguoiDung.SDT", "Không được để trống số điện thoại");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(gv.NguoiDung.SDT, @"^0\d{9}$"))
                ModelState.AddModelError("NguoiDung.SDT", "Số điện thoại không hợp lệ");

            if (!ModelState.IsValid)
                return View(gv);

            // UPDATE DB
            var giaovien = db.GiaoVien
                .Include("NguoiDung")
                .Include(g => g.MonHoc)
                .FirstOrDefault(x => x.GiaoVienID == gv.GiaoVienID);

            if (giaovien == null) return HttpNotFound();

            giaovien.NguoiDung.HoTen = gv.NguoiDung.HoTen;
            giaovien.GioiTinh = gv.GioiTinh;
            giaovien.NgaySinh = gv.NgaySinh;
            giaovien.NguoiDung.Email = gv.NguoiDung.Email;
            giaovien.NguoiDung.SDT = gv.NguoiDung.SDT;

            db.SaveChanges();

            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }


      
        public ActionResult CapNhatAvataGV()
        {
            int nguoidungId = (int)Session["UserID"];

            var giaovien = db.GiaoVien
                .Include(g => g.NguoiDung)
                .FirstOrDefault(g => g.NguoiDungID == nguoidungId);

            if (giaovien == null) return HttpNotFound();

            return View(giaovien);
        }

        [HttpPost]
        public ActionResult CapNhatAvataGV(HttpPostedFileBase AvatarFile)
        {
            if (AvatarFile == null || AvatarFile.ContentLength == 0)
            {
                TempData["Error"] = "Vui lòng chọn ảnh hợp lệ.";
                return RedirectToAction("CapNhatAvataGV");
            }

            int nguoidungId = (int)Session["UserID"];

            var gv = db.GiaoVien
                .Include(g => g.NguoiDung)
                .FirstOrDefault(g => g.NguoiDungID == nguoidungId);

            if (gv == null) return HttpNotFound();

            string fileName = Guid.NewGuid() + System.IO.Path.GetExtension(AvatarFile.FileName);
            string path = Server.MapPath("~/img/" + fileName);
            AvatarFile.SaveAs(path);

            gv.NguoiDung.AnhDaiDien = "/img/" + fileName;
            db.SaveChanges();

            Session["AvatarUrl"] = "/img/" + fileName;

            TempData["Success"] = "Cập nhật ảnh đại diện thành công!";
            return RedirectToAction("Index");
        }
    }
}
