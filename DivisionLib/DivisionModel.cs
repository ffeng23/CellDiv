using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;



namespace DivisionLib
{
    /// <summary>
    /// the interface of the division model. 
    /// </summary>
    /// 
    [Serializable] 
    public abstract class DivisionModel
    {
        /// <summary>
        /// Constructor, this is the one used to build the founder cells.
        /// </summary>
        /// <param name="signal">the input used to indicate the activation level. It has values between 0 and 1. 
        /// For now, only takes 1 as activated with CpG and the cell is undergoing division. Will do more for later. </param>
        public DivisionModel(float signal=0){}

        /// <summary>
        /// Constructor, this is the one used to build all the daughter cells.  
        /// </summary>
        /// <param name="_m">the Division model/machine from its mother cell. </param>
        public DivisionModel(DivisionModel _m){}
        
        //protected abstract void decideFate();

        /// <summary>
        /// the function to run one step of the model to evaluate the whether the cell to divide/die/keep running
        /// </summary>
        /// <param name="time">the time step to run at</param>
        ///
        /// <returns>an integer to indicate wheter the cell to divide (1), die(-1) or keep going(0)</returns>
        public abstract int Step(double time);

        /// <summary>
        /// the method used to reset the values of current model for reusing the current object for the daughter cell.
        /// </summary>
        public abstract void Regenerate();

        /// <summary>
        /// Property to return time of the last division
        /// </summary>
        public double TLastDivision
        {
            get { return tLastDivision; }
        }
        /// <summary>
        /// Property to return division number of current model/cell
        /// </summary>
        public int DivisionNum
        {
            get { return divisionNum; }
        }
        /// <summary>
        /// Property to return the division time of the current model
        /// </summary>
        public double TDivision
        {
            get { return tDivision; }
        }
        /// <summary>
        /// Property to return the death time of the current model
        /// </summary>
        public double TDeath
        {
            get { return tDeath; }
        }
        /// <summary>
        /// Property to return the fate of the current cell, true for division and false for death. 
        /// </summary>
        public bool Fate
        {
            get { return fate; }
        }

        /// <summary>
        /// division number
        /// </summary>
        protected int divisionNum;
        /// <summary>
        /// time of the last division
        /// </summary>
        protected double tLastDivision;
        /// <summary>
        /// time to division
        /// </summary>
        protected double tDivision;
        /// <summary>
        /// time to death
        /// </summary>
        protected double tDeath;
        /// <summary>
        /// fate of the current cell
        /// </summary>
        protected bool fate;
    }
}
