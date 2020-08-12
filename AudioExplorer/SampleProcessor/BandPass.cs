using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace AudioExplorer.SampleProcessor
{
    class BandPassFilter : BiQuadFilter
    {
        public BandPassFilter(WaveFormat waveFormat, Scalar.Scalar frequency, ISampleSource source) : base(waveFormat, frequency, source)
        {

        }

        public BandPassFilter(WaveFormat waveFormat, Scalar.Scalar frequency, Scalar.Scalar q, Scalar.Scalar gain, ISampleSource source) : base(waveFormat, frequency, q, gain, source)
        {

        }

        protected override void ComputeCoefficients()
        {
            double k = Math.Tan(Math.PI * _curr_frequency / WaveFormat.SampleRate);
            double norm = 1 / (1 + (k / _curr_q) + (k * k));
            A0 = (k / _curr_q) * norm;
            A1 = 0;
            A2 = -A0;
            B1 = 2 * ((k * k) - 1) * norm;
            B2 = (1 - (k / _curr_q) + (k * k)) * norm;
        }
    }
}
