using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using AudioExplorer.Scalar;

namespace AudioExplorer.SampleProcessor
{
    class ScalarPassthrough : SampleProcessor
    {
        private Scalar.Scalar scalar { get; set; }

        public ScalarPassthrough(WaveFormat waveformat, Scalar.Scalar scalar) : base(waveformat)
        {
            this.scalar = scalar;
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            scalar.Read(buffer, offset, count);
            return count;
        }
    }
}
