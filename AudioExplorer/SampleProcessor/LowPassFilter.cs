using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace AudioExplorer.SampleProcessor
{
    class LowPassFilter : BiQuadFilter
    {
        public LowPassFilter(WaveFormat waveFormat, double frequency) : base(waveFormat, frequency)
        {

        }

        protected override void ComputeCoefficients()
        {
            double k = Math.Tan(Math.PI * Frequency / WaveFormat.SampleRate);
            double norm = 1 / (1 + (k / Q) + (k * k));
            A0 = k * k * norm;
            A1 = 2 * A0;
            A2 = A0;
            B1 = 2 * ((k * k) - 1) * norm;
            B2 = (1 - (k / Q) + (k * k)) * norm;
        }
    }
}
