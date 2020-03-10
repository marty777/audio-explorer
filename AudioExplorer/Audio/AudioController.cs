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
    class MidiKeyPlaying
    {
        public double freq { get; set; }
        public string name { get; set; }
        public float vol { get; set; }
        public float vol_vel { get; set; }
    }

    class AudioController
    {
        private List<double> frequencies;
        private List<MidiKeyPlaying> midiKeys { get; set; }
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
            midiKeys = new List<MidiKeyPlaying>();
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
        
        public void startPlayingMIDIKey(int midikey, float vel)
        {
            midiKeys[midikey].vol_vel = vel;
        }

        public void stopPlayingMIDIKey(int midikey, float vel)
        {
            midiKeys[midikey].vol_vel = -vel;
        }

        public void updateMidiKeys(float delta)
        {
            for (int i = 0; i < midiKeys.Count; i++)
            {
                if (midiKeys[i].vol_vel == 0.0f)
                {
                    continue;
                }
                midiKeys[i].vol += midiKeys[i].vol_vel * delta;
                if (midiKeys[i].vol > 1.0f)
                {
                    midiKeys[i].vol = 1.0f;
                    midiKeys[i].vol_vel = 0.0f;
                }
                else if (midiKeys[i].vol < 0.0f)
                {
                    midiKeys[i].vol = 0.0f;
                    midiKeys[i].vol_vel = 0.0f;
                }
                mixer.setSourceVolume(i, midiKeys[i].vol);
            }
        }

        public void startPlayingMidi()
        {
            ChromaticScale.ChromaticScale scale = new ChromaticScale.ChromaticScale();
            for(int i = 0; i <= 128; i++)
            {
                Console.WriteLine("Adding {0} - freq {1} name {2} to mixer", i, scale.getMidiFreqFromKeyNum(i), scale.getMidNoteNameFromKeyNum(i));
                WaveGenerator generator = new WaveGenerator(WaveGenerator.WaveType.TriangleWave, scale.getMidiFreqFromKeyNum(i), 1.0, 0.0);
                VolumeSource vol;
                mixer.AddSource(
                    generator.ToWaveSource()
                    .AppendSource(x => new DmoChannelResampler(x, monoToStereoChannelMatrix, sampleRate))
                    .AppendSource(x => new VolumeSource(x.ToSampleSource()), out vol)
                    );
                mixer.setSourceVolume(i, 0.0f);
                midiKeys.Add(new MidiKeyPlaying { freq = scale.getMidiFreqFromKeyNum(i), name = scale.getMidNoteNameFromKeyNum(i), vol = 0.0f, vol_vel = 0.0f });
            }

            soundOut.Initialize(mixer.ToWaveSource());
            soundOut.Play();
        }

        public void updatePlayingMidi()
        {

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

        public void updatePlaying(double freq, WaveGenerator.WaveType wavetype)
        {
            frequencies.Clear();
            frequencies.Add(freq);
            mixer.RemoveAllSources();
            VolumeSource vol;
            WaveGenerator generator = new WaveGenerator(wavetype, freq, 1.0, 0);
            mixer.AddSource(
                  generator.ToWaveSource()
                  .AppendSource(x => new DmoChannelResampler(x, monoToStereoChannelMatrix, sampleRate))
                  .AppendSource(x => new VolumeSource(x.ToSampleSource()), out vol)
                  );
            
            Debug.WriteLine(frequencies.Count().ToString() + " frequencies");
            
        }

        public ISampleSource sampleSource()
        {
            return mixer;
        }
    }
}
