using OfficeOpenXml.Drawing.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace demomvc.ViewModel
{
    public class DSBangDiemVM
    {
        public int DiemID { get; set; }
        public int HocSinhID { get; set; }
        public int HocKyID { get; set; }
        public int GiaoVienID { get; set; }
        public int NamHocID { get; set; }
        public int LopHocID { get; set; }
        public string TenHocKy { get; set; }
        public string TenHocSinh { get; set; }
        public int MonHocID { get; set; }
        public string TenMonHoc { get; set; }
        [Range(0, 10, ErrorMessage ="Điểm chỉ được nhập từ 0 - 10")]
        public double? Diem15p { get; set; }
        [Range(0, 10, ErrorMessage = "Điểm chỉ được nhập từ 0 - 10")]
        public double? DiemMieng { get;set; }
        [Range(0, 10, ErrorMessage = "Điểm chỉ được nhập từ 0 - 10")]
        public double? DiemGK { get; set; }
        [Range(0, 10, ErrorMessage = "Điểm chỉ được nhập từ 0 - 10")]
        public double? DiemCK { get; set; }
        public double? DiemTB { get
            {
                
                if (Diem15p == null || DiemMieng == null || DiemGK == null || DiemCK == null)
                {
                    return null;
                }
                else 
                {
                     double dtb = (Diem15p.Value + DiemMieng.Value + DiemGK.Value * 2  + DiemCK.Value * 3) / 7;
                    return Math.Round(dtb,2);
                }
            }  }

        
    }

    
    

    
}