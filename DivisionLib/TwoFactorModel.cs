using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meta.Numerics.Statistics;

namespace DivisionLib
{
    /// <summary>
    /// This is the two minimum factor model describing the cell division with the
    /// emphasis on the correlation between generations
    /// See reference Markham JF, 2010 J R Soc Interface 7,1049.
    /// To summarize this model,
    /// 1)the cell decide the fate (divide or not at the beginning or right after the division)
    /// 2)then at the time of division/death to divide/die
    /// </summary>
    
    [Serializable]
    public class TwoFactorModel: DivisionModel   
    {
        /// <summary>
        /// Constructor, this is the one used to build the founder cells.
        /// </summary>
        /// <param name="signal">the input used to indicate the activation level. It has values between 0 and 1. 
        /// 
        /// For now, only takes 1 as activated with CpG and the cell is undergoing division. Will do more for later. </param>
        public TwoFactorModel(float signal=1)
        {
            divMachine = new DivisionMachine();
            dthMachine = new DeathMachine();

            //decide to divide or die
            
            divisionNum = 0;decideFate();
            this.tLastDivision = divMachine.TLastDivision;
            this.tDivision = divMachine.TDivision;
            this.tDeath = dthMachine.TDeath;
        }

        /// <summary>
        /// Constructor, this is the one used to build all the daughter cells.  
        /// </summary>
        /// <param name="m">the Division model/machine from its mother cell. </param>
        public TwoFactorModel(DivisionModel m)
        {
            divMachine = new DivisionMachine(((TwoFactorModel)m).divMachine.RM, ((TwoFactorModel)m).divMachine.RM_D, ((TwoFactorModel)m).divMachine.TLastDivision + ((TwoFactorModel)m).divMachine.TDivision, ((TwoFactorModel)m).divMachine.ProgenyDestiny );
            dthMachine = new DeathMachine(((TwoFactorModel)m).dthMachine.TDeath);
            this.divisionNum = m.DivisionNum +1;

            this.tLastDivision = divMachine.TLastDivision;
            decideFate();
            this.tDivision = divMachine.TDivision;
            this.tDeath = dthMachine.TDeath;

        }

        /// <summary>
        /// the method used to decide the fate of the current cell (die or divide). It should called at the time building the cell.
        /// </summary>
        protected void decideFate()
        {
            if (divisionNum > divMachine.EffectiveProgenyDestiny)
            {
                fate = false;
                return ;
            }
            if (divMachine.RM + divMachine.RM_D < divMachine.RLow)
            {
                fate = false;//die
            }
            else
            {
                if (divMachine.RM + divMachine.RM_D > divMachine.RHigh)
                {
                    fate = true;
                }
                else
                {
                    //if(divMachine.RM + divMachine.RM_D > divMachine.RHigh) in the middle area
                    if(dthMachine.TDeath >=(dthMachine.MuDeathTm -dthMachine.SigmaDeathTm ))
                    {
                        //decide based on an probability
                        double temp=TwoFactorModelRandomGenerators.uniRandGenerator.GetRandomValue(TwoFactorModelRandomGenerators.uniRng );

                        if(temp<(divMachine.RM+divMachine.RM_D - divMachine.RLow)/(divMachine.RHigh-divMachine.RLow))
                        {
                            fate=true;
                        }
                        else
                        {
                            fate=false;
                        }
                    }
                    else
                    {
                        fate=false;
                    }
                }
            }
        }

        /// <summary>
        /// the function to run one step of the model to evaluate the whether the cell to divide/die/keep running
        /// </summary>
        /// <param name="time">the time step to run at</param>
        /// 
        /// <returns>an integer to indicate wheter the cell to divide (1), die(-1) or keep going(0)</returns>
        public override int Step(double time)
        {
            //based on the fate and death time and division time decide at this moment, what cell should do
            if (fate)//for division
            {
                if (divMachine.TDivision <= time-divMachine.TLastDivision)
                {
                    return 1;//tell the cell to divide
                }
            }
            else //death
            {
                if (dthMachine.TDeath <= time-divMachine.TLastDivision )
                {
                    return -1;
                }
            }

            //if we are here, we need to wait
            return 0;
        }
        /// <summary>
        /// this is the function to reuse the current division model for division
        /// </summary>
        public override void Regenerate()
        {
            divMachine.Regenerate();
            dthMachine.Regenerate();
            this.divisionNum = this.divisionNum + 1;

            this.tLastDivision = divMachine.TLastDivision;
            decideFate();
            this.tDivision = divMachine.TDivision;
            this.tDeath = dthMachine.TDeath;
        }
        /*public double TLastDivision
        {
            get { return tLastDivision; }
        }
        public int DivisionNum
        {
            get { return divisionNum; }
        }
        public double TDivision
        {
            get { return divMachine.TDivision; }
        }
        public double TDeath
        {
            get { return dthMachine.TDeath ; }
        }
        public bool Fate
        {
            get { return fate; }
        }
        */

