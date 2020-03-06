using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using CSCore.Codecs;
using CSCore.DSP;
using CSCore.SoundOut;
using CSCore.Streams;
using System.Diagnostics;
using AudioExplorer.SampleSource;

namespace AudioExplorer.Audio
{
    class AudioController
    {
        private List<double> frequencies;
        private SoundMixer mixer;
        private const int sampleRate = 4410;
        private ChannelMatrix monoToStereoChannelMatrix;
        private ISoundOut soundOut;

        public AudioController(ISoundOut programSoundOut)
        {
            frequencies = new List<double>();
            mixer = new SoundMixer(2, sampleRate)
            {
                FillWithZeros = true,
                DivideResult = true
            };

            monoToStereoChannelMatrix = new ChannelMatrix(ChannelMask.SpeakerFrontCenter, ChannelMask.SpeakerFrontLeft | ChannelMask.SpeakerFrontRight);
            monoToStereoChannelMatrix.SetMatrix(
            new[,]
            {
            {1.0f, 1.0f}
            });

            soundOut = programSoundOut;
        }

        public void Dispose()
        {
            mixer.Dispose();
            soundOut.Dispose();
        }

        public void clear()
        {
            frequencies.Clear();
        }

        public void addFrequency(double freq)
        {
            frequencies.Add(freq);
        }

        public void startPlaying()
        {

            foreach (double freq in frequencies)
            {
                SineGenerator generator = new SineGenerator(freq, 1.0, 0);
                VolumeSource vol;
                mixer.AddSource(

                    generator.ToWaveSource()
                    .AppendSource(x => new DmoChannelResampler(x, monoToStereoChannelMatrix, sampleRate))
                    .AppendSource(x => new VolumeSource(x.ToSampleSource()), out vol)
                    );
            }

            soundOut.Initialize(mixer.ToWaveSource());
            soundOut.Play();
        }

        public void stopPlaying()
        {
            soundOut.Stop();
            frequencies.Clear();
            mixer.RemoveAllSources();
        }

        public void removeAll()
        {
            frequencies.Clear();
            mixer.RemoveAllSources();
        }

        public void updatePlaying(double freq, int waveform = 0)
        {
            frequencies.Clear();
            frequencies.Add(freq);
            mixer.RemoveAllSources();
            VolumeSource vol;
            if (waveform == 0) {
                SineGenerator generator = new SineGenerator(freq, 1.0, 0);
                mixer.AddSource(
                   generator.ToWaveSource()
                   .AppendSource(x => new DmoChannelResampler(x, monoToStereoChannelMatrix, sampleRate))
                   .AppendSource(x => new VolumeSource(x.ToSampleSource()), out vol)
                   );
            }
            else if (waveform == 1)
            {
                SquareGenerator generator = new SquareGenerator(freq, 1.0, 0);
                mixer.AddSource(
                  generator.ToWaveSource()
                  .AppendSource(x => new DmoChannelResampler(x, monoToStereoChannelMatrix, sampleRate))
                  .AppendSource(x => new VolumeSource(x.ToSampleSource()), out vol)
                  );
            }
            else if (waveform == 2)
            {
                SawtoothGenerator generator = new SawtoothGenerator(freq, 1.0, 0);
                mixer.AddSource(
                  generator.ToWaveSource()
                  .AppendSource(x => new DmoChannelResampler(x, monoToStereoChannelMatrix, sampleRate))
                  .AppendSource(x => new VolumeSource(x.ToSampleSource()), out vol)
                  );
            }
            else
            {
                TriangleGenerator generator = new TriangleGenerator(freq, 1.0, 0);
                mixer.AddSource(
                  generator.ToWaveSource()
                  .AppendSource(x => new DmoChannelResampler(x, monoToStereoChannelMatrix, sampleRate))
                  .AppendSource(x => new VolumeSource(x.ToSampleSource()), out vol)
                  );
            }
          
            Debug.WriteLine(frequencies.Count().ToString() + " frequencies");
            
        }

        public ISampleSource sampleSource()
        {
            return mixer;
        }
    }
}
