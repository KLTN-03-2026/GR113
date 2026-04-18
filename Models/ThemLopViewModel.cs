using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace demomvc.Models
{
	public class ThemLopViewModel
	{
        [Required(ErrorMessage = "Tên lớp không được để trống!")]
        public string TenLop { get; set; }

        
        public int? GiaoVienID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn năm học!")]
        public int NamHocID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khối lớp!")]
        public int KhoiLopID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ca học!")]
        public string CaHoc { get; set; }

        public List<SelectListItem> ListGiaoVien { get; set; }
        public List<SelectListItem> ListNamHoc { get; set; }
        public List<SelectListItem> ListKhoiLop { get; set; }
    }
}