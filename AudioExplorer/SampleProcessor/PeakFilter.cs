using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace AudioExplorer.SampleProcessor
{
    class PeakFilter : BiQuadFilter
    {
        public PeakFilter(WaveFormat waveFormat, Scalar.Scalar frequency, ISampleSource source) : base(waveFormat, frequency, source)
        {

        }

        public PeakFilter(WaveFormat waveFormat, Scalar.Scalar frequency, Scalar.Scalar q, Scalar.Scalar gain, ISampleSource source) : base(waveFormat, frequency, q, gain, source)
        {

        }

        protected override void ComputeCoefficients()
        {
            double k = Math.Tan(Math.PI * _curr_frequency / WaveFormat.SampleRate);
            double v = Math.Pow(10, Math.Abs(_curr_gain) / 20);
            double norm;
            if(_curr_gain >= 0) // boost
            {
                norm = 1 / (1 + (k / _curr_q) + (k * k));
                A0 = (1 + ((v * k) / _curr_q) + (k * k)) * norm;
                A1 = 2 * ((k * k) - 1) * norm;
                A2 = (1 - ((v * k) / _curr_q) + (k * k)) * norm;
                B1 = A1;
                B2 = (1 - (k / _curr_q) + (k * k)) * norm;
            }
            else // cut
            {
                norm = 1 / (1 + (v / _curr_q) + (k * k));
                A0 = (1 + ((v * k) / _curr_q) + (k * k)) * norm;
                A1 = 2 * ((k * k) - 1) * norm;
                A2 = (1 - (k / _curr_q) + (k * k)) * norm;
                B1 = A1;
                B2 = (1 - ((v * k) / _curr_q) + (k * k)) * norm;
            }
        }
    }
}
