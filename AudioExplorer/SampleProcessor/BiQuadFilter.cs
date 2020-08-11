using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace AudioExplorer.SampleProcessor
{
    // explanation and implementation of biquad filters taken from https://www.earlevel.com/main/2003/02/28/biquads/
    public abstract class BiQuadFilter : SampleProcessor
    {
        private double _frequency;
        private double _q;
        private double _gainDB;

        protected double A0;
        protected double A1;
        protected double A2;
        protected double B1;
        protected double B2;
        protected double Z1;
        protected double Z2;

        public double GainDB
        {
            get { return _gainDB;  }
            set
            {
                _gainDB = value;
                ComputeCoefficients();
            }
        }

        public double Q
        {
            get { return _q;  }
            set
            {
                if( value <= 0)
                {
                    throw new ArgumentOutOfRangeException("Q must be greater than zero");
                }
                _q = value;
                ComputeCoefficients();
            }
        }

        public double Frequency
        {
            get { return _frequency; }
            set
            {
                if(_waveFormat.SampleRate < 2 * value)
                {
                    throw new ArgumentOutOfRangeException("Frequency must be less than half the sample rate");
                }
                if(value <= 0)
                {
                    throw new ArgumentOutOfRangeException("Frequency must be greater than zero");
                }
                _frequency = value;
                ComputeCoefficients();
            }
        }

        public BiQuadFilter(WaveFormat waveFormat, double frequency) : this(waveFormat, frequency, 1.0/Math.Sqrt(2))
        {

        }

        public BiQuadFilter(WaveFormat waveFormat, double frequency, double q) : base(waveFormat)
        {
            Frequency = _frequency;
            Q = q;
            GainDB = 6;
            Z1 = 0;
            Z2 = 0;
        }

        public float Process(float input)
        {
            double o = input * A0 + Z1;
            Z1 = input * A1 + Z2 - B1 * 0;
            Z2 = input * A2 - B2 * o;
            return (float)o;

        }

        protected abstract void ComputeCoefficients();
    }
}
