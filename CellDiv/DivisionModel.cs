using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DivisionModel
{
    public abstract class DivisionModel
    {
        public DivisionModel(float signal=0){}

        public DivisionModel(DivisionModel _m){}
        
        protected abstract void decideFate();

        public abstract int Step(double time);

        public abstract void Regenerate();

        public double TLastDivision
        {
            get { return tLastDivision; }
        }
        public int DivisionNum
        {
            get { return divisionNum; }
        }
        public double TDivision
        {
            get { return tDivision; }
        }
        public double TDeath
        {
            get { return tDeath; }
        }
        public bool Fate
        {
            get { return fate; }
        }

        protected int divisionNum;
        protected double tLastDivision;
        protected double tDivision;
        protected double tDeath;
        protected bool fate;
    }
}
