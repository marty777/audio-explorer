using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace AudioExplorer.Modulation
{
    /// <summary>
    /// Provides a modulator value that can change over time. Base class for ADSR, LFO, etc.
    /// </summary>
    public interface Modulator: IReadableAudioSource<float>
    {
        
    }
}
