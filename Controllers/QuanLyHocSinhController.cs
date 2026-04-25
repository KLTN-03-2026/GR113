using demomvc.Models;
using demomvc.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace demomvc.Controllers
{
    public class QuanLyHocSinhController : Controller
    {
        QuanLyTruongHocEntities db = new QuanLyTruongHocEntities();
        // GET: QuanLyHocSinh
        [HttpGet]
        public ActionResult DsHocSinh(string TypeSearch,string Keyword)
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
            if (TypeSearch == null||Keyword==null)
            {
                return View(ListHS);
            }else 
            {
                if (TypeSearch=="Name")
                {
                    
                    var hs = ListHS.Where(q=>q.HoTen.ToLower().Trim().Contains(Keyword.ToLower().Trim())).ToList();
                    return View(hs);
                }else if (TypeSearch=="Id")
                {
                    int id = int.Parse(Keyword);
                    var hs = ListHS.Where(q => q.MaHocSinh==id).ToList();
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
                var nguoiDung = new NguoiDung()
                {
                    TenDangNhap = HocSinhVM.TenDangNhap,
                    MatKhau = HocSinhVM.MatKhau,
                    HoTen = HocSinhVM.HoTen,

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

            var DelND = DelHs.NguoiDung;
            db.HocSinh.Remove(DelHs);
            if (DelND != null)
            {
                db.NguoiDung.Remove(DelND);

            }

            db.SaveChanges();

            return RedirectToAction("DsHocSinh");
        }

    }
}
