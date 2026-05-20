using demomvc.App_Start;
using demomvc.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace demomvc.Controllers
{
    [RoleAuthorize(RolesRequired = "HieuTruong,GiaoVien,HocSinh,HieuPho,BiThu")]
    public class DoiMatKhauController : Controller
    {
        QuanLyTruongHocEntities2 db = new QuanLyTruongHocEntities2();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DoiMatKhau(string MatKhauCu, string MatKhauMoi, string XacNhanMatKhau)
        {
            // =========================
            // 1️⃣ VALIDATE
            // =========================
            if (string.IsNullOrWhiteSpace(MatKhauCu))
                ModelState.AddModelError("MatKhauCu", "Nhập mật khẩu cũ");

            if (string.IsNullOrWhiteSpace(MatKhauMoi))
                ModelState.AddModelError("MatKhauMoi", "Nhập mật khẩu mới");

            if (MatKhauMoi != XacNhanMatKhau)
                ModelState.AddModelError("XacNhanMatKhau", "Xác nhận mật khẩu không khớp");

            if (!ModelState.IsValid)
                return View("Index");

            // =========================
            // 2️⃣ CHECK LOGIN
            // =========================
            if (Session["UserID"] == null)
            {
                ModelState.AddModelError("", "Bạn chưa đăng nhập");
                return View("Index");
            }

            int userId = (int)Session["UserID"];

            // =========================
            // 3️⃣ GET USER
            // =========================
            var user = db.NguoiDung.FirstOrDefault(x => x.NguoiDungID == userId);
            if (user == null)
                return HttpNotFound();

            // =========================
            // 4️⃣ KIỂM TRA MẬT KHẨU CŨ (HASH)
            // =========================
            // ✔ ĐÚNG: so sánh mật khẩu nhập với hash trong DB
            if (!PasswordHelper.VerifyPassword(MatKhauCu, user.MatKhau))
            {
                ModelState.AddModelError("MatKhauCu", "Mật khẩu cũ không đúng");
                return View("Index");
            }

            // =========================
            // 5️⃣ HASH MẬT KHẨU MỚI & LƯU
            // =========================
            user.MatKhau = PasswordHelper.HashPassword(MatKhauMoi);
            db.SaveChanges();

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index", "TrangChu");
        }
    }
}
