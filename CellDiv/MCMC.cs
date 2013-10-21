using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meta.Numerics.Statistics.Distributions;
using DivisionLib;
using System.Windows.Controls;
using System.IO;
namespace CellDiv
{
    /// <summary>
    /// class of driver to run mcmc for ODE model parameter estimation
    /// </summary>
    public class MCMC
    {
        /// <summary>
        /// constructor
        /// </summary>
        public MCMC()
        {
            //initialize the arrays
            alphaArr = new List<double>();
            epsilonArr = new List<double>();
            betaArr = new List<double>();
            gammaArr = new List<double>();
            logLLArr = new List<double>();
            lamdaArr = new List<double>();
            qArr = new List<double>();
            lamdaEffectiveArr = new List<double>();
            qEffectiveArr = new List<double>();
            v0Arr = new List<double>();
            totalSigmaLiveArr=new List<double>();
            totalSigmaDeadArr=new List<double>();

            initialCellNumber = 80;
            simLengthTotal = 200.0;


            readTotalCellByTimeModels("totalLiveCellsByTime.csv", "totalDeadCellsByTime.csv");
            zRand = new NormalDistribution();
            rng = new Random();

            uRand = new UniformDistribution();
            rng2 = new Random();

            uRand2 = new UniformDistribution();
            rng3 = new Random();
           
        }

