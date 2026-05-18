using demomvc.App_Start;
using demomvc.Models;
using DocumentFormat.OpenXml.EMMA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace demomvc.Controllers
{
    [RoleAuthorize(RolesRequired = "HocSinh,GiaoVien,BiThu,HieuPho")]
    public class XemThoiKhoaBieuController : Controller
    {
        QuanLyTruongHocEntities2 db = new QuanLyTruongHocEntities2();
        // GET: XemThoiKhoaBieu
        public ActionResult Index(int? tuan)
        {
            //int userID = Convert.ToInt32(Session["UserID"]);
            //var hocSinh = db.HocSinh.Find(userID);
            int userID = Convert.ToInt32(Session["UserID"]);

            var hocSinh = db.HocSinh
                .FirstOrDefault(x => x.NguoiDungID == userID);

            //--------------------
            string vaitro = Session["VaiTro"].ToString();
            DateTime? ngayBatDauHoc = null;


            XemTKB vn = new XemTKB()
            {
                Tuan = tuan ?? 1,
                NgayBatDauHoc = ngayBatDauHoc,
                LaHocSinh = (vaitro == "HocSinh"),
                ThoiKhoaBieus = new List<ThoiKhoaBieu>()
            };



            if (vaitro == "HocSinh")
            {
                var HocSinh = db.HocSinh
                    .Include("LopHoc")
                   // .FirstOrDefault(x => x.HocSinhID == userID);
                   .FirstOrDefault(x => x.NguoiDungID == userID);
                //if (HocSinh == null)
                //    return HttpNotFound();

                if (HocSinh == null)
                {
                    TempData["Error"] = "Tài khoản chưa được gán lớp học";
                    return RedirectToAction("Index", "TrangChu");
                }


                vn.HocSinhID = HocSinh.HocSinhID;
                vn.LopHocId = HocSinh.LopHocID;
                vn.CaHoc = HocSinh.LopHoc.CaHoc;

                // LOAD TKB + FULL INCLUDE
                vn.ThoiKhoaBieus = db.ThoiKhoaBieu
                    .Include("LopHoc.NamHoc.HocKy")
                    .Where(x => x.LopHocID == HocSinh.LopHocID && x.Tuan == vn.Tuan)
                    .OrderBy(x => x.Thu)
                    .ThenBy(x => x.TietHoc)
                    .ToList();

                // LẤY HỌC KỲ CỦA TKB ĐANG HIỂN THỊ
                var kk = vn.ThoiKhoaBieus.FirstOrDefault();

                vn.NgayBatDauHoc = kk?
                    .LopHoc?
                    .NamHoc?
                    .HocKy?
                    .FirstOrDefault()?
                    .NgayBatDauHoc;


                if (!tuan.HasValue && vn.NgayBatDauHoc.HasValue)
                {
                    DateTime start = vn.NgayBatDauHoc.Value.Date;
                    DateTime today = DateTime.Now.Date;

                    int soNgay = (today - start).Days;

                    if (soNgay >= 0)
                    {
                        vn.Tuan = soNgay / 7 + 1;
                    }
                    else
                    {
                        vn.Tuan = 1;
                    }
                }
                else
                {
                    vn.Tuan = tuan ?? 1;
                }

            }



            if (vaitro == "GiaoVien")
            {
                //var GiaoVien = db.GiaoVien.Find(userID);
                var GiaoVien = db.GiaoVien
    .FirstOrDefault(x => x.NguoiDungID == userID);

                //if (GiaoVien == null)
                //    return HttpNotFound();
                if (GiaoVien == null)
                {
                    TempData["Error"] = "Tài khoản chưa được gán giáo viên";
                    return RedirectToAction("Index", "TrangChu");
                }

                vn.GiaoVienId = GiaoVien.GiaoVienID;

                // ✅ LOAD TKB + INCLUDE ĐỦ ĐƯỜNG
                vn.ThoiKhoaBieus = db.ThoiKhoaBieu
                    .Include("LopHoc.NamHoc.HocKy")
                    .Where(x => x.GiaoVienID == GiaoVien.GiaoVienID && x.Tuan == vn.Tuan)
                    .OrderBy(x => x.Thu)
                    .ThenBy(x => x.TietHoc)
                    .ToList();

                // ✅ SAU KHI CÓ TKB → LẤY HỌC KỲ CỦA TKB ĐANG HIỂN THỊ
                var kk = vn.ThoiKhoaBieus.FirstOrDefault();

                vn.NgayBatDauHoc = kk?
                    .LopHoc?
                    .NamHoc?
                    .HocKy?
                    .FirstOrDefault()?   // ✅ HocKies là ICollection
                    .NgayBatDauHoc;

                if (!tuan.HasValue && vn.NgayBatDauHoc.HasValue)
                {
                    DateTime start = vn.NgayBatDauHoc.Value.Date;
                    DateTime today = DateTime.Now.Date;

                    int soNgay = (today - start).Days;

                    if (soNgay >= 0)
                    {
                        vn.Tuan = soNgay / 7 + 1;
                    }
                    else
                    {
                        vn.Tuan = 1;
                    }
                }
                else
                {
                    vn.Tuan = tuan ?? 1;
                }

            }
            vn.DsNgayNghi = db.NgayHoc
                .Where(n =>
                    n.TrangThai == "NGHI"
                    && n.Tuan == vn.Tuan
                )
                .ToList();

            //hien thi cho hoj bu
            vn.DsHocBu = db.HocBu
             .Where(h =>
                 db.NgayHoc.Any(n =>
                     n.NgayHocID == h.NgayHocBuID &&
                     n.Tuan == vn.Tuan
                 )
             )
             .ToList();

            vn.DsNgayHoc = db.NgayHoc.ToList();

            return View(vn);
        }
    }
}