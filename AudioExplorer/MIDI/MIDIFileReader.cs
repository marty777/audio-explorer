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
                byte[] fileBytes = File.ReadAllBytes(filePath);

                for (int i = 0; i < fileBytes.Length; i += 16)
                {
                    for (int j = i; j < fileBytes.Length && j < i + 16; j++)
                    {
                        Console.Write("{0:X2}", fileBytes[j]);
                    }
                    Console.Write("\t");
                    for (int j = i; i < fileBytes.Length && j < i + 16; j++)
                    {
                        Console.Write((char)fileBytes[j]);
                    }
                    Console.WriteLine();
                    if(i > 128)
                    {
                        break;
                    }
                }
                UInt64 trackChunkStart = MIDIFileReader.readHeader(fileBytes, data);
                Console.WriteLine("format: {0}, ntracks:{1}, tickdiv:{2} ", data.format, data.ntracks, data.tickdiv);
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
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed to read file {0}: {1}", filePath, e.Message);
            }

           
            
            return data;
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
                if ((UInt64)fileData.Length < index + chunklen)
                {
                    throw new Exception("File data too short to contain track chunk of length " + chunklen +" at index " + (index - 8));
                }
                MIDITrack track;
                track.events = new List<MIDIEvent>();
                Console.WriteLine("Got here index {0} chunklen {1}", index, chunklen);
                UInt64 trackindex = index;
                while(trackindex < index + chunklen)
                {
                    MIDIEvent new_event = new MIDIEvent();
                    switch (fileData[trackindex + 1] >> 4 & 0xF) // first 4 bits
                    {
                        case 0x8:
                            Console.WriteLine("Got here noteoff");
                            new_event.type = EventType.MIDIEvent;
                            new_event.midieventtype = MIDIEventType.NoteOff;
                            new_event.val1 = (uint)(fileData[trackindex + 1] & 0x7); // channel
                            new_event.val2 = (uint)(fileData[trackindex + 2]); // key number
                            new_event.val3 = (uint)(fileData[trackindex + 3]); // velocity
                            trackindex += 3;
                            break;
                        case 0x9:
                            Console.WriteLine("Got here noteon");
                            new_event.type = EventType.MIDIEvent;
                            new_event.midieventtype = MIDIEventType.NoteOn;
                            new_event.val1 = (uint)(fileData[trackindex + 1] & 0x7); // channel
                            new_event.val2 = (uint)(fileData[trackindex + 2]); //  key number
                            new_event.val3 = (uint)(fileData[trackindex + 3]); // velocity
                            trackindex += 3;
                            break;
                        case 0xA:
                            Console.WriteLine("Got here polyphonic presure");
                            new_event.type = EventType.MIDIEvent;
                            new_event.midieventtype = MIDIEventType.NoteOn;
                            new_event.val1 = (uint)(fileData[trackindex + 1] & 0x7); // channel
                            new_event.val2 = (uint)(fileData[trackindex + 2]); //  key number
                            new_event.val3 = (uint)(fileData[trackindex + 3]); // velocity
                            trackindex += 3;
                            break;
                        case 0xB:

                            break;
                        case 0xC:

                            break;
                        case 0xD:

                            break;
                        case 0xE:

                            break;
                        case 0xF:
                  
                            if((fileData[trackindex + 1] & 0xF) == 0xF) // Meta event
                            {
                                new_event.type = EventType.MetaEvent;
                                ushort type = (ushort)fileData[trackindex + 2];
                                trackindex = trackindex + 3;
                                uint length = MIDIFileReader.readVariableLengthQuantity(fileData, ref trackindex); // trackindex updated to end of length field
                                Console.WriteLine("Event FF type {0:X2} length {1}, trackindex {2}", type, length, trackindex);
                                trackindex++;
                                if (trackindex + length >= (UInt64)fileData.Length) {
                                    throw new Exception("File data too short to contain metadata event of length " + length + " at index " + (trackindex));
                                }
                                
                                switch (type)
                                {
                                    case 0x00:
                                        new_event.metaeventtype = MetaEventType.SequenceNumber;
                                        if(length != 2)
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
                                        if(length != 0x01)
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
                                        new_event.val1 = (uint)(fileData[trackindex] << 16 | fileData[trackindex + 1] << 8 | fileData[trackindex + 2]) ;
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
                                        Console.WriteLine("Unknown metadata field found FF {0:X2} length {1}", type, length);
                                        for(ulong i = trackindex; i < trackindex + length; i++)
                                        {
                                            Console.Write("{0:X2}", fileData[i]);
                                            if((i - trackindex) % 16 == 15) {
                                                Console.WriteLine();
                                            }
                                        }
                                        Console.WriteLine();
                                        break;

                                }
                                trackindex += length;
                            }
                            else // SysEx event (or error)
                            {
                                if(fileData[trackindex + 1] == 0xF0)
                                {
                                    new_event.type = EventType.SysExEvent;
                                    trackindex += 2;
                                    uint length = MIDIFileReader.readVariableLengthQuantity(fileData, ref trackindex); // trackindex updated to end of length field
                                    new_event.message = MIDIFileReader.readEventMessage(fileData, trackindex, length);
                                    if(new_event.message[new_event.message.Count - 1] == 0xF7)
                                    {
                                        new_event.sysexeeventtype = SysExEventType.SingleEvent;
                                    }
                                    if (new_event.message[new_event.message.Count - 1] == 0x00)
                                    {
                                        new_event.sysexeeventtype = SysExEventType.ContinuationEvent;
                                        uint delta_time = MIDIFileReader.readVariableLengthQuantity(fileData, ref trackindex);
                                    }
                                }
                                

                                // this gets a little complicated with continuation events. 
                            }
                            break;
                        default:
                            throw new Exception("Undefined event start byte at index " + trackindex + "(" + (fileData[trackindex] >> 4 & 0xF) + ")");
                            break;
                        
                    }
                    //Console.WriteLine("Adding event type {0} midievent {1} sysexevent {2} metaevent {3}  val1 {4} message ", new_event.type, new_event.midieventtype, new_event.sysexeeventtype, new_event.metaeventtype, new_event.val1);
                    //if (new_event.message != null)
                    //{
                    //    for(int i =0; i < new_event.message.Count; i++)
                    //    {
                    //        Console.Write("{0:X2}", new_event.message[i]);
                    //    }
                    //    Console.WriteLine();
                    //    for (int i = 0; i < new_event.message.Count; i++)
                    //    {
                    //        Console.Write("{0}", (char)new_event.message[i]);
                    //    }
                    //    Console.WriteLine();
                    //}
                    //Console.WriteLine();
                    track.events.Add(new_event);
                }
                data.tracks.Add(track);
                index = trackindex;
                break;
            }
            return index;
        }

        // variable length fields can be 1-4 bytes indicated by formatting. This method will update the index pointer to the end of the field
        public static uint readVariableLengthQuantity(byte[] fileData, ref UInt64 index)
        {
            int bytecount = 0;
            uint accumulator = 0;
            UInt64 index2 = index;
            while(bytecount < 4) {
                int high_bit = (fileData[index] >> 7) & 0x01;
                accumulator |= (uint)(fileData[index2] & 0x7f);
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
            index = index2;
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
        //public static string readEventText(byte[] fileData, UInt64 start_index, UInt64 length)
        //{
        //    if ((UInt64)fileData.Length <= start_index + length)
        //    {
        //        throw new Exception("File to short (length " + fileData.Length + ") to to read string at index " + start_index + " with length " + length);
        //    }
        //    StringBuilder builder = new StringBuilder();
        //    for (UInt64 i = start_index; i < start_index + length; i++)
        //    {
        //        builder.Append((char)fileData[i]);
        //    }
        //    return builder.ToString();
        //}
    }

    
}
