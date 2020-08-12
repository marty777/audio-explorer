using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace AudioExplorer.SampleProcessor
{
    class HighShelfFilter : BiQuadFilter
    {
        public HighShelfFilter(WaveFormat waveFormat, Scalar.Scalar frequency, ISampleSource source) : base(waveFormat, frequency, source)
        {

        }

        public HighShelfFilter(WaveFormat waveFormat, Scalar.Scalar frequency, Scalar.Scalar q, Scalar.Scalar gain, ISampleSource source) : base(waveFormat, frequency, q, gain, source)
        {

        }

        protected override void ComputeCoefficients()
        {
            double k = Math.Tan(Math.PI * _curr_frequency / WaveFormat.SampleRate);
            double v = Math.Pow(10, Math.Abs(_curr_gain) / 20);
            double sqrt2 = Math.Sqrt(2);
            double sqrt2v = Math.Sqrt(2 * v);
            double ksquared = k * k;
            double norm;
            if (_curr_gain >= 0) // boost
            {
                norm = 1 / (1 + (sqrt2 * k) + (ksquared));
                A0 = (v + (sqrt2v * k) + (ksquared)) * norm;
                A1 = 2 * ((ksquared) - v) * norm;
                A2 = (v - (sqrt2v * k) + (ksquared)) * norm;
                B1 = 2 * (ksquared - 1) * norm;
                B2 = (1 - (sqrt2 * k) + (ksquared)) * norm;
            }
            else // cut
            {
                norm = 1 / (v + (sqrt2v * k) + (ksquared));
                A0 = (1 + (sqrt2 * k) + (ksquared)) * norm;
                A1 = 2 * ((ksquared) - 1) * norm;
                A2 = (1 - (sqrt2 * k) + (ksquared)) * norm;
                B1 = 2 * ((ksquared) - v) * norm;
                B2 = (v - (sqrt2v * k) + (ksquared)) * norm;
            }
        }
    }
}
