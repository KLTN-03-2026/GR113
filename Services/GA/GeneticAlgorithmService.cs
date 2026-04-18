using demonvc.Services.GA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace demomvc.Services.GA
{
    public class GeneticAlgorithmService
    {
        private readonly Random _rnd = new Random();
        private readonly FitnessCalculator _fitness = new FitnessCalculator();

        public Chromosome Generate(
            List<Gene> baseGenes,
            int soTuanHoc,
            Action<string> report,
            int populationSize = 30,
            int generations = 80)
        {
            List<Chromosome> population = InitPopulation(baseGenes, populationSize, soTuanHoc);

            for (int gen = 0; gen < generations; gen++)
            {
                foreach (var c in population)
                    c.Fitness = _fitness.Calculate(c);

                population = population
                    .OrderByDescending(x => x.Fitness)
                    .Take(populationSize / 2)
                    .ToList();

                while (population.Count < populationSize)
                {
                    var p1 = population[_rnd.Next(population.Count)];
                    var p2 = population[_rnd.Next(population.Count)];

                    var child = Crossover(p1, p2);
                    Mutate(child);

                    population.Add(child);
                }
                //bao cao tien trinh
                report?.Invoke($"đang tối ưu thế hệ{gen}/{generations}...");
            }
            report?.Invoke("Hoàn tất xếp thời khóa biểu");
            return population.OrderByDescending(x => x.Fitness).First();
        }

        private List<Chromosome> InitPopulation(List<Gene> genes, int size, int soTuanHoc)
        {
            var list = new List<Chromosome>();

            for (int i = 0; i < size; i++)
            {
                var c = new Chromosome
                {
                    Genes = genes.Select(g =>
                    {
                        int thu, tiet;
                        do
                        {
                            thu = _rnd.Next(2, 8);
                            tiet = _rnd.Next(1, 11);
                        }
                        while (FixedSlotHelper.IsFixedSlot(thu, tiet, g.CaHoc));

                        return new Gene
                        {
                            LopHocID = g.LopHocID,
                            KhoiLopID = g.KhoiLopID,
                            MonHocID = g.MonHocID,
                            GiaoVienID = g.GiaoVienID,
                            PhongHocID = g.PhongHocID,
                            HocKyID = g.HocKyID,
                            CaHoc = g.CaHoc,
                            Tuan = _rnd.Next(1, soTuanHoc + 1),
                            Thu = thu,
                            Tiet = tiet
                        };
                    }).ToList()
                };

                list.Add(c);
            }

            return list;
        }

        private Chromosome Crossover(Chromosome a, Chromosome b)
        {
            int cut = _rnd.Next(a.Genes.Count);

            var genes = a.Genes.Take(cut)
                .Concat(b.Genes.Skip(cut))
                .Select(g => new Gene
                {
                    LopHocID = g.LopHocID,
                    KhoiLopID = g.KhoiLopID,
                    MonHocID = g.MonHocID,
                    GiaoVienID = g.GiaoVienID,
                    PhongHocID = g.PhongHocID,
                    HocKyID = g.HocKyID,
                    CaHoc = g.CaHoc,
                    Tuan = g.Tuan,
                    Thu = g.Thu,
                    Tiet = g.Tiet
                }).ToList();

            return new Chromosome { Genes = genes };
        }

        private void Mutate(Chromosome c)
        {
            if (_rnd.NextDouble() < 0.1)
            {
                var g = c.Genes[_rnd.Next(c.Genes.Count)];
                int thu, tiet;

                do
                {
                    thu = _rnd.Next(2, 8);
                    tiet = _rnd.Next(1, 11);
                }
                while (FixedSlotHelper.IsFixedSlot(thu, tiet, g.CaHoc));

                g.Thu = thu;
                g.Tiet = tiet;
            }
        }
    }
}