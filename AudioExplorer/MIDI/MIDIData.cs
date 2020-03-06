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

    public struct MIDIEvent
    {
        public EventType type;
        public MIDIEventType midieventtype;
        public SysExEventType sysexeeventtype;
        public MetaEventType metaeventtype;
        public uint val1;
        public uint val2;
        public uint val3;
        public uint val4;
        public uint val5;
        public List<byte> message;
        //public string text;
    }

    struct MIDITrack
    {   

        public List<MIDIEvent> events;
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
