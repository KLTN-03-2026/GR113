using demomvc.Services.GA;
using System.Collections.Generic;
using System.Linq;

namespace demonvc.Services.GA
{
    public class Chromosome
    {
        public List<Gene> Genes { get; set; } = new List<Gene>();

        public double Fitness { get; set; }

        public Chromosome Clone()
        {
            return new Chromosome
            {
                Genes = Genes.Select(g => new Gene
                {
                    LopHocID = g.LopHocID,
                    KhoiLopID = g.KhoiLopID,
                    MonHocID = g.MonHocID,
                    GiaoVienID = g.GiaoVienID,
                    PhongHocID = g.PhongHocID,
                    HocKyID = g.HocKyID,
                    Tuan = g.Tuan,
                    Thu = g.Thu,
                    Tiet = g.Tiet
                }).ToList()
            };
        }
    }
}