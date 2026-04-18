using demomvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.EnterpriseServices.Internal;//thêm
using System.Data.Entity.Validation;
using demomvc.App_Start;

namespace demomvc.Controllers
{
    [RoleAuthorize(RolesRequired = "HocSinh")]
    public class ThongTinHocSinhController : Controller
    {
        QuanLyTruongHocEntities1 db = new QuanLyTruongHocEntities1();
        // GET: ThongTinHocSinh
        public ActionResult Index()
        {
            int nguoiDungID = (int)Session["UserID"];

            var model = db.HocSinh
                .Include(h => h.NguoiDung)
                .Include(h => h.LopHoc)
                .Include(h => h.LopHoc.GiaoVien)
                .Include(h => h.LopHoc.GiaoVien.NguoiDung)
                .Include(h => h.HocSinh_PhuHuynh.Select(p => p.PhuHuynh)) // Phụ huynh
                .FirstOrDefault(h => h.NguoiDungID == nguoiDungID);

            if (model == null)
                return HttpNotFound();

            return View("ThongTin", model);
        }


        //public ActionResult CapNhat(int id)
        //{
        //    HocSinh hocsinh = db.HocSinh.Where(row => row.HocSinhID == id).FirstOrDefault();
        //    ViewBag.KhoiLop = db.KhoiLop.ToList();
        //    ViewBag.LopHoc = db.LopHoc.ToList();
        //    ViewBag.NguoiDung = db.NguoiDung.ToList();
        //    ViewBag.PhuHuynh_HocSinh = db.HocSinh_PhuHuynh.ToList();
        //    ViewBag.PhuHuynh = db.PhuHuynh.ToList();
        //    return View(hocsinh);
        //}
        public ActionResult CapNhat(int id)
        {
            var hocsinh = db.HocSinh
                .Include("NguoiDung")
                .Include("LopHoc")
                .Include("LopHoc.KhoiLop")
                .Include("HocSinh_PhuHuynh.PhuHuynh")
                .FirstOrDefault(x => x.HocSinhID == id);

            if (hocsinh == null) return HttpNotFound();

            return View(hocsinh);
        }


        //[HttpPost]
        //public ActionResult CapNhat(HocSinh hs, int hocSinhID, string HoTenBa, string SDTBa, string NgheNghiepBa, string EmailBa,
        //                            string HoTenMe, string SDTMe, string NgheNghiepMe, string EmailMe)
        //{
        //    if (string.IsNullOrWhiteSpace(hs.NguoiDung?.HoTen))
        //        ModelState.AddModelError("NguoiDung.HoTen", "Họ tên học sinh không được để trống");
        //    if (hs.NgaySinh == null)
        //        ModelState.AddModelError("NgaySinh", "Ngày sinh không được để trống");
        //    if (string.IsNullOrWhiteSpace(hs.GioiTinh))
        //        ModelState.AddModelError("GioiTinh", "Giới tính không được để trống");
        //    if (string.IsNullOrWhiteSpace(hs.LopHoc?.KhoiLop?.TenKhoi))
        //        ModelState.AddModelError("LopHoc.KhoiLop.TenKhoi", "Khối không được để trống");
        //    if (string.IsNullOrWhiteSpace(hs.LopHoc?.TenLop))
        //        ModelState.AddModelError("LopHoc.TenLop", "Lớp không được để trống");
        //    if (string.IsNullOrWhiteSpace(hs.CCCD))
        //        ModelState.AddModelError("CCCD", "CCCD không được để trống");
        //    if (string.IsNullOrWhiteSpace(hs.DanToc))
        //        ModelState.AddModelError("DanToc", "Dân tộc không được để trống");
        //    if (string.IsNullOrWhiteSpace(hs.NguoiDung?.Email))
        //        ModelState.AddModelError("NguoiDung.Email", "Email không được để trống");
        //    if (string.IsNullOrWhiteSpace(hs.TinhThanhPho))
        //        ModelState.AddModelError("TinhThanhPho", "Thành phố không được để trống");
        //    if (string.IsNullOrWhiteSpace(hs.QuanHuyen))
        //        ModelState.AddModelError("QuanHuyen", "Quận huyện không được để trống");
        //    if (string.IsNullOrWhiteSpace(hs.DiaChiNha))
        //        ModelState.AddModelError("DiaChiNha", "Địa chỉ không được để trống");
        //    if (string.IsNullOrWhiteSpace(HoTenBa))
        //        ModelState.AddModelError("HoTenBa", "Họ tên Ba không được để trống");
        //    if (string.IsNullOrWhiteSpace(SDTBa))
        //        ModelState.AddModelError("SDTBa", "SĐT Ba không được để trống");
        //    if (string.IsNullOrWhiteSpace(NgheNghiepBa))
        //        ModelState.AddModelError("NgheNghiepBa", "Nghề nghiệp Ba không được để trống");
        //    if (string.IsNullOrWhiteSpace(EmailBa))
        //        ModelState.AddModelError("EmailBa", "Email Ba không được để trống");
        //    if (string.IsNullOrWhiteSpace(HoTenMe))
        //        ModelState.AddModelError("HoTenMe", "Họ tên Mẹ không được để trống");
        //    if (string.IsNullOrWhiteSpace(SDTMe))
        //        ModelState.AddModelError("SDTMe", "SĐT Mẹ không được để trống");
        //    if (string.IsNullOrWhiteSpace(NgheNghiepMe))
        //        ModelState.AddModelError("NgheNghiepMe", "Nghề nghiệp Mẹ không được để trống");
        //    if (string.IsNullOrWhiteSpace(EmailMe))
        //        ModelState.AddModelError("EmailMe", "Email Mẹ không được để trống");
        //    if (!ModelState.IsValid)
        //        return View(hs);

