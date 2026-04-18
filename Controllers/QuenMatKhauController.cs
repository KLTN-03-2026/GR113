using demomvc.App_Start;
using demomvc.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Web.Mvc;

namespace demomvc.Controllers
{
    public class QuenMatKhauController : Controller
    {
        private QuanLyTruongHocEntities1 db = new QuanLyTruongHocEntities1();

        //nhap email
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Vui lòng nhập email";
                return View();
            }

            var user = db.NguoiDung.FirstOrDefault(x => x.Email == email);

            if (user != null)
            {
                string token = GenerateToken();

                user.ResetToken = token;
                user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);

                db.SaveChanges();

                SendResetEmail(user.Email, token);
            }

            ViewBag.Message = "Nếu email tồn tại, link reset đã được gửi.";
            return View();
        }

        // 2. mo link email
        public ActionResult DatLaiMatKhau(string token)
        {
            if (string.IsNullOrEmpty(token))
                return View("TokenKhongHopLe");

            var user = db.NguoiDung.FirstOrDefault(x =>
                x.ResetToken == token &&
                x.ResetTokenExpiry > DateTime.UtcNow
            );

            if (user == null)
                return View("TokenKhongHopLe");

            return View(new ResetModel { Token = token });
        }

        //luu lai mat khau
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DatLaiMatKhau(ResetModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = db.NguoiDung.FirstOrDefault(x =>
                x.ResetToken == model.Token &&
                x.ResetTokenExpiry > DateTime.UtcNow
            );

            if (user == null)
                return View("TokenKhongHopLe");

            user.MatKhau = PasswordHelper.HashPassword(model.MatKhauMoi);

            // XÓA TOKEN
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            db.SaveChanges();

            TempData["Success"] = "Đặt lại mật khẩu thành công";
            return RedirectToAction("Index", "DangNhap");
        }

        //helper

        private string GenerateToken()
        {
            byte[] data = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(data);
            }
            return Convert.ToBase64String(data);
        }

        private void SendResetEmail(string email, string token)
        {
            string link = Url.Action(
                 "DatLaiMatKhau",
                    "QuenMatKhau",
                 new { token = token },
                 protocol: Request.Url.Scheme
            );

            // 👇 THÊM DÒNG NÀY
            System.Diagnostics.Debug.WriteLine("RESET LINK: " + link);

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("caoquangtuanhung2004@gmail.com");
                mail.To.Add(email);
                mail.Subject = "Đặt lại mật khẩu";
                mail.Body = $"Click vào link: <a href='{link}'>Reset Password</a>";
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(
                        "caoquangtuanhung2004@gmail.com",
                        "qbmnapuiioipbjly"
                    );
                    smtp.EnableSsl = true;

                    smtp.Send(mail);
                }
            }
        }
    }
}