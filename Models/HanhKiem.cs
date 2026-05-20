using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace demomvc.Models
{
	public class HanhKiem
	{

        public List<NamHoc> NamHocs { get; set; }
        public List<HocKy> HocKys { get; set; }

        public int? NamHocID { get; set; }
        public int? HocKyID { get; set; }

        public List<LopHoc> DanhSachLop { get; set; }

        public int? LopID { get; set; }
        public string TenLop { get; set; }
      
        public List<HocSinh> HocSinhs { get; set; }


        public int HocSinhID { get; set; }
        public string TenHocSinh { get; set; }
        public string HanhKiemf { get; set; }

    }


}