        public void run()
        {
            //first run simulation to get cycle data
            //set up the initial values
            alpha = alpha0;
            beta = beta0;
            gamma = gamma0;
            epsilon = epsilon0;
            lamda = lamda0;
            q = q0;
            lamdaEffective = lamdaEffective0;
            qEffective = qEffective0;
            v0 = v0_0;
            totalSigmaLive = totalSigmaLive0;
            totalSigmaDead = totalSigmaDead0;

            int n = 50000;
            //now go through 
            Console.Write( ">>>>>Starting.........\r\n");
            for (int i = 0; i < n; i++)
            {
                MCMCStep();
                if (i % 100 == 0)
                {
                    Console.Write("loop:" + i + "\r\n");
                }
            }
            Console.Write("Done.............\r\n");
        }
        public void output(string filename)
        {
            Console.WriteLine("writing output to \"" + filename + "\"........");
            StreamWriter w = new StreamWriter(filename);
            w.WriteLine("ID\talpha\tgamma\tbeta\tepsilon\tlamda\tq\tlamdaEffective\tqEff\tV0\tsigmaLive\tsigmaDead\tlogLL");
            for (int i = 0; i < alphaArr.Count; i++)
            {
                w.WriteLine(i+"\t" + alphaArr[i] + "\t"
                        + gammaArr[i] + "\t"
                        + betaArr[i] + "\t"
                        + epsilonArr[i] + "\t"
                        + lamdaArr[i]+"\t"
                        + qArr[i]+"\t"
                        +lamdaEffectiveArr[i]+"\t"
                        +qEffectiveArr[i]+"\t"
                        +v0Arr[i]+"\t"
                        +totalSigmaLiveArr[i]+"\t"
                        +totalSigmaDeadArr[i]+"\t"
                        +logLLArr[i]);

            }
            w.Close();
            Console.WriteLine("done........!!!!");
        }
        /// <summary>
        /// to do one step of mcmc MH algorithm
        /// </summary>
        private void MCMCStep()
        {
            double curAlpha=alpha;
            double curBeta=beta;
            double curGamma=gamma;
            double curEpsilon=epsilon;
            double curSigmaLive = totalSigmaLive;
            double curSigmaDead = totalSigmaDead;
            double curLamda=lamda;
            double curQ=q;
            double curLamdaEffective=lamdaEffective;
            double curQEffective = qEffective;
            double curV0 = v0;

            double sdA = 0.5;
            double sdB = 0.5;
            double sdG = 0.5;
            double sdE = 0.5;
            double sdSLive=0.5;//keep this unchange
            double sdSDead=0.05;//keep this unchange
            double sdL = 0.1;
            double sdQ = 0.01;
            double sdLE = 0.1;
            double sdQE = 0.01;
            double sdV0 = 0.00;

            
            //first calculate the logLikelihood of the current parameters
            List<double> firstGenDivTime;
            List<double> firstGenDeathTime;
            List<double> subSequentGenDivTime;
            List<double> subSequentGenDeathTime;
            List<int> modelPredictedTotalLiveCell;
            List<int> modelPredictedTotalDeadCell;

            ODEModelParameters.resetParameters(curAlpha, curGamma, curEpsilon, curBeta,curLamda ,curQ,curLamdaEffective,curQEffective, 0, curV0);
            runCellDivisionSimForCycleTimes(initialCellNumber, simLengthTotal , 1,
                out firstGenDivTime, out firstGenDeathTime, out subSequentGenDivTime, out subSequentGenDeathTime,
                out modelPredictedTotalLiveCell,out modelPredictedTotalDeadCell);

            double loglld = logLikelihood(firstGenDivTime, firstGenDeathTime, subSequentGenDivTime, subSequentGenDeathTime, 
                modelPredictedTotalLiveCell,curSigmaLive, modelPredictedTotalDeadCell, curSigmaDead );

            //update the parameters.
            double nextAlpha = Math.Exp(Math.Log(curAlpha) + sdA * zRand.GetRandomValue(rng));
            double nextBeta = Math.Exp(Math.Log(curBeta) + sdB * zRand.GetRandomValue(rng));
            double nextGamma = Math.Exp(Math.Log(curGamma) + sdG * zRand.GetRandomValue(rng));
            double nextEpsilon = Math.Exp(Math.Log(curEpsilon) + sdE * zRand.GetRandomValue(rng));
            double nextLamda = Math.Exp(Math.Log(curLamda) + sdL * zRand.GetRandomValue(rng));
            double nextQ=uRand2.GetRandomValue(rng3);
            double nextLamdaEffective = Math.Exp(Math.Log(curLamdaEffective) + sdLE * zRand.GetRandomValue(rng));
            double nextQEffective = uRand2.GetRandomValue(rng3);
            double nextV0 = Math.Exp(Math.Log(curV0) + sdV0 * zRand.GetRandomValue(rng));

            double nextSigmaLive=Math.Exp(Math.Log(curSigmaLive ) + sdSLive * zRand.GetRandomValue(rng));
            double nextSigmaDead=Math.Exp(Math.Log(curSigmaDead ) + sdSDead * zRand.GetRandomValue(rng));

            ODEModelParameters.resetParameters(nextAlpha, nextGamma, nextEpsilon, nextBeta, nextLamda,nextQ,
                nextLamdaEffective, nextQEffective,0,nextV0);
            runCellDivisionSimForCycleTimes(initialCellNumber, simLengthTotal, 1,
                out firstGenDivTime, out firstGenDeathTime, out subSequentGenDivTime, out subSequentGenDeathTime,
                out modelPredictedTotalLiveCell,out modelPredictedTotalDeadCell);

            double nextLoglld = logLikelihood(firstGenDivTime, firstGenDeathTime, subSequentGenDivTime, subSequentGenDeathTime,
                modelPredictedTotalLiveCell, nextSigmaLive, modelPredictedTotalDeadCell, nextSigmaDead );

            //now test whether to accept or reject for the chain
            
            bool accept;
            if (nextLoglld > loglld)
            {
                accept = true;
            }
            else
            {
                double u = uRand.GetRandomValue(rng2);
                //cout<<"\tnot accepted"<<endl;
                //cout<<"\tu is "<<u<<";logu is "<<log(u)<<endl;
                if (Math.Log(u) < nextLoglld - loglld)
                {
                    //cout<<"\tsecond accepted"<<endl;
                    accept = true;
                }
                else
                {
                    //cout<<"\tsecond Not"<<endl;
                    accept = false;
                }
            }

            //for accept we need 
            if (accept)
            {
                alphaArr.Add(nextAlpha);
                betaArr.Add(nextBeta);
                gammaArr.Add(nextGamma);
                epsilonArr.Add(nextEpsilon);
                logLLArr.Add( nextLoglld);
                lamdaArr.Add(nextLamda);
                qArr.Add(nextQ);
                lamdaEffectiveArr.Add(nextLamdaEffective);
                qEffectiveArr.Add(nextQEffective);
                v0Arr.Add(nextV0);

                totalSigmaLiveArr.Add(nextSigmaLive);
                totalSigmaDeadArr.Add(nextSigmaDead);

                alpha = nextAlpha;
                beta = nextBeta;
                gamma = nextGamma;
                epsilon = nextEpsilon;
                totalSigmaLive = nextSigmaLive;
                totalSigmaDead = nextSigmaDead;
                lamda = nextLamda ;
                q=nextQ;
                lamdaEffective=nextLamdaEffective;
                qEffective=nextQEffective;
                v0 = nextV0;
                
            }
            else
            {
                alphaArr.Add(curAlpha);
                betaArr.Add(curBeta);
                gammaArr.Add(curGamma);
                epsilonArr.Add(curEpsilon);
                lamdaArr.Add(curLamda);
                qArr.Add(curQ);
                lamdaEffectiveArr.Add(curLamdaEffective);
                qEffectiveArr.Add(curQEffective);
                logLLArr.Add( loglld);
                v0Arr.Add(curV0);
                totalSigmaLiveArr.Add(curSigmaLive);
                totalSigmaDeadArr.Add(curSigmaDead);
            }

            //we are done here
        }
        private double logLikelihood(List<double> firstGenDivTime, List<double> firstGenDeathTime,
            List<double> subSequentGenDivTime, List<double> subSequentGenDeathTime, List<int> modelPredictedLive, double _totalLiveSigma,
            List<int> modelPredictedDead, double _totalDeadSigma)
        {
            double logLL = 0; double weight = 0.000;
            //logLL += weight*logLikelihoodEventTime(firstGenDivTime, firstGenDeathTime, subSequentGenDivTime, subSequentGenDeathTime);
            logLL += logLikelihoodTotalCells(modelPredictedLive,_totalLiveSigma, modelPredictedDead, _totalDeadSigma);
            return logLL;
        }
        private double logLikelihoodTotalCells(List<int> _modelPredictedLive,double _totalSigmaLive, List<int> _modelPredictedDead,double _totalSigmaDead)
        {
            double logLL = 0;

            //using the normal distribution to fit the observed data with _modelPredicted values

            //for live data, calculate the 
            for (int i = 0; i < totalLiveByTime.Count; i++)
            {
                double temp= -0.5 * (Math.Log(2 * Math.PI * _totalSigmaLive * _totalSigmaLive) +
                    (_modelPredictedLive[i] - totalLiveByTime[i]) * (_modelPredictedLive[i] - totalLiveByTime[i]) / (_totalSigmaLive * _totalSigmaLive));
                logLL +=temp;
            }

            //only fit for live cells.
            /*
            for (int i = 0; i < totalDeadByTime.Count; i++)
            {
                double temp= -0.5 * (Math.Log(2 * Math.PI * _totalSigmaDead * _totalSigmaDead) +
                    (_modelPredictedDead[i] - totalDeadByTime[i]) * (_modelPredictedDead[i] - totalDeadByTime[i]) / (_totalSigmaDead * _totalSigmaDead));
                logLL +=temp;
            }
            */
            return logLL;
        }
        private double logLikelihoodEventTime(List<double> firstGenDivTime, List<double> firstGenDeathTime,
            List<double> subSequentGenDivTime, List<double> subSequentGenDeathTime)
        {
            double loglld = 0;
            int n = 0;
            //calculate the firstGenDiv logLikelihood
            for (int i = 0; i < firstGenDivTime.Count; i++)
            {
                n++;
                loglld += logLNormalPdf(muDiv1, sigmaDiv1, firstGenDivTime[i]);
            }
            //calculate the firstGenDiv Survival
            for (int i = 0; i < firstGenDeathTime.Count; i++)
            {
                n++;
                loglld += logLNormalSurvival(muDiv1, sigmaDiv1, firstGenDeathTime[i]);
            }

            //calculate the first gen Dth logLikelihood
            for (int i = 0; i < firstGenDeathTime.Count; i++)
            {
                n++;
                loglld += logLNormalPdf(muDth2, sigmaDth2, firstGenDeathTime[i]);
            }
            //calculate the subsequentGenDth Survival
            for (int i = 0; i < firstGenDivTime.Count; i++)
            {
                n++;
                loglld += logLNormalSurvival(muDth2, sigmaDth2, firstGenDivTime[i]);
            }


            //88888888888888888888888888888888888888888888888888
            //calculate the SubseqGenDiv logLikelihood
            for (int i = 0; i < subSequentGenDivTime.Count; i++)
            {
                n++;
                loglld += logLNormalPdf(muDiv2, sigmaDiv2, subSequentGenDivTime[i]);
            }
            //calculate the subsequentGenDiv Survival
            for (int i = 0; i < subSequentGenDeathTime.Count; i++)
            {
                n++;
                loglld += logLNormalSurvival(muDiv2, sigmaDiv2, subSequentGenDeathTime[i]);
            }
            //*************************************
            //calculate the subsequentGenDth logLikelihood
            for (int i = 0; i < subSequentGenDeathTime.Count; i++)
            {
                n++;
                loglld += logLNormalPdf(muDth2, sigmaDth2, subSequentGenDeathTime[i]);
            }
            //calculate the subsequentGenDth Survival
            for (int i = 0; i < subSequentGenDivTime.Count; i++)
            {
                n++;
                loglld += logLNormalSurvival(muDth2, sigmaDth2, subSequentGenDivTime[i]);
            }
            //*********************end********************
            //**************************************
            if (firstGenDivTime.Count <= 0)//no first division 
            {
                loglld = eps;
            }
            return loglld/n;
        }

