using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DivisionLib;
namespace CellDiv
{
    public class Cell
    {
        /// <summary>
        /// constructor for founder cells
        /// 
        /// </summary>
        public Cell()
        {
            divModel = new ODEModel();


        }

        /// <summary>
        /// constructor for daughter cells
        /// </summary>
       ///<param name="m" >divisionModel from mother cell </param>

        public Cell(DivisionModel m)
        {
            divModel = new ODEModel(m); 
            
        }

        public double LastDivisionTime
        {
            get { return divModel.TLastDivision ; }
            //set { _lastDivisionTime = value; }
        }
        public int Step(double time)
        {
            return divModel.Step(time);
            //return 0;
        }
        public void Divide(out List<Cell> c)
        {
            c = new List<Cell>(2);
            c.Add( new Cell(divModel));
            //this.Regenerate();
            //c.Add(this);
            c.Add(new Cell(divModel));
        }
        public void Regenerate()
        {
            this.divModel.Regenerate();
        }
        public double DeathTime
        {
            get { return divModel.TDeath; }
        }
        public double DivisionTime
        {
            get { return divModel.TDivision; }
        }
        public int DivisionNumber
        {
            get { return divModel.DivisionNum ; }
            //set { _divNumber = value; }
        }

        public bool Fate
        {
            get {return divModel.Fate;}
        }

        private DivisionModel  divModel;
        //private int _divNumber;

        //private double _deathTime;
        //private double _divTime;
    }
}
