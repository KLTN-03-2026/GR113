using demomvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace demomvc.Controllers
{
    public class DemThongBaoController : Controller
    {
        QuanLyTruongHocEntities2 db = new QuanLyTruongHocEntities2();
        // GET: DemThongBao
        public ActionResult ThongBaoNguoiDung()
        {
            if (Session["UserID"] == null || Session["VaiTro"] == null)
                return RedirectToAction("Index", "DangNhap");

            int userId = (int)Session["UserID"];
            string vaiTro = Session["VaiTro"].ToString();

            var query = db.ThongBao.Where(tb => tb.TrangThai);

            if (vaiTro == "HocSinh")
                query = query.Where(tb => tb.GuiChoHocSinh);
            else
                query = query.Where(tb => tb.GuiChoGiaoVien);

            var dsThongBao = query
                .OrderByDescending(tb => tb.NgayThongBao)
                .ToList();

            // 👉 LẤY DANH SÁCH ID THÔNG BÁO ĐÃ ĐỌC
            var dsDaDoc = db.ThongBaoDaDoc
                .Where(d => d.NguoiDungID == userId)
                .Select(d => d.ThongBaoID)
                .ToList();

            ViewBag.DaDoc = dsDaDoc;

            return View(dsThongBao);
        }

        //[HttpGet]
        //public JsonResult DemThongBaoChuaDoc()
        //{
        //    if (Session["UserID"] == null || Session["VaiTro"] == null)
        //        return Json(0, JsonRequestBehavior.AllowGet);

        //    int nguoiDungid = (int)Session["UserID"];
        //    string vaiTro = Session["VaiTro"].ToString();

        //    var query = db.ThongBao.Where(tb => tb.TrangThai);

        //    if (vaiTro == "HocSinh")
        //        query = query.Where(tb => tb.GuiChoHocSinh);
        //    else
        //        query = query.Where(tb => tb.GuiChoGiaoVien);

        //    int count = query.Count(tb =>
        //        !db.ThongBaoDaDoc.Any(d =>
        //            d.ThongBaoID == tb.ThongBaoID &&
        //            d.NguoiDungID == nguoiDungid));

        //    return Json(count, JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public JsonResult DemThongBaoChuaDoc()
        {
            if (Session["UserID"] == null || Session["VaiTro"] == null)
                return Json(0, JsonRequestBehavior.AllowGet);

            int nguoiDungID = (int)Session["UserID"];
            string vaiTro = Session["VaiTro"].ToString();

            // 1️⃣ Lấy danh sách ID thông báo đã đọc
            var dsDaDoc = db.ThongBaoDaDoc
                .Where(d => d.NguoiDungID == nguoiDungID)
                .Select(d => d.ThongBaoID)
                .ToList();

            // 2️⃣ Lọc thông báo theo vai trò
            var query = db.ThongBao.Where(tb => tb.TrangThai);

            if (vaiTro == "HocSinh")
                query = query.Where(tb => tb.GuiChoHocSinh);
            else
                query = query.Where(tb => tb.GuiChoGiaoVien);

            // 3️⃣ Đếm thông báo CHƯA đọc
            int count = query.Count(tb => !dsDaDoc.Contains(tb.ThongBaoID));

            return Json(count, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult ChiTietThongBaoNguoiDung(int id)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Index", "DangNhap");

            int userId = (int)Session["UserID"];

            var tb = db.ThongBao.Find(id);
            if (tb == null || !tb.TrangThai)
                return HttpNotFound();

            // 🔴 KIỂM TRA ĐÃ ĐỌC CHƯA
            bool daDoc = db.ThongBaoDaDoc.Any(d =>
                d.ThongBaoID == id &&
                d.NguoiDungID == userId);

            // 🔴 NẾU CHƯA ĐỌC → GHI VÀO DB
            if (!daDoc)
            {
                ThongBaoDaDoc doc = new ThongBaoDaDoc
                {
                    ThongBaoID = id,
                    NguoiDungID = userId,
                    NgayDoc = DateTime.Now
                };

                db.ThongBaoDaDoc.Add(doc);
                db.SaveChanges();
            }

            return View(tb);
        }


    }
}