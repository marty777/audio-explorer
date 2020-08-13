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

        private IReadableAudioSource<float> Source {
            get { return _source; }
            set
             {
                if (value == null)
                    throw new ArgumentNullException("value");
                _source = value;
            }
        }

        public DistortionEffect(WaveFormat waveFormat, IReadableAudioSource<float> source) : base(waveFormat)
        {
            Source = source;
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            float[] input = new float[buffer.Length];
            Source.Read(input, offset, count);
            
            for (int i = offset; i < count; i++)
            {
                float x = input[i];
                if (x < -1)
                {
                    buffer[i] = -1;
                }
                else if (x > 1)
                {
                    buffer[i] = 1;
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
