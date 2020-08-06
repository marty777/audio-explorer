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


namespace AudioExplorer.Audio
{
    class BasicAudioController
    {
        private SoundMixer mixer;
        private ChannelMatrix monoToStereoChannelMatrix;
        private ISoundOut soundOut;

        public BasicAudioController(ISoundOut programSoundOut, int channels, int sampleRate)
        {
            mixer = new SoundMixer(channels, sampleRate)
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

        public void addSource(ISampleSource source)
        {
            mixer.AddSource(source);
        }

        public void startPlaying()
        {
            
            soundOut.Initialize(mixer.ToWaveSource());
            soundOut.Play();
        }

        public void stopPlaying()
        {
            soundOut.Stop();
            mixer.RemoveAllSources();
        }

        public ISampleSource sampleSource()
        {
            return mixer;
        }
    }
}