        //    //HocSinh hocsinh = db.HocSinh.Where(row => row.HocSinhID == hs.HocSinhID).FirstOrDefault();
        //    var hocsinh = db.HocSinh
        //             .Include("NguoiDung")
        //             .Include(h => h.LopHoc.KhoiLop)
        //             .FirstOrDefault(x => x.HocSinhID == hs.HocSinhID);

        //    if (hocsinh == null) return HttpNotFound();

        //    hocsinh.NguoiDung.HoTen = hs.NguoiDung.HoTen;
        //    hocsinh.GioiTinh = hs.GioiTinh;
        //    hocsinh.NgaySinh = hs.NgaySinh;
        //    hocsinh.LopHoc.KhoiLop.TenKhoi = hs.LopHoc.KhoiLop.TenKhoi;
        //    hocsinh.LopHoc.TenLop = hs.LopHoc.TenLop;
        //    hocsinh.CCCD = hs.CCCD;
        //    hocsinh.DanToc = hs.DanToc;
        //    hocsinh.NguoiDung.Email = hs.NguoiDung.Email;
        //    hocsinh.TinhThanhPho = hs.TinhThanhPho;
        //    hocsinh.QuanHuyen = hs.QuanHuyen;
        //    hocsinh.DiaChiNha = hs.DiaChiNha;


        //    //phụ huynh
        //    var ba = db.HocSinh_PhuHuynh
        //        .Include(hp => hp.PhuHuynh)
        //        .FirstOrDefault(x => x.HocSinhID == hs.HocSinhID && x.VaiTroTrongGiaDinh == "Ba")?.PhuHuynh;

        //    if (ba != null)
        //    {
        //        ba.HoTen = HoTenBa;
        //        ba.SDT = SDTBa;
        //        ba.NgheNghiep = NgheNghiepBa;
        //        ba.Email = EmailBa;
        //    }

        //    //mẹ
        //    var me = db.HocSinh_PhuHuynh
        //        .Include(hp => hp.PhuHuynh)
        //        .FirstOrDefault(x => x.HocSinhID == hs.HocSinhID && x.VaiTroTrongGiaDinh == "Mẹ")?.PhuHuynh;

