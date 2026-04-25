using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace demomvc.Services.GA
{
    public class Gene
    {
        public int LopHocID { get; set; }
        public int KhoiLopID { get; set; }
        public int MonHocID { get; set; }
        public int GiaoVienID { get; set; }
        public int? PhongHocID { get; set; }
        public int HocKyID { get; set; }
        public int Tuan { get; set; }
        public int Thu { get; set; }
        public int Tiet { get; set; }

        public string CaHoc { get; set; }
    }
}