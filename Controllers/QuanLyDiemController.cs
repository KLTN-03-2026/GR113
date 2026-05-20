using demomvc.App_Start;
using demomvc.Models;
using demomvc.ViewModel;
using DocumentFormat.OpenXml.Drawing.Charts;
using iText.StyledXmlParser.Node;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions;
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


        public ActionResult DsLop(int? NamHocID, int? HocKyID)
        {
            int UserId = (int)Session["UserID"];
            var nguoiDung = db.NguoiDung.FirstOrDefault(nd => nd.NguoiDungID == UserId);
            var giaoVien = db.GiaoVien.FirstOrDefault(gv => gv.NguoiDungID == UserId);
            var monHoc = db.MonHoc.FirstOrDefault(mh => mh.MonHocID == giaoVien.MonHocID);
            if (!NamHocID.HasValue || !HocKyID.HasValue)
            {
                return View(new DanhSachPhanCongVM()
                {
                    TenMonHoc = monHoc.TenMonHoc,
                    TenGiaoVien = nguoiDung.HoTen,
                    DsNamHoc = db.NamHoc.ToList(),
                    DsHocKi = db.HocKy.ToList()
                });
            }



            var result = (from pc in db.PhanCongGiangDay
                          join l in db.LopHoc on pc.LopHocID equals l.LopHocID
                          join hk in db.HocKy on pc.HocKyID equals hk.HocKyID
                          where pc.GiaoVienID == giaoVien.GiaoVienID
                                && pc.HocKyID == HocKyID
                                && hk.NamHocID == NamHocID
                          select new ThongTinLopHocVM
                          {
                              MaLop = l.LopHocID,
                              TenKhoi = l.KhoiLop.TenKhoi,
                              TenLop = l.TenLop,
                              NamHoc = l.NamHoc.TenNamHoc,
                              GVCN = l.GiaoVien.NguoiDung.HoTen,
                          }).ToList();

            var DsLopVM = new DanhSachPhanCongVM()
            {

                UserId = UserId,
                TenGiaoVien = nguoiDung.HoTen,
                DanhSachLop = result,
                TenMonHoc = monHoc.TenMonHoc,
                MonHocID = monHoc.MonHocID,
                GiaoVienID = giaoVien.GiaoVienID,

                DsNamHoc = db.NamHoc.ToList(),
                DsHocKi = db.HocKy.ToList(),
                NamHocID = NamHocID,
                HocKyID = HocKyID

            };

            return View(DsLopVM);
        }


        public ActionResult BangDiem(int id, int idGV, int idMH, int idHocKi)
        {
            var lophoc = db.LopHoc.FirstOrDefault(l => l.LopHocID == id);
            var giaoVien = db.GiaoVien.FirstOrDefault(g => g.GiaoVienID == idGV);
            var monhoc = db.MonHoc.FirstOrDefault(mh => mh.MonHocID == idMH);
            ViewBag.GiaoVien = giaoVien.NguoiDung.HoTen;
            ViewBag.MonHoc = monhoc.TenMonHoc;
            ViewBag.GiaoVienID = giaoVien.GiaoVienID;
            ViewBag.LopHocID = id;
            ViewBag.TenLop = lophoc.TenLop;
            ViewBag.MonHocID = idMH;
            ViewBag.HocKiID = idHocKi;
            var bangdiem = (from hs in db.HocSinh
                            join l in db.LopHoc on hs.LopHocID equals l.LopHocID

                            // dung  lèt join lay du lieu tat ca hoc sinh ke ca chưa co diem 
                            join d in db.Diem
                            on new
                            {
                                hs.HocSinhID,
                                MonHocID = idMH,
                                HocKyID = idHocKi
                            }
                            equals new
                            {
                                d.HocSinhID,
                                d.MonHocID,
                                d.HocKyID
                            }
                            into diemGroup
                            from d in diemGroup.DefaultIfEmpty()   //khong co diem thi de trong 

                            where hs.LopHocID == id


                            select new DSBangDiemVM()
                            {
                                HocSinhID = hs.HocSinhID,
                                MonHocID = idMH,
                                HocKyID = idHocKi,
                                NamHocID = hs.LopHoc.NamHoc.NamHocID,
                                TenHocSinh = hs.NguoiDung.HoTen,
                                GiaoVienID = giaoVien.GiaoVienID,
                                DiemID = d != null ? d.DiemID : 0,
                                Diem15p = d != null ? d.Diem15p : null,
                                DiemMieng = d != null ? d.DiemMieng : null,
                                DiemGK = d != null ? d.DiemGK : null,
                                DiemCK = d != null ? d.DiemCK : null,

                            }).ToList();

            return View(bangdiem);
        }

        [HttpGet]

        public ActionResult NhapDiem(int? id, int? hs, int? mh, int? hk, int? nh, int? gv)
        {
            var hocSinh = db.HocSinh.FirstOrDefault(h => h.HocSinhID == hs);
            var dbDiem = db.Diem.FirstOrDefault(c => c.DiemID == id);
            var NamHoc = db.HocKy.FirstOrDefault(h => h.NamHocID == nh);
            if (id.HasValue && id.Value > 0)
            {

                if (dbDiem == null)
                {
                    return HttpNotFound();
                }
                var diem = new DSBangDiemVM()
                {
                    LopHocID = hocSinh.LopHocID,
                    MonHocID = mh ?? 0,
                    DiemID = dbDiem.DiemID,
                    HocKyID = hk ?? 0,
                    GiaoVienID = gv ?? 0,
                    NamHocID = NamHoc.NamHocID,
                    HocSinhID = dbDiem.HocSinhID,
                    TenHocSinh = dbDiem.HocSinh.NguoiDung.HoTen,
                    DiemMieng = dbDiem.DiemMieng,
                    Diem15p = dbDiem.Diem15p,
                    DiemGK = dbDiem.DiemGK,
                    DiemCK = dbDiem.DiemCK,

                };
                return View(diem);
            }
            else
            {
                var hsinh = db.HocSinh.FirstOrDefault(h => h.HocSinhID == hs);
                var vm = new DSBangDiemVM()
                {
                    LopHocID = hsinh.LopHocID,
                    HocSinhID = hsinh.HocSinhID,
                    TenHocSinh = hsinh.NguoiDung.HoTen,
                    MonHocID = mh ?? 0,
                    HocKyID = hk ?? 0,
                    NamHocID = NamHoc.NamHocID,
                    GiaoVienID = gv ?? 0,


                };
                return View(vm);
            }

        }

        [HttpPost]
        public ActionResult NhapDiem(DSBangDiemVM diem)
        {
            string[] fields = { "DiemMieng", "Diem15p", "DiemGK", "DiemCK" };
            foreach (var field in fields)
            {
                if (ModelState[field]?.Errors.Count > 0)
                {
                    var value = Request[field];
                    ModelState[field].Errors.Clear();
                    if (!string.IsNullOrEmpty(value) && !double.TryParse(value, out _))
                    {
                        ModelState.AddModelError(field, "Định dạng bạn đang nhập không đúng");
                    }
                    else
                    {
                        ModelState.AddModelError(field, "Điểm chỉ được nhập từ 0 -> 10");
                    }

                }
            }

            if (!ModelState.IsValid)
            {
                return View(diem); 
            }

            var diems = db.Diem.FirstOrDefault(d => d.DiemID == diem.DiemID);
            if (diems != null)
            {

                diems.DiemMieng = diem.DiemMieng;
                diems.Diem15p = diem.Diem15p;
                diems.DiemGK = diem.DiemGK;
                diems.DiemCK = diem.DiemCK;
                diems.DiemTB = diem.DiemTB;

                db.SaveChanges();
                UpdateKQ(diem.HocSinhID, diem.HocKyID, diem.NamHocID); 
                return RedirectToAction("BangDiem", new { id = diem.LopHocID, idGV = diem.GiaoVienID, idMH = diem.MonHocID, idHocKi = diem.HocKyID });
            }
            else
            {

                var newDiem = new Diem()
                {
                    HocSinhID = diem.HocSinhID,
                    HocKyID = diem.HocKyID,
                    MonHocID = diem.MonHocID,
                    NamHocID = diem.NamHocID,
                    DiemMieng = diem.DiemMieng,
                    Diem15p = diem.Diem15p,
                    DiemGK = diem.DiemGK,
                    DiemCK = diem.DiemCK,
                    DiemTB = diem.DiemTB,


                };
                db.Diem.Add(newDiem);
                db.SaveChanges();

                UpdateKQ(diem.HocSinhID, diem.HocKyID, diem.NamHocID);

                
                return RedirectToAction("BangDiem", new { id = diem.LopHocID, idGV = diem.GiaoVienID, idMH = diem.MonHocID, idHocKi = diem.HocKyID });
            }
        }

        public ActionResult DownLoadBangDiem(int LopHocID, int GiaoVienID, int HocKiID)
        {

            ExcelPackage.License.SetNonCommercialPersonal("KhanhTai");

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("NhapDiem");

                ws.Cells[1, 1].Value = " ID học sinh";
                ws.Cells[1, 2].Value = " Tên Học Sinh";
                ws.Cells[1, 3].Value = " Điểm Miệng";
                ws.Cells[1, 4].Value = " Điểm 15 phút";
                ws.Cells[1, 5].Value = " Điểm giữa kì";
                ws.Cells[1, 6].Value = " Điểm cuối kì";

                var listHs = db.HocSinh.Where(h => h.LopHocID == LopHocID).ToList();
                var giaovien = db.GiaoVien.FirstOrDefault(gv => gv.GiaoVienID == GiaoVienID);
                var monHoc = db.MonHoc.FirstOrDefault(m => m.MonHocID == giaovien.MonHocID);
                int row = 2;

                var listDiem = db.Diem.Where(x => x.MonHocID ==monHoc.MonHocID && x.HocKyID == HocKiID).ToList();

                foreach (var hs in listHs)
                {
                    ws.Cells[row, 1].Value = hs.HocSinhID;
                    ws.Cells[row, 2].Value = hs.NguoiDung.HoTen;
                    

                    var diem = listDiem.FirstOrDefault(x => x.HocSinhID == hs.HocSinhID);

                    if (diem != null)
                    {
                        ws.Cells[row, 3].Value = diem.DiemMieng;
                        ws.Cells[row, 4].Value = diem.Diem15p;
                        ws.Cells[row, 5].Value = diem.DiemGK;
                        ws.Cells[row, 6].Value = diem.DiemCK;
                    }
                    row++;
                }
                ws.Cells.AutoFitColumns();
                var lophoc = db.LopHoc.FirstOrDefault(l => l.LopHocID == LopHocID);
                string tenFile = $"BangDiem_Lop{lophoc.TenLop}-HK{HocKiID}-{monHoc.TenMonHoc}.xlsx";
                return File(package.GetAsByteArray(),
       "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
       tenFile);
            }


        }

        public ActionResult ImportDiem(HttpPostedFileBase file, int LopHocID, int GiaoVienID, int HocKiID)
        {

            if (file == null || file.ContentLength == 0)
            {
                TempData["Error"] = "Vui lòng chọn một file excel";
                return RedirectToAction("BangDiem");
            }
            var giaovien = db.GiaoVien.FirstOrDefault(gv => gv.GiaoVienID == GiaoVienID);
            var monHoc = db.MonHoc.FirstOrDefault(m => m.MonHocID == giaovien.MonHocID);
            var hocky = db.HocKy.FirstOrDefault(hk => hk.HocKyID == HocKiID);
            ExcelPackage.License.SetNonCommercialPersonal("KhanhTai");


            int insertCount = 0;
            int updateCount = 0;

            var errors = new List<string>();
            var DsUpdate = new HashSet<int>();

            using(var package = new ExcelPackage(file.InputStream))
            {
                var ws = package.Workbook.Worksheets[0];
                if (ws == null)
                {
                    TempData["Error"] = "File Excel không có sheet nào";
                    return RedirectToAction("BangDiem", new { id = LopHocID, idGV = GiaoVienID, idMH = giaovien.MonHocID, idHocKi = HocKiID });
                }

                if (ws.Dimension == null)
                {
                    TempData["Error"] = "File Excel không có dữ liệu";
                    return RedirectToAction("BangDiem", new { id = LopHocID, idGV = GiaoVienID, idMH = giaovien.MonHocID, idHocKi = HocKiID });
                }
                int rowCount = ws.Dimension.Rows;

                for(int row=2; row<=rowCount; row++)
                {
                    try
                    {
                        if (!int.TryParse(ws.Cells[row,1].Text, out int hocSinhID))
                        {
                            errors.Add($"Dòng {row}: Học Sinh ID sai định dạng");
                            continue;
                        }
                        var hs = db.HocSinh.Find(hocSinhID);
                        if (hs == null)
                        {
                            errors.Add($"Dòng {row}: Học sinh không tồn tại");
                            continue;
                        }
                        if (hs.LopHocID != LopHocID)
                        {
                            TempData["Error"] = "Bảng điểm mà bạn import khong phải của lớp này";
                            return RedirectToAction("BangDiem", new { id = LopHocID, idGV = GiaoVienID, idMH = giaovien.MonHocID, idHocKi = HocKiID });
                        }
                       
                        double? diemMieng = ParseDiem(ws.Cells[row, 3].Text, row, "Miệng", errors);
                        double? diem15p = ParseDiem(ws.Cells[row, 4].Text, row, "15p", errors);
                        double? diemGK = ParseDiem(ws.Cells[row, 5].Text, row, "GK", errors);
                        double? diemCK = ParseDiem(ws.Cells[row, 6].Text, row, "CK", errors);

                        var dsDiem = new DSBangDiemVM()
                        {
                            DiemMieng = diemMieng,
                            Diem15p = diem15p,
                            DiemGK = diemGK,
                            DiemCK = diemCK,
                            
                        };
                        var existing = db.Diem.FirstOrDefault(x =>
                            x.HocSinhID == hocSinhID &&
                            x.MonHocID == monHoc.MonHocID &&
                            x.HocKyID == HocKiID);

                        if (existing != null)
                        {
                           
                            existing.Diem15p = dsDiem.Diem15p;
                            existing.DiemMieng =dsDiem.DiemMieng;
                            existing.DiemGK = dsDiem.DiemGK;
                            existing.DiemCK = dsDiem.DiemCK;
                            existing.DiemTB = dsDiem.DiemTB;
                            DsUpdate.Add(hocSinhID);

                            updateCount++;
                        }
                        else
                        {

                            db.Diem.Add(new Diem
                            {
                                HocSinhID = hocSinhID,
                                MonHocID = monHoc.MonHocID,
                                NamHocID = hocky.NamHocID,
                                HocKyID = HocKiID,
                                Diem15p = dsDiem.Diem15p,
                                DiemMieng = dsDiem.DiemMieng,
                                DiemGK = dsDiem.DiemGK,
                                DiemCK = dsDiem.DiemCK,
                                DiemTB = dsDiem.DiemTB,
                                
                            });
                            DsUpdate.Add(hocSinhID);

                            insertCount++;
                        }
                    }
                    catch(Exception ex)
                    {
                        errors.Add($"Dòng {row}: {ex.Message}");
                    }
                }

                db.SaveChanges();
                foreach(var hs in DsUpdate)
                {
                    UpdateKQ(hs, HocKiID, hocky.NamHocID);
                }
                

                TempData["Success"] = $"✔ Thêm mới: {insertCount}, ✔ Ghi đè: {updateCount}";
                TempData["Errors"] = errors;

                return RedirectToAction("BangDiem", new { id=LopHocID, idGV=GiaoVienID, idMH=giaovien.MonHocID, idHocKi=HocKiID });
             }

        }

        private double? ParseDiem(string input, int row, string tenCot, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }
            if (!double.TryParse(input, out double diem))
            {
                errors.Add($"Dòng {row}: Điểm {tenCot} sai định dạng");
                return null;
            }

            if (diem < 0 || diem > 10)
            {
                errors.Add($"Dòng {row}: Điểm {tenCot} phải từ 0 đến 10");
                return null;
            }

            return diem;
        }

        private void UpdateKQ(int HocSinhID, int HocKyID, int NamHocID)
        {
            var diemhs = db.Diem.Where(d => d.HocSinhID == HocSinhID && d.HocKyID == HocKyID && d.DiemTB != null).ToList();

            if (!diemhs.Any())
            {
                return;
            }
            var diemTong = new DiemTongVM()
            {
                HocSinhID = HocSinhID,
                HocKyID = HocKyID,
                NamHocID = NamHocID,
                DSdiemTB = diemhs,
            };
            var KQua = db.KetQuaHocTap.FirstOrDefault(kq => kq.HocSinhID == HocSinhID && kq.HocKyID == HocKyID);
            if(KQua!= null)
            {
                KQua.DTBTong = diemTong.DTBTong();
                KQua.HocLuc = diemTong.HocLuc();

            }
            else
            {
                var KetQuaHT = new KetQuaHocTap()
                {
                    HocSinhID = HocSinhID,
                    HocKyID = HocKyID,
                    NamHocID = NamHocID,
                    DTBTong = diemTong.DTBTong(),
                    HocLuc = diemTong.HocLuc(),
                    
                };
                db.KetQuaHocTap.Add(KetQuaHT);
            }
            db.SaveChanges();
        }

    }
   


    }