        //    if (me != null)
        //    {
        //        me.HoTen = HoTenMe;
        //        me.SDT = SDTMe;
        //        me.NgheNghiep = NgheNghiepMe;
        //        me.Email = EmailMe;
        //    }


        //    db.SaveChanges();
        //    TempData["Success"] = "Cập nhật thành công!";
        //    return RedirectToAction("Index");
        //}
        //[HttpPost]
        //public ActionResult CapNhat(
        //        HocSinh hs,
        //        int hocSinhID,
        //        string HoTenBa, string SDTBa, string NgheNghiepBa, string EmailBa,
        //        string HoTenMe, string SDTMe, string NgheNghiepMe, string EmailMe)
        //{
        //    // VALIDATE
        //    if (string.IsNullOrWhiteSpace(hs.NguoiDung?.HoTen))
        //        ModelState.AddModelError("NguoiDung.HoTen", "Họ tên học sinh không được để trống");

        //    if (hs.NgaySinh == null)
        //        ModelState.AddModelError("NgaySinh", "Ngày sinh không được để trống");

        //    if (string.IsNullOrWhiteSpace(hs.GioiTinh))
        //        ModelState.AddModelError("GioiTinh", "Giới tính không được để trống");

        //    if (string.IsNullOrWhiteSpace(hs.CCCD))
        //        ModelState.AddModelError("CCCD", "CCCD không được để trống");

        //    if (string.IsNullOrWhiteSpace(hs.DanToc))
        //        ModelState.AddModelError("DanToc", "Dân tộc không được để trống");

        //    if (string.IsNullOrWhiteSpace(hs.NguoiDung?.Email))
        //        ModelState.AddModelError("NguoiDung.Email", "Email không được để trống");

        //    if (string.IsNullOrWhiteSpace(hs.TinhThanhPho))
        //        ModelState.AddModelError("TinhThanhPho", "Thành phố không được để trống");

        //    if (string.IsNullOrWhiteSpace(hs.QuanHuyen))
        //        ModelState.AddModelError("QuanHuyen", "Quận huyện không được để trống");

        //    if (string.IsNullOrWhiteSpace(hs.DiaChiNha))
        //        ModelState.AddModelError("DiaChiNha", "Địa chỉ không được để trống");

        //    // phụ huynh
        //    if (string.IsNullOrWhiteSpace(HoTenBa))
        //        ModelState.AddModelError("HoTenBa", "Họ tên Ba không được để trống");

        //    if (string.IsNullOrWhiteSpace(SDTBa))
        //        ModelState.AddModelError("SDTBa", "SĐT Ba không được để trống");

        //    if (string.IsNullOrWhiteSpace(NgheNghiepBa))
        //        ModelState.AddModelError("NgheNghiepBa", "Nghề nghiệp Ba không được để trống");

        //    if (string.IsNullOrWhiteSpace(EmailBa))
        //        ModelState.AddModelError("EmailBa", "Email Ba không được để trống");

        //    if (string.IsNullOrWhiteSpace(HoTenMe))
        //        ModelState.AddModelError("HoTenMe", "Họ tên Mẹ không được để trống");

        //    if (string.IsNullOrWhiteSpace(SDTMe))
        //        ModelState.AddModelError("SDTMe", "SĐT Mẹ không được để trống");

        //    if (string.IsNullOrWhiteSpace(NgheNghiepMe))
        //        ModelState.AddModelError("NgheNghiepMe", "Nghề nghiệp Mẹ không được để trống");

        //    if (string.IsNullOrWhiteSpace(EmailMe))
        //        ModelState.AddModelError("EmailMe", "Email Mẹ không được để trống");



        //    // Nếu có lỗi => load lại học sinh đầy đủ để View render đúng
        //    if (!ModelState.IsValid)
        //    {
        //        var hsReload = db.HocSinh
        //            .Include("NguoiDung")
        //            .Include("LopHoc")
        //            .Include("LopHoc.KhoiLop")
        //            .Include("HocSinh_PhuHuynh.PhuHuynh")
        //            .FirstOrDefault(x => x.HocSinhID == hs.HocSinhID);

