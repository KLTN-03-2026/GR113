using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using demomvc.Helpers;
using demomvc.Models;

namespace demomvc
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            Task.Run(() =>
            {
                using (var db = new QuanLyTruongHocEntities2())
                {
                    var ds = db.Diem
                        .Where(d => d.DiemTB != null)
                        .Select(d => new
                        {
                            d.HocSinhID,
                            d.HocKyID,
                            d.NamHocID
                        })
                        .Distinct()
                        .ToList();

                    var service = new KetQuaService();

                    foreach (var item in ds)
                    {
                        service.UpdateKQ(
                            item.HocSinhID,
                            item.HocKyID,
                            item.NamHocID
                        );
                    }


                    var dsNam = db.KetQuaHocTap
                    .Where(k => k.DTBTong != null && k.HocKyID == 2)
                    .Select(k => new
                    {
                        k.HocSinhID,
                        k.NamHocID
                    })
                    .Distinct()
                    .ToList();

                    foreach (var item in dsNam)
                    {
                        service.UpdateDiemCaNam(
                            item.HocSinhID,
                            item.NamHocID
                        );
                    }
                }
            });
        }
    }
}
