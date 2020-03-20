using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace AudioExplorer.Modulation
{
    class ADSRModulator : Modulator
    {
        WaveFormat _waveFormat;

        public ADSRModulator(WaveFormat waveFormat)
        {
            _waveFormat = waveFormat;
        }

        public int Read(float[] buffer, int offset, int count) {
            return 0;
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
