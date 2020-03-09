using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioExplorer.ChromaticScale
{
    class Note
    {
        public List<string> identifier;
        public float base_freq;

        public Note(string id, float freq) 
        {
            this.identifier = new List<string>();
            this.identifier.Add(id);
            this.base_freq = freq;
        }
    }

    class ChromaticScale
    {
        public List<Note> notes { get; set; }

        public ChromaticScale()
        {
            notes = new List<Note>();
            notes.Add(new Note("A", 440));
            notes.Add(new Note("Bb", 466));
            notes[1].identifier.Add("A#");
            notes.Add(new Note("B", 494));
            notes.Add(new Note("C", 523));
            notes.Add(new Note("C#", 554));
            notes[4].identifier.Add("Db");
            notes.Add(new Note("D", 587));
            notes.Add(new Note("D#", 622));
            notes[6].identifier.Add("Eb");
            notes.Add(new Note("E", 659));
            notes.Add(new Note("F", 698));
            notes.Add(new Note("F#", 740));
            notes[9].identifier.Add("Gb");
            notes.Add(new Note("G", 784));
            notes.Add(new Note("Ab", 831));
            notes[11].identifier.Add("G#");
        }

        public double getMidiFreqFromKeyNum(int keynum)
        {
            // MIDI key indexes go between 21 (low A, 27.5 Hz) and 128 (high G#, 13289.75 Hz). Indexes can run down to 0 (very low C at 8.1 Hz), well outside
            // the low end for human hearing (20 Hz is the general limit). Middle A 440 Hz is index 69.
            if (keynum < 0 || keynum > 128)
            {
                throw new ArgumentOutOfRangeException("Specified MIDI key index is out of expected range");
            }
            if(keynum >= 0 && keynum < 8)
            {
                return notes[keynum + 3].base_freq / 64;
            }
            else if (keynum >= 9 && keynum <= 20)
            {
                return notes[keynum - 9].base_freq / 32;
            }
            else if (keynum >= 21 && keynum <= 32)
            {
                return notes[keynum - 21].base_freq / 16;
            }
            else if(keynum >= 33 && keynum <= 44)
            {
                return notes[keynum - 33].base_freq / 8;
            }
            else if (keynum >= 45 && keynum <= 56)
            {
                return notes[keynum - 45].base_freq / 4;
            }
            else if (keynum >= 57 && keynum <= 68)
            {
                return notes[keynum - 57].base_freq / 2;
            }
            else if (keynum >= 69 && keynum <= 80)
            {
                return notes[keynum - 69].base_freq;
            }
            else if (keynum >= 81 && keynum <= 92)
            {
                return notes[keynum - 81].base_freq * 2;
            }
            else if (keynum >= 93 && keynum <= 104)
            {
                return notes[keynum - 93].base_freq * 4;
            }
            else if (keynum >= 105 && keynum <= 116)
            {
                return notes[keynum - 105].base_freq * 8;
            }
            else if (keynum >= 117 && keynum <= 128)
            {
                return notes[keynum - 117].base_freq * 16;
            }

            return 0;
        }
    }
    
}
