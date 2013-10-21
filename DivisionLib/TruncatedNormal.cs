using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meta.Numerics.Statistics;
using Meta.Numerics.Statistics.Distributions;

namespace DivisionLib
{
    class TruncatedNormal
    {
        /// <summary>
        /// Generate standard truncated normal distributions, using accept and rejected method
        /// so, using the inefficient algorith, simply draw samples from Normal distribution and
        /// then accept it if it is "[leftBound, rightbound]"
        /// </summary>
        public TruncatedNormal(double leftBound, double rightBound)
        {
            mu = 0;
            sigma = 1;
            a = leftBound;
            b = rightBound;
            nd = new NormalDistribution();
            
        }
        /// <summary>
        ///Generate truncated normal distributions, using accept and rejected method
        /// so, using the inefficient algorith, simply draw samples from Normal distribution and
        /// then accept it if it is "[leftBound, rightbound]"
        /// </summary>
        /// <param name="leftBound">lower limit of the range</param>
        /// <param name="rightBound">upper limit of the range</param>
        public TruncatedNormal(double leftBound, double rightBound, double _mu, double _sigma)
        {
            mu = _mu;
            sigma = _sigma;
            a = leftBound;
            b = rightBound;
            a = (a - mu) / sigma;
            b = (b - mu) / sigma;
            nd = new NormalDistribution();
        }

        public double GetRandomValue(Random rng)
        {
            double temp;
            do
            {
                temp = nd.GetRandomValue(rng);

            } while (temp < a || temp > b);
            return temp*sigma+mu;
        }

        private double a;
        private double b;
        private double mu;
        private double sigma;
        //private static Random rng = new Random();
        private NormalDistribution nd;
    }
}
