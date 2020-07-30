using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioExplorer.LFO
{
    class WaveLFO : LFO
    {

        public enum WaveLFOType
        {
            ConstantLFOWave,
            SineLFOWave,
            SquareLFOWave,
            TriangleLFOWave,
            SawtoothLFOWave,
            InverseSawtoothLFOWave,
            PulseLFOWaveHalf,
            PulseLFOWaveQuarter,
        }


        /// <summary>
        /// Gets or sets the frequency of the oscillator.
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
        /// Gets or sets the amplitude of the oscillator.
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
        /// Gets or sets the phase of the oscillator.
        /// </summary>
        public double Phase { get; set; }

        /// <summary>
        /// Gets or sets the offset from zero of the oscillator wave.
        /// </summary>
        public double Constant
        {
            get { return _constant; }
            set
            {
                if (value < -1 || value > 1)
                    throw new ArgumentOutOfRangeException("value");
                _constant = value;
            }
        }

        /// <summary>
        /// Gets or sets the sample rate of the oscillator
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
        public WaveLFOType waveform { get; set; }

        
        private double _frequency;
        private double _amplitude;
        private double _constant;
        private int _samplerate;

        public WaveLFO()
            : this(WaveLFOType.SineLFOWave, 44100, 1000, 0.5, 0, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveLFO"/> class.
        /// </summary>
        /// <param name="wavetype">Specifies the type of waveform generated.</param>
        /// <param name="samplerate">Specifies the sample rate of the oscillator. Use a value greater than 0.</param>
        /// <param name="frequency">Specifies the frequency of the oscillator in Hz. Use a value greater than 0.</param>
        /// <param name="amplitude">Specifies the amplitude of the oscillator. Use a value between 0 and 1.</param>
        /// <param name="phase">Specifies the initial phase. Use a value between 0 and 1.</param>
        /// <param name="constant">Specifies a constant offset from zero. Use a value between -1 and 1.</param>
        public WaveLFO(WaveLFOType wavetype, int samplerate, double frequency, double amplitude, double phase, double constant)
        {
            SampleRate = samplerate;
            Frequency = frequency;
            Amplitude = amplitude;
            Phase = phase;
            Constant = constant;
            waveform = wavetype;
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            if (Phase > 1)
                Phase = 0;

            double phaseinc = (1.0 / _samplerate);
            float t = 0;
            float sine = 0;
            for (int i = offset; i < count; i++)
            {
                switch (this.waveform)
                {
                    case WaveLFOType.ConstantLFOWave:
                        buffer[i] = (float)_constant;
                        break;
                    case WaveLFOType.SineLFOWave:
                        sine = (float)(Amplitude * Math.Sin(Frequency * Phase * Math.PI * 2));
                        buffer[i] = LFOClamp(sine + (float)_constant);
                        break;
                    case WaveLFOType.SquareLFOWave:
                        t = (float)((Phase * Frequency) % 1.0);
                        if (t < 0.5)
                        {
                            buffer[i] = LFOClamp((float)(_constant - Amplitude));
                        }
                        else
                        {
                            buffer[i] = LFOClamp((float)(_constant + Amplitude));
                        }
                        break;
                    case WaveLFOType.TriangleLFOWave:
                        t = (float)((Phase * Frequency) % 1.0);
                        if (t <= 0.5)
                        {
                            buffer[i] = LFOClamp((float)(_constant - Amplitude + (4 * Amplitude * t)));
                        }
                        else
                        {
                            buffer[i] = LFOClamp((float)(_constant + Amplitude - (4 * Amplitude * (t - 0.5))));
                        }
                        break;
                    case WaveLFOType.SawtoothLFOWave:
                        t = (float)((Phase * Frequency) % 1.0);
                        buffer[i] = LFOClamp((float)(_constant - Amplitude + (2 * Amplitude * t)));
                        break;
                    case WaveLFOType.InverseSawtoothLFOWave:
                        t = (float)((Phase * Frequency) % 1.0);
                        buffer[i] = LFOClamp((float)(_constant + Amplitude - (2 * Amplitude * t)));
                        break;
                    case WaveLFOType.PulseLFOWaveHalf:
                        t = (float)((Phase * Frequency) % 1.0);
                        if (t < 0.25)
                        {
                            buffer[i] = LFOClamp((float)(_constant - Amplitude));
                        }
                        else
                        {
                            buffer[i] = LFOClamp((float)(_constant + Amplitude));
                        }
                        break;
                    case WaveLFOType.PulseLFOWaveQuarter:
                        t = (float)((Phase * Frequency) % 1.0);
                        if (t < 0.125)
                        {
                            buffer[i] = LFOClamp((float)(_constant - Amplitude));
                        }
                        else
                        {
                            buffer[i] = LFOClamp((float)(_constant + Amplitude));
                        }
                        break;
                    default:
                        buffer[i] = 0;
                        break;

                }

                Phase += phaseinc;
            }

            return count;
        }
    }
}