        private DeathMachine dthMachine;
        private DivisionMachine divMachine;
        //*** the following will be inherited from the base class
        //private int divisionNum;
        //private double tLastDivision;
        //private double tDivision;
        //private double tDeath;
        //private bool fate;//yes, for division; no, for death. either die or divide, no third option
    }
    [Serializable]
    class DivisionMachine
    {
        /// <summary>
        /// constructor for building the founder cells withoug passing the founder rm.
        /// </summary>
        public DivisionMachine()
        {
            initializeValues();

            generateInitialRs();
            

            calculateDivisionTime();
            growRs();
            tLastDivision = 0;

            //we need to generate the progeny distiny based on the initial R, rm, it is more like to scale the rm to progeny destiny dis
            double tempProgenyDestiny =Convert.ToInt32(Math.Round(((this.rM-muDivisionM)/sigmaDivisionM)*sigmaProgDest +muProgDest,0));
            if (!TwoFactorModelRandomGenerators.progDestParameterSetFlag)
            {
                TwoFactorModelRandomGenerators.progDestNormGenerator.SetValues(muProgDest, sigmaProgDest);
                TwoFactorModelRandomGenerators.progDestParameterSetFlag = true;

            }

            //check only to receive the values that larger than the scale rM values.
            do
            {
                progenyDestiny = TwoFactorModelRandomGenerators.progDestNormGenerator.GetRandomValue(TwoFactorModelRandomGenerators.progDestRng);
            } while (ProgenyDestiny < tempProgenyDestiny);
            effectiveProgenyDestiny = progenyDestiny;
        }

        /// <summary>
        /// contructor for building the daughter cells.
        /// </summary>
        /// <param name="rm">rm from mother cell</param>
        /// <param name="rm_d">rm_d from mother cell</param>
        /// <param name="tLastDivision">the time of the division for the mother cell.</param>
        public DivisionMachine(double rm, double rm_d, double tLastDivision, int progenyDestiny)
        {
            initializeValues();
            contributeRs();
            rM = 0.5 * (rm + rm_d);
            rM += rD;
            this.tLastDivision  = tLastDivision;
            

            calculateDivisionTime();
            growRs(); //the R grows as the cell grow towards the division after the dicision has been made;

            this.progenyDestiny = progenyDestiny;
            //this one,we know the progenyDestiny from mom, non need to variate it.
            effectiveProgenyDestiny =Convert.ToInt32(Math.Floor(TwoFactorModelRandomGenerators.progDestVarNormGenerator.GetRandomValue(TwoFactorModelRandomGenerators.progDestVarRng)*sigmaVarProgDest));
            effectiveProgenyDestiny+=progenyDestiny;
        }

        private void calculateDivisionTime()
        {
            //rM = boundingDivisionTime(rM);
            tDivision = tFixed + 1 / boundingDivisionTime(rM);
            
        }
        /// <summary>
        /// to bounding the division time in case of the small or negative r;
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private double boundingDivisionTime(double r)
        {
            double a=1/tThreshold;
            double b=1/tMax;
            double kOut=1/(2*(a-b));
            double kIn=4*kOut;

            if (r < a)
            {
                return (1 / kOut) * (1 / (1 + Math.Exp(-1 * kIn * (r - a)))) + b;
            }
            else
            {
                return r;
            }
        }

        public void Regenerate()
        {
            
            //initializeValues(); //don't have to re-initialize values since those values are all fixed for the model so far
            contributeRs();
            this.rM = 0.5 * (this.rM + this.rM_D);
            this.rM +=this.rD;
            this.tLastDivision = this.tLastDivision+this.tDivision ;


            calculateDivisionTime();
            growRs(); //the R grows as the cell grow towards the division after the dicision has been made;
            effectiveProgenyDestiny = Convert.ToInt32(Math.Round(TwoFactorModelRandomGenerators.progDestVarNormGenerator.GetRandomValue(TwoFactorModelRandomGenerators.progDestVarRng) * sigmaVarProgDest));
            effectiveProgenyDestiny += progenyDestiny;
        }

