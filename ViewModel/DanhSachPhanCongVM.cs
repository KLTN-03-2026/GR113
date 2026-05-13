using demomvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace demomvc.ViewModel
{
    public class DanhSachPhanCongVM
    {
        public int UserId { get; set; }

        public List<ThongTinLopHocVM> DanhSachLop { get; set; }
        public int GiaoVienID { get; set; }
        public string TenGiaoVien { get; set; }
        public int MonHocID { get; set; }
        public string TenMonHoc { get; set; }
        public int? HocKyID { get; set; }
        public int? NamHocID { get; set; }
        public List<HocKy> DsHocKi { get; set; }
        public List<NamHoc> DsNamHoc { get; set; }

    }
}