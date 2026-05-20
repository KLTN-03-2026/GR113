using demomvc.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace demomvc.Controllers
{
    public class XetHanhKiemController : Controller
    {
        QuanLyTruongHocEntities2 db = new QuanLyTruongHocEntities2();

        // GET: XetHanhKiem
        public ActionResult Index(int? namHocId)
        {
            var model = new HanhKiem();

            // 1. lấy danh sách năm học
            model.NamHocs = db.NamHoc
                .OrderBy(x => x.TenNamHoc)
                .ToList();

            // 2. chọn mặc định nếu chưa chọn
            if (namHocId == null && model.NamHocs.Any())
            {
                namHocId = model.NamHocs.First().NamHocID;
            }

            model.NamHocID = namHocId;

            // 3. lấy học kỳ theo năm học
            if (namHocId != null)
            {
                model.HocKys = db.HocKy
                    .Where(x => x.NamHocID == namHocId)
                    .ToList();
            }
            else
            {
                model.HocKys = new List<HocKy>();
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(int NamHocID, int HocKyID)
        {
            if (Session["UserID"] == null)
                return Content("Bạn chưa đăng nhập!");

            int nguoiDungId = Convert.ToInt32(Session["UserID"]);
            var gv = db.GiaoVien.FirstOrDefault(g => g.NguoiDungID == nguoiDungId);

            var model = new HanhKiem();

            // dropdown
            model.NamHocs = db.NamHoc.OrderBy(x => x.TenNamHoc).ToList();
            model.HocKys = db.HocKy.Where(x => x.NamHocID == NamHocID).ToList();

            model.NamHocID = NamHocID;
            model.HocKyID = HocKyID;

            //TRUYEN TEN HO KI
            var hocKy = db.HocKy.FirstOrDefault(x => x.HocKyID == HocKyID);

            ViewBag.TenHocKy = hocKy != null ? hocKy.TenHocKy : "";
            // lớp chủ nhiệm
            var lop = db.LopHoc.FirstOrDefault(l =>
                l.GiaoVienChuNhiem == gv.GiaoVienID &&
                l.NienKhoa == NamHocID
            );

            if (lop == null)
            {
                ViewBag.Loi = "Bạn không có lớp chủ nhiệm ở năm học này.";
                return View(model);
            }

            model.LopID = lop.LopHocID;
            model.TenLop = lop.TenLop;

            //  LOAD HS (bình thường)
            model.HocSinhs = db.HocSinh
                .Where(x => x.LopHocID == lop.LopHocID)
                .ToList();

            //  LOAD HẠNH KIỂM RIÊNG
            var ketQuas = db.KetQuaHocTap
                .Where(k => k.NamHocID == NamHocID && k.HocKyID == HocKyID)
                .ToList();

            //  MAP vào Dictionary
            ViewBag.HanhKiemDict = ketQuas.ToDictionary(
                k => k.HocSinhID,
                k => k.HanhKiem
            );

            //hk ca nam
            //var ketQuaNam = db.KetQuaHocTap
            //.Where(k => k.NamHocID == NamHocID)
            //.ToList();
            var listHocKy = db.HocKy
                .Where(x => x.NamHocID == NamHocID)
                .OrderBy(x => x.HocKyID)
                .ToList();

            int hocKyCuoi = listHocKy.Any() ? listHocKy.Last().HocKyID : 0;

            // có phải HK cuối không
            ViewBag.LaHocKy2 = HocKyID == hocKyCuoi;

            // map dữ liệu cả năm
            ViewBag.HanhKiemNamDict = db.KetQuaHocTap
                .Where(k => k.NamHocID == NamHocID && k.HocKyID == hocKyCuoi)
                .ToDictionary(k => k.HocSinhID, k => k.HanhKiemCaNam);
            // map cả năm
           // ViewBag.LaHocKy2 = hocKy != null && hocKy.TenHocKy.Contains("2");

            return View(model);
        }
        //[HttpPost]
        //public ActionResult AddHanhKiem(int NamHocID, int HocKyID, FormCollection f)
        //{
        //    // lấy tất cả dropdown hạnh kiểm
        //    var keys = f.AllKeys
        //        .Where(x => x.StartsWith("hanhKiem["))
        //        .ToList();

        //    var listHocKy = db.HocKy
        //            .Where(x => x.NamHocID == NamHocID)
        //            .OrderBy(x => x.HocKyID)
        //            .ToList();

        //    bool laHocKyCuoi = listHocKy.Last().HocKyID == HocKyID;
        //    foreach (var key in keys)
        //    {
        //        // lấy id học sinh
        //        string idStr = key.Replace("hanhKiem[", "").Replace("]", "");
        //        int hocSinhId = Convert.ToInt32(idStr);

        //        // lấy giá trị hạnh kiểm
        //        string hanhKiem = f[key];

        //        // bỏ qua nếu chưa chọn
        //        if (string.IsNullOrEmpty(hanhKiem))
        //            continue;

        //        // tìm kết quả cũ
        //        var ketQua = db.KetQuaHocTap.FirstOrDefault(x =>
        //            x.HocSinhID == hocSinhId &&
        //            x.NamHocID == NamHocID &&
        //            x.HocKyID == HocKyID
        //        );
        //        // chưa có => thêm mới
        //        if (ketQua == null)
        //        {
        //            ketQua = new KetQuaHocTap
        //            {
        //                HocSinhID = hocSinhId,
        //                NamHocID = NamHocID,
        //                HocKyID = HocKyID,
        //                HanhKiem = hanhKiem
        //            };

        //            db.KetQuaHocTap.Add(ketQua);
        //        }
        //        else
        //        {
        //            // cập nhật
        //            ketQua.HanhKiem = hanhKiem;
        //        }


        //        if (laHocKyCuoi)
        //        {
        //            ketQua.HanhKiemCaNam = hanhKiem;
        //        }


        //    }
        //    db.SaveChanges();

        //    TempData["Success"] = "Lưu hạnh kiểm thành công!";

        //    // load lại đúng lớp
        //    return RedirectToAction("Index");
        //}
        [HttpPost]
        public ActionResult AddHanhKiem(int NamHocID, int HocKyID, FormCollection f)
        {
            var keys = f.AllKeys
                .Where(x => x.StartsWith("hanhKiem["))
                .ToList();

            // ✅ Lấy danh sách học kỳ
            var listHocKy = db.HocKy
                .Where(x => x.NamHocID == NamHocID)
                .OrderBy(x => x.HocKyID)
                .ToList();

            // ✅ Kiểm tra học kỳ cuối an toàn
            bool laHocKyCuoi = listHocKy.Any() && listHocKy.Last().HocKyID == HocKyID;

            foreach (var key in keys)
            {
                string idStr = key.Replace("hanhKiem[", "").Replace("]", "");
                int hocSinhId = Convert.ToInt32(idStr);

                string hanhKiem = f[key];

                if (string.IsNullOrEmpty(hanhKiem))
                    continue;

                var ketQua = db.KetQuaHocTap.FirstOrDefault(x =>
                    x.HocSinhID == hocSinhId &&
                    x.NamHocID == NamHocID &&
                    x.HocKyID == HocKyID
                );

                if (ketQua == null)
                {
                    ketQua = new KetQuaHocTap
                    {
                        HocSinhID = hocSinhId,
                        NamHocID = NamHocID,
                        HocKyID = HocKyID,
                        HanhKiem = hanhKiem
                    };

                    db.KetQuaHocTap.Add(ketQua);
                }
                else
                {
                    ketQua.HanhKiem = hanhKiem;
                }

                // ✅ HK CUỐI → cập nhật cả năm
                if (laHocKyCuoi)
                {
                    ketQua.HanhKiemCaNam = hanhKiem;
                }
            }

            db.SaveChanges();

            TempData["Success"] = "Lưu hạnh kiểm thành công!";

            return RedirectToAction("Index");
        }
    }
}