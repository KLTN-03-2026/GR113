using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Web;

namespace demomvc.Models
{
	public class GiaoVienVM
	{

		public int GiaoVienID { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Giới tính không được để trống")]
        public string GioiTinh { get; set; }
		public string TrangThaiGiangDay { get; set; }
		public string TenMonHoc { get; set; }
		public string TenBoMon { get; set; }
		public string LopChuNhiem { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải là 10 số và bắt đầu bằng 0")]
        public string SDT { get; set; }
		public string TrangThaiTK { get; set; }

        [Required(ErrorMessage = "Vai trò không được để trống")]
        public string VaiTro { get; set; }


		//new
        public int? MonHocID { get; set; }
        public int? BoMonID { get; set; }  // từ bảng BoMon
        public int? LopChuNhiemID { get; set; }
    }
}