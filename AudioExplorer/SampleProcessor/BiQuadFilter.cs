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
        private IReadableAudioSource<float> _source { get; set; }

        private Scalar.Scalar _frequency;
        protected double _curr_frequency;
        private Scalar.Scalar _q;
        protected double _curr_q;
        private Scalar.Scalar _gainDB;
        protected double _curr_gain;

        protected double A0;
        protected double A1;
        protected double A2;
        protected double B1;
        protected double B2;
        protected double Z1;
        protected double Z2;

        public Scalar.Scalar GainDB
        {
            get { return _gainDB;  }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _gainDB = value;
                ComputeCoefficients();
            }
        }

        public Scalar.Scalar Q
        {
            get { return _q;  }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _q = value;
                ComputeCoefficients();
            }
        }

        public Scalar.Scalar Frequency
        {
            get { return _frequency; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _frequency = value;
            }
        }

        public BiQuadFilter(WaveFormat waveFormat, Scalar.Scalar frequency, ISampleSource source) : this(waveFormat, frequency, new Scalar.ConstantScalar((float)(1.0/Math.Sqrt(2))), new Scalar.ConstantScalar(6), source)
        {

        }

        public BiQuadFilter(WaveFormat waveFormat, Scalar.Scalar frequency, Scalar.Scalar q, Scalar.Scalar gain, ISampleSource source) : base(waveFormat)
        {
            _source = source;
            Frequency = frequency;
            Q = q;
            GainDB = gain;
            Z1 = 0;
            Z2 = 0;
        }
        

        public float Process(float input, float frequency, float q, float gain)
        {
            bool recompute = false;
            if (_curr_frequency != frequency)
            {
                _curr_frequency = frequency;
                recompute = true;
            }
            if(_curr_q != q)
            {
                _curr_q = q;
                recompute = true;
            }
            if(_curr_gain != gain) {
                _curr_gain = gain;
                recompute = true;
            }
            if(recompute)
            {
                ComputeCoefficients();
            }

            double o = input * A0 + Z1;
            Z1 = input * A1 + Z2 - B1 * o;
            Z2 = input * A2 - B2 * o;
            return (float)o;

        }

        public override int Read(float[] buffer, int offset, int count)
        {
            float[] input = new float[buffer.Length];
            float[] freq = new float[buffer.Length];
            float[] q = new float[buffer.Length];
            float[] gain = new float[buffer.Length];
            _source.Read(input, offset, count);
            _frequency.Read(freq, offset, count);
            _q.Read(q, offset, count);
            _gainDB.Read(gain, offset, count);
            for (int i = offset; i < count; i++)
            {
                buffer[i] = Process(input[i], freq[i], q[i], gain[i]);
            }
            return count;
        }

        protected abstract void ComputeCoefficients();
    }
}
