using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace demomvc.Models
{
    public class DiemMonViewModel
    {
        public string TenMon { get; set; }
        public double? DiemMieng { get; set; }
        public double? Diem15p { get; set; }
        public double? DiemGK { get; set; }
        public double? DiemCK { get; set; }
        public double? DiemTBMon { get; set; }
        public bool LaMonDanhGia { get; set; }  // Âm nhạc / Thể dục
        public string DiemDanhGia { get; set; } // Đ hoặc S
    }

    public class BangDiemViewModel
    {
        public int NamHocID { get; set; }
        public int HocKyID { get; set; }

        public List<DiemMonViewModel> DiemMonList { get; set; }

        public string HanhKiem { get; set; }
        public double? DiemTBHocKy { get; set; }
        public string XepLoai { get; set; }

        public List<NamHoc> NamHocs { get; set; }
        public List<HocKy> HocKys { get; set; }

        public double? DiemTBHocCaNam { get; set; }
        public string XepLoaiCaNam { get; set; }
        public string HanhKiemCaNam { get; set; }

    }

}