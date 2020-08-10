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
using AudioExplorer.Audio;
using AudioExplorer.SampleSource;
using AudioExplorer.SampleProcessor;
using AudioExplorer.Scalar;

namespace AudioExplorer
{
    class Program
    {
        static void Main(string[] args)
        {

            WaveFormat waveFormat = new WaveFormat(44100, 16, 1);
            
            Oscillator freqOsc = new Oscillator(Oscillator.WaveType.SineWave, waveFormat.SampleRate, new ConstantScalar(0.1f), new ConstantScalar(100.0f), new ConstantScalar(0), new ConstantScalar(220.0f));
            Oscillator ampOsc = new Oscillator(Oscillator.WaveType.SineWave, waveFormat.SampleRate, new ConstantScalar(4.0f), new ConstantScalar(0.5f), new ConstantScalar(0), new ConstantScalar(0.5f));
            Oscillator variableFreq = new Oscillator(Oscillator.WaveType.SquareWave, waveFormat.SampleRate, freqOsc, ampOsc, new ConstantScalar(0), new ConstantScalar(0));
            
            ScalarPassthrough scalarPassthru = new ScalarPassthrough(waveFormat, variableFreq);

            BasicAudioController basicAudioController = new BasicAudioController(GetSoundOut(), 1, 44100);
            basicAudioController.addSource((ISampleSource)scalarPassthru);
            basicAudioController.startPlaying();

            Console.ReadKey();
            basicAudioController.stopPlaying();
            return;

            MIDI.MIDIData data = MIDI.MIDIFileReader.readFile(@"..\..\sampledata\MIDI_sample.mid");

            MIDI.MIDIData testdata = new MIDI.MIDIData();
            testdata.format = 0;
            testdata.timing = MIDI.TimingScheme.TimeCode;
            testdata.timecode_fps = 24;
            testdata.timecode_sfr = 4;
            testdata.ntracks = 1;
            testdata.tracks.Add(new MIDI.MIDITrack());
            MIDI.MIDIEvent event0 = new MIDI.MIDIEvent();
            MIDI.MIDIEvent event1 = new MIDI.MIDIEvent();

            event0.delta = 4 * 24 * 5; // wait 5 seconds from start
            event1.delta = 4 * 24 * 2; // 2 seconds

            //event0.type = MIDI.EventType.MIDIEvent;
            //event0.midieventtype = MIDI.MIDIEventType.NoteOn;
            //event0.val1 = 0; // channel 0
            //event0.val2 = 60; // middle C
            //event0.val3 = 1;

            //event1.type = MIDI.EventType.MIDIEvent;
            //event1.midieventtype = MIDI.MIDIEventType.NoteOff;
            //event1.val1 = 0; // channel 0
            //event1.val2 = 60; // middle C
            //event1.val3 = 1;

            //testdata.tracks[0].events.Add(event0);
            //testdata.tracks[0].events.Add(event1);


            MIDI.MIDIPlayer player = new MIDI.MIDIPlayer(data);
            Console.WriteLine("Track 1 events:");
            player.playTrack(1);

            Console.ReadKey();
            return;

            MIDIAudioController audioController = new MIDIAudioController(GetSoundOut());
            ChromaticScale.ChromaticScale scale = new ChromaticScale.ChromaticScale();
            audioController.startPlaying();
            SampleSource.WaveGenerator.WaveType wavetype = WaveGenerator.WaveType.SineWave;
            
            Console.ReadKey(); // wait for input
            for (int i = 0; i < scale.notes.Count(); i++)
            {
                float freq = scale.notes[i].base_freq / 2;
                Console.WriteLine(scale.notes[i].identifier[0] + " - " + freq + " - " + wavetype);
                audioController.updatePlaying(freq, wavetype);
                Console.ReadKey(); // wait for input

                wavetype = Next(wavetype);
                
            }
            for (int i = 0; i < scale.notes.Count(); i++)
            {

                Console.WriteLine(scale.notes[i].identifier[0] + " - " + scale.notes[i].base_freq.ToString() + " - " + wavetype);
                audioController.updatePlaying(scale.notes[i].base_freq, wavetype);
                Console.ReadKey(); // wait for input
                wavetype = Next(wavetype);
            }
            for (int i = 0; i < scale.notes.Count(); i++)
            {
                float freq = scale.notes[i].base_freq * 2;
                Console.WriteLine(scale.notes[i].identifier[0] + " - " + freq + " - " + wavetype);
                audioController.updatePlaying(freq, wavetype);
                Console.ReadKey(); // wait for input
                wavetype = Next(wavetype);

            }

            audioController.stopPlaying();
            audioController.Dispose();

        }

        static private ISoundOut GetSoundOut()
        {
            if (WasapiOut.IsSupportedOnCurrentPlatform)
                return new WasapiOut();
            else
                return new DirectSoundOut();
        }

        public static WaveGenerator.WaveType Next(WaveGenerator.WaveType myEnum)
        {
            switch (myEnum)
            {
                case WaveGenerator.WaveType.SineWave:
                    return WaveGenerator.WaveType.SquareWave;
                case WaveGenerator.WaveType.SquareWave:
                    return WaveGenerator.WaveType.TriangleWave;
                case WaveGenerator.WaveType.TriangleWave:
                    return WaveGenerator.WaveType.SawtoothWave;
                case WaveGenerator.WaveType.SawtoothWave:
                    return WaveGenerator.WaveType.InverseSawtoothWave;
                case WaveGenerator.WaveType.InverseSawtoothWave:
                    return WaveGenerator.WaveType.SineWave;
                default:
                    return WaveGenerator.WaveType.SineWave;
            }
        }
    }
}