        private double logLNormalPdf(double _mu, double _sigma, double _x)
        {
            return -0.5*Math.Log(2 * Math.PI * _sigma * _sigma)-Math.Log(_x) + (-1 * (Math.Log(_x) - _mu) * (Math.Log(_x) - _mu)/(2*_sigma*_sigma));
        }
        private double logLNormalSurvival(double _mu, double _sigma, double _x)
        {
            double retVal;
            LognormalDistribution lnd = new LognormalDistribution(_mu, _sigma);
            retVal=lnd.RightProbability(_x);

            return Math.Log(retVal);
        }

        /// <summary>
        /// this is the function run one full simulation of cell cycle in order to record the division/death time for MCMC analysis
        /// </summary>
        /// <param name="simLength"></param>    
        /// <param name="initialCellNumber"></param>
        private void runCellDivisionSimForCycleTimes(int initialCellNumber, double simLength,
                double samplingFrequency, out List<double> firstGenDivTime, out List<double> firstGenDeathTime,
                out List<double> subSequentGenDivTime, out List<double> subSequentGenDeathTime, 
            out List<int> _modelPredictedTotalLiveCellsByTime, out List<int> _modelPredictedTotalDeadCellsByTime
                    )
        {
            List<Cell> deathCells = new List<Cell>();
            List<Cell> liveCells = new List<Cell>(initialCellNumber);

            //prepare the output arrays
            firstGenDivTime = new List<double>();
            firstGenDeathTime = new List<double>();
            subSequentGenDivTime = new List<double>();
            subSequentGenDeathTime = new List<double>();
            _modelPredictedTotalLiveCellsByTime = new List<int>();
            _modelPredictedTotalDeadCellsByTime = new List<int>();

            //initialize the cells,
            for (int i = 0; i < initialCellNumber; i++)
            {
                liveCells.Add(new Cell());
            }
            int totalLiveIndex = 0;
            int totalDeadIndex = 0;
            for (double sampleTime = 0; sampleTime < simLength; sampleTime = sampleTime + samplingFrequency)
            {
                
                runSimulationStep(sampleTime, ref liveCells, ref deathCells, ref firstGenDivTime,ref subSequentGenDivTime, ref firstGenDeathTime, ref subSequentGenDeathTime);
                //first check for total live for observed data so as to generate modelpredicted values
                //now compare the totalLive time and total dead time to do the step() with whichever fast
                while (( totalDeadIndex<timeArrDead.Count &&timeArrDead[totalDeadIndex]<sampleTime+samplingFrequency)
                    ||(totalLiveIndex <timeArrLive.Count&&timeArrLive[totalLiveIndex]<sampleTime +samplingFrequency )
                )
                {
                    if ((totalDeadIndex>=timeArrDead.Count)||
                        ((totalDeadIndex<timeArrDead.Count && totalLiveIndex<timeArrLive.Count)&&(timeArrLive[totalLiveIndex] < timeArrDead[totalDeadIndex])))
                    {
                        if (timeArrLive[totalLiveIndex] >= sampleTime && timeArrLive[totalLiveIndex]<sampleTime+samplingFrequency)
                        {
                            if (timeArrLive[totalLiveIndex] != sampleTime)
                            {
                                runSimulationStep(timeArrLive[totalLiveIndex], ref liveCells, ref deathCells, ref firstGenDivTime, ref subSequentGenDivTime, ref firstGenDeathTime, ref subSequentGenDeathTime);
                            }
                            _modelPredictedTotalLiveCellsByTime.Add(liveCells.Count);
                            totalLiveIndex++;
                        }
                    }
                    else //(timeArrLive[totalLiveIndex]>timeArrDead[totalDeadIndex])
                    {
                        if ((totalLiveIndex>=timeArrLive.Count )||
                            ((totalLiveIndex<timeArrLive.Count &&totalDeadIndex<timeArrDead.Count)&&(timeArrLive[totalLiveIndex] > timeArrDead[totalDeadIndex]))
                        )
                        {
                            if (timeArrDead[totalDeadIndex] >= sampleTime && timeArrDead[totalDeadIndex] < sampleTime+samplingFrequency)
                            {
                                if (timeArrDead[totalDeadIndex] != sampleTime)
                                {
                                    runSimulationStep(timeArrDead[totalDeadIndex], ref liveCells, ref deathCells, ref firstGenDivTime, ref subSequentGenDivTime, ref firstGenDeathTime, ref subSequentGenDeathTime);
                                }
                                _modelPredictedTotalDeadCellsByTime.Add(deathCells.Count);
                                totalDeadIndex++;
                            }
                            
                        }
                        else //they are equal
                        {
                            if (timeArrDead[totalDeadIndex] >= sampleTime && timeArrDead[totalDeadIndex] > sampleTime+samplingFrequency)
                            {
                                if(timeArrDead[totalDeadIndex] != sampleTime)
                                {
                                    runSimulationStep(timeArrDead[totalDeadIndex], ref liveCells, ref deathCells, ref firstGenDivTime, ref subSequentGenDivTime, ref firstGenDeathTime, ref subSequentGenDeathTime);
                                }
                                _modelPredictedTotalDeadCellsByTime.Add(deathCells.Count);
                                totalDeadIndex++;
                                _modelPredictedTotalLiveCellsByTime.Add(liveCells.Count);
                                totalLiveIndex++;

                            }
                            

                        }
                    }

                }

            }//check sample time loop


        }//end of cellcycle simulation 

