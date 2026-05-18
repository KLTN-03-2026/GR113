using demomvc.App_Start;
using demomvc.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer.Symbols;
using System.Web.UI.WebControls;

namespace demomvc.Controllers
{
    [RoleAuthorize(RolesRequired = "HieuTruong")]
    public class QuanLyGiaoVienController : Controller
    {
        // GET: QuanLyGiaoVien
        QuanLyTruongHocEntities2 db = new QuanLyTruongHocEntities2();
        public ActionResult Index(string type, string keyword)
        {
            keyword = keyword ?? "";
            string kw = bodau(keyword.ToLower());

            var dsGV = (from gv in db.GiaoVien
                        join nd in db.NguoiDung on gv.NguoiDungID equals nd.NguoiDungID

                        join mh in db.MonHoc on gv.MonHocID equals mh.MonHocID into monhocGroup
                        from mh in monhocGroup.DefaultIfEmpty()

                        join bm in db.BoMon on mh.BoMonID equals bm.BoMonID into bomonGroup
                        from bm in bomonGroup.DefaultIfEmpty()

                        join lh in db.LopHoc on gv.GiaoVienID equals lh.GiaoVienChuNhiem into temp
                        from lop in temp.DefaultIfEmpty()

                        select new GiaoVienVM
                        {
                            GiaoVienID = gv.GiaoVienID,
                            HoTen = nd.HoTen,
                            NgaySinh = gv.NgaySinh,
                            GioiTinh = gv.GioiTinh,
                            LopChuNhiem = lop != null ? lop.TenLop : "Không chủ nhiệm",

                            TenMonHoc = mh != null ? mh.TenMonHoc : "Chưa phân công",
                            TenBoMon = bm != null ? bm.TenBoMon : "Chưa phân công",

                            VaiTro = nd.VaiTro,
                            TrangThaiGiangDay = gv.TrangThaiGiangDay,
                        }).ToList();

            //--------- TÌM KIẾM ----------
            ViewBag.Type = type;
            ViewBag.Keyword = keyword;

            if (string.IsNullOrWhiteSpace(keyword))
                return View(dsGV);

            if (type == "hoten")
            {
                dsGV = dsGV.Where(g => bodau(g.HoTen.ToLower()).Contains(kw)).ToList();
            }
            else if (type == "monhoc")
            {
                dsGV = dsGV.Where(g => bodau(g.TenMonHoc.ToLower()).Contains(kw)).ToList();
            }
            else if (type == "vaitro")
            {
                dsGV = dsGV.Where(g =>
                    convertRoleSearch(g.VaiTro).Contains(kw)
                ).ToList();
            }


            return View(dsGV);
        }


        public string bodau(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // TRẢ VỀ FormC ĐÚNG CHUẨN
            return stringBuilder.ToString().Replace("đ", "d").Replace("Đ", "D")
                .Normalize(NormalizationForm.FormC);
        }

        private string convertRoleSearch(string role)
        {
            if (string.IsNullOrEmpty(role)) return "";

            // Tách chữ hoa thành khoảng trắng
            string spaced = System.Text.RegularExpressions.Regex.Replace(role, "([A-Z])", " $1");

            return bodau(spaced.ToLower()).Trim();
        }




       
        public ActionResult ChiTietGV(int id)
        {
            var gvct = (from gv in db.GiaoVien
                        join nd in db.NguoiDung on gv.NguoiDungID equals nd.NguoiDungID

                        join mh in db.MonHoc on gv.MonHocID equals mh.MonHocID into monhocGroup
                        from mh in monhocGroup.DefaultIfEmpty()

                        join bm in db.BoMon on mh.BoMonID equals bm.BoMonID into bomonGroup
                        from bm in bomonGroup.DefaultIfEmpty()

                        join lh in db.LopHoc on gv.GiaoVienID equals lh.GiaoVienChuNhiem into temp
                        from lop in temp.DefaultIfEmpty()

                        where gv.GiaoVienID == id

                        select new GiaoVienVM
                        {
                            GiaoVienID = gv.GiaoVienID,
                            HoTen = nd.HoTen,
                            NgaySinh = gv.NgaySinh,
                            GioiTinh = gv.GioiTinh,

                            //  HIỂN THỊ
                            LopChuNhiem = lop != null ? lop.TenLop : "Không chủ nhiệm",
                            TenMonHoc = mh != null ? mh.TenMonHoc : "Chưa phân công",
                            TenBoMon = bm != null ? bm.TenBoMon : "Chưa phân công",

                            // QUAN TRỌNG (DÙNG CHO EDIT)
                            MonHocID = gv.MonHocID,
                            BoMonID = mh != null ? mh.BoMonID : (int?)null,
                            LopChuNhiemID = lop != null ? (int?)lop.LopHocID : null,

                            TrangThaiGiangDay = gv.TrangThaiGiangDay,
                            Email = nd.Email,
                            SDT = nd.SDT,
                            TrangThaiTK = nd.TrangThaiTK,
                            VaiTro = nd.VaiTro
                        }).FirstOrDefault();

            if (gvct == null)
            {
                return HttpNotFound();
            }

            // ✅ BỔ SUNG DROPDOWN (KHÔNG MẤT LOGIC CŨ)
            ViewBag.ListMonHoc = db.MonHoc
                .ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.MonHocID.ToString(),
                    Text = x.TenMonHoc
                }).ToList();

