using demomvc.App_Start;
using demomvc.Models;
using demomvc.ViewModel;
using ExcelFormula;
using OfficeOpenXml;
using OpenXmlPowerTools.HtmlToWml.CSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace demomvc.Controllers
{
    [RoleAuthorize(RolesRequired = "HieuTruong,GiaoVien")]
    public class QuanLyHocSinhController : Controller
    {
        QuanLyTruongHocEntities db = new QuanLyTruongHocEntities();
        // GET: QuanLyHocSinh
        [HttpGet]
        public ActionResult DsHocSinh(string TypeSearch, string Keyword)
        {

            var ListHS = db.HocSinh.Select(hs => new ThongTinHocSinhVM
            {
                MaHocSinh = hs.HocSinhID,
                HoTen = hs.NguoiDung.HoTen,
                TenKhoi = hs.LopHoc.KhoiLop.TenKhoi,
                KhoiLopID = hs.LopHoc.KhoiLop.KhoiLopID,
                LopHocID = hs.LopHoc.LopHocID,
                TenLop = hs.LopHoc.TenLop,
                NgaySinh = hs.NgaySinh,
                GioiTinh = hs.GioiTinh,
                TinhThanh = hs.TinhThanhPho,
                QuanHuyen = hs.QuanHuyen,
                DiaChi = hs.DiaChiNha,
                CCCD = hs.CCCD,
                DanToc = hs.DanToc,
                TrangThaiHocTap = hs.TrangThaiHocTap,
                GiaoVienChuNhiem = hs.LopHoc.GiaoVien.GiaoVienID


            }).ToList();
            if (TypeSearch == null || Keyword == null)
            {
                return View(ListHS);
            }
            else
            {
                if (TypeSearch == "Name")
                {

                    var hs = ListHS.Where(q => q.HoTen.ToLower().Trim().Contains(Keyword.ToLower().Trim())).ToList();
                    return View(hs);
                }
                else if (TypeSearch == "Id")
                {
                    int id;

                    if (!int.TryParse(Keyword, out id))
                    {
                        ViewBag.Error = "Sai định dạng ID (phải là số)";
                        return View(new List<ThongTinHocSinhVM>());
                    }
                    var hs = ListHS.Where(q => q.MaHocSinh == id).ToList();
                    return View(hs);
                }
                else
                {

                    var hs = ListHS.Where(q => q.TenLop.ToLower().Trim().Contains(Keyword.ToLower().Trim())).ToList();
                    return View(hs);
                }
            }

        }



        [HttpGet]
        public ActionResult ThemHocSinh()
        {
            var lophoc = db.LopHoc.ToList();
            return View(lophoc);
        }

        [HttpPost]
        public ActionResult ThemHocSinh(ThongTinHocSinhVM HocSinhVM)
        {
            try
            {
                string matKhauHash = PasswordHelper.HashPassword(HocSinhVM.MatKhau);//ma hóa maatjj khẩu 
                var nguoiDung = new NguoiDung()
                {
                    TenDangNhap = HocSinhVM.TenDangNhap,
                    MatKhau = matKhauHash,
                    HoTen = HocSinhVM.HoTen,
                    VaiTro = "HocSinh"

                };

                db.NguoiDung.Add(nguoiDung);
                db.SaveChanges();

                var hocsinh = new HocSinh()
                {
                    NguoiDungID = nguoiDung.NguoiDungID,
                    LopHocID = HocSinhVM.LopHocID,
                    NgaySinh = HocSinhVM.NgaySinh,
                    GioiTinh = HocSinhVM.GioiTinh,
                    TinhThanhPho = HocSinhVM.TinhThanh,
                    QuanHuyen = HocSinhVM.QuanHuyen,
                    DiaChiNha = HocSinhVM.DiaChi,
                    CCCD = HocSinhVM.CCCD,
                    DanToc = HocSinhVM.DanToc,
                    TrangThaiHocTap = HocSinhVM.TrangThaiHocTap

                };

                db.HocSinh.Add(hocsinh);
                db.SaveChanges();

                TempData["reponse"] = "success";
            }
            catch (Exception)
            {
                TempData["reponse"] = "error";
            }


            return RedirectToAction("ThemHocSinh");

        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var hocsinh = db.HocSinh.FirstOrDefault(hs => hs.HocSinhID == id);
            if (hocsinh == null)
            {
                return HttpNotFound(); // ✔ đúng chuẩn
            }

            var displaystudent = new ThongTinHocSinhVM()
            {
                MaHocSinh = hocsinh.HocSinhID,
                HoTen = hocsinh.NguoiDung.HoTen,
                LopHocID = hocsinh.LopHoc.LopHocID,
                DanhSachKhoi = db.KhoiLop.ToList(),
                DanhsachLop = db.LopHoc.ToList(),
                KhoiLopID = hocsinh.LopHoc.KhoiLop.KhoiLopID,
                NgaySinh = hocsinh.NgaySinh,
                GioiTinh = hocsinh.GioiTinh,
                DanToc = hocsinh.DanToc,
                CCCD = hocsinh.CCCD,
                TrangThaiHocTap = hocsinh.TrangThaiHocTap,
                TinhThanh = hocsinh.TinhThanhPho,
                QuanHuyen = hocsinh.QuanHuyen,
                DiaChi = hocsinh.DiaChiNha,


            };


            return View(displaystudent);
        }

        [HttpPost]
        public ActionResult Edit(ThongTinHocSinhVM HocSinh)
        {
            try
            {
                var hsinh = db.HocSinh.FirstOrDefault(hs => hs.HocSinhID == HocSinh.MaHocSinh);
                if (hsinh == null)
                {
                    return HttpNotFound();
                }
                hsinh.NguoiDung.HoTen = HocSinh.HoTen;
                hsinh.LopHoc.KhoiLopID = HocSinh.KhoiLopID;
                hsinh.LopHocID = HocSinh.LopHocID;
                hsinh.NgaySinh = HocSinh.NgaySinh;
                hsinh.GioiTinh = HocSinh.GioiTinh;
                hsinh.DanToc = HocSinh.DanToc;
                hsinh.CCCD = HocSinh.CCCD;
                hsinh.TrangThaiHocTap = HocSinh.TrangThaiHocTap;
                hsinh.TinhThanhPho = HocSinh.TinhThanh;
                hsinh.QuanHuyen = HocSinh.QuanHuyen;
                hsinh.DiaChiNha = HocSinh.DiaChi;

                db.SaveChanges();
                TempData["reponse"] = "success";



            }
            catch (Exception e)
            {

                TempData["reponse"] = "error";
                TempData["message"] = e.Message;
                throw;
            }

            return RedirectToAction("Edit", new { id = HocSinh.MaHocSinh });

        }

        public ActionResult Delete(int id)
        {
            var DelHs = db.HocSinh.Find(id);
            if (DelHs == null)
            {
                return HttpNotFound();
            }
            var diemhs = db.Diem.Where(d => d.HocSinhID == id).ToList();


            foreach (var d in diemhs)
            {
                db.Diem.Remove(d);
            }

            var DelND = DelHs.NguoiDung;
            db.HocSinh.Remove(DelHs);
            if (DelND != null)
            {
                db.NguoiDung.Remove(DelND);

            }

            db.SaveChanges();

            return RedirectToAction("DsHocSinh");
        }

        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                return View();
            }

            ExcelPackage.License.SetNonCommercialPersonal("KhanhTai");

            var listHocSinh = new List<HocSinh>();
            var errors = new List<string>();


            using (var package = new ExcelPackage(file.InputStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var hoTen = worksheet.Cells[row, 1].Text;
                        if (string.IsNullOrEmpty(hoTen))
                        {
                            continue;
                        }
                        var lophocIdText = worksheet.Cells[row, 2].Text;
                        int lophocId = int.TryParse(lophocIdText, out var lop) ? lop : 0;
                        if (!db.LopHoc.Any(x => x.LopHocID == lophocId))
                        {
                            errors.Add($"Dòng {row}: Lớp không tồn tại");
                            continue;
                        }
                        var ngaySinhText = worksheet.Cells[row, 3].Text;
                        DateTime? ngaysinh = null;

                        if (DateTime.TryParse(ngaySinhText, out var ns))
                        {
                            ngaysinh = ns;
                        }

                        var gioiTinh = worksheet.Cells[row, 4].Text;
                        if (string.IsNullOrWhiteSpace(gioiTinh))
                        {
                            errors.Add($"Dòng {row}: giới tính đang bị bỏ trống, Nếu bạn muốn thêm vào thì hãy sử dụng chức năng cập nhật ");
                        }
                        else if (gioiTinh != "Nam" && gioiTinh != "Nữ" && gioiTinh != "Khác")
                        {
                            errors.Add($"Dòng {row}: Giới tính bạn nhập không bao gồm trong định dạng của chúng tôi");
                            continue;
                        }

                        var danToc = worksheet.Cells[row, 5].Text;

                        var SoCCCD = worksheet.Cells[row, 6].Text;
                        if (string.IsNullOrEmpty(SoCCCD) || db.HocSinh.Any(x => x.CCCD == SoCCCD))
                        {
                            errors.Add($"Dòng {row}: Bạn đã bỏ trống hoặc nhập trùng số CCCD với một học sinh khác");
                            continue;
                        }

                        var tinhThanh = worksheet.Cells[row, 7].Text;

                        var quanHuyen = worksheet.Cells[row, 8].Text;

                        var diaChi = worksheet.Cells[row, 9].Text;

                        var User = worksheet.Cells[row, 10].Text;
                        if (string.IsNullOrEmpty(User))
                        {
                            errors.Add($"Dòng {row}: Tên đăng nhập không được để trống");
                            continue;
                        }
                        else if (db.NguoiDung.Any(x => x.TenDangNhap == User))
                        {
                            errors.Add($"Dòng {row}: Tên đăng nhập đã được tồn tại");
                            continue;
                        }
                        var Password = worksheet.Cells[row, 11].Text;
                      
                        if (string.IsNullOrEmpty(Password))
                        {
                            errors.Add($"Dòng {row}: Mật khẩu không được để trống");
                            continue;
                        }
                        string matKhauHash = PasswordHelper.HashPassword(Password);//ma hóa maatjj khẩu 
                        var Vaitro = "HocSinh";
                        var trangThai = "Đang học";

                        var nguoiDung = new NguoiDung
                        {
                            HoTen = hoTen,
                            TenDangNhap = User,
                            MatKhau = matKhauHash,
                            VaiTro = Vaitro,
                        };



                        var hocSinh = new HocSinh
                        {
                            LopHocID = lophocId,
                            NgaySinh = ngaysinh,
                            GioiTinh = gioiTinh,
                            TinhThanhPho = tinhThanh,
                            QuanHuyen = quanHuyen,
                            DanToc = danToc,
                            CCCD = SoCCCD,
                            DiaChiNha = diaChi,
                            TrangThaiHocTap = trangThai,
                            NguoiDung = nguoiDung,
                        };

                        listHocSinh.Add(hocSinh);
                    }
                    catch
                    {
                        errors.Add($"Dòng {row} lỗi dữ liệu");
                    }





                }
            }
            TempData["Errors"] = errors;
            TempData["Success"] = $"Đã thêm {listHocSinh.Count} học sinh";
            foreach (var hs in listHocSinh)
            {
                db.HocSinh.Add(hs);
            }
            db.SaveChanges();

            return RedirectToAction("DsHocSinh");
        }

    }
}
