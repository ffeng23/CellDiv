using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meta.Numerics.Statistics;
using Meta.Numerics.Statistics.Distributions;

namespace DivisionLib
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

        public static Random progDestRng = new Random();
        public static DiscreteTruncatedNormal progDestNormGenerator = new DiscreteTruncatedNormal (0,10);
        public static bool progDestParameterSetFlag=false; //this is the flag to indicate whether we need to set the parameters for progeneDestiny, false need to set it, 
        
        public static Random progDestVarRng = new Random();
        public static NormalDistribution progDestVarNormGenerator = new NormalDistribution();
    }
}
