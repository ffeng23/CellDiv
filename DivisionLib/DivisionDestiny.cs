using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meta.Numerics.Statistics.Distributions;
namespace DivisionLib
{
    class DivisionDestiny
    {
        /// <summary>
        /// constructor
        /// </summary>
        public DivisionDestiny( )
        {
            //default value;
            lamda = ODEModelParameters.lamda;
            q = ODEModelParameters.q;
            
            //for effective destiny
            lamdaEffective = ODEModelParameters.lamdaEffective;
            qEffective = ODEModelParameters.qEffective;

            pmf = new double[8];
            rvX = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            pmfEffectiveDiff = new double[rvXEffectiveDivisionDiff.Count()];
            //rng = _rng;
            generatePMF();
            generatePMFEffectiveDiff();
        }
        public DivisionDestiny(double _lamda, double _q)
        {
            //default value;
            lamda = ODEModelParameters.lamda;
            q = ODEModelParameters.q;

            //for effective destiny
            lamdaEffective = ODEModelParameters.lamdaEffective;
            qEffective = ODEModelParameters.qEffective;

            pmf = new double[8];

            pmfEffectiveDiff = new double[rvXEffectiveDivisionDiff.Count()];
            //rng = _rng;
            generatePMF();
            generatePMFEffectiveDiff();
        }


        public void setDiscreteNormalParameters(double _lamda, double _q)
        {
            lamda = _lamda;
            q = _q;
            generatePMF();
        }

        /// <summary>
        /// calculate the probability associated with input x
        /// </summary>
        /// <param name="_x">the input x for probability calculation, x could be any interger, for those are not in the rvX range, return 0</param>
        /// <returns></returns>
        public double getProbability(int _x)
        { 
            //double p;
            //go through the rvX defined range to see whether it is in there
            for (int i = 0; i < rvX.Count(); i++)
            {
                if (rvX[i] == _x)
                {
                    return pmf[i];
                }
            }
            //if we are here, mean this _x is not in the range
            return 0;
        }
        /// <summary>
        /// the function used to generate the random value folliwing the (discrete normal) distribution
        /// </summary>
        /// <param name="_rng">random number input, try to follow the meta numeric format</param>
        /// <returns></returns>
        public int getNextRandomValue(Random _rng)
        {
            //int retX;
            //need to generate one random number and check for the range and then return retX;
            double rd=_rng.NextDouble();
            double [] cdf = new double[pmf.Count()];
            double sum = 0;
            for (int i = 0; i < pmf.Count(); i++)
            {
                sum += pmf[i];
                cdf[i]=sum;
                /*if (i == 0 && rd <= runningCumulativeP)
                {
                    return rvX[i];
                }
                //if (i < pmf.Count() - 1)
                //{
                    if (rd > runningCumulativeP-pmf[i]  && rd <= runningCumulativeP)
                    {
                        return rvX[i];
                    }
                //}
                */
            }
            return rvX[ checkForDiscreteRVIndex(rd, cdf)];
            //return rvX[pmf.Count()-1];
        }
        /// <summary>
        /// this function is based on the getNextRandomValue(), but add another layer of variation to model
        /// the correlation among siblings, based on the founders destiny and correlation matrix
        /// </summary>
        /// <param name="_rng"></param>
        /// <param name="_divisionDestiny"></param>
        /// <returns></returns>
        public int getNextRandomValudEffective(Random _rng, int _divisionDestiny)
        {
            int effectiveDD = _divisionDestiny;

            //generate a random number
            double rd = _rng.NextDouble();
            //check for the 
            double[] cdf = new double[pmfEffectiveDiff.Count()];
            double sum = 0;
            for (int i = 0; i < pmfEffectiveDiff.Count(); i++)
            {
                sum += pmfEffectiveDiff[i];
                cdf[i] = sum;
            }
            //now let's get the numbers
            //only allow for 1 to 8.
            //Experimental for now............using discrete normal distribution 
            //the rvX - {-2,-1,0,1,2} is fixed
            //will need to use the MCMC to estimate a good parameters for this.
            
            //*************for testing......................
            effectiveDD += rvXEffectiveDivisionDiff[checkForDiscreteRVIndex(rd, cdf)];

            //effectiveDD += 0;
            return effectiveDD;
        }
        private int checkForDiscreteRVIndex(double p, double[] matrix)
        {
            //int divDif;
            for (int i = 0; i < matrix.Count(); i++)
            {
                if(i==0&&p<=matrix[i])
                 return i;
                if (i >0)
                {
                    if (p > matrix[i-1] && p <= matrix[i])
                    {
                        return i;
                    }
                }
            }
            return matrix.Count()-1;
        }

        public void setParametersEffectiveDiff(double _lamdaEffective, double _qEffective, int[] _rvXEffective)
        {
            lamdaEffective = _lamdaEffective;
            qEffective = _qEffective;
            rvXEffectiveDivisionDiff = _rvXEffective;
            generatePMFEffectiveDiff();
        }

        //***********private members******************
        private void generatePMF()
        {
            double denominator = 0;
            //first generate denominator
            for (int i = 0; i < rvX.Count(); i++)
            {
                denominator += Math.Pow(lamda, rvX[i]) * Math.Pow(q, rvX[i] * (rvX[i] - 1) / 2);
            }
            for (int j = 0; j < rvX.Count(); j++)
            {
                pmf[j] = Math.Pow(lamda, rvX[j]) * Math.Pow(q, rvX[j] * (rvX[j] - 1) / 2) / denominator;
            }
        }
        private void generatePMFEffectiveDiff()
        {
            double denominator = 0;
            //first generate denominator
            for (int i = 0; i < rvXEffectiveDivisionDiff.Count(); i++)
            {
                denominator += Math.Pow(lamdaEffective, rvXEffectiveDivisionDiff[i]) * Math.Pow(qEffective, rvXEffectiveDivisionDiff[i] * (rvXEffectiveDivisionDiff[i] - 1) / 2);
            }
            for (int j = 0; j < rvXEffectiveDivisionDiff.Count(); j++)
            {
                pmfEffectiveDiff[j] = Math.Pow(lamdaEffective, rvXEffectiveDivisionDiff[j]) * Math.Pow(qEffective, rvXEffectiveDivisionDiff[j] * (rvXEffectiveDivisionDiff[j] - 1) / 2) / denominator;
            }
        }



        public int[] RvX
        {
            get { return rvX; }
        }
        public double[] PMF
        {
            get { return pmf; }
        }
        public double Lamda
        {
            get { return lamda; }
        }
        public double Q
        {
            get { return Q; }
        }

        private double lamda;
        private double q;
        private double[] pmf;//fixed size for 8 intergers, 1~8;
        private int[] rvX;
        //private Random rng;

        //default lamda, q for 
        public const double lamda0=3.12;
        public const double q0=0.4;

        //default value for effective maxium generation
        ///this is the array containing the correlation for division destiny among siblings 
        ///it mean for siblings die in the same generation, p=0.451
        ///for siblings die in adjacent generation, p=0.902-0.451 
        ///for otherwise, p=1-aboveP
        //private double[] correlationDivisionDestinySiblings0={0.451, 0.902-0.451};
        private int[] rvXEffectiveDivisionDiff = { -2, -1, 0, 1, 2 };
        private double lamdaEffective;
        private double qEffective;
        private double[] pmfEffectiveDiff;

        public const double lamdaEffective0=1.2;
        public const double qEffective0=0.3;
        
    }
}
