using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DivisionLib
{
    /// <summary>
    /// public class implementing Differential Equation based cell division model 
    /// </summary>
    [Serializable]
    public class ODEModel : DivisionModel
    {
        /// <summary>
        /// Constructor, this is the one used to build the founder cells.
        /// </summary>
        /// <param name="signal">the input used to indicate the activation level. It has values between 0 and 1. 
        /// 
        /// For now, only takes 1 as activated with CpG and the cell is undergoing division. Will do more for later. </param>
        public ODEModel(float signal=1)
        {
            divMachine = new ODEDivisionMachine();
            dthMachine = new ODEDeathMachine();

            //decide to divide or die
            divisionNum = 0;
            this.tLastDivision = divMachine.TLastDivision;
            this.tDivision = divMachine.TDivision;
            //this.tDivisionTotal = divMachine.TDivisionTotal;
            this.tDeath = dthMachine.TDeath;
            decideFate();
        }

        /// <summary>
        /// Constructor, this is the one used to build all the daughter cells.  
        /// </summary>
        /// <param name="m">the Division model/machine from its mother cell. </param>
        public ODEModel(DivisionModel m)
        {
            divMachine = new ODEDivisionMachine(((ODEModel)m).divMachine.TDivision, ((ODEModel)m).divMachine.V0, ((ODEModel)m).divMachine.TLastDivision + ((ODEModel)m).divMachine.TDivision, ((ODEModel)m).divMachine.ProgenyDestiny, ((ODEModel)m).divMachine.DivDestiny
                );
            dthMachine = new ODEDeathMachine(((ODEModel)m).divMachine.TDivision, ((ODEModel)m).dthMachine.M0, ((ODEModel)m).divMachine.TLastDivision + ((ODEModel)m).divMachine.TDivision);
            this.divisionNum = m.DivisionNum +1;

            this.tLastDivision = divMachine.TLastDivision;
            this.tDivision = divMachine.TDivision;
            this.tDeath = dthMachine.TDeath;
            decideFate();
        }
        
        /// <summary>
        /// the method used to decide the fate of the current cell (die or divide). It should called at the time building the cell.
        /// </summary>
        protected void decideFate()
        {
            if (divisionNum >= divMachine.EffectiveProgenyDestiny)
            {
                fate = false;
                return ;
            }
            if (tDivision <= tDeath)
            {
                fate = true;
            }
            else
            {
                fate = false;
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
            dthMachine.Regenerate(divMachine.TDivision);
            this.divisionNum = this.divisionNum + 1;

            this.tLastDivision = divMachine.TLastDivision;
            decideFate();
            this.tDivision = divMachine.TDivision;
            this.tDeath = dthMachine.TDeath;
         }
        
        private ODEDeathMachine dthMachine;
        private ODEDivisionMachine divMachine;
        //*** the following will be inherited from the base class
        //private int divisionNum;
        //private double tLastDivision;
        //private double tDivision;
        //private double tDeath;
        //private bool fate;//yes, for division; no, for death. either die or divide, no third option
    }
    [Serializable]
    class ODEDivisionMachine
    {
        /// <summary>
        /// constructor for building the founder cells without passing the parameters.
        /// </summary>
        public ODEDivisionMachine()
        {
            initializeValues();

            //generateInitialRs();
            
            calculateDivisionTime();
            //tDivision += 30;
            //growRs();
            tLastDivision = 0;

            //first we got the divDestiny for the founder cells.
            divDestiny = new DivisionDestiny();//this one now using the default values specified in the divisiondestiny class, 
                                                //will need to reset later for MCMC
            progenyDestiny = divDestiny.getNextRandomValue(TwoFactorModelRandomGenerators.progDestRng);

            
            //we need to generate the effective progeny distiny by variation
            /*double tempProgenyDestiny = 5;//Convert.ToInt32(Math.Round(((this.rM-muDivisionM)/sigmaDivisionM)*sigmaProgDest +muProgDest,0));
            if (!TwoFactorModelRandomGenerators.progDestParameterSetFlag)
            {
                TwoFactorModelRandomGenerators.progDestNormGenerator.SetValues(muProgDest, sigmaProgDest);
                TwoFactorModelRandomGenerators.progDestParameterSetFlag = true;

            }

            //check only to receive the values that larger than the scale rM values.
            do
            {
                progenyDestiny = TwoFactorModelRandomGenerators.progDestNormGenerator.GetRandomValue(TwoFactorModelRandomGenerators.progDestRng);
            } while (ProgenyDestiny < tempProgenyDestiny);*/
            effectiveProgenyDestiny = divDestiny.getNextRandomValudEffective(TwoFactorModelRandomGenerators.progDestVarRng,progenyDestiny );
             
        }
        /// <summary>
        /// contructor for building the daughter cells.
        /// </summary>
        /// <param name="_tLastDivision">the relative division time for last generation, this is the divisiton time for its mother since the mother was born</param>
        /// <param name="v0Last">the v0(protein controlling the division) concentration at the mother birth time for the mother</param>
        /// <param name="_tDivisionTotalLast">the absolution birth time for this daughter cell since system started</param>
        public ODEDivisionMachine( double _tLastDivision, double v0Last, double _tDivisionTotalLast,int _progenyMother
                                  ,DivisionDestiny _divDestiny)
        {
            initializeValues();
            //contributeRs();
            v0 = 0.5 * (v0Last + gammaDivision*_tLastDivision );
            
            calculateDivisionTime();
            this.tLastDivision  = _tDivisionTotalLast;
            //growRs(); //the R grows as the cell grow towards the division after the dicision has been made;
            //tDivisionTotal = tDivision + _tDivisionTotalLast;
            
            this.progenyDestiny = _progenyMother;
            //this one,we know the progenyDestiny from mom, non need to variate it.
            divDestiny=_divDestiny;
            effectiveProgenyDestiny =divDestiny.getNextRandomValudEffective(TwoFactorModelRandomGenerators.progDestVarRng,progenyDestiny);
        }
        
            
        private void calculateDivisionTime()
        {
            //rM = boundingDivisionTime(rM);
            tDivision = tFixed + boundingDivisionTime();
            
        }
        /// <summary>
        /// to bounding the division time in case of the small or negative r;
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private double boundingDivisionTime()
        {
            //first generate a random number
            double fu=ODEModelRandomGenerator.divGenerateor.GetRandomValue(ODEModelRandomGenerator.divRng);
            //now get the reverse of it
            return Math.Sqrt(v0*v0/(gammaDivision*gammaDivision)-2/(alphaDivsion*gammaDivision)*Math.Log(1-fu))-v0/gammaDivision;
        }

        public void Regenerate()
        {
            
            //initializeValues(); //don't have to re-initialize values since those values are all fixed for the model so far
            //contributeRs();
            //this.rM = 0.5 * (this.rM + this.rM_D);
            //this.rM +=this.rD;
            //this.tLastDivision = this.tLastDivision+this.tDivision ;
            v0 = 0.5 * (v0 + gammaDivision * tDivision);

            calculateDivisionTime();
            //growRs(); //the R grows as the cell grow towards the division after the dicision has been made;
            effectiveProgenyDestiny = Convert.ToInt32(Math.Round(TwoFactorModelRandomGenerators.progDestVarNormGenerator.GetRandomValue(TwoFactorModelRandomGenerators.progDestVarRng) * sigmaVarProgDest));
            effectiveProgenyDestiny += progenyDestiny;
             
        }
        
        private void initializeValues()
        {
            //the parameters are setup according to the reference paper.
            //for division parameters;
            v0 = ODEModelParameters.v0 ;//0.9
            alphaDivsion = ODEModelParameters.alphaDivsion ;

            gammaDivision  = ODEModelParameters.gammaDivision ;//-0.03
            tFixed = ODEModelParameters.tFixed ;
            //effectiveProgenyDestiny = 6;
            /*sigmaDivisionM_D = 0.095;

            sigmaDivisionD = 0.20;//0.032

            rHigh = 1.0 / 12;
            rLow = 1.0 / 22;
            

            tThreshold = 10.0;
            tMax = 35.0;
            */
            //add new parameter for progeny destiny
            muProgDest = ODEModelParameters.muProgDest ;//2.5
            sigmaProgDest = ODEModelParameters.sigmaProgDest ;

            //new parameter for variation in progeny destiny 
            sigmaVarProgDest = ODEModelParameters.sigmaVarProgDest;
            
            nInitialCells = 40;
        }

        /*private void generateInitialRs()
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
            

        } */


        public double TDivision
        {
            get { return tDivision; }
        }
        public double TLastDivision
        {
            get { return tLastDivision; }
        }
        public double V0
        {
            get { return v0; }
        }
        /*public double TDivisionTotal
        {
            get { return tDivisionTotal; }
        }
        */
        public int ProgenyDestiny
        {
            get { return this.progenyDestiny; }
        }
    
        public int EffectiveProgenyDestiny
        {
            get { return this.effectiveProgenyDestiny; }
        }

        public DivisionDestiny DivDestiny
        {
            get { return this.divDestiny; }
        }

        //define the model parameters
        //for division
        private double gammaDivision;
        private double alphaDivsion;
        /// <summary>
        /// the parameter for differential equation model, the initial concentration of v protein controlling the division
        /// </summary>
        private double v0;

        private double tDivision;
        private double tLastDivision;
        //private double tDivisionTotal;

        private double nInitialCells;
        private double tFixed;
        
        private double muProgDest;
        private double sigmaProgDest;
        private double sigmaVarProgDest;
        
        private int progenyDestiny;
        
        private int effectiveProgenyDestiny;
        private DivisionDestiny divDestiny;
        
    }
    [Serializable]
    class ODEDeathMachine
    {
        /// <summary>
        /// Constructors for the founder cells 
        ///
        /// </summary>
        public ODEDeathMachine()
        {
            initializeValues();

            tDeath=calculateDeathTime ();
            tLastDivision = 0;
            
        }
        /// <summary>
        /// contructors for the daughter cells
        /// </summary>
        /// <param name="_tLastDivision">the relative time of last division, since the birth of the mother</param>
        /// <param name="m0Last">the m (protein controlling the mortality) concentration at the begin of this daughter cell</param>
        /// <param name="_tLastDivisionTotal">the total last division time, is the absolute time for the birth of the this daughter cell since the system started</param>
        public ODEDeathMachine(double _tLastDivision, double m0Last, double _tLastDivisionTotal)
        {
            initializeValues();
            m0 = 0.5*(m0Last + epsilonDeath * _tLastDivision);
            tDeath=calculateDeathTime();
            tLastDivision =  _tLastDivisionTotal;
        }
        //user to divide as a new object with reusing the memory.
        /// <summary>
        /// the function to reassign the values of the current object and reuse the object for the daughter cell at the division
        /// </summary>
        public void Regenerate(double _tLastDivision)
        {
            m0 = 0.5 * (m0 + epsilonDeath * _tLastDivision);
            tDeath=calculateDeathTime();
            tLastDivision += _tLastDivision;
        }


        private void initializeValues()
        {
            //for death parameters
            epsilonDeath = ODEModelParameters.epsilonDeath ;//3.623091;
            betaDeath = ODEModelParameters.betaDeath;

            m0 = ODEModelParameters.m0;
            tLastDivision = 0;

        }

        private double calculateDeathTime()
        {
            double fu =  ODEModelRandomGenerator.deathGenerateor.GetRandomValue(ODEModelRandomGenerator.deathRng);

            return Math.Sqrt(m0 * m0 / (epsilonDeath * epsilonDeath) - 2 / (epsilonDeath * betaDeath ) * Math.Log(1 - fu)) - m0 / epsilonDeath;
        }

        /*private void generateInitialDeathTime()
        {
            tDeath = TwoFactorModelRandomGenerators.tDthNormGenerator.GetRandomValue(TwoFactorModelRandomGenerators.tDthRng);
            tDeath = tDeath * sigmaDeathTm + muDeathTm;
            tDeath = Math.Exp(tDeath);
        }*/

        public double TDeath
        {
            get { return tDeath; }
        }
        public double TLastDivision
        {
            get { return tLastDivision ; }
        }
        public double M0
        {
            get { return m0 ; }
        }
        //for death
        private double epsilonDeath;
        private double betaDeath;
        private double m0;

        private double tDeath;
        private double tLastDivision;
        
    }

    [Serializable]
    
    ///

    public static class ODEModelParameters
    {
        //this is used to reset the parameters for doing the MCMC only
        /// <summary>
        /// static method for resetParameters for all the values
        /// </summary>
        /// <param name="_alphaDivsion"></param>
        /// <param name="_gammaDivision"></param>
        /// <param name="_epsilonDeath"></param>
        /// <param name="_betaDeath"></param>
        /// <param name="_m0"></param>
        /// <param name="_v0"></param>
        public static void resetParameters(double _alphaDivsion, double _gammaDivision, 
                                double _epsilonDeath, double _betaDeath,
                                double _lamda, double _q,
                                double _lamdaEffective, double _qEffective,
                                double _m0=0, double _v0 = 0.000)
        {
            gammaDivision =_gammaDivision;
            alphaDivsion = _alphaDivsion;
            v0=_v0;

            epsilonDeath=_epsilonDeath;
            betaDeath=_betaDeath;
            m0=_m0;

            lamda = _lamda;
            q = _q;

            lamdaEffective = _lamdaEffective;
            qEffective = _qEffective;

        }

        //for division
        public static double gammaDivision=0.08;
        public static double alphaDivsion=0.1;
        public static double v0=0;

        public static double nInitialCells=40;
        public static double tFixed=0;
        
        //for mortality
        public static double epsilonDeath=0.06007;
        public static double betaDeath=0.08;
        public static double m0=0.0;

        public static double muProgDest = 2.5;
        public static double sigmaProgDest=1.5;
        public static double sigmaVarProgDest =0.8364;
        
        //public static int progenyDestiny;
        public static double lamda = 4.40;
        public static double q = 0.65;
        public static double lamdaEffective = 1.5;
        public static double qEffective = 0.3;
        //public static int effectiveProgenyDestiny;
        
    }
}
