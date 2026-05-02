using demomvc.App_Start;
using demomvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace demomvc.Controllers
{
    [RoleAuthorize(RolesRequired = "HocSinh,GiaoVien")]
    public class XemThoiKhoaBieuController : Controller
    {
        QuanLyTruongHocEntities1 db = new QuanLyTruongHocEntities1();
        // GET: XemThoiKhoaBieu
        public ActionResult Index(int tuan = 1)
        {
            int userID = Convert.ToInt32(Session["UserID"]);
            var hocSinh = db.HocSinh.Find(userID);
            string vaitro = Session["VaiTro"].ToString();
            DateTime? ngayBatDauHoc = null;

           
            XemTKB vn = new XemTKB()
            {
                Tuan = tuan,
                NgayBatDauHoc=ngayBatDauHoc,
                LaHocSinh = (vaitro == "HocSinh"),
                ThoiKhoaBieus = new List<ThoiKhoaBieu>()
            };

            //hien thi cho hoc sinh
            //if (vaitro == "HocSinh")
            //{
            //    var HocSinh = db.HocSinh.Include("LopHoc").FirstOrDefault(x => x.HocSinhID == userID);
            //    if (hocSinh == null)
            //    {
            //        return HttpNotFound();

            //    }

            //    vn.HocSinhID = hocSinh.HocSinhID;
            //    vn.LopHocId = hocSinh.LopHocID;

            //    vn.CaHoc = hocSinh.LopHoc.CaHoc;





            //    vn.ThoiKhoaBieus = db.ThoiKhoaBieu
            //        .Where(x => x.LopHocID == HocSinh.LopHocID && x.Tuan == tuan)

            //        .OrderBy(x => x.Thu)
            //        .ThenBy(x => x.TietHoc)
            //        .ToList();
            //}

            if (vaitro == "HocSinh")
            {
                var HocSinh = db.HocSinh
                    .Include("LopHoc")
                    .FirstOrDefault(x => x.HocSinhID == userID);

                if (HocSinh == null)
                    return HttpNotFound();

                vn.HocSinhID = HocSinh.HocSinhID;
                vn.LopHocId = HocSinh.LopHocID;
                vn.CaHoc = HocSinh.LopHoc.CaHoc;

                // ✅ LOAD TKB + FULL INCLUDE
                vn.ThoiKhoaBieus = db.ThoiKhoaBieu
                    .Include("LopHoc.NamHoc.HocKy")
                    .Where(x => x.LopHocID == HocSinh.LopHocID && x.Tuan == tuan)
                    .OrderBy(x => x.Thu)
                    .ThenBy(x => x.TietHoc)
                    .ToList();

                // ✅ LẤY HỌC KỲ CỦA TKB ĐANG HIỂN THỊ
                var kk = vn.ThoiKhoaBieus.FirstOrDefault();

                vn.NgayBatDauHoc = kk?
                    .LopHoc?
                    .NamHoc?
                    .HocKy?
                    .FirstOrDefault()?
                    .NgayBatDauHoc;
            }

            //if (vaitro == "GiaoVien")
            //{
            //    var GiaoVien = db.GiaoVien.Find(userID);
            //    if (GiaoVien == null)
            //    {
            //        return HttpNotFound();
            //    }

            //    vn.GiaoVienId = GiaoVien.GiaoVienID;
            //    vn.ThoiKhoaBieus = db.ThoiKhoaBieu
            //         .Where(x => x.GiaoVienID == GiaoVien.GiaoVienID && x.Tuan == tuan)
            //         .OrderBy(x => x.Thu)
            //         .ThenBy(x => x.TietHoc)
            //         .ToList();
            //}

            if (vaitro == "GiaoVien")
            {
                var GiaoVien = db.GiaoVien.Find(userID);
                if (GiaoVien == null)
                    return HttpNotFound();

                vn.GiaoVienId = GiaoVien.GiaoVienID;

                // ✅ LOAD TKB + INCLUDE ĐỦ ĐƯỜNG
                vn.ThoiKhoaBieus = db.ThoiKhoaBieu
                    .Include("LopHoc.NamHoc.HocKy")
                    .Where(x => x.GiaoVienID == GiaoVien.GiaoVienID && x.Tuan == tuan)
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
            }


            return View(vn);
        }
    }
}