using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioExplorer.Scalar
{
    class Oscillator : Scalar
    {

        public enum WaveType
        {
            SineWave,
            SquareWave,
            TriangleWave,
            SawtoothWave,
            InverseSawtoothWave,
            PulseWaveHalf,
            PulseWaveQuarter,
        }


        /// <summary>
        /// Gets or sets the frequency of the oscillator.
        /// </summary>
        public Scalar Frequency
        {
            get { return _frequency; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _frequency = value;
            }
        }

        /// <summary>
        /// Gets or sets the amplitude of the oscillator.
        /// </summary>
        public Scalar Amplitude
        {
            get { return _amplitude; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _amplitude = value;
            }
        }

        /// <summary>
        /// Gets or sets the starting phase of the oscillator.
        /// </summary>
        public Scalar PhaseOffset
        {
            get { return _phaseoffset; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _phaseoffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the offset from zero of the oscillator wave.
        /// </summary>
        public Scalar Origin
        {
            get { return _origin; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _origin = value;
            }
        }

        /// <summary>
        /// Gets or sets the sample rate of the oscillator. THis value should probably not be modified after initialization
        /// although it shouldn't break anything directly.
        /// </summary>
        public int SampleRate
        {
            get { return _samplerate; }
            set
            {
                if (value <= 0 )
                    throw new ArgumentOutOfRangeException("value");
                _samplerate = value;
            }
        }

        /// <summary>
        /// Gets or sets the wave type of the wave generator
        /// </summary>
        public WaveType waveform { get; set; }

        
        private Scalar _frequency;
        private Scalar _amplitude;
        private Scalar _origin;
        private Scalar _phaseoffset;
        private float phase;
        private int _samplerate;

        public Oscillator()
            : this(WaveType.SineWave, 44100, new ConstantScalar(1000), new ConstantScalar(0.5f), new ConstantScalar(0), new ConstantScalar(0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Oscillator"/> class.
        /// </summary>
        /// <param name="wavetype">Specifies the type of waveform generated.</param>
        /// <param name="samplerate">Specifies the sample rate of the oscillator. Use a value greater than 0.</param>
        /// <param name="frequency">Specifies the frequency of the oscillator in Hz. Use a value greater than 0.</param>
        /// <param name="amplitude">Specifies the amplitude of the oscillator. Use a value between 0 and 1.</param>
        /// <param name="phase">Specifies the initial phase. Use a value between 0 and 1.</param>
        /// <param name="origin">Specifies an offset from zero on the amplitude axis. Use a value between -1 and 1. 
        /// Note that computed values will be clamped to [-1,1] if the amplitude of the resulting waveform added to the offset exceeds those bounds</param>
        public Oscillator(WaveType wavetype, int samplerate, Scalar frequency, Scalar amplitude, Scalar phase, Scalar origin)
        {
            SampleRate = samplerate;
            Frequency = frequency;
            Amplitude = amplitude;
            PhaseOffset = phase;
            Origin = origin;
            waveform = wavetype;
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            //if (phase > 1.0f)
            //{
            //    //Console.WriteLine("Phase reset {0}", phase);
            //    phase = phase % 1.0f;
            //}

            float phaseinc = (1.0f / _samplerate);
            float phaseinc2 = 0;
            float t = 0;
            float sine = 0;
            float[] frequencysamples = new float[buffer.Length];
            float[] amplitudesamples = new float[buffer.Length];
            float[] phaseoffsetsamples = new float[buffer.Length];
            float[] originsamples = new float[buffer.Length];
            Frequency.Read(frequencysamples, offset, count);
            Amplitude.Read(amplitudesamples, offset, count);
            PhaseOffset.Read(phaseoffsetsamples, offset, count);
            Origin.Read(originsamples, offset, count);
            float freq = 0;
            float amp = 0;
            float offsetphase = 0;
            float origin = 0;

            float value = 0;
            float twopi = (float)Math.PI * 2;

            for (int i = offset; i < count; i++)
            {
                freq = frequencysamples[i];
                amp = amplitudesamples[i];
                //if (phase > freq)
                //{
                //    phase = phase % (1.0f / freq);
                //}
                
                phaseinc2 = (1.0f / (_samplerate)) * freq;

                offsetphase = (phase + phaseoffsetsamples[i]);
                
                origin = originsamples[i];
               
                switch (this.waveform)
                {
                    case WaveType.SineWave:
                        value = (float)(Math.Sin(offsetphase * twopi));
                        break;
                    case WaveType.SquareWave:
                        t = (float)((offsetphase));
                        if (t < 0.5)
                        {
                            value = -1.0f;
                        }
                        else
                        {
                            value = 1.0f;
                        }
                        break;
                    case WaveType.TriangleWave:
                        t = (float)((offsetphase));
                        if (t <= 0.5)
                        {
                            value = 1.0f + (4  * t);
                        }
                        else
                        {
                            value = 1.0f - (4  * (t - 0.5f));
                        }

                        break;
                    case WaveType.SawtoothWave:
                        t = (float)((offsetphase));
                        value = -1.0f + (2 * t);
                        break;
                    case WaveType.InverseSawtoothWave:
                        t = (float)((offsetphase));
                        value = 1.0f - (2 * t);
                        break;
                    case WaveType.PulseWaveHalf:
                        t = (float)((offsetphase));
                        if (t < 0.25)
                        {
                            value = -1.0f;
                        }
                        else
                        {
                            value = 1.0f;
                        }
                        break;
                    case WaveType.PulseWaveQuarter:
                        t = (float)((offsetphase));
                        if (t < 0.125)
                        {
                            value = -1.0f;
                        }
                        else
                        {
                            value = 1.0f;
                        }
                        break;
                    default:
                        value = 0;
                        break;

                }
                buffer[i] = origin + (amp * value);
                phase = (phase + phaseinc2) % 1.0f;
                
            }

            return count;
        }
    }
}
