using demomvc.App_Start;
using demomvc.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace demomvc.Controllers
{
    public class DangNhapController : Controller
    {
        QuanLyTruongHocEntities1 db = new QuanLyTruongHocEntities1();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string TenDangNhap, string MatKhau)
        {
            if (string.IsNullOrEmpty(TenDangNhap) || string.IsNullOrEmpty(MatKhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tài khoản và mật khẩu!";
                return View();
            }

            //  CHỈ TÌM USER THEO TÊN
            var user = db.NguoiDung
                .FirstOrDefault(u => u.TenDangNhap == TenDangNhap);

            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View();
            }

            //  SO SÁNH HASH
            if (!PasswordHelper.VerifyPassword(MatKhau, user.MatKhau))
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View();
            }

            //  ĐĂNG NHẬP THÀNH CÔNG
            Session["UserID"] = user.NguoiDungID;
            Session["UserName"] = user.TenDangNhap;
            Session["HoTen"] = user.HoTen;
            Session["VaiTro"] = user.VaiTro;

            Session["AvatarUrl"] = !string.IsNullOrEmpty(user.AnhDaiDien)
                ? Url.Content(user.AnhDaiDien)
                : Url.Content("~/img/imgUser.png");

            return RedirectToAction("Index", "TrangChu");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "DangNhap");
        }

      


    }
}
