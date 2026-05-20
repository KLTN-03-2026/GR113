using demomvc.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Globalization;
using System.Text;
using demomvc.App_Start;
namespace demomvc.Controllers
{
    [RoleAuthorize(RolesRequired = "HieuTruong,GiaoVien,HocSinh,HieuPho,BiThu")]
    public class TrangchuController : Controller
    {
        QuanLyTruongHocEntities2 db = new QuanLyTruongHocEntities2();

        public ActionResult Index()
        {
            var listGV = (from gv in db.GiaoVien
                          join nd in db.NguoiDung on gv.NguoiDungID equals nd.NguoiDungID

                          // GiaoVien → MonHoc
                          join mh in db.MonHoc on gv.MonHocID equals mh.MonHocID

                          // MonHoc → BoMon (LEFT JOIN)
                          join bm in db.BoMon on mh.BoMonID equals bm.BoMonID into tempBM
                          from bm in tempBM.DefaultIfEmpty()

                          select new GiaoVienModel
                          {
                              GiaoVienID = gv.GiaoVienID,
                              HoTen = nd.HoTen,
                              TenBoMon = bm != null ? bm.TenBoMon : "",
                              SDT = nd.SDT,
                              AnhDaiDien = nd.AnhDaiDien,
                              VaiTro = nd.VaiTro
                          }).ToList();

            // Convert VaiTro
            foreach (var item in listGV)
            {
                item.VaiTro = ConvertRole(item.VaiTro);
            }

            // HIỂN THỊ THÔNG BÁO MỚI NHẤT
            string vaiTro = Session["VaiTro"]?.ToString();
            DateTime now = DateTime.Now;

            var thongBaoMoiNhat = db.ThongBao
                .Where(tb =>
                    tb.TrangThai == true &&
                    tb.NgayThongBao <= now &&
                    (tb.NgayKetThucTB == null || tb.NgayKetThucTB >= now) &&
                    (
                        (vaiTro == "HocSinh" && tb.GuiChoHocSinh) ||
                        (vaiTro == "GiaoVien" && tb.GuiChoGiaoVien) ||
                        (vaiTro == "HieuTruong" && tb.GuiChoGiaoVien) ||
                        (vaiTro == "HieuPho" && tb.GuiChoGiaoVien) ||
                        (vaiTro == "BiThu" && tb.GuiChoGiaoVien)
                    )
                )
                .OrderByDescending(tb => tb.NgayThongBao) // ⭐ MỚI NHẤT
                .FirstOrDefault();                        // ⭐ CHỈ 1 CÁI

            ViewBag.ThongBaoPopup = thongBaoMoiNhat;

            return View(listGV); // hoặc View() tương ứng


        }


        private string ConvertRole(string role)
        {
            switch (role)
            {
                case "HieuTruong":
                    return "Hiệu trưởng";

                case "HieuPho":
                    return "Phó hiệu trưởng";

                case "BiThu":
                    return "Bí thư";

                case "PhoBiThu":
                    return "Phó bí thư";

                case "GiaoVien":
                    return "Giáo viên";

                default:
                    return role; // fallback
            }
        }


    }
}
