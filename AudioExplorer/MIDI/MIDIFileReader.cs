using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AudioExplorer.MIDI
{
    static class MIDIFileReader
    {

        public static MIDIData readFile(string filePath)
        {
            MIDIData data = new MIDIData();
            try
            {
                Console.WriteLine("Reading file {0}", filePath);
                byte[] fileBytes = File.ReadAllBytes(filePath);

                UInt64 trackChunkStart = MIDIFileReader.readHeader(fileBytes, data);
                Console.WriteLine("format: {0}, ntracks: {1}, tickdiv: {2} ", data.format, data.ntracks, data.tickdiv);
                if (data.timing == TimingScheme.MetricalTiming)
                {
                    Console.WriteLine("Metrical timing PPQN: {0}", data.metrical_ppqn);
                }
                else
                {
                    Console.WriteLine("Timecode FPS: {0} SFR:{1}", data.timecode_fps, data.timecode_sfr);
                }
                UInt64 trackChunkEnd = trackChunkStart;
                while (trackChunkEnd <  (UInt64)fileBytes.Length && data.tracks.Count < 3) {
                    trackChunkEnd = MIDIFileReader.readTracks(fileBytes, data, trackChunkEnd);
                    Console.WriteLine("Track {0} read with {1} events", data.tracks.Count, data.tracks[data.tracks.Count - 1].events.Count);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed to read file {0}: {1}\n{2}", filePath, e.Message, e.StackTrace);
            }
            
            return data;
        }

        static void dumpHex(byte[] bytes)
        {
            int len = 16;
            List<string> lines = new List<string>();
            
            for(int i = 0; i < bytes.Length; i+=len)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat("{0}:\t", i);
                for(int j = 0; j < len && i + j < bytes.Length; j++)
                {
                    builder.AppendFormat("{0:X2} ", bytes[i + j]);
                }
                builder.AppendFormat("\t");
                for (int j = 0; j < len && i + j < bytes.Length; j++)
                {
                    builder.AppendFormat("{0} ", (char)bytes[i + j]);
                }
                lines.Add(builder.ToString());
            }
            System.IO.File.WriteAllLines(@"..\output.txt", lines);
        }

        // returns length of header
        static UInt64 readHeader(byte[] fileData, MIDIData data)
        {
            ushort expectedChunkLen = 6;

            if(fileData.Length < 8)
            {
                throw new Exception("File data too short to contain header");
            }
            if(!(fileData[0] == 0x4d && fileData[1] == 0x54 && fileData[2] == 0x68 && fileData[3] == 0x64)) // "MThd"
            {
                throw new Exception("File data does not contain header identifier");
            }
            ushort chunklen = 0;
            chunklen |= (ushort)((ushort)fileData[4] << 24);
            chunklen |= (ushort)((ushort)fileData[5] << 16);
            chunklen |= (ushort)((ushort)fileData[6] << 8);
            chunklen |= (ushort)((ushort)fileData[7]);
            Console.WriteLine("Chunk length: {0}", chunklen);
            if(fileData.Length < 8 + chunklen) {
                throw new Exception("File data too short to contain header chunk");
            }
            if(chunklen != expectedChunkLen) {
                throw new Exception("Unexpected chunk length in header (was "+ chunklen + " expected "+ expectedChunkLen + ")");
            }
            data.format = (ushort) ((ushort)fileData[8] << 8 | (ushort)fileData[9]);
            data.ntracks = (ushort)((ushort)fileData[10] << 8 | (ushort)fileData[11]);
            data.tickdiv = (ushort)((ushort)fileData[12] << 8 | (ushort)fileData[13]);
            
            if(((data.tickdiv >> 15) & 0x01) == 1) {
                data.timing = TimingScheme.TimeCode;
                data.timecode_fps = (ushort)((data.tickdiv >> 8) & 0x7);
                data.timecode_sfr = (ushort)(data.tickdiv & 0xf);
            }
            else
            {
                data.timing = TimingScheme.MetricalTiming;
                data.metrical_ppqn = (ushort)(data.tickdiv & 0x7f);
            }

            return (UInt64)(chunklen + 8);
        }

        static UInt64 readTracks(byte[] fileData, MIDIData data, UInt64 chunkStart)
        {
            UInt64 index = chunkStart;
            while (index < (UInt64)fileData.Length) {
                // start reading a track chunk
                if((UInt64)fileData.Length < index + 8)
                {
                    throw new Exception("File data too short to contain track chunk at index " + index);
                }
                if(!(fileData[index + 0] == 0x4d && fileData[index + 1] == 0x54 && fileData[index + 2] == 0x72 && fileData[index + 3] == 0x6B)) { // "MTrk"
                    throw new Exception("File data does not contain expected track identifier at index " + index);
                }
                index += 4;
                ushort chunklen = 0;
                chunklen |= (ushort)((ushort)fileData[index + 0] << 24);
                chunklen |= (ushort)((ushort)fileData[index + 1] << 16);
                chunklen |= (ushort)((ushort)fileData[index + 2] << 8);
                chunklen |= (ushort)((ushort)fileData[index + 3]);
                index += 4;
                Console.WriteLine("Chunklen {0}", chunklen);
                if ((UInt64)fileData.Length < index + chunklen)
                {
                    throw new Exception("File data too short to contain track chunk of length " + chunklen +" at index " + (index - 8));
                }
                MIDITrack track;
                track.events = new List<MIDIEvent>();
                UInt64 trackindex = index;
                MIDIEvent previous = new MIDIEvent();
                while(trackindex < index + chunklen)
                {
                    MIDIEvent new_event = MIDIFileReader.readEvent(fileData, ref trackindex, previous);
                    previous = new_event;
                    track.events.Add(new_event);
                }
                data.tracks.Add(track);
                
                index = trackindex;
                break;
            }
            return index;
        }

        // reads event or throws exception, advances index to end of event fields
        // In a breathtaking display of parsimony, instructions that repeat the status
        // byte of the previous instruction *may be* written without that status byte. 
        // If there isn't a high-bit byte following the delta-time, assume it's a repeated
        // instruction. The transmission speeds of early equipment must have been 
        // extremely low.
        public static MIDIEvent readEvent(byte[] fileData, ref UInt64 index, MIDIEvent previous)
        {
            MIDIEvent new_event = new MIDIEvent();
            UInt64 trackindex = index;
            new_event.delta = readVariableLengthQuantity(fileData, ref trackindex);
            new_event.pos = index;
            switch (fileData[trackindex] >> 4 & 0xF) // first 4 bits
            {
                case 0x8:
                    new_event.type = EventType.MIDIEvent;
                    new_event.midieventtype = MIDIEventType.NoteOff;
                    new_event.val1 = (uint)(fileData[trackindex] & 0x7); // channel
                    new_event.val2 = (uint)(fileData[trackindex + 1]); // key number
                    new_event.val3 = (uint)(fileData[trackindex + 2]); // velocity
                    trackindex += 3;
                    break;
                case 0x9:
                    new_event.type = EventType.MIDIEvent;
                    new_event.midieventtype = MIDIEventType.NoteOn;
                    new_event.val1 = (uint)(fileData[trackindex] & 0x7); // channel
                    new_event.val2 = (uint)(fileData[trackindex + 1]); //  key number
                    new_event.val3 = (uint)(fileData[trackindex + 2]); // velocity
                    trackindex += 3;
                    break;
                case 0xA:
                    new_event.type = EventType.MIDIEvent;
                    new_event.midieventtype = MIDIEventType.NoteOn;
                    new_event.val1 = (uint)(fileData[trackindex] & 0x7); // channel
                    new_event.val2 = (uint)(fileData[trackindex + 1]); //  key number
                    new_event.val3 = (uint)(fileData[trackindex + 2]); // velocity
                    trackindex += 3;
                    break;
                case 0xB:
                    new_event.type = EventType.MIDIEvent;
                    new_event.midieventtype = MIDIEventType.Controller;
                    new_event.val1 = (uint)(fileData[trackindex] & 0x7); // channel
                    new_event.val2 = (uint)(fileData[trackindex + 1]); //  controller
                    new_event.val3 = (uint)(fileData[trackindex + 2]); // value
                    trackindex += 3;
                    break;
                case 0xC:
                    new_event.type = EventType.MIDIEvent;
                    new_event.midieventtype = MIDIEventType.ProgramChange;
                    new_event.val1 = (uint)(fileData[trackindex] & 0x7); // channel
                    new_event.val2 = (uint)(fileData[trackindex + 1]); //  program
                    trackindex += 2;
                    break;
                case 0xD:
                    new_event.type = EventType.MIDIEvent;
                    new_event.midieventtype = MIDIEventType.ChannelPresure;
                    new_event.val1 = (uint)(fileData[trackindex] & 0x7); // channel
                    new_event.val2 = (uint)(fileData[trackindex + 1]); //  pressure
                    trackindex += 2;
                    break;
                case 0xE:
                    new_event.type = EventType.MIDIEvent;
                    new_event.midieventtype = MIDIEventType.PitchBend;
                    new_event.val1 = (uint)(fileData[trackindex] & 0x7); // channel
                    new_event.val2 = (uint)(fileData[trackindex + 1]); //  lsb
                    new_event.val3 = (uint)(fileData[trackindex + 2]); // msb
                    trackindex += 3;
                    break;
                case 0xF:
                    if ((fileData[trackindex] & 0xF) == 0xF) // Meta event
                    {
                        new_event.type = EventType.MetaEvent;
                        ushort type = (ushort)fileData[trackindex + 1];
                        trackindex = trackindex + 2;
                        uint length = MIDIFileReader.readVariableLengthQuantity(fileData, ref trackindex); // trackindex updated to end of length field
                       
                        if (trackindex + length >= (UInt64)fileData.Length)
                        {
                            throw new Exception("File data too short to contain metadata event of length " + length + " at index " + (trackindex));
                        }

                        switch (type)
                        {
                            case 0x00:
                                new_event.metaeventtype = MetaEventType.SequenceNumber;
                                if (length != 2)
                                {
                                    throw new Exception("Metadata sequence number event with incorrect length " + length + " (should be 2) found at index " + (trackindex));
                                }
                                new_event.val1 = (ushort)((fileData[trackindex] << 8) | fileData[trackindex + 1]);
                                break;
                            case 0x01:
                                new_event.metaeventtype = MetaEventType.Text;
                                new_event.message = MIDIFileReader.readEventMessage(fileData, trackindex, length);
                                break;
                            case 0x02:
                                new_event.metaeventtype = MetaEventType.Copyright;
                                new_event.message = MIDIFileReader.readEventMessage(fileData, trackindex, length);
                                break;
                            case 0x03:
                                new_event.metaeventtype = MetaEventType.SequenceTrackName;
                                new_event.message = MIDIFileReader.readEventMessage(fileData, trackindex, length);
                                break;
                            case 0x04:
                                new_event.metaeventtype = MetaEventType.InstrumentName;
                                new_event.message = MIDIFileReader.readEventMessage(fileData, trackindex, length);
                                break;
                            case 0x05:
                                new_event.metaeventtype = MetaEventType.Lyric;
                                new_event.message = MIDIFileReader.readEventMessage(fileData, trackindex, length);
                                break;
                            case 0x06:
                                new_event.metaeventtype = MetaEventType.Marker;
                                new_event.message = MIDIFileReader.readEventMessage(fileData, trackindex, length);
                                break;
                            case 0x07:
                                new_event.metaeventtype = MetaEventType.CuePoint;
                                new_event.message = MIDIFileReader.readEventMessage(fileData, trackindex, length);
                                break;
                            case 0x08:
                                new_event.metaeventtype = MetaEventType.ProgramName;
                                new_event.message = MIDIFileReader.readEventMessage(fileData, trackindex, length);
                                break;
                            case 0x09:
                                new_event.metaeventtype = MetaEventType.DeviceName;
                                new_event.message = MIDIFileReader.readEventMessage(fileData, trackindex, length);
                                break;
                            case 0x20:
                                new_event.metaeventtype = MetaEventType.MIDIChannelPrefix;
                                if (length != 0x01)
                                {
                                    throw new Exception("Metadata MIDI channel prefix event with incorrect length " + length + " (should be 1) found at index " + (trackindex));
                                }
                                new_event.val1 = fileData[trackindex];
                                break;
                            case 0x21:
                                new_event.metaeventtype = MetaEventType.MIDIPort;
                                if (length != 0x01)
                                {
                                    throw new Exception("Metadata MIDI port event with incorrect length " + length + " (should be 1) found at index " + (trackindex));
                                }
                                new_event.val1 = fileData[trackindex];
                                break;
                            case 0x2F:
                                new_event.metaeventtype = MetaEventType.EndOfTrack;
                                if (length != 0)
                                {
                                    throw new Exception("Metadata end of track event with incorrect length " + length + " (should be 0) found at index " + (trackindex));
                                }
                                break;
                            case 0x51:
                                new_event.metaeventtype = MetaEventType.Tempo;
                                if (length != 0x03)
                                {
                                    throw new Exception("Metadata tempo event with incorrect length " + length + " (should be 3) found at index " + (trackindex));
                                }
                                new_event.val1 = (uint)(fileData[trackindex] << 16 | fileData[trackindex + 1] << 8 | fileData[trackindex + 2]);
                                break;
                            case 0x54:
                                new_event.metaeventtype = MetaEventType.SMPTEOffset;
                                if (length != 0x05)
                                {
                                    throw new Exception("Metadata SMPTE offset event with incorrect length " + length + " (should be 5) found at index " + (trackindex));
                                }
                                new_event.val1 = (uint)(fileData[trackindex]);
                                new_event.val2 = (uint)(fileData[trackindex + 1]);
                                new_event.val3 = (uint)(fileData[trackindex + 2]);
                                new_event.val4 = (uint)(fileData[trackindex + 3]);
                                new_event.val5 = (uint)(fileData[trackindex + 4]);
                                break;
                            case 0x58:
                                new_event.metaeventtype = MetaEventType.TimeSignature;
                                if (length != 0x04)
                                {
                                    throw new Exception("Metadata time signature event with incorrect length " + length + " (should be 4) found at index " + (trackindex));
                                }
                                new_event.val1 = (uint)(fileData[trackindex]);
                                new_event.val2 = (uint)(fileData[trackindex + 1]);
                                new_event.val3 = (uint)(fileData[trackindex + 2]);
                                new_event.val4 = (uint)(fileData[trackindex + 3]);
                                break;
                            case 0x59:
                                new_event.metaeventtype = MetaEventType.KeySignature;
                                if (length != 0x02)
                                {
                                    throw new Exception("Metadata key signature event with incorrect length " + length + " (should be 2) found at index " + (trackindex));
                                }
                                new_event.val1 = (uint)(fileData[trackindex]);
                                new_event.val2 = (uint)(fileData[trackindex + 1]);
                                break;
                            case 0x7F:
                                new_event.metaeventtype = MetaEventType.SequencerSpecificEvent;
                                new_event.message = MIDIFileReader.readEventMessage(fileData, trackindex, length);
                                break;
                            default:
                                throw new Exception("Unknown metadata field type "+type+" found at index " + (trackindex));
                                break;

                        }
                        trackindex += length;
                    }
                    else // SysEx event (or error)
                    {
                        if (fileData[trackindex] == 0xF0)
                        {
                            new_event.type = EventType.SysExEvent;
                            trackindex += 2;
                            UInt64 temp = trackindex - 1;
                            uint length = MIDIFileReader.readVariableLengthQuantity(fileData, ref trackindex); // trackindex updated to end of length field
                            new_event.message = MIDIFileReader.readEventMessage(fileData, trackindex, length);
                            if (new_event.message[new_event.message.Count - 1] == 0xF7)
                            {
                                new_event.sysexeeventtype = SysExEventType.SingleEvent;
                            }
                            if (new_event.message[new_event.message.Count - 1] == 0x00)
                            {
                                new_event.sysexeeventtype = SysExEventType.ContinuationEvent;
                                uint delta_time = MIDIFileReader.readVariableLengthQuantity(fileData, ref trackindex);
                            }
                        }
                        else
                        {
                            // hope that this is only two bytes long
                            Console.WriteLine("Unknown event at index {0} with opening byte {1:X2}", trackindex, fileData[trackindex]);
                            new_event.type = EventType.UnknownEvent;
                            new_event.val1 = fileData[trackindex];//fileData[trackindex + 1];
                            new_event.val2 = fileData[trackindex + 1];
                            trackindex += 2;
                            //throw new Exception(String.Format("Unknown field found {0:X2} at index {1}", fileData[trackindex], trackindex));
                        }
                        
                    }
                    break;
                default:
                    // okay, might be a running status. Check the provided previous event, which must be a MIDI event rather than a sysex or meta event
                    if(previous.type != EventType.MIDIEvent)
                    {
                        throw new Exception(String.Format("Undefined event start {0:X2} byte at index {1} ", fileData[trackindex], trackindex));
                    }
                    switch(previous.midieventtype)
                    {
                        case MIDIEventType.NoteOff:
                            new_event.midieventtype = previous.midieventtype;
                            new_event.val1 = previous.val1;
                            new_event.val2 = fileData[trackindex];
                            new_event.val3 = fileData[trackindex + 1];
                            trackindex += 2;
                            new_event.running = true;
                            break;
                        case MIDIEventType.NoteOn:
                            new_event.midieventtype = previous.midieventtype;
                            new_event.val1 = previous.val1;
                            new_event.val2 = fileData[trackindex];
                            new_event.val3 = fileData[trackindex + 1];
                            trackindex += 2;
                            new_event.running = true;
                            break;
                        case MIDIEventType.PolyphonicPressure:
                            new_event.midieventtype = previous.midieventtype;
                            new_event.val1 = previous.val1;
                            new_event.val2 = fileData[trackindex];
                            new_event.val3 = fileData[trackindex + 1];
                            trackindex += 2;
                            new_event.running = true;
                            break;
                        case MIDIEventType.Controller:
                            new_event.midieventtype = previous.midieventtype;
                            new_event.val1 = previous.val1;
                            new_event.val2 = fileData[trackindex];
                            new_event.val3 = fileData[trackindex + 1];
                            trackindex += 2;
                            new_event.running = true;
                            break;
                        case MIDIEventType.ProgramChange:
                            new_event.midieventtype = previous.midieventtype;
                            new_event.val1 = previous.val1;
                            new_event.val2 = fileData[trackindex];
                            trackindex += 1;
                            new_event.running = true;
                            break;
                        case MIDIEventType.ChannelPresure:
                            new_event.midieventtype = previous.midieventtype;
                            new_event.val1 = previous.val1;
                            new_event.val2 = fileData[trackindex];
                            trackindex += 1;
                            new_event.running = true;
                            break;
                        case MIDIEventType.PitchBend:
                            new_event.midieventtype = previous.midieventtype;
                            new_event.val1 = previous.val1;
                            new_event.val2 = fileData[trackindex];
                            new_event.val3 = fileData[trackindex + 1];
                            trackindex += 2;
                            new_event.running = true;
                            break;
                        default:
                            throw new Exception(String.Format("Undefined event start {0:X2} byte at index {1} ", fileData[trackindex], trackindex));
                            break;
                    }

                    
                    break;

            }
            index = trackindex;
            return new_event;
        }

        // variable length fields can be 1-4 bytes indicated by formatting. This method will update the index pointer to the end of the field
        public static uint readVariableLengthQuantity(byte[] fileData, ref UInt64 index)
        {
            int bytecount = 0;
            uint accumulator = 0;
            UInt64 index2 = index;
            while(bytecount < 4) {
                int high_bit = (fileData[index2] >> 7) & 0x01;
                accumulator = accumulator | (uint)(fileData[index2] & 0x7f);
                if (high_bit == 1)
                {
                    if(bytecount >= 3)
                    {
                        throw new Exception("Error reading variable length field at index " + index + ": bytecount exceeds maximum width");
                    }
                    accumulator = accumulator << 7;
                    index2++;
                }
                else
                {
                    break;
                }
                bytecount++;
            }
            index = index2 + 1;
            return accumulator;
        }

        // note that non-ASCII encodings aren't currently supported. It looks like the MIDI standard can theoretically handle any format of string encoding
        // as we're just dumping raw bytes, but it doesn't look like there's a field for holding an encoding identifier. This would probably mean some
        // guesswork on the part of the reader.
        public static List<byte> readEventMessage(byte[] fileData, UInt64 start_index, UInt64 length)
        {
            if ((UInt64)fileData.Length <= start_index + length)
            {
                throw new Exception("File to short (length " + fileData.Length + ") to to read message at index " + start_index + " with length " + length);
            }
            List<byte> bytes = new List<byte>();
            for (UInt64 i = start_index; i < start_index + length; i++)
            {
                bytes.Add(fileData[i]);
            }
            return bytes;
        }
    }
}
