using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.EnterpriseServices.Internal;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace demomvc.Models
{
    public class TimLopViewModel
    {
        public DateTime? NgayBatDauHoc { get; set; }
        public int? HocKyID { get; set; }

        public int? NamHocID { get; set; }
        public List<NgayHoc> DsNgayNghi { get; set; }
        public List<HocBu> DsHocBu { get; set; }
        public List<NgayHoc> DsNgayHoc { get; set; }
        public List<SelectListItem> ListHocKy { get; set; }
        public List<SelectListItem> ListNamHoc { get; set; }

        public int? SelectedLopHocID { get; set; }
        public int? SelectedGiaoVienID { get; set; }

        public List<SelectListItem> ListLopHoc { get; set; }
        public List<SelectListItem> ListGiaoVien { get; set; }


        public List<LopHocViewModel> ListLop { get; set; }
        public List<ThoiKhoaBieu> ThoiKhoaBieus { get; set; }
        public int ThoiKhoaBieuID { get; set; }
        public string CaHoc { get; set; }
        public int Tuan { get; set; }
    }
    public class LopHocViewModel
    {

        public int LopHocID { get; set; }
        public string TenLop { get; set; }
        public int SiSo { get; set; }
        public string GiaoVienChuNhiem { get; set; }
        public string TenKhoi { get; set; }
        public string CaHoc { get; set; }
        public string TrangThaiNamHoc { get; set; }

    }
}