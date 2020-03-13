using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace AudioExplorer.SampleSource
{
    /// <summary>
    /// Generates a sawtooth wave.
    /// </summary>
    public class WaveGenerator : ISampleSource
    {
        public enum WaveType
        {
            SineWave,
            SquareWave,
            TriangleWave,
            SawtoothWave,
            InverseSawtoothWave,
        }

        /// <summary>
        /// Gets or sets the frequency of the wave.
        /// </summary>
        public double Frequency
        {
            get { return _frequency; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");
                _frequency = value;
            }
        }

        /// <summary>
        /// Gets or sets the amplitude of the wave.
        /// </summary>
        public double Amplitude
        {
            get { return _amplitude; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException("value");
                _amplitude = value;
            }
        }

        /// <summary>
        /// Gets or sets the phase of the wave.
        /// </summary>
        public double Phase { get; set; }
        public WaveType waveform { get; set; }


        private readonly WaveFormat _waveFormat;
        private double _frequency;
        private double _amplitude;

        /// <summary>
        /// 1000Hz, 0.5 amplitude, 0.0 phase
        /// </summary>
        public WaveGenerator()
            : this(WaveType.SineWave, 1000, 0.5, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriangleGenerator"/> class.
        /// </summary>
        /// <param name="frequency">Specifies the frequency of the wave in Hz.</param>
        /// <param name="amplitude">Specifies the amplitude of the wave. Use a value between 0 and 1.</param>
        /// <param name="phase">Specifies the initial phase. Use a value between 0 and 1.</param>
        public WaveGenerator(WaveType wavetype, double frequency, double amplitude, double phase)
        {
            if (frequency <= 0)
                throw new ArgumentOutOfRangeException("frequency");
            if (amplitude < 0 || amplitude > 1)
                throw new ArgumentOutOfRangeException("amplitude");

            Frequency = frequency;
            Amplitude = amplitude;
            Phase = phase;
            waveform = wavetype;

            _waveFormat = new WaveFormat(44100, 32, 1, AudioEncoding.IeeeFloat);
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
            if (Phase > 1)
                Phase = 0;

            double phaseinc = (1.0 / WaveFormat.SampleRate);
            float t = 0;
            float sine = 0;
            for (int i = offset; i < count; i++)
            {
                switch(this.waveform)
                {
                    case WaveType.SineWave:
                        sine = (float)(Amplitude * Math.Sin(Frequency * Phase * Math.PI * 2));
                        buffer[i] = sine;
                        break;
                    case WaveType.SquareWave:
                        t = (float)((Phase * Frequency) % 1.0);
                        if (t < 0.5)
                        {
                            buffer[i] = (float)-Amplitude;
                        }
                        else
                        {
                            buffer[i] = (float)(Amplitude);
                        }
                        break;
                    case WaveType.TriangleWave:
                        t = (float)((Phase * Frequency) % 1.0);
                        if (t <= 0.5)
                        {
                            buffer[i] = (float)(-Amplitude + (4 * Amplitude * t));
                        }
                        else
                        {
                            buffer[i] = (float)(Amplitude - (4 * Amplitude * (t - 0.5)));
                        }
                        break;
                    case WaveType.SawtoothWave:
                        t = (float)((Phase * Frequency) % 1.0);
                        buffer[i] = (float)(-Amplitude + (2 * Amplitude * t));
                        break;
                    case WaveType.InverseSawtoothWave:
                        t = (float)((Phase * Frequency) % 1.0);
                        buffer[i] = (float)(Amplitude - (2 * Amplitude * t));
                        break;
                    default:
                        buffer[i] = 0;
                        break;

                }
               
                Phase += phaseinc;
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
