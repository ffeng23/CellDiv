using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meta.Numerics.Statistics ;
using Meta.Numerics.Statistics.Distributions;


namespace CellDivCyt
{
    class CytonModel
    {
        public static void buildCytonModel(double divMean, double divVar, double deathMean, double deathVar)
        {
            firstDivDist = new NormalDistribution(Math.Log(divMean),divVar);
            secondDivDist = new NormalDistribution(Math.Log(divMean), divVar);

            firstDeathDist = new NormalDistribution(Math.Log(deathMean), deathVar);
            secondDeathDist = new NormalDistribution(Math.Log(deathMean), deathVar);
        }

        public static void buildCytonModel(double divMean, double divVar, double deathMean, double deathVar,
                          double divMean2, double divVar2, double deathMean2, double deathVar2)
        {
            firstDivDist = new NormalDistribution(Math.Log(divMean), divVar);
            secondDivDist = new NormalDistribution(Math.Log(divMean2), divVar2);

            firstDeathDist = new NormalDistribution(Math.Log(deathMean), deathVar);
            secondDeathDist = new NormalDistribution(Math.Log(deathMean2), deathVar2);
        }

        public static  NormalDistribution FirstDivDist
        {
            get { return firstDivDist; }
        }
        public static NormalDistribution SecondDivDist
        {
            get { return secondDivDist; }
        }

        public static  NormalDistribution FirstDeathDist
        {
            get { return firstDeathDist; }
        }
        public static NormalDistribution SecondDeathDist
        {
            get { return secondDeathDist; }
        }

        public static Random rng=new Random();
        private static NormalDistribution firstDivDist;
        private static NormalDistribution secondDivDist;
        private static NormalDistribution firstDeathDist;
        private static NormalDistribution secondDeathDist;

    }

    class DeathMachine
    {
        public DeathMachine(int divisionNumber = 0)
        {
            this._firstDeathD = CytonModel.FirstDeathDist;
            this._secondDeathD = CytonModel.SecondDeathDist;
            this._divisionNumber = divisionNumber;
            calDeathTime(divisionNumber);
        }
        public DeathMachine(NormalDistribution firstNDist, int divisionNumber = 0)
        {
            this._firstDeathD = firstNDist;
            this._secondDeathD = firstNDist;
            this._divisionNumber = divisionNumber;
            calDeathTime(divisionNumber);
        }
        public DeathMachine(NormalDistribution firstNDist, NormalDistribution secondNDist, int divisionNumber = 0)
        {
            this._firstDeathD = firstNDist;
            this._secondDeathD = secondNDist;
            this._divisionNumber = divisionNumber;
            calDeathTime(divisionNumber);
        }

        private void calDeathTime(int divisionNumber)
        {

            if (_divisionNumber < 1)
            {
                _deathTime = Math.Exp(_firstDeathD.GetRandomValue(CytonModel.rng));

            }
            else
            {
                _deathTime = Math.Exp(_secondDeathD.GetRandomValue(CytonModel.rng));
            }
        }

        public double DeathTime
        {
            get { return _deathTime; }
        }


        private double _deathTime;
        private int _divisionNumber;
        private NormalDistribution _firstDeathD;
        private NormalDistribution _secondDeathD;
    }

    class DivisionMachine
    {
        public DivisionMachine(int divisionNumber = 0)
        {
            this._firstDivD = CytonModel.FirstDivDist;
            this._secondDivD = CytonModel.SecondDivDist;

            this._divisionNumber = divisionNumber;
            calDivisionTime(divisionNumber);
        }
        public DivisionMachine(NormalDistribution firstNDist, int divisionNumber = 0)
        {
            this._firstDivD = firstNDist;
            this._secondDivD = firstNDist;
            this._divisionNumber = divisionNumber;
            calDivisionTime(divisionNumber);
        }
        public DivisionMachine(NormalDistribution firstNDist, NormalDistribution secondNDist, int divisionNumber = 0)
        {
            this._firstDivD = firstNDist;
            this._secondDivD = secondNDist;
            this._divisionNumber = divisionNumber;
            calDivisionTime(divisionNumber);
        }

        private void calDivisionTime(int divisionNumber)
        {

            if (_divisionNumber < 1)
            {
                _divTime = Math.Exp(_firstDivD.GetRandomValue(CytonModel.rng));

            }
            else
            {
                _divTime = Math.Exp(_secondDivD.GetRandomValue(CytonModel.rng));
            }
        }

        public double DivTime
        {
            get { return _divTime; }
        }


        private double _divTime;
        private int _divisionNumber;
        private NormalDistribution _firstDivD;
        private NormalDistribution _secondDivD;

    }

    class Cell
    {
        public Cell(int divNumber = 0, double lastDivisionTime = 0)
        {
            //empty
            //_cm = cm;

            _divNumber = divNumber;

            _dthm = new DeathMachine(divNumber);
            _divm = new DivisionMachine(divNumber);
            _deathTime = _dthm.DeathTime;
            _divTime = _divm.DivTime;
            _lastDivisionTime = lastDivisionTime;
        }

        public Cell(double divMean, double divVar, double deathMean, double deathVar,
                          double divMean2, double divVar2, double deathMean2, double deathVar2, int divNumber = 0)
        {
            //_cm = new CytonModel();
            CytonModel.buildCytonModel(divMean, divVar, deathMean, deathVar,
                          divMean2, divVar2, deathMean2, deathVar2);

            _divNumber = divNumber;

            _dthm = new DeathMachine(divNumber);
            _divm = new DivisionMachine(divNumber);
            _deathTime = _dthm.DeathTime;
            _divTime = _divm.DivTime;
            _lastDivisionTime = 0;
        }

        public double LastDivisionTime
        {
            get { return _lastDivisionTime; }
            set { _lastDivisionTime = value; }
        }

        public double DeathTime
        {
            get { return _deathTime + _lastDivisionTime; }
        }
        public double DivisionTime
        {
            get { return _divTime + _lastDivisionTime; }
        }
        public int DivisionNumber
        {
            get { return _divNumber; }
            set { _divNumber = value; }
        }

        //private CytonModel _cm;
        private int _divNumber;

        private DeathMachine _dthm;
        private DivisionMachine _divm;

        private double _deathTime;
        private double _divTime;
        private double _lastDivisionTime;
    }
}
