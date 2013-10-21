using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meta.Numerics.Statistics;
using Meta.Numerics.Statistics.Distributions;

namespace DivisionLib
{
    class DiscreteNormal
    {
        
        public DiscreteNormal(double mu, double sigma)
        {
            this.mu = mu;
            this.sigma = sigma;
            nd = new NormalDistribution(mu, sigma);
        }

        public int GetRandomValue(Random rng)
        {
            double temp;
            temp = nd.GetRandomValue(rng);
            return Convert.ToInt32(Math.Round(temp));
        }

        private double mu;
        private double sigma;
        private NormalDistribution nd;
    }

    class DiscreteTruncatedNormal
    {
       
        public DiscreteTruncatedNormal(double a, double b, double mu=0, double sigma=1)
        {
            this.a = a;
            this.b = b;
            this.mu = mu;
            this.sigma = sigma;
            nd = new TruncatedNormal(a, b, mu, sigma);
        }

        public int GetRandomValue(Random rng)
        {
            double temp;
            temp = nd.GetRandomValue(rng);
            return Convert.ToInt32(Math.Round(temp,0));
        }

        public void SetValues(double _mu, double _sigma)
        {
            this.mu = _mu;
            this.sigma=_sigma;
            nd=new TruncatedNormal (a,b,mu, sigma);
        }

        private double a; private double b;
        private double mu;
        private double sigma;
        private TruncatedNormal nd;
    }
}
