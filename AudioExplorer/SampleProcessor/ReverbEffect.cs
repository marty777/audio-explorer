using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace AudioExplorer.SampleProcessor
{
    class ReverbEffect : SampleProcessor
    {
        private IReadableAudioSource<float> _source;
        private Scalar.Scalar _decay;
        private Scalar.Scalar _delay;
        private float[] _buffer;
        private int _buffer_index;

        private IReadableAudioSource<float> Source
        {
            get { return _source; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _source = value;
            }
        }

        private Scalar.Scalar Decay
        {
            get { return _decay; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _decay = value;
            }
        }

        private Scalar.Scalar Delay
        {
            get { return _delay; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _delay = value;
            }
        }

        public ReverbEffect(WaveFormat waveFormat, Scalar.Scalar decay, Scalar.Scalar delay, int max_buffer_len, IReadableAudioSource<float> source) : base(waveFormat)
        {
            Source = source;
            Decay = decay;
            Delay = delay;
            if (max_buffer_len < 0 || max_buffer_len > waveFormat.SampleRate)
            {
                throw new ArgumentOutOfRangeException("max_buffer_len must be greater than zero and cannot exceed the samples per second of the provided WaveFormat");
            }
            _buffer = new float[max_buffer_len];
            _buffer_index = 0;
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            float[] input = new float[buffer.Length];
            float[] decay = new float[buffer.Length];
            float[] delay = new float[buffer.Length];
            float[] cutoff = new float[buffer.Length];
            Source.Read(input, offset, count);
            Decay.Read(decay, offset, count);
            
            for (int i = offset; i < count; i++)
            {
                int sample_delay = (int)Math.Floor(Math.Min(Math.Max(delay[i], 0.0f), 1.0f) * _waveFormat.SampleRate);
                int delay_index = (_buffer_index - sample_delay) % _buffer.Length;
                float x = input[i] + (_buffer[delay_index] * decay[i]);
                buffer[i] = x;
                _buffer[_buffer_index] = x;
                _buffer_index = (_buffer_index + 1) % _buffer.Length;
            }
            return count;
        }
    }
}
