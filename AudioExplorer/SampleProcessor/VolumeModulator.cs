using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using AudioExplorer.Scalar;

namespace AudioExplorer.SampleProcessor
{
    class VolumeModulator : SampleProcessor
    {
        private Scalar.Scalar scalar { get; set; }
        private IReadableAudioSource<float> source { get; set; }

        public VolumeModulator(WaveFormat waveformat, IReadableAudioSource<float> source, Scalar.Scalar scalar) : base (waveformat)
        {
            this.scalar = scalar;
            this.source = source;
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            float[] sourceBuffer = new float[buffer.Length];
            float[] scalarBuffer = new float[buffer.Length];
            source.Read(sourceBuffer, offset, count);
            scalar.Read(scalarBuffer, offset, count);
            float amplitude;
            for (int i = offset; i < count; i++)
            {
                amplitude = (0.5f * scalarBuffer[i]) + 1; // map from [-1,1] to [0,1]
                buffer[i] = amplitude * sourceBuffer[i];
            }
            return count;
        }
    }
}
