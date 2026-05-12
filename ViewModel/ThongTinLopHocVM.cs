using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace demomvc.ViewModel
{
    public class ThongTinLopHocVM
    {
        public int MaLop { get; set; }
        public string TenLop { get; set; }
        public string GVCN { get; set; }
        public string TenKhoi { get; set; }
        public string NamHoc { get; set; }

        public string TenNamHoc { get; set; }
        public List<DSBangDiemVM> BangDiem { get; set; }

    }

    
}