            ViewBag.ListLop = db.LopHoc
                .ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.LopHocID.ToString(),
                    Text = x.TenLop
                }).ToList();

            return View(gvct);
        }


        private string TaoTenDangNhap(string hoTen)
        {
            //chuan ho aho ten
            hoTen = hoTen.Trim().ToLower();

            //bo dau tieng viet
            string normalized = hoTen.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            for (int i = 0; i < normalized.Length; i++)
            {
                var c = normalized[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            string cleanName = sb.ToString().Replace("đ", "d");

            //tach ho ten
            var parts = cleanName.Split(' ');
            string lastName = parts.Last();
            string firstChars = string.Concat(parts.Take(parts.Length - 1).Select(x => x[0]));

            string baseUsername = firstChars + lastName;

            //kiem tra trung sername
            string username = baseUsername;
            int count = 1;

            while (db.NguoiDung.Any(x => x.TenDangNhap == username))
            {
                username = baseUsername + count;
                count++;
            }
            return username;
        }
        public ActionResult ThemMoiGV()
        {
            ViewBag.MonHoc = db.MonHoc.ToList();
            return View();
        }

        [HttpPost]
        public ActionResult ThemMoiGV(GiaoVienVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MonHoc = db.MonHoc.ToList();  // Gán lại cho dropdown
                return View(model);
            }

            // Tạo tên đăng nhập
            string username = TaoTenDangNhap(model.HoTen);

            string matkhaumacdinh = "12345";
            string matKhauHash = PasswordHelper.HashPassword(matkhaumacdinh);//ma hóa maatjj khẩu 

            var nd = new NguoiDung
            {
                HoTen = model.HoTen,
                Email = model.Email,
                SDT = model.SDT,
                VaiTro = model.VaiTro,
                TrangThaiTK = model.TrangThaiTK,
                TenDangNhap = username,
                MatKhau = matKhauHash//mat khau da mã hóa
            };

            db.NguoiDung.Add(nd);
            db.SaveChanges();

            var gv = new GiaoVien
            {
                NguoiDungID = nd.NguoiDungID,
                NgaySinh = model.NgaySinh,
                GioiTinh = model.GioiTinh,
                TrangThaiGiangDay = model.TrangThaiGiangDay,
                MonHocID = model.MonHocID
            };

            db.GiaoVien.Add(gv);
            db.SaveChanges();
            //ViewBag.Message = "Thêm thành công";
            TempData["Success"] = "Thêm giáo viên thành công!";
            return RedirectToAction("Index");
        }

        //phan cong giang day
        public ActionResult PhanCongGiangDay(string type, string keyword)
        {
            var dsgv = (from gv in db.GiaoVien
                        join nd in db.NguoiDung on gv.NguoiDungID equals nd.NguoiDungID
                        join mh in db.MonHoc on gv.MonHocID equals mh.MonHocID into tempMH
                        from mon in tempMH.DefaultIfEmpty()
                        select new GiaoVienVM
                        {
                            GiaoVienID = gv.GiaoVienID,
                            HoTen = nd.HoTen,
                            TenMonHoc = mon != null ? mon.TenMonHoc : "Chưa phân công"
                        }).ToList();
            ViewBag.Keyword = keyword;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                dsgv = dsgv.Where(g =>
                    bodau(g.HoTen.ToLower()).Contains(bodau(keyword.ToLower()))
                ).ToList();
            }

            ViewBag.MonHoc = db.MonHoc.ToList();
            return View(dsgv);
        }

        [HttpPost]
        public ActionResult CapNhatPhanCong(int giaoVienID, int monHocID)
        {
            var gv = db.GiaoVien.FirstOrDefault(x => x.GiaoVienID == giaoVienID);
            if (gv == null) return Json(new { success = false });

            gv.MonHocID = monHocID;
            db.SaveChanges();
            return Json(new { success = true });
        }



        /// <summary>
        /// //////
        /// </summary>
        /// <returns></returns>
        public ActionResult PhanCongGiangDayTKB()
        {
            var model = db.GiaoVien
                .Join(db.NguoiDung,
                    gv => gv.NguoiDungID,
                    nd => nd.NguoiDungID,
                    (gv, nd) => new GiaoVienVM
                    {
                        GiaoVienID = gv.GiaoVienID,
                        HoTen = nd.HoTen,
                        TenMonHoc = gv.MonHoc != null ? gv.MonHoc.TenMonHoc : "Chưa có môn"
                    })
                .ToList();

            ViewBag.NamHoc = db.NamHoc.ToList();
            ViewBag.HocKy = db.HocKy.ToList();

            return View(model);
        }

        // ====== LOAD LỚP ======
        [HttpGet]
        public JsonResult GetLopByNamHoc(int namHocId, int hocKyId, int giaoVienId)
        {
            var gv = db.GiaoVien.FirstOrDefault(x => x.GiaoVienID == giaoVienId);
            if (gv == null)
                return Json(new { }, JsonRequestBehavior.AllowGet);

            int? monHocId = gv.MonHocID;

            var dsLop = db.LopHoc
                .Where(x => x.NienKhoa == namHocId)
                .Select(x => new
                {
                    x.LopHocID,
                    x.TenLop
                })
                .ToList();

            var daChon = db.PhanCongGiangDay
                .Where(x => x.GiaoVienID == giaoVienId && x.HocKyID == hocKyId)
                .Select(x => x.LopHocID)
                .ToList();

            var lopBiTrung = db.PhanCongGiangDay
                .Where(x => x.HocKyID == hocKyId &&
                            x.GiaoVien.MonHocID == monHocId &&
                            x.GiaoVienID != giaoVienId)
                .Select(x => x.LopHocID)
                .ToList();

            return Json(new
            {
                dsLop,
                daChon,
                lopBiTrung
            }, JsonRequestBehavior.AllowGet);
        }

        // ====== PHÂN CÔNG ======
        [HttpPost]
        public JsonResult PhanCongTableAjax(int giaoVienId, int hocKyId, List<int> lopIDs)
        {
            if (lopIDs == null || !lopIDs.Any())
            {
                return Json(new { success = false, message = "Chưa chọn lớp" });
            }

            var gv = db.GiaoVien.FirstOrDefault(x => x.GiaoVienID == giaoVienId);
            if (gv == null)
                return Json(new { success = false, message = "Không có giáo viên" });

            if (!gv.MonHocID.HasValue)
                return Json(new { success = false, message = "GV chưa có môn" });

            int monHocId = gv.MonHocID.Value;

            foreach (var lopID in lopIDs)
            {
                // đã tồn tại → bỏ qua
                bool daTonTai = db.PhanCongGiangDay.Any(x =>
                    x.GiaoVienID == giaoVienId &&
                    x.LopHocID == lopID &&
                    x.HocKyID == hocKyId);

                if (daTonTai) continue;

                // trùng môn
                bool trung = db.PhanCongGiangDay.Any(x =>
                    x.LopHocID == lopID &&
                    x.HocKyID == hocKyId &&
                    x.GiaoVien.MonHocID == monHocId);

                if (trung)
                {
                    return Json(new
                    {
                        success = false,
                        message = "❌ Lớp đã có GV dạy môn này!"
                    });
                }

                db.PhanCongGiangDay.Add(new PhanCongGiangDay
                {
                    GiaoVienID = giaoVienId,
                    LopHocID = lopID,
                    HocKyID = hocKyId
                });
            }

            db.SaveChanges();

            return Json(new { success = true, message = "✅ Thành công!" });
        }

        public JsonResult GetBoMonByMon(int monHocID)
        {
            var mon = db.MonHoc.FirstOrDefault(x => x.MonHocID == monHocID);

            if (mon == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            var bm = db.BoMon.FirstOrDefault(x => x.BoMonID == mon.BoMonID);

            return Json(new
            {
                tenBoMon = bm.TenBoMon
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatGV(GiaoVienVM model)
        {
            var gv = db.GiaoVien.FirstOrDefault(x => x.GiaoVienID == model.GiaoVienID);
            if (gv == null)
            {
                TempData["Error"] = "Không tìm thấy giáo viên";
                return RedirectToAction("Index");
            }

            // 2.Lấy người dùng tương ứng
            var nd = db.NguoiDung.FirstOrDefault(x => x.NguoiDungID == gv.NguoiDungID);

            // ==== UPDATE NGUOIDUNG ====
            nd.HoTen = model.HoTen;
            nd.Email = model.Email;
            nd.SDT = model.SDT;
            nd.TrangThaiTK = model.TrangThaiTK;
            nd.VaiTro = model.VaiTro;

            // ==== UPDATE GIAOVIEN ====
            gv.NgaySinh = model.NgaySinh;
            gv.GioiTinh = model.GioiTinh;
            gv.TrangThaiGiangDay = model.TrangThaiGiangDay;

            // ✅ cập nhật môn
            gv.MonHocID = model.MonHocID;

            // ✅ cập nhật lớp chủ nhiệm
            if (model.LopChuNhiemID.HasValue)
            {
                // reset lớp cũ nếu có
                var lopCu = db.LopHoc
                    .Where(x => x.GiaoVienChuNhiem == gv.GiaoVienID)
                    .ToList();

                foreach (var l in lopCu)
                {
                    l.GiaoVienChuNhiem = null;
                }

                // set lớp mới
                var lopMoi = db.LopHoc.FirstOrDefault(x => x.LopHocID == model.LopChuNhiemID);
                if (lopMoi != null)
                {
                    lopMoi.GiaoVienChuNhiem = gv.GiaoVienID;
                }
            }

            db.SaveChanges();

            TempData["Success"] = "✅ Cập nhật giáo viên thành công!";
            return RedirectToAction("Index");

        }

    }
}