        private void runSimulationStep(double sampleTime, ref List<Cell> liveCells, ref List<Cell> deathCells, 
            ref List<double> firstGenDivTime, ref List<double> subSequentGenDivTime, 
            ref List<double> firstGenDeathTime, ref List<double> subSequentGenDeathTime)
        {
            List<Cell> tempLiveCells = new List<Cell>();
            int fate;

            //at each sampling point, run to check for each cells states
            for (int j = 0; j < liveCells.Count; j++)
            {
                //get the division or death time and compare with the corrent time 
                //make decision the death or division
                fate = liveCells[j].Step(sampleTime);
                if (fate == 1) //cell divide
                {
                    //temp cell buffer of size 2 to hold the divided cells.
                    List<Cell> cellBuf;
                    if (liveCells[j].DivisionNumber == 0)
                    {
                        firstGenDivTime.Add(liveCells[j].DivisionTime);

                    }
                    else
                    {
                        subSequentGenDivTime.Add(liveCells[j].DivisionTime);
                    }

                    liveCells[j].Divide(out cellBuf);


                    tempLiveCells.Add(cellBuf[0]);
                    tempLiveCells.Add(cellBuf[1]);

                }

                else //cell dead or keep going......
                {
                    if (fate == -1)
                    {
                        //make the cell die
                        deathCells.Add(liveCells[j]);
                        if (liveCells[j].DivisionNumber == 0)
                        {
                            firstGenDeathTime.Add(liveCells[j].DeathTime);

                        }
                        else
                        {
                            subSequentGenDeathTime.Add(liveCells[j].DeathTime);
                        }
                    }
                    else//cell keep going
                    {
                        tempLiveCells.Add(liveCells[j]);
                    }
                    //liveCells.RemoveAt(j);
                }//else
            }//check sample for loop

            liveCells = tempLiveCells;
        }


