using demomvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace demomvc.ViewModel
{
    public class ThongTinHocSinhVM
    {
        public int MaHocSinh { get; set; }
        public  string HoTen { get; set; }
        public int KhoiLopID { get; set; }
        public string TenKhoi { get; set; }
        public List<KhoiLop> DanhSachKhoi { get; set; }
        public int LopHocID { get; set; }
        public List<LopHoc> DanhsachLop { get; set; }
      public string TenLop { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string TinhThanh { get; set; }
        public string QuanHuyen { get; set; }
        public string DiaChi { get; set; }
        public string CCCD { get; set; }
        public string DanToc { get; set; }
        public string TrangThaiHocTap { get; set; }

        public int? GiaoVienChuNhiem { get; set; }

        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }

    }
}