        private void initializeValues()
        {
            //the parameters are setup according to the reference paper.
            //for division parameters;
            muDivisionM = 0.9;
            sigmaDivisionM = 0.02;

            muDivisionM_D = -0.03;
            sigmaDivisionM_D = 0.095;

            sigmaDivisionD = 0.032;

            rHigh = 1.0 / 12;
            rLow = 1.0 / 22;
            tFixed = 5.0;

            tThreshold = 10.0;
            tMax = 35.0;

            //add new parameter for progeny destiny
            muProgDest = 2.5;
            sigmaProgDest = 1.5;

            //new parameter for variation in progeny destiny 
            sigmaVarProgDest = 0.836456;

            nInitialCells = 40;
        }

        private void generateInitialRs()
        {
            //need to decide the parameters
            rM = TwoFactorModelRandomGenerators.rMNormGenerateor.GetRandomValue(TwoFactorModelRandomGenerators.rMRng);
            rM = rM * sigmaDivisionM + muDivisionM;
            rM = boundingDivisionTime(rM);
            
        }
        private void growRs() //this is the R growed after the decision of division for rM_D
        {
            rM_D = TwoFactorModelRandomGenerators.rM_DNormGenerator.GetRandomValue(TwoFactorModelRandomGenerators.rM_DRng);
            rM_D = rM_D * sigmaDivisionM_D + muDivisionM_D;
        }

        private void contributeRs()  //the Rs contribute to the dicision by the daughter cells. for rD
        {
            rD = TwoFactorModelRandomGenerators.rDNormGenerator.GetRandomValue(TwoFactorModelRandomGenerators.rDRng);
            rD = rD * sigmaDivisionD;
        }

        public double RHigh
        {
            get { return rHigh; }
        }

        public double RLow
        {
            get { return rLow; }
        }
        public double RM
        {
            get { return rM; }
        }
        public double RM_D
        {
            get { return rM_D; }
        }

        public double TDivision
        {
            get { return tDivision; }
        }
        public double TLastDivision
        {
            get { return tLastDivision; }
        }

        public int ProgenyDestiny
        {
            get { return this.progenyDestiny; }
        }

        public int EffectiveProgenyDestiny
        {
            get { return this.effectiveProgenyDestiny; }
        }

        //define the model parameters
        //for division
        private double muDivisionM;
        private double sigmaDivisionM;

        private double muDivisionM_D;
        private double sigmaDivisionM_D;

        private double sigmaDivisionD;

        private double rHigh;
        private double rLow;
        private double tFixed;

        private double rM;
        private double rM_D;
        private double rD;

        private double tDivision;
        private double tLastDivision;

        private double tThreshold;
        private double tMax;
        private double nInitialCells;

        private double muProgDest;
        private double sigmaProgDest;
        private double sigmaVarProgDest;
        private int progenyDestiny;
        private int effectiveProgenyDestiny;
    }
    [Serializable]
    class DeathMachine
    {
        /// <summary>
        /// Constructors for the founder cells 
        ///
        /// </summary>
        public DeathMachine()
        {
            initializeValues();
            generateInitialDeathTime();

        }
        /// <summary>
        /// contructors for the daughter cells
        /// </summary>
        /// <param name="_tDeath">death time of the mother cell</param>
        public DeathMachine(double _tDeath)
        {
            initializeValues();
            calculateDeathTime(_tDeath);
            
        }
        //user to divide as a new object with reusing the memory.
        /// <summary>
        /// the function to reassign the values of the current object and reuse the object for the daughter cell at the division
        /// </summary>
        public void Regenerate()
        {
            calculateDeathTime(this.tDeath);
        }


        private void initializeValues()
        {
            //for death parameters
            muDeathTm = 3.623091;
            sigmaDeathTm = 0.362735;

            muDeathKdn = -0.15804;
            sigmaDeathKdn = 0.324593;

            

        }

        private void calculateDeathTime(double tmDeath)
        {
            tDeath =  TwoFactorModelRandomGenerators.tKdnNormGenerator.GetRandomValue(TwoFactorModelRandomGenerators.tKdnRng);
            tDeath = tDeath * sigmaDeathKdn + muDeathKdn;
            tDeath = Math.Exp(tDeath);
            tDeath *= tmDeath;
        }

        private void generateInitialDeathTime()
        {
            tDeath = TwoFactorModelRandomGenerators.tDthNormGenerator.GetRandomValue(TwoFactorModelRandomGenerators.tDthRng);
            tDeath = tDeath * sigmaDeathTm + muDeathTm;
            tDeath = Math.Exp(tDeath);
        }

        public double TDeath
        {
            get { return tDeath; }
        }
        public double MuDeathTm
        {
            get { return muDeathTm ; }
        }
        public double SigmaDeathTm
        {
            get { return sigmaDeathTm ; }
        }
        //for death
        private double muDeathTm;
        private double sigmaDeathTm;

        private double muDeathKdn;
        private double sigmaDeathKdn;

        

        private double tDeath;
    }
}
