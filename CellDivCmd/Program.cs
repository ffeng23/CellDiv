using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CellDiv;
using DivisionLib;

namespace CellDivCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("testing...........");
            MCMC m = new MCMC();
            m.run();
            m.output("result.txt");


            int totalCellNumber =m.initialCellNumber ;
            List<Cell> deathCells = new List<Cell>();
            
            List<Cell> liveCells = new List<Cell>(totalCellNumber);

            //a very good fit with 20000 runs for generating totalcell_withDivisionDestiny.jpg
            //19912	0.00145564595309824	1.7070429251297	0.822407992181008	0.00231311603172408	1.77948615358725	0.270687081977114	13.6808824542915	0.305769407332767	0	76.0489254860717	0.405243363280072	-177.78027653617

            //bad data for demo withoutDivisionDestiny, jpg
            //61753.473633418,	1.8677965784426E-08,	237201.245867917,	4.4806179692149E-09,	5.472259142423913,	0.384525221485889,	247.158575015769,	0.229926508958417,	0,	62.7553555986278
            /*double alpha = 0.0710415853343429;	2.07782034499901E-05	1.18451122822509	0.000289315651017502	3.68669443652003	0.820693346122603	1.40609331314403	0.283023117241926
            double gamma = 0.181871297098108;
            double beta = 0.12649848089638;
            double epsilon = 0.0449834299753192;*/
            double[] arr = {61753.473633418,	1.8677965784426E-08,	237201.245867917,	4.4806179692149E-09,	5.472259142423913,	0.384525221485889,	247.158575015769,	0.229926508958417,	0,	62.7553555986278};

            double alpha = arr[0];// m.alpha0;
            double gamma = arr[1];// m.gamma0;
            double beta = arr[2];// m.beta0;
            double epsilon = arr[3];// m.epsilon0;
            double lamda = arr[4];// m.lamda0;
            double q = arr[5];// m.q0;
            double lamdaEffective = arr[6];//m.lamdaEffective0;
            double qEffective = arr[7];// m.qEffective0;
            double v0 = arr[8];
             /*
            double alpha =  m.alpha0;
            double gamma =  m.gamma0;
            double beta =  m.beta0;
            double epsilon =  m.epsilon0;
            double lamda =  m.lamda0;
            double q =  m.q0;
            double lamdaEffective = m.lamdaEffective0;
            double qEffective =  m.qEffective0;*/




            ODEModelParameters.resetParameters(alpha, gamma, epsilon, beta, lamda,q,lamdaEffective,qEffective,0,v0);
            for (int i = 0; i < totalCellNumber; i++)
            {
                liveCells.Add(new Cell());
            }
            //so far the death pool is empty

            const double experimentalDuration = 200;//(300hrs)
            double samplingFrequency = 2.0;//sample every 2minutes

            StreamWriter writer = new StreamWriter("div.txt");
            writer.WriteLine("time\tlive\tdeath");

            StreamWriter writerSbl = new StreamWriter("divSibling.txt");
            writerSbl.WriteLine("time\tsibling1\tsibling2");

            StreamWriter writerMotherD = new StreamWriter("divMD.txt");
            writerMotherD.WriteLine("time\tsibling1\tmother");

            StreamWriter writerByGen = new StreamWriter("divByGen.txt");
            writerByGen.Write("time\t");
            //start the sampling
            //make another temperary live cells pool

            List<List<double>> sibling = new List<List<double>>();
            List<double> temp_t;

            List<List<double>> momDaughter = new List<List<double>>();
            List<double> temp_MD;
            int maxGen = 15;
            List<int> byGeneration = new List<int>();
            for (int i = 0; i < maxGen; i++)
            {
                byGeneration.Add(0);
                writerByGen.Write("gene" + i);
                if (i < maxGen - 1)
                {
                    writerByGen.Write("\t");
                }
                else
                {
                    writerByGen.Write("\r\n");
                }
            }

            int fate;
            for (double sampleTime = 0; sampleTime < experimentalDuration; sampleTime = sampleTime + samplingFrequency)
            {

                for (int i = 0; i < maxGen; i++)
                {
                    byGeneration[i] = 0;

                }

                List<Cell> tempLiveCells = new List<Cell>();
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
                        double motherDivisionTime = liveCells[j].DivisionTime;
                        liveCells[j].Divide(out cellBuf);


                        tempLiveCells.Add(cellBuf[0]);
                        tempLiveCells.Add(cellBuf[1]);
                        //need to write down the sibling division time
                        if (cellBuf[0].Fate || cellBuf[1].Fate)
                        {
                            temp_MD = new List<double>();
                            if (cellBuf[0].Fate)
                            {
                                temp_MD.Add(cellBuf[0].DivisionTime);
                                temp_MD.Add(motherDivisionTime);
                                momDaughter.Add(temp_MD);
                            }
                            temp_MD = new List<double>();
                            if (cellBuf[1].Fate)
                            {
                                temp_MD.Add(cellBuf[1].DivisionTime);
                                temp_MD.Add(motherDivisionTime);
                                momDaughter.Add(temp_MD);
                            }
                            if (cellBuf[0].Fate && cellBuf[1].Fate)
                            {
                                temp_t = new List<double>();
                                temp_t.Add(cellBuf[0].DivisionTime);
                                temp_t.Add(cellBuf[1].DivisionTime);
                                //temp_t.Add(liveCells[j].DivisionTime);
                                sibling.Add(temp_t);
                            }
                        }
                    }

                    else //cell dead or keep going......
                    {
                        if (fate == -1)
                        {
                            //make the cell die
                            deathCells.Add(liveCells[j]);

                        }
                        else//cell keep going
                        {
                            tempLiveCells.Add(liveCells[j]);
                        }
                        //liveCells.RemoveAt(j);
                    }//else
                }//check sample for loop
                writer.WriteLine(sampleTime + "\t" + tempLiveCells.Count + "\t" + deathCells.Count);
                liveCells = tempLiveCells;

                //need to go through the liveCell list to pickup the by generation information
                for (int j = 0; j < liveCells.Count(); j++)
                {
                    byGeneration[(liveCells[j].DivisionNumber)]++;
                }
                writerByGen.Write(sampleTime + "\t");
                for (int k = 0; k < maxGen; k++)
                {
                    writerByGen.Write(byGeneration[k]);
                    if (k < maxGen - 1)
                    {
                        writerByGen.Write("\t");
                    }
                    else
                    {
                        writerByGen.Write("\r\n");
                    }
                }

            }//check sample time loop

            for (int i = 0; i < sibling.Count; i++)
            {
                writerSbl.WriteLine(i + "\t" + sibling[i][0] + "\t" + sibling[i][1]);
            }

            for (int i = 0; i < momDaughter.Count; i++)
            {
                writerMotherD.WriteLine(i + "\t" + momDaughter[i][0] + "\t" + momDaughter[i][1]);
            }


            //now just need to print out the result 
            writer.Close();
            writerSbl.Close();
            writerMotherD.Close();
            writerByGen.Close();

            //run MCMC.
            
        }
    }
}
