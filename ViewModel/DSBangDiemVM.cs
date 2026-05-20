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
        public string TenNamHoc { get; set; }
        public int LopHocID { get; set; }
        public string TenHocKy { get; set; }
        public string TenHocSinh { get; set; }
        public int MonHocID { get; set; }
        public string TenMonHoc { get; set; }
        private double? diem15p;
        private double? diemMieng;
        private double? diemGK;
        private double? diemCK;

        [Range(0, 10, ErrorMessage = "Điểm chỉ được nhập từ 0 - 10")]
        public double? Diem15p
        {
            get
            {
                return diem15p.HasValue ? (double?)Math.Round(diem15p.Value, 1) : null;
            }
            set
            {
                diem15p = value.HasValue ? (double?)Math.Round(value.Value, 1) : null;
            }
        }

        [Range(0, 10, ErrorMessage = "Điểm chỉ được nhập từ 0 - 10")]
        public double? DiemMieng
        {
            get
            {
                return diemMieng.HasValue ? (double?)Math.Round(diemMieng.Value, 1) : null;
            }
            set
            {
                diemMieng = value.HasValue ? (double?)Math.Round(value.Value, 1) : null;
            }
        }
        [Range(0, 10, ErrorMessage = "Điểm chỉ được nhập từ 0 - 10")]
        public double? DiemGK
        {
            get
            {
                return diemGK.HasValue ? (double?)Math.Round(diemGK.Value, 1) : null;
            }
            set
            {
                diemGK = value.HasValue ? (double?)Math.Round(value.Value, 1) : null;
            }
        }
        [Range(0, 10, ErrorMessage = "Điểm chỉ được nhập từ 0 - 10")]
        public double? DiemCK
        {
            get
            {
                return diemCK.HasValue ? (double?)Math.Round(diemCK.Value, 1) : null;
            }
            set
            {
                diemCK = value.HasValue ? (double?)Math.Round(value.Value, 1) : null;
            }
        }
        public double? DiemTB
        {
            get
            {

                if (Diem15p == null || DiemMieng == null || DiemGK == null || DiemCK == null)
                {
                    return null;
                }
                else
                {
                    double dtb = (Diem15p.Value + DiemMieng.Value + DiemGK.Value * 2 + DiemCK.Value * 3) / 7;
                    return Math.Round(dtb, 1);
                }
            }
        }


    }





}