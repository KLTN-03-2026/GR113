using demomvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using System.Text;
using System.Net.NetworkInformation;
using demomvc.App_Start;
namespace demomvc.Controllers
{
    [RoleAuthorize(RolesRequired = "HieuTruong")]
    public class QuanLyPhongHocController : Controller
    {
        QuanLyTruongHocEntities1 db = new QuanLyTruongHocEntities1();
        // GET: QuanLyPhongHoc
        public ActionResult Index(string keyword)
        {
            keyword = RemoveDiacritics(keyword ?? "").ToLower().Trim();

            var danhsachphong = db.PhongHoc.ToList()
                .Select(p => new PhongHocViewModel
                {
                    PhongHocID = p.PhongHocID,
                    TenPhong = p.TenPhong,
                    SoLuongChoNgoi = p.SoLuongChoNgoi ?? 0,
                    LoaiPhong = p.LoaiPhong,
                    TrangThai = p.TrangThai,
                    TenPhongKhongDau = RemoveDiacritics(p.TenPhong).ToLower()
                })
                .ToList();

            if (!string.IsNullOrEmpty(keyword))
            {
                danhsachphong = danhsachphong
                    .Where(x => x.TenPhongKhongDau.Contains(keyword))   // chỉ tìm theo tên phòng
                    .ToList();
            }

            ViewBag.Keyword = keyword;
            return View(danhsachphong);
        }

        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public ActionResult ThemMoiPhong()
        {
            return View();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemMoiPhong(PhongHocViewModel model)
        {
            if (ModelState.IsValid)
            {
                //ktr trungf  phongf 
                bool phongTrung = db.PhongHoc.Any(p => p.TenPhong.Trim().ToLower() == model.TenPhong.Trim().ToLower());
                if (phongTrung)
                {
                    ModelState.AddModelError("TenPhong", "Tên phòng đã tồn tại!");
                    return View(model);
                }

                var phong = new PhongHoc
                {
                    TenPhong = model.TenPhong,
                    LoaiPhong = model.LoaiPhong,
                    SoLuongChoNgoi = model.SoLuongChoNgoi,
                    TrangThai = model.TrangThai
                };

                db.PhongHoc.Add(phong);
                db.SaveChanges();
                TempData["Success"] = "Thêm phòng học thành công!";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var phong = db.PhongHoc.FirstOrDefault(p => p.PhongHocID == id);
            if(phong == null)
            {
                TempData["Error"]= "Phòng không tồn tại!";
                return RedirectToAction("Index");
            }

            bool isUsed = db.ThoiKhoaBieu.Any(t => t.PhongHocID == id);

            if (isUsed)
            {
                TempData["Error"] = "Phòng đang được sử dụng trong thời khóa biểu. Vui lòng đổi sang phòng khác trước khi xóa!";
                             return RedirectToAction("Index");
            }
          
            db.PhongHoc.Remove(phong);
            db.SaveChanges();

            TempData["Success"] = "Xóa phòng thành công!";
            return RedirectToAction("Index");
        }
        public ActionResult CapNhatPhong(int id)
        {
            if (id <= 0)
                return HttpNotFound();

            var phong = db.PhongHoc.FirstOrDefault(p => p.PhongHocID == id);
            if (phong == null)
                return HttpNotFound();

            var model = new PhongHocViewModel
            {
                PhongHocID = phong.PhongHocID,
                TenPhong = phong.TenPhong,
                SoLuongChoNgoi = phong.SoLuongChoNgoi ?? 0,
                LoaiPhong = phong.LoaiPhong,
                TrangThai = phong.TrangThai
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatPhong(PhongHocViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Kiểm tra trùng tên (không tính phòng hiện tại)
            bool trungTen = db.PhongHoc.Any(p =>
                p.TenPhong.Trim().ToLower() == model.TenPhong.Trim().ToLower()
                && p.PhongHocID != model.PhongHocID
            );

            if (trungTen)
            {
                ModelState.AddModelError("TenPhong", "Tên phòng đã tồn tại!");
                return View(model);
            }

            var phong = db.PhongHoc.Find(model.PhongHocID);
            if (phong == null)
                return HttpNotFound();

            phong.TenPhong = model.TenPhong;
            phong.LoaiPhong = model.LoaiPhong;
            phong.SoLuongChoNgoi = model.SoLuongChoNgoi;
            phong.TrangThai = model.TrangThai;

            db.SaveChanges();

            TempData["Success"] = "Cập nhật phòng học thành công!";
            return RedirectToAction("Index");
        }




    }
}