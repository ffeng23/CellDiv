using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

using DivisionLib;
//using DivisionModel;
namespace CellDiv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            //need to initializing cells and keep track of them for division/death
            int totalCellNumber = Convert.ToInt32(this.txtFounderCellNum.Text);
            List<Cell> deathCells = new List<Cell>();
            List<Cell> liveCells = new List<Cell>(totalCellNumber);

            double divMean1=Convert.ToDouble(txt1DivMean.Text);
            double divVar1=Convert.ToDouble(txt1DivVar.Text);
            
            double divMean2=Convert.ToDouble(txt2DivMean.Text);
            double divVar2=Convert.ToDouble(txt2DivVar.Text);

            double deathMean1=Convert.ToDouble(txt1DeathMean.Text);
            double deathVar1=Convert.ToDouble(txt1DeathVar.Text);

            double deathMean2=Convert.ToDouble(txt2DeathMean.Text);
            double deathVar2=Convert.ToDouble(txt2DeathVar.Text);

            //CytonModel.buildCytonModel(divMean1, divVar1, deathMean1, deathVar1,
            //        divMean2, divVar2, deathMean2, deathVar2);

            totalCellNumber =40;
            
            for (int i = 0; i < totalCellNumber; i++)
            {
                liveCells.Add( new Cell());
            }
            //so far the death pool is empty

            const double experimentalDuration =300;//(300hrs)
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

            List<List<double>> sibling=new List<List<double> >();
            List<double> temp_t;

            List<List<double>> momDaughter = new List<List<double>>();
            List<double> temp_MD;
            int maxGen = 11;
            List<int> byGeneration = new List<int>();
            for (int i = 0; i < maxGen; i++)
            {
                byGeneration.Add(0);
                writerByGen.Write("gene" + i);
                if (i < maxGen-1)
                {
                    writerByGen.Write("\t");
                }
                else
                {
                    writerByGen.Write("\r\n");
                }
            }

            int fate;
            for (double sampleTime = 0; sampleTime < experimentalDuration; sampleTime=sampleTime+samplingFrequency)
            {

                for (int i = 0; i < maxGen; i++)
                {
                    byGeneration[i]=0;
                    
                }

                List<Cell> tempLiveCells=new List<Cell>();
                //at each sampling point, run to check for each cells states
                for (int j = 0; j < liveCells.Count; j++)
                {
                    //get the division or death time and compare with the corrent time 
                    //make decision the death or division
                   fate=liveCells[j].Step(sampleTime);
                    if ( fate==1) //cell divide
                    {
                        //temp cell buffer of size 2 to hold the divided cells.
                        List<Cell> cellBuf; 
                        double motherDivisionTime=liveCells[j].DivisionTime;
                        liveCells[j].Divide(out cellBuf);
                        
                                             
                        tempLiveCells.Add(cellBuf[0]);
                        tempLiveCells.Add(cellBuf[1]);
                        //need to write down the sibling division time
                        if (cellBuf[0].Fate || cellBuf[1].Fate)
                        {
                            temp_MD=new List<double>();
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
                        if (fate==-1 )
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
                writer.WriteLine(sampleTime + "\t" + tempLiveCells.Count + "\t" + deathCells.Count );
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
                    if (k <maxGen-1)
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
                writerSbl.WriteLine(i + "\t" + sibling[i][0] + "\t"+sibling[i][1]);
            }

            for (int i = 0; i <momDaughter.Count; i++)
            {
                writerMotherD.WriteLine(i + "\t" + momDaughter[i][0] + "\t" + momDaughter[i][1] );
            }
            

            //now just need to print out the result 
            writer.Close();
            writerSbl.Close();
            writerMotherD.Close();
            writerByGen.Close();

            //run MCMC.
            MCMC m = new MCMC();
            m.run();
            
           
            //now we have the output, we
        }//end of run button
        
    }//end of main window class
}//end of namesapce celldiv
