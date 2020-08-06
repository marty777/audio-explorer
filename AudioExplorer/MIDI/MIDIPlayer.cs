using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AudioExplorer.Audio;
using AudioExplorer.SampleSource;
using CSCore.SoundOut;

namespace AudioExplorer.MIDI
{
    class MIDIPlayer
    {
        MIDIData data;

        public MIDIPlayer(MIDIData data)
        {
            this.data = data;
        }

        public void playTrack(int trackIndex)
        {
            if (trackIndex >= data.tracks.Count) {
                throw new Exception(String.Format("Invalid track index {0} specified. Highest available track index is {1}", trackIndex, data.tracks.Count - 1));
            }

            // determine timing increments
            UInt64 time_inc_us = 1;
            UInt64 time_offset_us = 0; // start time offset
            if (data.timing == TimingScheme.MetricalTiming) {
                // locate tempo event in track 0 (format 0 or 1) or playing track for format 2
                UInt64 us_per_quarternote = 500000; // 120 bpm default (I think)
                switch (data.format)
                {
                    case 0:
                        for (int i = 0; i < data.tracks[trackIndex].events.Count; i++) {
                            if(data.tracks[trackIndex].events[i].type == EventType.MetaEvent && data.tracks[trackIndex].events[i].metaeventtype == MetaEventType.Tempo)
                            {
                                Console.WriteLine("Tempo event with value {0} ({0:X2})", data.tracks[trackIndex].events[i].val1);
                                us_per_quarternote = data.tracks[trackIndex].events[i].val1;
                                time_inc_us = us_per_quarternote / data.tickdiv;
                                break;
                            }
                            
                        }
                        break;
                    case 1:
                        for (int i = 0; i < data.tracks[0].events.Count; i++)
                        {
                            if (data.tracks[0].events[i].type == EventType.MetaEvent && data.tracks[0].events[i].metaeventtype == MetaEventType.Tempo)
                            {
                                Console.WriteLine("Tempo event with value {0} ({0:X2})", data.tracks[0].events[i].val1);
                                us_per_quarternote = data.tracks[0].events[i].val1;
                                time_inc_us = us_per_quarternote / data.tickdiv; // should give us/p
                                break;
                            }
                            else if (data.tracks[0].events[i].type == EventType.MetaEvent && data.tracks[0].events[i].metaeventtype == MetaEventType.TimeSignature)
                            {
                                Console.WriteLine("TimeSignature event with value {0} {1} {2} {3} ({0:X2} {1:X2} {2:X2} {3:X2})", data.tracks[0].events[i].val1, data.tracks[0].events[i].val2, data.tracks[0].events[i].val3, data.tracks[0].events[i].val4);
                                break;
                            }
                        }
                        break;
                    case 2:
                        for (int i = 0; i < data.tracks[trackIndex].events.Count; i++)
                        {
                            if (data.tracks[trackIndex].events[i].type == EventType.MetaEvent && data.tracks[trackIndex].events[i].metaeventtype == MetaEventType.Tempo)
                            {
                                Console.WriteLine("Tempo event with value {0} ({0:X2})", data.tracks[trackIndex].events[i].val1);
                                us_per_quarternote = data.tracks[trackIndex].events[i].val1;
                                time_inc_us = us_per_quarternote / data.tickdiv;
                                break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            } else
            {
                // 1 tick is the sub-frame resolution 
                time_inc_us = (UInt64)(1000000 / (data.timecode_fps * data.timecode_sfr));
            }

            UInt64 ticks = 0; // ticks are in microseconds
            int event_index = 0;
            UInt64 curr_ticks = data.tracks[trackIndex].events[0].delta;
            MIDIAudioController audioController = new MIDIAudioController(GetSoundOut());
            audioController.startPlayingMidi();
            while (event_index < data.tracks[trackIndex].events.Count)
            {
                while (curr_ticks == 0 && event_index < data.tracks[trackIndex].events.Count) {
                    if (data.tracks[trackIndex].events[event_index].type == EventType.MIDIEvent)
                    {
                        MIDIEvent the_event = data.tracks[trackIndex].events[event_index];
                        if (the_event.val1 == 0) // channel 0 for note events
                        {
                            switch (the_event.midieventtype)
                            {
                                case MIDIEventType.NoteOn:
                                    audioController.startPlayingMIDIKey((int)the_event.val2, (float)the_event.val3 * 0.0001f);
                                    Console.WriteLine("NoteOn {0} {1}", the_event.val2, the_event.val3);
                                    break;
                                case MIDIEventType.NoteOff:
                                    audioController.stopPlayingMIDIKey((int)the_event.val2, (float)the_event.val3 * 0.0001f);
                                    Console.WriteLine("NoteOff {0} {1}", the_event.val2, the_event.val3);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    event_index += 1;
                    if (event_index >= data.tracks[trackIndex].events.Count)
                    {
                        break;
                    }
                    curr_ticks = data.tracks[trackIndex].events[event_index].delta;
                }
                audioController.updateMidiKeys(1.0f); // 1 increment
                Thread.Sleep((int)time_inc_us/1000);
                ticks += time_inc_us;
                curr_ticks--;
                
                if(event_index >= data.tracks[trackIndex].events.Count)
                {
                    break;
                }
                //Console.WriteLine("ticks {0} event_index {1} ({2}) curr_ticks {3}", ticks, event_index, data.tracks[trackIndex].events[event_index].delta, curr_ticks);
            }
            
        }

        public void printEvent(MIDIEvent midievent) {
            switch(midievent.type)
            {
                case EventType.UnknownEvent:
                    Console.WriteLine("{0}\tUnknown Event {1:X2} {2:X2} pos {3}", midievent.delta, midievent.val1, midievent.val2, midievent.pos);
                    break;
                case EventType.MetaEvent:
                    Console.WriteLine("{0}\tMeta Event {1} pos {2} running {3}", midievent.delta, midievent.metaeventtype, midievent.pos, midievent.running);
                    break;
                case EventType.SysExEvent:
                    Console.WriteLine("{0}\tSysEx Event {1} pos {2} running {3}", midievent.delta, midievent.sysexeeventtype, midievent.pos, midievent.running);
                    break;
                case EventType.MIDIEvent:
                    Console.WriteLine("{0}\tMIDI Event {1} pos {2} running {3}", midievent.delta, midievent.midieventtype, midievent.pos, midievent.running);
                    break;
            }
        }

        static private ISoundOut GetSoundOut()
        {
            if (WasapiOut.IsSupportedOnCurrentPlatform)
                return new WasapiOut();
            else
                return new DirectSoundOut();
        }
    }
}
