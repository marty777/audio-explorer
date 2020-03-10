using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioExplorer.MIDI
{
    public enum TimingScheme
    {
        MetricalTiming,
        TimeCode,
    }

   public enum EventType
    {
        MIDIEvent,
        SysExEvent,
        MetaEvent,
        UnknownEvent,
    }

    public enum MIDIEventType
    {
        NoteOff,
        NoteOn,
        PolyphonicPressure,
        Controller,
        ProgramChange,
        ChannelPresure,
        PitchBend,
    }

    public enum SysExEventType
    {
        SingleEvent,
        ContinuationEvent,
    }

    public enum MetaEventType
    {
        SequenceNumber,
        Text,
        Copyright,
        SequenceTrackName,
        InstrumentName,
        Lyric,
        Marker,
        CuePoint,
        ProgramName,
        DeviceName,
        MIDIChannelPrefix,
        MIDIPort,
        EndOfTrack,
        Tempo,
        SMPTEOffset,
        TimeSignature,
        KeySignature,
        SequencerSpecificEvent,
    }

    public class MIDIEvent
    {
        public EventType type { get; set; }
        public MIDIEventType midieventtype { get; set; }
        public SysExEventType sysexeeventtype { get; set; }
        public MetaEventType metaeventtype { get; set; }
        public uint val1 { get; set; }
        public uint val2 { get; set; }
        public uint val3 { get; set; }
        public uint val4 { get; set; }
        public uint val5 { get; set; }
        public List<byte> message { get; set; }
        public uint delta { get; set; }
        public UInt64 pos { get; set; }
        public bool running;
    }

    class MIDITrack
    {   

        public List<MIDIEvent> events;

        public MIDITrack()
        {
            events = new List<MIDIEvent>();
        }
    }

    class MIDIData
    {
        public ushort format;
        public ushort ntracks;
        public ushort tickdiv;
        public TimingScheme timing;
        public ushort metrical_ppqn; // pulses per quarter note in metrical timing
        public ushort timecode_fps; // frames per second in timecode timing
        public ushort timecode_sfr; // sub-frame resolution in timecode timing
        public List<MIDITrack> tracks;
        public MIDIData()
        {
            this.tracks = new List<MIDITrack>();
        }
    }
}
