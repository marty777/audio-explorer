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
    }
}
