using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace demomvc.Models
{
	public class XemTKB
	{
		public int NamHocID{ get; set; }
		public int HocKyID { get; set; }
		public int HocSinhID { get; set; }
		public int MonHocId { get; set; }
		public int GiaoVienId { get; set; }
		public int LopHocId { get; set; }
		public List<ThoiKhoaBieu> ThoiKhoaBieus  { get; set;}        
        public int KhoiLopID { get; set; }
        public int Tuan { get; set; }
        public int Thu { get; set; }
        public int TietHoc { get; set; }
		public string CaHoc { get; set; }

		public bool LaHocSinh { get; set; }


		public int TKBID { get; set; }
		public int PhongHocID { get; set; }

    }
}