using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace AudioExplorer.SampleSource
{
    class NoiseGenerator : ISampleSource
    {

        public enum NoiseType
        {
            WhiteNoise,
        }

        private readonly WaveFormat _waveFormat;
        private Random _random;

        public NoiseType noisetype { get; set; }

        public NoiseGenerator(WaveFormat waveformat, NoiseType type)
        {
            _waveFormat = waveformat;
            noisetype = type;
            _random = new Random();
        }

        /// <summary>
        ///     Reads a sequence of samples from the <see cref="WaveGenerator" />.
        /// </summary>
        /// <param name="buffer">
        ///     An array of floats. When this method returns, the <paramref name="buffer" /> contains the specified
        ///     float array with the values between <paramref name="offset" /> and (<paramref name="offset" /> +
        ///     <paramref name="count" /> - 1) replaced by the floats read from the current source.
        /// </param>
        /// <param name="offset">
        ///     The zero-based offset in the <paramref name="buffer" /> at which to begin storing the data
        ///     read from the current stream.
        /// </param>
        /// <param name="count">The maximum number of samples to read from the current source.</param>
        /// <returns>The total number of samples read into the buffer.</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            double rand = 0;
            for(int i = offset; i < count; i++)
            {
                rand = _random.NextDouble();
                switch(this.noisetype)
                {
                    case NoiseType.WhiteNoise:
                        buffer[i] = (float)rand * 0.25f - 0.125f;
                        break;
                    default:
                        buffer[i] = 0;
                        break;
                }
            }
            return count;
        }

        /// <summary>
        ///     Gets the <see cref="IAudioSource.WaveFormat" /> of the waveform-audio data.
        /// </summary>
        public WaveFormat WaveFormat
        {
            get { return _waveFormat; }
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        public long Position
        {
            get { return 0; }
            set { throw new InvalidOperationException(); }
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        public long Length
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IAudioSource"/> supports seeking.
        /// </summary>
        public bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Not used.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
