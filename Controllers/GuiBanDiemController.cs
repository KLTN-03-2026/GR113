using demomvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web.Mvc;
using demomvc.App_Start;

namespace demomvc.Controllers
{
    [RoleAuthorize(RolesRequired = "HieuTruong,GiaoVien,HieuPho,BiThu")]
    public class GuiBanDiemController : Controller
    {
        QuanLyTruongHocEntities2 db = new QuanLyTruongHocEntities2();

        public ActionResult Index()
        {
            var namhoc = db.NamHoc.ToList();
            ViewBag.NamHocID = new SelectList(namhoc, "NamHocID", "TenNamHoc");

            // Load học kỳ rỗng ban đầu
            ViewBag.HocKyID = new SelectList(new List<HocKy>(), "HocKyID", "TenHocKy");

            return View();
        }

        public JsonResult LoadHocKy(int namHocId)
        {
            var hk = db.HocKy
                .Where(x => x.NamHocID == namHocId)
                .Select(x => new { x.HocKyID, x.TenHocKy })
                .ToList();

            return Json(hk, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GuiBangDiemPost(int NamHocID, int HocKyID)
        {
            // 1. Lấy giáo viên đăng nhập
            if (Session["UserID"] == null)
                return Content("Bạn chưa đăng nhập!");

            int userID = Convert.ToInt32(Session["UserID"]);

            // 2. Tìm giáo viên theo NguoiDungID
            var gv = db.GiaoVien.FirstOrDefault(g => g.NguoiDungID == userID);

            if (gv == null)
                return Content("Không tìm thấy giáo viên!");

            // 3. Lấy lớp CN trong NĂM HỌC
            var lop = db.LopHoc.FirstOrDefault(l =>
                l.GiaoVienChuNhiem == gv.GiaoVienID &&
                l.NienKhoa == NamHocID
            );

            if (lop == null)
            {
                TempData["Error"] = "Không có lớp chủ nhiệm để gửi bản điểm";
                return RedirectToAction("Index");
            }

            // 4. Lấy danh sách học sinh
            var hocsinh = db.HocSinh.Where(h => h.LopHocID == lop.LopHocID).ToList();

            foreach (var hs in hocsinh)
            {
                // 5. Lấy email phụ huynh
                var ph = (from p in db.PhuHuynh
                          join hp in db.HocSinh_PhuHuynh on p.PhuHuynhID equals hp.PhuHuynhID
                          where hp.HocSinhID == hs.HocSinhID
                          select p.Email).FirstOrDefault();

                if (string.IsNullOrEmpty(ph)) continue;

                string subject = $"Bảng điểm {hs.NguoiDung.HoTen} - {lop.TenLop}";

                // 6. Lấy điểm TẤT CẢ MÔN
                var diemMon = (from d in db.Diem
                               join m in db.MonHoc on d.MonHocID equals m.MonHocID
                               where d.HocSinhID == hs.HocSinhID
                                  && d.NamHocID == NamHocID
                                  && d.HocKyID == HocKyID
                               select new
                               {
                                   m.TenMonHoc,
                                   d.Diem15p,
                                   d.DiemMieng,
                                   d.DiemGK,
                                   d.DiemCK,
                                   d.DiemTB
                               }).ToList();

                // 7. Tạo HTML email
                string body = "<h2>BẢNG ĐIỂM HỌC SINH</h2>";
                body += $"<p><b>Họ tên:</b> {hs.NguoiDung.HoTen}</p>";
                body += $"<p><b>Lớp:</b> {lop.TenLop}</p>";
                body += $"<p><b>Năm học:</b> {lop.NamHoc.TenNamHoc}</p>";
                body += $"<p><b>Học kỳ:</b> HK{HocKyID}</p><hr>";

                body += "<table border='1' cellpadding='5' cellspacing='0'>";
                body += "<tr><th>Môn học</th><th>Miệng</th><th>15 phút</th><th>Giữa kỳ</th><th>Cuối kỳ</th><th>Điểm TB</th></tr>";

                foreach (var d in diemMon)
                {
                    body += "<tr>";
                    body += $"<td>{d.TenMonHoc}</td>";
                    body += $"<td>{d.DiemMieng}</td>";
                    body += $"<td>{d.Diem15p}</td>";
                    body += $"<td>{d.DiemGK}</td>";
                    body += $"<td>{d.DiemCK}</td>";
                    body += $"<td>{d.DiemTB}</td>";
                    body += "</tr>";
                }
                body += "</table><br/>";

                /*
                 * ===========================
                 *  PHẦN HIỂN THỊ HK1 / HK2 / CẢ NĂM
                 * ===========================
                 */

                // Lấy điểm HK1 và HK2
                var hk1 = db.KetQuaHocTap.FirstOrDefault(x =>
                    x.HocSinhID == hs.HocSinhID &&
                    x.NamHocID == NamHocID &&
                    x.HocKyID == 1);

                var hk2 = db.KetQuaHocTap.FirstOrDefault(x =>
                    x.HocSinhID == hs.HocSinhID &&
                    x.NamHocID == NamHocID &&
                    x.HocKyID == 2);

                // ===========================
                //  GỬI HỌC KỲ 1
                // ===========================
                if (HocKyID == 1)
                {
                    if (hk1 != null)
                    {
                        body += "<hr>";
                        body += "<h3>KẾT QUẢ HỌC KỲ I</h3>";
                        body += $"<p><b>Điểm TB HK1:</b> {hk1.DTBTong}</p>";
                        body += $"<p><b>Hạnh kiểm HK1:</b> {hk1.HanhKiem}</p>";
                        body += $"<p><b>Học lực HK1:</b> {hk1.HocLuc}</p>";
                    }
                }

                // ===========================
                //  GỬI HỌC KỲ 2
                // ===========================
                if (HocKyID == 2)
                {
                    if (hk2 != null)
                    {
                        body += "<hr>";
                        body += "<h3>KẾT QUẢ HỌC KỲ II</h3>";
                        body += $"<p><b>Điểm TB HK2:</b> {hk2.DTBTong}</p>";
                        body += $"<p><b>Hạnh kiểm HK2:</b> {hk2.HanhKiem}</p>";
                        body += $"<p><b>Học lực HK2:</b> {hk2.HocLuc}</p>";
                    }

                    // ===========================
                    //  CHỈ TÍNH CẢ NĂM NẾU CÓ HK1 + HK2
                    // ===========================
                    if (hk1 != null && hk2 != null)
                    {
                        double dtb1 = hk1.DTBTong ?? 0;
                        double dtb2 = hk2.DTBTong ?? 0;

                        double diemCN = Math.Round((dtb1 + dtb2 * 2) / 3, 2);

                        body += "<hr>";
                        body += "<h3>KẾT QUẢ CẢ NĂM</h3>";
                        body += $"<p><b>Điểm TB cả năm:</b> {diemCN}</p>";
                        body += $"<p><b>Hạnh kiểm cả năm:</b> {hk2.HanhKiem}</p>";
                        body += $"<p><b>Học lực cả năm:</b> {hk2.HocLuc}</p>";
                    }
                }

                // 8. Gửi email
                GuiEmailThuc(ph, subject, body);
            }
            TempData["Success"] ="Gửi bản điểm cho học sinh thành công";
            return RedirectToAction("Index");
        }


        private void GuiEmailThuc(string to, string subject, string body)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("caoquangtuanhung2004@gmail.com"); // PHẢI trùng với App Password
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.EnableSsl = true;

            // 👉 App password: viết liền, không khoảng trắng
            smtp.Credentials = new NetworkCredential(
                "caoquangtuanhung2004@gmail.com",
                "qbmnapuiioipbjly"   // <-- sửa lại đúng 
            );

            smtp.Send(mail);
        }

    }
}
