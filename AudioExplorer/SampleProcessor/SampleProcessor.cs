using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace AudioExplorer.SampleProcessor
{
    /// <summary>
    /// Takes input from one or more iReadableAudioSources and performs operations on them
    /// before producing a single audio outputs. The base class for filters, mixing, etc.
    /// </summary>
    public abstract class SampleProcessor : ISampleSource
    {

        protected readonly WaveFormat _waveFormat;

        public SampleProcessor(WaveFormat waveFormat)
        {
            _waveFormat = waveFormat;
        }

        public virtual int Read(float[] buffer, int offset, int count)
        {
            for (int i = offset; i < count; i++)
            {
                buffer[i] = 0;
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
