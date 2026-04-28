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

            XemTKB vn = new XemTKB()
            {
                Tuan = tuan,
                LaHocSinh = (vaitro == "HocSinh"),
                ThoiKhoaBieus = new List<ThoiKhoaBieu>()
            };
            
            //hien thi cho hoc sinh
            if (vaitro == "HocSinh")
            {
                var HocSinh = db.HocSinh.Include("LopHoc").FirstOrDefault(x => x.HocSinhID == userID);
                if (hocSinh == null)
                {
                    return HttpNotFound();

                }

                vn.HocSinhID = hocSinh.HocSinhID;
                vn.LopHocId = hocSinh.LopHocID;

                vn.CaHoc = hocSinh.LopHoc.CaHoc;


                vn.ThoiKhoaBieus = db.ThoiKhoaBieu
                    .Where(x => x.LopHocID == HocSinh.LopHocID && x.Tuan == tuan)

                    .OrderBy(x => x.Thu)
                    .ThenBy(x => x.TietHoc)
                    .ToList();
            }

            if (vaitro == "GiaoVien")
            {
                var GiaoVien = db.GiaoVien.Find(userID);
                if (GiaoVien == null)
                {
                    return HttpNotFound();
                }

                vn.GiaoVienId = GiaoVien.GiaoVienID;
                vn.ThoiKhoaBieus = db.ThoiKhoaBieu
                     .Where(x => x.GiaoVienID == GiaoVien.GiaoVienID && x.Tuan == tuan)
                     .OrderBy(x => x.Thu)
                     .ThenBy(x => x.TietHoc)
                     .ToList();
            }


            return View(vn);
        }
    }
}