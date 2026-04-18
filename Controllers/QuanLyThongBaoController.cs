using demomvc.App_Start;
using demomvc.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Packaging;
using iText.Kernel.Pdf;
using iText.Html2pdf;


using OpenXmlPowerTools;




// 👉 ALIAS
using WordHtmlConverter = OpenXmlPowerTools.HtmlConverter;
using PdfHtmlConverter = iText.Html2pdf.HtmlConverter;



using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Configuration;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data.SqlClient;

namespace demomvc.Controllers


{
    [RoleAuthorize(RolesRequired = "HieuTruong")]
    public class QuanLyThongBaoController : Controller
    {
        QuanLyTruongHocEntities1 db = new QuanLyTruongHocEntities1();

        // GET
        public ActionResult Index()
        {
            ViewBag.DanhSachThongBao = db.ThongBao
                                         .OrderByDescending(t => t.NgayThongBao)
                                         .ToList();

            return View(new ThongBaoVM());
        }

        // POST
        [HttpPost]
        public ActionResult ThemMoiThongBao(ThongBaoVM model, HttpPostedFileBase file)
        {
            bool coNoiDungTay = !string.IsNullOrWhiteSpace(model.NoiDung);
            bool coFile = file != null && file.ContentLength > 0;
            if (!coNoiDungTay && !coFile)
            {
                ModelState.AddModelError("", "⚠ Vui lòng nhập nội dung hoặc upload file Word");
            }

            // ❌ Có cả 2
            if (coNoiDungTay && coFile)
            {
                ModelState.AddModelError("", "⚠ Chỉ được chọn 1: nhập tay HOẶC upload file");
            }
            // kiểm tra chọn đối tượng
            if (!model.GuiChoHocSinh && !model.GuiChoGiaoVien)
            {
                ModelState.AddModelError("", "⚠ Vui lòng chọn ít nhất Học sinh hoặc Giáo viên");
            }


            // kiểm tra ngày kết thúc
            if (model.NgayKetThucTB == null)
            {
                ModelState.AddModelError("", "Chọn ngày kết thúc");
            }
            else if (model.NgayKetThucTB <= DateTime.Now)
            {
                ModelState.AddModelError("", "Ngày kết thúc lớn hơn ngày hiện tại");
            }
            // Upload file Word
            if (coFile)
            {
                var ext = Path.GetExtension(file.FileName).ToLower();
                if (ext != ".docx")
                {
                    ModelState.AddModelError("", "Chỉ hỗ trợ file Word (.docx)");
                }
                else
                {
                    model.NoiDung = ConvertWordToHtml(file); // 🔥 HTML CHUẨN
                }
            }

            // nếu lỗi
            if (!ModelState.IsValid)
            {
                ViewBag.ShowForm = true;
                ViewBag.DanhSachThongBao = db.ThongBao
                                             .OrderByDescending(t => t.NgayThongBao)
                                             .ToList();

                return View("Index", model);
            }

            // lưu DB
            ThongBao tb = new ThongBao
            {
                TieuDe = model.TieuDe,
                NoiDung = model.NoiDung,
                GuiChoHocSinh = model.GuiChoHocSinh,
                GuiChoGiaoVien = model.GuiChoGiaoVien,
                NgayThongBao = DateTime.Now,
                NgayKetThucTB = model.NgayKetThucTB,//láy ngày tư form
                NguoiTaoID = Convert.ToInt32(Session["UserID"]),
                TrangThai = true
            };

            db.ThongBao.Add(tb);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        [HttpPost]
        public JsonResult XoaThongBao(int id)
        {
            try
            {
                using (var db = new QuanLyTruongHocEntities1()) // đổi theo DbContext của bạn
                {
                    var tb = db.ThongBao.FirstOrDefault(x => x.ThongBaoID == id);
                    if (tb == null)
                    {
                        return Json(new { success = false });
                    }

                    db.ThongBao.Remove(tb);
                    db.SaveChanges();
                }
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        //[HttpPost]
        //public JsonResult SuaThongBao(ThongBao model)
        //{
        //    try
        //    {
        //        if (model == null || model.ThongBaoID == 0)
        //        {
        //            return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
        //        }

        //        var tb = db.ThongBao.Find(model.ThongBaoID); // ✅ SỬA Ở ĐÂY
        //        if (tb == null)
        //        {
        //            return Json(new { success = false, message = "Không tìm thấy thông báo" });
        //        }

        //        tb.TieuDe = model.TieuDe;
        //        tb.NoiDung = model.NoiDung;
        //        tb.GuiChoHocSinh = model.GuiChoHocSinh;
        //        tb.GuiChoGiaoVien = model.GuiChoGiaoVien;

        //        db.SaveChanges();

        //        return Json(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}

        [HttpPost]
        [ValidateInput(false)] // ⭐ BẮT BUỘC KHI NHẬN HTML
        public JsonResult SuaThongBao(ThongBao model)
        {
            try
            {
                if (model == null || model.ThongBaoID <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ"
                    });
                }

                var tb = db.ThongBao.Find(model.ThongBaoID);
                if (tb == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy thông báo"
                    });
                }

                // Cập nhật dữ liệu
                tb.TieuDe = model.TieuDe?.Trim();
                tb.NoiDung = model.NoiDung; // HTML Word / HTML viết tay
                tb.GuiChoHocSinh = model.GuiChoHocSinh;
                tb.GuiChoGiaoVien = model.GuiChoGiaoVien;
                tb.NgayKetThucTB = model.NgayKetThucTB;
                db.SaveChanges();

                return Json(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }

        [HttpGet]
        public JsonResult ChiTietSua(int id)
        {
            var tb = db.ThongBao.Find(id);
            if (tb == null)
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                success = true,
                id = tb.ThongBaoID,
                tieuDe = tb.TieuDe,
                noiDung = tb.NoiDung,
                guiChoHocSinh = tb.GuiChoHocSinh,
                guiChoGiaoVien = tb.GuiChoGiaoVien,
                ngayKetThucTB = tb.NgayKetThucTB.HasValue
                ? tb.NgayKetThucTB.Value.ToString("yyyy-MM-ddTHH:mm")
                : null
            }, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult DoiTrangThai(int id)
        {
            var tb = db.ThongBao.Find(id);
            if (tb == null)
            {
                return Json(new { success = false });

            }
            tb.TrangThai = !tb.TrangThai; // đảo trạng thái
            db.SaveChanges();

            return Json(new { success = true });

        }




        private string ConvertWordToHtml(HttpPostedFileBase file)
        {
            // Copy sang MemoryStream (ghi được)
            using (var ms = new MemoryStream())
            {
                file.InputStream.CopyTo(ms);
                ms.Position = 0;

                using (var wordDoc = WordprocessingDocument.Open(ms, true)) // ✅ OK
                {
                    var settings = new HtmlConverterSettings()
                    {
                        PageTitle = "Thong bao"
                    };

                    var htmlElement =
                        OpenXmlPowerTools.HtmlConverter.ConvertToHtml(wordDoc, settings);

                    return htmlElement.ToString(SaveOptions.DisableFormatting);
                }
            }
        }



        [HttpPost]
        public JsonResult DocNoiDungWord(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                return Json(new { success = false, message = "Chưa chọn file" });

            if (!file.FileName.EndsWith(".docx"))
                return Json(new { success = false, message = "Chỉ hỗ trợ .docx" });

            try
            {
                string html;

                using (var ms = new MemoryStream())
                {
                    // 🔥 BẮT BUỘC: copy sang stream ghi được
                    file.InputStream.CopyTo(ms);
                    ms.Position = 0;

                    using (var wordDoc = WordprocessingDocument.Open(ms, true)) // ✅ TRUE
                    {
                        var settings = new HtmlConverterSettings()
                        {
                            PageTitle = "Thong bao"
                        };

                        var element =
                            WordHtmlConverter.ConvertToHtml(wordDoc, settings);

                        html = element.ToString(SaveOptions.DisableFormatting);
                    }
                }

                return Json(new { success = true, html });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult ChiTiet(int id)
        {
            var tb = db.ThongBao.Find(id);
            if (tb == null)
                return Json(new { }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                tieuDe = tb.TieuDe,
                ngay = tb.NgayThongBao.ToString("dd/MM/yyyy HH:mm"),
                doiTuong =
                    (tb.GuiChoHocSinh ? "Học sinh " : "") +
                    (tb.GuiChoGiaoVien ? "Giáo viên" : ""),
                noiDung = tb.NoiDung // HTML WORD / HTML TAY
            }, JsonRequestBehavior.AllowGet);
        }



        //Thong baos do luong 











    }
}
