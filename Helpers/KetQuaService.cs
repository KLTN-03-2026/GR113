using demomvc.Models;
using demomvc.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace demomvc.Helpers
{
    public class KetQuaService

    {
        private readonly QuanLyTruongHocEntities2 db;

        public KetQuaService()
        {
            db = new QuanLyTruongHocEntities2();
        }
        public void UpdateKQ(int HocSinhID, int HocKyID, int NamHocID)
        {
            var diemhs = db.Diem.Where(d => d.HocSinhID == HocSinhID && d.HocKyID == HocKyID && d.DiemTB != null).ToList();


            if (!diemhs.Any())
            {
                return;
            }
            var hocSinh = db.HocSinh.FirstOrDefault(h => h.HocSinhID == HocSinhID);

            int khoi = hocSinh.LopHoc.KhoiLopID;
            var tongmon = db.MonHoc.Count();
            if (khoi == 1 || khoi == 2)
            {
                tongmon = tongmon - 1;
            }
           
            var diemTong = new DiemTongVM()
            {
                HocSinhID = HocSinhID,
                HocKyID = HocKyID,
                NamHocID = NamHocID,
                DSdiemTB = diemhs,
            };

            var monDaCoDiem = db.Diem.Where(d => d.HocSinhID == HocSinhID
            && d.HocKyID == HocKyID && d.DiemTB != null).Select(d => d.MonHocID).Distinct().Count();
            if (tongmon > monDaCoDiem)
            {
                var kqCu = db.KetQuaHocTap
        .FirstOrDefault(k => k.HocSinhID == HocSinhID
                          && k.HocKyID == HocKyID);

                if (kqCu != null)
                {
                    kqCu.DTBTong = null;
                    kqCu.HocLuc = null;

                    db.SaveChanges();
                }

                return;
            }
            var KQua = db.KetQuaHocTap.FirstOrDefault(kq => kq.HocSinhID == HocSinhID && kq.HocKyID == HocKyID);
            if (KQua != null)
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

        public void UpdateDiemCaNam(int HocSinhID,int NamHocID)
        {
            var hk1 = db.KetQuaHocTap.FirstOrDefault(k => k.HocSinhID == HocSinhID && k.NamHocID == NamHocID && k.HocKyID == 1);
            var hk2 = db.KetQuaHocTap.FirstOrDefault(k => k.HocSinhID == HocSinhID && k.NamHocID == NamHocID && k.HocKyID == 2);

            double? diemNam = null;
            if (hk1 == null || hk2 == null)
            {
                return;
            }
            if (hk1.DTBTong == null || hk2.DTBTong == null)
            {
                return;
            }

            diemNam = Math.Round(((double)hk1.DTBTong + (double)hk2.DTBTong * 2) / 3, 2);

            hk2.DTBCaNam = diemNam;
            db.SaveChanges();
        }
    }
}