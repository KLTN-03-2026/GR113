using demomvc.Helpers;
using demomvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace demomvc.ViewModel
{
    public class DiemTongVM
    {
        public int HocSinhID { get; set; }
        public string TenHocSinh { get; set; }

        public int HocKyID { get; set; }

        public int NamHocID { get; set; }
        public string TenMonHoc { get; set; }
        public List<Diem> DSdiemTB { get; set; }




        public double? DTBTong()
        {
            double sum = 0;
            int heSo = 0;
            string tenMon;

            foreach (var item in DSdiemTB)
            {
                tenMon = RemoveDiacritics.RemoveDiacritic(
                    item.MonHoc.TenMonHoc
                ).ToLower();
  
                if (tenMon == "amnhac" ||
                    tenMon == "theduc" ||
                    tenMon == "mithuat")
                {
                    continue;
                }

                if (item.DiemTB == null)
                {
                    return null;
                }

                if (tenMon == "toan" ||
                    tenMon == "nguvan")
                {
                    sum += item.DiemTB.Value * 2;
                    heSo += 2;
                }
                else
                {
                    sum += item.DiemTB.Value;
                    heSo += 1;
                }
            }

            if (heSo == 0)
            {
                return null;
            }

            return Math.Round(sum / heSo, 1);
        }

        public string HocLuc()
        {
            double? TongDiem = DTBTong();
            if (TongDiem == null)
            {
                return " ";
            }
            if (TongDiem >= 8)
            {
                return "Giỏi";
            }
            else if (TongDiem >= 6.5)
            {
                return "Khá";
            }
            else if (TongDiem >= 5)
            {
                return "Trung Bình";
            }
            else
            {
                return "Yếu";
            }
        }

       

       
    }
}