        //        return View(hsReload);
        //    }


        //    // Lấy từ DB
        //    var hocsinh = db.HocSinh
        //        .Include("NguoiDung")
        //        .Include("LopHoc")
        //        .Include("LopHoc.KhoiLop")
        //        .Include("HocSinh_PhuHuynh.PhuHuynh")
        //        .FirstOrDefault(x => x.HocSinhID == hs.HocSinhID);

        //    if (hocsinh == null) return HttpNotFound();

        //    // Cập nhật học sinh
        //    hocsinh.NguoiDung.HoTen = hs.NguoiDung.HoTen;
        //    hocsinh.GioiTinh = hs.GioiTinh;
        //    hocsinh.NgaySinh = hs.NgaySinh;
        //    hocsinh.CCCD = hs.CCCD;
        //    hocsinh.DanToc = hs.DanToc;
        //    hocsinh.NguoiDung.Email = hs.NguoiDung.Email;
        //    hocsinh.TinhThanhPho = hs.TinhThanhPho;
        //    hocsinh.QuanHuyen = hs.QuanHuyen;
        //    hocsinh.DiaChiNha = hs.DiaChiNha;


        //    // PHỤ HUYNH - BA
        //    var baLink = hocsinh.HocSinh_PhuHuynh.FirstOrDefault(x => x.VaiTroTrongGiaDinh == "Ba");
        //    var ba = baLink?.PhuHuynh;

        //    if (ba != null)
        //    {
        //        ba.HoTen = HoTenBa;
        //        ba.SDT = SDTBa;
        //        ba.NgheNghiep = NgheNghiepBa;
        //        ba.Email = EmailBa;
        //    }

        //    // PHỤ HUYNH - MẸ
        //    var meLink = hocsinh.HocSinh_PhuHuynh.FirstOrDefault(x => x.VaiTroTrongGiaDinh == "Mẹ");
        //    var me = meLink?.PhuHuynh;

        //    if (me != null)
        //    {
        //        me.HoTen = HoTenMe;
        //        me.SDT = SDTMe;
        //        me.NgheNghiep = NgheNghiepMe;
        //        me.Email = EmailMe;
        //    }


        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (DbEntityValidationException ex)
        //    {
        //        var errors = ex.EntityValidationErrors
        //            .SelectMany(e => e.ValidationErrors)
        //            .Select(e => "Property: " + e.PropertyName + " - Error: " + e.ErrorMessage);

        //        throw new Exception(string.Join("<br/>", errors));
        //    }


