using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// LFOs take in a time offset and produce a value in [-1..1]
namespace AudioExplorer.Scalar
{
    /// <summary>
    ///     Reads a sequence of samples from the <see cref="Scalar" />.
    /// </summary>
    /// <param name="buffer">
    ///     An array of floats. When this method returns, the <paramref name="buffer" /> contains the specified
    ///     float array with the values between <paramref name="offset" /> and (<paramref name="offset" /> +
    ///     <paramref name="count" /> - 1) replaced by the floats read from the current source.
    /// </param>
    /// <param name="offset">
    ///     The zero-based offset in the <paramref name="buffer" /> at which to begin storing the data
    ///     read from the current stream.
    /// </param>
    /// <param name="count">The maximum number of samples to read from the current source.</param>
    /// <returns>The total number of samples read into the buffer.</returns>
    abstract class Scalar
    {
        public const float LFO_LO = -1;
        public const float LFO_HI = 1;

        public abstract int Read(float[] buffer, int offset, int count);

        // clamps value between LFO_LOW and FLO_HIGH inclusive.
        public virtual float LFOClamp(float value)
        {
            if (value < LFO_LO)
            {
                return LFO_LO;
            }
            else if (value > LFO_HI)
            {
                return LFO_HI;
            }
            return value;
        }

    }
}
