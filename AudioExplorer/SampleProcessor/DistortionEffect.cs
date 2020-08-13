using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace AudioExplorer.SampleProcessor
{
    class DistortionEffect : SampleProcessor
    {
        private IReadableAudioSource<float> _source;
        private Scalar.Scalar _gain;
        private Scalar.Scalar _cutoff;


        private IReadableAudioSource<float> Source {
            get { return _source; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _source = value;
            }
        }

        private Scalar.Scalar Gain
        {
            get { return _gain; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _gain = value;
            }
        }

        private Scalar.Scalar Cutoff
        {
            get { return _cutoff; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _cutoff = value;
            }
        }

        public DistortionEffect(WaveFormat waveFormat, Scalar.Scalar gain, Scalar.Scalar cutoff, IReadableAudioSource<float> source) : base(waveFormat)
        {
            Source = source;
            Gain = gain;
            Cutoff = cutoff;
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            float[] input = new float[buffer.Length];
            float[] gain = new float[buffer.Length];
            float[] cutoff = new float[buffer.Length];
            Source.Read(input, offset, count);
            Gain.Read(gain, offset, count);
            Cutoff.Read(cutoff, offset, count);
            for (int i = offset; i < count; i++)
            {
                // basic hard clipping
                float x = input[i] * gain[i];
                float cut = Math.Abs(cutoff[i]);
                if (x < -cut)
                {
                    buffer[i] = -cut;
                }
                else if (x > cut)
                {
                    buffer[i] = cut;
                }
                else
                {
                    buffer[i] = x;
                }
                
            }
            return count;
        }
    }
}