        private void readTotalCellByTimeModels(string livefilename,string deadfilename 
            )
        {
            //initialize the output arrays
            timeArrDead = new List<double>();
            timeArrLive = new List<double>();
            totalDeadByTime = new List<double>();
            totalLiveByTime = new List<double>();
            //open the files for reading. assume it is the csv file
            Console.WriteLine("Reading the total live cell file..........");
            using (StreamReader sr=File.OpenText(livefilename))
            {
                string s = "";
                string[] buf;//=new string[];
                char[] c = { ',' };
                while ((s = sr.ReadLine()) != null)
                {
                    //read the parse the text with ","
                    buf = s.Split(c);
                    if (buf.Count() != 2)
                    {
                        Console.WriteLine("*******ERROR in passing the input");
                        Environment.Exit(-1);
                    }
                    //good, so read it into arrays
                    timeArrLive.Add(Convert.ToDouble(buf[0]));
                    totalLiveByTime.Add(Convert.ToDouble(buf[1]));
                }
            }
            Console.WriteLine("Total " + timeArrLive.Count + " entries read in.........");
            Console.WriteLine("Done....");
            Console.WriteLine("Reading the total dead cell file..........");
            using (StreamReader sr = File.OpenText(deadfilename))
            {
                string s = "";
                string[] buf;//=new string[];
                char[] c = { ',' };
                while ((s = sr.ReadLine()) != null)
                {
                    //read the parse the text with ","
                    buf = s.Split(c);
                    if (buf.Count() != 2)
                    {
                        Console.WriteLine("*******ERROR in passing the input");
                        Environment.Exit(-1);
                    }
                    //good, so read it into arrays
                    timeArrDead.Add(Convert.ToDouble(buf[0]));
                    totalDeadByTime.Add(Convert.ToDouble(buf[1]));
                }
            }
            Console.WriteLine("Total " + timeArrDead.Count + " entries read in.........");
            Console.WriteLine("Done.............");

        }

