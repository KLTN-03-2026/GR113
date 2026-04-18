using demomvc.App_Start;
using demomvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace demomvc.Controllers
{
    [RoleAuthorize(RolesRequired = "HieuTruong,GiaoVien,HieuPho,BiThu")]
    public class QuanLyDiemController : Controller
    {
        QuanLyTruongHocEntities1 db = new QuanLyTruongHocEntities1();
        // GET: QuanLyDiem
        public ActionResult Index()
        {
           
            return View();
        }
        
    }
}