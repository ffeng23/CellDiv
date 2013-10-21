using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meta.Numerics.Statistics.Distributions;

namespace DivisionLib
{
    class ODEModelRandomGenerator
    {

        public static UniformDistribution  deathGenerateor = new UniformDistribution();
        public static Random deathRng = new Random();

        public static UniformDistribution divGenerateor = new UniformDistribution();
        public static Random divRng = new Random();

    }
}