        //    TempData["Success"] = "Cập nhật thành công!";
        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        public ActionResult CapNhat(
            HocSinh hs,
            int hocSinhID,
            string HoTenBa, string SDTBa, string NgheNghiepBa, string EmailBa,
            string HoTenMe, string SDTMe, string NgheNghiepMe, string EmailMe)
        {
            // ===== TRIM dữ liệu =====
            hs.NguoiDung.HoTen = hs.NguoiDung?.HoTen?.Trim();
            hs.NguoiDung.Email = hs.NguoiDung?.Email?.Trim();

            HoTenBa = HoTenBa?.Trim();
            HoTenMe = HoTenMe?.Trim();
            SDTBa = SDTBa?.Trim();
            SDTMe = SDTMe?.Trim();
            EmailBa = EmailBa?.Trim();
            EmailMe = EmailMe?.Trim();

            // ===== REGEX họ tên (giống Entity) =====
            var regexTen = new System.Text.RegularExpressions.Regex(@"^[a-zA-ZÀ-ỹ\s]+$");

            // ===== VALIDATE HỌC SINH =====
            if (string.IsNullOrWhiteSpace(hs.NguoiDung?.HoTen))
                ModelState.AddModelError("NguoiDung.HoTen", "Họ tên học sinh không được để trống");
            else if (!regexTen.IsMatch(hs.NguoiDung.HoTen))
                ModelState.AddModelError("NguoiDung.HoTen", "Họ tên học sinh chỉ được chứa chữ cái và khoảng trắng");

            if (hs.NgaySinh == null)
                ModelState.AddModelError("NgaySinh", "Ngày sinh không được để trống");

            if (string.IsNullOrWhiteSpace(hs.GioiTinh))
                ModelState.AddModelError("GioiTinh", "Giới tính không được để trống");

            if (string.IsNullOrWhiteSpace(hs.CCCD))
                ModelState.AddModelError("CCCD", "CCCD không được để trống");

            if (string.IsNullOrWhiteSpace(hs.DanToc))
                ModelState.AddModelError("DanToc", "Dân tộc không được để trống");

            if (string.IsNullOrWhiteSpace(hs.NguoiDung?.Email))
                ModelState.AddModelError("NguoiDung.Email", "Email không được để trống");

            if (string.IsNullOrWhiteSpace(hs.TinhThanhPho))
                ModelState.AddModelError("TinhThanhPho", "Thành phố không được để trống");

            if (string.IsNullOrWhiteSpace(hs.QuanHuyen))
                ModelState.AddModelError("QuanHuyen", "Quận huyện không được để trống");

            if (string.IsNullOrWhiteSpace(hs.DiaChiNha))
                ModelState.AddModelError("DiaChiNha", "Địa chỉ không được để trống");

            // ===== VALIDATE PHỤ HUYNH - BA =====
            if (string.IsNullOrWhiteSpace(HoTenBa))
                ModelState.AddModelError("HoTenBa", "Họ tên Ba không được để trống");
            else if (!regexTen.IsMatch(HoTenBa))
                ModelState.AddModelError("HoTenBa", "Họ tên Ba chỉ được chứa chữ cái và khoảng trắng");

            if (string.IsNullOrWhiteSpace(SDTBa))
                ModelState.AddModelError("SDTBa", "SĐT Ba không được để trống");

            if (string.IsNullOrWhiteSpace(NgheNghiepBa))
                ModelState.AddModelError("NgheNghiepBa", "Nghề nghiệp Ba không được để trống");

            if (string.IsNullOrWhiteSpace(EmailBa))
                ModelState.AddModelError("EmailBa", "Email Ba không được để trống");

            // ===== VALIDATE PHỤ HUYNH - MẸ =====
            if (string.IsNullOrWhiteSpace(HoTenMe))
                ModelState.AddModelError("HoTenMe", "Họ tên Mẹ không được để trống");
            else if (!regexTen.IsMatch(HoTenMe))
                ModelState.AddModelError("HoTenMe", "Họ tên Mẹ chỉ được chứa chữ cái và khoảng trắng");

            if (string.IsNullOrWhiteSpace(SDTMe))
                ModelState.AddModelError("SDTMe", "SĐT Mẹ không được để trống");

            if (string.IsNullOrWhiteSpace(NgheNghiepMe))
                ModelState.AddModelError("NgheNghiepMe", "Nghề nghiệp Mẹ không được để trống");

            if (string.IsNullOrWhiteSpace(EmailMe))
                ModelState.AddModelError("EmailMe", "Email Mẹ không được để trống");

            // ===== CÓ LỖI => LOAD LẠI VIEW =====
            if (!ModelState.IsValid)
            {
                var hsReload = db.HocSinh
                    .Include("NguoiDung")
                    .Include("LopHoc")
                    .Include("LopHoc.KhoiLop")
                    .Include("HocSinh_PhuHuynh.PhuHuynh")
                    .FirstOrDefault(x => x.HocSinhID == hs.HocSinhID);

                return View(hsReload);
            }

            // ===== LẤY HỌC SINH TỪ DB =====
            var hocsinh = db.HocSinh
                .Include("NguoiDung")
                .Include("HocSinh_PhuHuynh.PhuHuynh")
                .FirstOrDefault(x => x.HocSinhID == hs.HocSinhID);

            if (hocsinh == null)
                return HttpNotFound();

            // ===== UPDATE HỌC SINH =====
            hocsinh.NguoiDung.HoTen = hs.NguoiDung.HoTen;
            hocsinh.GioiTinh = hs.GioiTinh;
            hocsinh.NgaySinh = hs.NgaySinh;
            hocsinh.CCCD = hs.CCCD;
            hocsinh.DanToc = hs.DanToc;
            hocsinh.NguoiDung.Email = hs.NguoiDung.Email;
            hocsinh.TinhThanhPho = hs.TinhThanhPho;
            hocsinh.QuanHuyen = hs.QuanHuyen;
            hocsinh.DiaChiNha = hs.DiaChiNha;

            // ===== UPDATE BA =====
            // ===== UPDATE BA =====
            var baRelation = hocsinh.HocSinh_PhuHuynh
                .FirstOrDefault(x => x.VaiTroTrongGiaDinh == "Ba");

            if (baRelation != null && baRelation.PhuHuynh != null)
            {
                var ba = baRelation.PhuHuynh;

                // ✅ THÊM Ở ĐÂY
                ba.HoTen = HoTenBa?.Trim();
                ba.SDT = SDTBa?.Trim();
                ba.Email = EmailBa?.Trim();
                ba.NgheNghiep = NgheNghiepBa?.Trim();

                
            }



            // ===== UPDATE MẸ =====
            // ===== UPDATE MẸ =====
            var meRelation = hocsinh.HocSinh_PhuHuynh
                .FirstOrDefault(x => x.VaiTroTrongGiaDinh == "Mẹ");

            if (meRelation != null && meRelation.PhuHuynh != null)
            {
                var me = meRelation.PhuHuynh;

                // ✅ THÊM Ở ĐÂY
                me.HoTen = HoTenMe?.Trim();
                me.SDT = SDTMe?.Trim();
                me.Email = EmailMe?.Trim();
                me.NgheNghiep = NgheNghiepMe?.Trim();

               
            }


            // ===== SAVE =====
           
                db.SaveChanges();
               
            TempData["Success"] = "Cập nhật thông tin học sinh & phụ huynh thành công!";
            return RedirectToAction("Index");
        }


