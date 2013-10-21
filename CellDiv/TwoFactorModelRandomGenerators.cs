using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meta.Numerics.Statistics;

namespace DivisionModel
{
    class TwoFactorModelRandomGenerators
    {
        public static NormalDistribution rMNormGenerateor = new NormalDistribution();
        public static Random rMRng = new Random();

        public static Random rM_DRng = new Random();
        public static NormalDistribution rM_DNormGenerator = new NormalDistribution();

        public static Random rDRng = new Random();
        public static NormalDistribution rDNormGenerator = new NormalDistribution();

        public static Random tDthRng = new Random();
        public static NormalDistribution tDthNormGenerator = new NormalDistribution();

        public static Random tKdnRng = new Random();
        public static NormalDistribution tKdnNormGenerator = new NormalDistribution();

        public static UniformDistribution uniRandGenerator = new UniformDistribution();
        public static Random uniRng=new Random();


    }
}