        //member definition, initial values for parameters
        public  double alpha0=0.003;
        public  double beta0=0.002895;
        public  double gamma0=1;
        public  double epsilon0=1.035;
        const double totalSigmaLive0 = 5;
        const double totalSigmaDead0 = 5;
        public double lamda0 = 1.0;
        public double q0 = 0.15;
        public double lamdaEffective0= 1.5;
        public double qEffective0 = 0.3;
        public double v0_0 = 0;


        //current value of parameters
        public int initialCellNumber;
        public double simLengthTotal;
        public double alpha;
        public double beta;
        public double gamma;
        public double epsilon;
        public double lamda;
        public double q;
        public double lamdaEffective;
        public double qEffective;
        public double v0;

        //two std error for fitting total cell number data
        double totalSigmaLive;
        double totalSigmaDead;

        //parameter array for posterior distribution
        List<double> alphaArr;
        List<double> betaArr;
        List<double> gammaArr;
        List<double> epsilonArr;
        List<double> logLLArr;
        List<double> totalSigmaLiveArr;
        List<double> totalSigmaDeadArr;
        List<double> lamdaArr;
        List<double> qArr;
        List<double> lamdaEffectiveArr;
        List<double> qEffectiveArr;
        List<double> v0Arr;

        //fixed input for logNormal of first gen 
        double muDiv1=3.611498;
        double sigmaDiv1=0.100792;
            //no first gene death distribution from the data
        
        //logNormal of subseq gen
        double muDiv2=2.194043;
        double sigmaDiv2=0.268221;
        double muDth2=3.40;
        double sigmaDth2=0.50;
        const double eps = -9E20;
        //TextBox log;

        //declare of arrays, so we don't need to pass them around.
        List<double> totalLiveByTime;
        List<double> timeArrDead;
        List<double> totalDeadByTime;
        List<double> timeArrLive;


        private NormalDistribution zRand;
        private Random rng;

        private UniformDistribution uRand;
        private Random rng2;

        private UniformDistribution uRand2;
        private Random rng3;
    }//end of class
}
