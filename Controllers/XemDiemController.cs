using demomvc.App_Start;
using demomvc.Models;
using demomvc.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace demomvc.Controllers
{
    [RoleAuthorize(RolesRequired = "HocSinh")]
    public class XemDiemController : Controller
    {
        // GET: XemDiem
        QuanLyTruongHocEntities2 db = new QuanLyTruongHocEntities2();
        public ActionResult Index()
        {
            BangDiemViewModel model = new BangDiemViewModel();
            
            model.NamHocs = db.NamHoc.ToList();
            model.HocKys = db.HocKy.ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(int NamHocID, int HocKyID)
        {
            int userID = Convert.ToInt32(Session["UserID"]);

            var hs = db.HocSinh.FirstOrDefault(x => x.NguoiDungID == userID);
            if (hs == null) return RedirectToAction("DangNhap");

            var diemList = db.Diem
            .Where(x => x.HocSinhID == hs.HocSinhID
                        && x.NamHocID == NamHocID
                        && x.HocKyID == HocKyID)
            .Join(db.MonHoc, d => d.MonHocID, m => m.MonHocID, (d, m) => new DiemMonViewModel
            {
                TenMon = m.TenMonHoc,
                DiemMieng = d.DiemMieng,
                Diem15p = d.Diem15p,

                DiemGK = d.DiemGK,
                DiemCK = d.DiemCK,
                DiemTBMon = d.DiemTB ?? 0,
            }).ToList();

            //DTBMON
            foreach (var item in diemList)
            {


                // ĐÁNH GIÁ Đ / CĐ CHO Âm nhạc + Mỹ thuật + Thể dục
                if (item.TenMon.Contains("Âm nhạc")
                    || item.TenMon.Contains("Mĩ thuật")
                    || item.TenMon.Contains("Thể dục"))
                {
                    item.DiemDanhGia = item.DiemTBMon >= 6 ? "Đ" : "CĐ";
                }
            }

            var ketQua = db.KetQuaHocTap
                .FirstOrDefault(x => x.HocSinhID == hs.HocSinhID
                        && x.NamHocID == NamHocID
                        && x.HocKyID == HocKyID);

            double tbHocKy = ketQua?.DTBTong ?? 0;
            string xepLoai = ketQua?.HocLuc ?? "_";
            string hanhKiem = ketQua?.HanhKiem ?? "_";

            var hk1 = db.KetQuaHocTap.FirstOrDefault(x => x.HocSinhID == hs.HocSinhID
                    && x.NamHocID == NamHocID
                    && x.HocKyID == 1);

            var hk2 = db.KetQuaHocTap.FirstOrDefault(x =>
                   x.HocSinhID == hs.HocSinhID &&
                   x.NamHocID == NamHocID &&
                   x.HocKyID == 2);



            //diem ca nam 
            double? DiemTB_CN = null;
            if (hk1 != null && hk2 != null)
            {
                DiemTB_CN = Math.Round(((hk1.DTBTong ?? 0) + (hk2.DTBTong ?? 0) * 2) / 3, 2);
                
            }

            //hanh kiem ca nam
            string hanhKiemCN = hk2?.HanhKiem ?? hk1?.HanhKiem ?? "";

            //xep loai hoc luc ca nam
            string xepLoaiCN = "_";
            if (DiemTB_CN != null)
            {
                if (DiemTB_CN >= 8) xepLoaiCN = "Giỏi";
                else if (DiemTB_CN >= 6.5) xepLoaiCN = "Khá";
                else if (DiemTB_CN >= 5) xepLoaiCN = "Trung bình";
                else if (DiemTB_CN >= 3.5) xepLoaiCN = "Yếu";
                else xepLoaiCN = "Kém";
            }

            var model = new BangDiemViewModel
            {
                NamHocID = NamHocID,
                HocKyID = HocKyID,
                DiemMonList = diemList,
                NamHocs = db.NamHoc.ToList(),
                HocKys = db.HocKy.ToList(),
                DiemTBHocKy = tbHocKy,
                XepLoai = xepLoai,
                HanhKiem = hanhKiem,
                DiemTBHocCaNam = DiemTB_CN,
                HanhKiemCaNam = hanhKiemCN,
                XepLoaiCaNam = xepLoaiCN
            };

            return View(model);
        }


        [HttpGet]
        public JsonResult GetHocKyByNamHoc(int namHocId)
        {
            using (var db = new QuanLyTruongHocEntities2())
            {
                var dsHocKy = db.HocKy
                    .Where(h => h.NamHocID == namHocId)
                    .Select(h => new
                    {
                        h.HocKyID,
                        h.TenHocKy
                    })
                    .ToList();

                return Json(dsHocKy, JsonRequestBehavior.AllowGet);
            }
        }
    }
}