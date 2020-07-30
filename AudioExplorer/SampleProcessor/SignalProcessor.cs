using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace AudioExplorer.SampleProcessor
{
    /// <summary>
    /// Takes input from one or more iReadableAudioSources and performs operations on them
    /// before producing a single audio outputs. The base class for filters, mixing, etc.
    /// </summary>
    public interface SampleProcessor : IReadableAudioSource<float>
    {
    }
}
