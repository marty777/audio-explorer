using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioExplorer.Audio;
using AudioExplorer.SampleSource;

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
            UInt64 ticks = 0;
            for(int i = 0; i < data.tracks[trackIndex].events.Count; i++)
            {
                printEvent(data.tracks[trackIndex].events[i]);
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
    }
}