        public ActionResult CapNhatAvatar()
        {
            int nguodungID = (int)Session["UserID"];

            //lay thong tin hojc inh nguoiw fdungf
            var hocsinh = db.HocSinh
                .Include(h => h.NguoiDung)
                .FirstOrDefault(h => h.NguoiDungID == nguodungID);

            if (hocsinh == null)
            {
                return HttpNotFound();
            }
            return View(hocsinh);
        }
        [HttpPost]
        public ActionResult CapNhatAvatar(HttpPostedFileBase AvatarFile)
        {
            if (AvatarFile != null && AvatarFile.ContentLength > 0)
            {
                int nguoidungID = (int)Session["UserID"];

                // Lấy học sinh + thông tin người dùng
                var hs = db.HocSinh.Include(h => h.NguoiDung)
                                   .FirstOrDefault(h => h.NguoiDungID == nguoidungID);
                if (hs == null) return HttpNotFound();

                // Tạo tên file duy nhất
                string fileName = Guid.NewGuid() + System.IO.Path.GetExtension(AvatarFile.FileName);
                string path = Server.MapPath("~/img/" + fileName);

                // Lưu file vào server
                AvatarFile.SaveAs(path);

                // Cập nhật avatar vào CSDL
                hs.NguoiDung.AnhDaiDien = "/img/" + fileName;
                db.SaveChanges();

                // Cập nhật session để hiển thị ngay lập tức
                Session["AvatarUrl"] = "/img/" + fileName;

                TempData["Success"] = "Cập nhật ảnh của bạn thành công!";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Vui lòng chọn lại ảnh hợp lệ";
            return RedirectToAction("CapNhatAvatar");
        }
    }
}