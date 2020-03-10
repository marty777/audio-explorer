using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace AudioExplorer.Audio
{
    class SoundMixer : ISampleSource
    {
        private readonly WaveFormat _waveFormat;
        private readonly List<ISampleSource> _sampleSources = new List<ISampleSource>();
        private readonly List<float> _sampleVolumes = new List<float>(); // between 0.0 and 1.0
        private readonly object _lockObj = new object();
        private float[] _mixerBuffer;

        public bool FillWithZeros { get; set; }

        public bool DivideResult { get; set; }

        public SoundMixer(int channelCount, int sampleRate)
        {
            if (channelCount < 1)
                throw new ArgumentOutOfRangeException("channelCount");
            if (sampleRate < 1)
                throw new ArgumentOutOfRangeException("sampleRate");

            _waveFormat = new WaveFormat(sampleRate, 32, channelCount, AudioEncoding.IeeeFloat);
            FillWithZeros = false;
        }

        public void AddSource(ISampleSource source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (source.WaveFormat.Channels != WaveFormat.Channels ||
               source.WaveFormat.SampleRate != WaveFormat.SampleRate)
                throw new ArgumentException("Invalid format.", "source");

            lock (_lockObj)
            {
                if (!Contains(source))
                {
                    _sampleSources.Add(source);
                    _sampleVolumes.Add(1.0f);
                }
            }
            
        }

        public void setSourceVolume(int index, float volume)
        {
            if (index < 0 || index >= _sampleVolumes.Count)
            {
                throw new IndexOutOfRangeException(String.Format("Index {0} outside range [0..{1}]", index, _sampleVolumes.Count));
            }
            if(volume < 0)
            {
                _sampleVolumes[index] = 0.0f;
            }
            else if(volume > 1.0f)
            {
                _sampleVolumes[index] = 1.0f;
            }
            else
            {
                _sampleVolumes[index] = volume;
            }
        }
        
        public void RemoveSource(ISampleSource source)
        {
            //don't throw null ex here
            lock (_lockObj)
            {
                if (Contains(source))
                {
                    int index = _sampleSources.IndexOf(source);
                    _sampleSources.RemoveAt(index);
                    _sampleVolumes.RemoveAt(index);
                }
            }
        }

        public void RemoveAllSources()
        {
            lock (_lockObj)
            {
                _sampleSources.Clear();
                _sampleVolumes.Clear();
            }
        }

        public int numSources()
        {
            return _sampleSources.Count;
        }

        public bool Contains(ISampleSource source)
        {
            if (source == null)
                return false;
            return _sampleSources.Contains(source);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int numberOfStoredSamples = 0;

            if (count > 0 && _sampleSources.Count > 0)
            {
                lock (_lockObj)
                {
                    _mixerBuffer = _mixerBuffer.CheckBuffer(count);
                    List<int> numberOfReadSamples = new List<int>();
                    for (int m = _sampleSources.Count - 1; m >= 0; m--)
                    {
                        if(_sampleVolumes[m] == 0)
                        {
                            continue;
                        }
                        var sampleSource = _sampleSources[m];
                        int read = sampleSource.Read(_mixerBuffer, 0, count);
                        for (int i = offset, n = 0; n < read; i++, n++)
                        {
                            //if (numberOfStoredSamples <= i)
                            //    buffer[i] = (m == 1 ? -_mixerBuffer[n] : _mixerBuffer[n]);
                            //else
                            //    buffer[i] += (m == 1 ? -_mixerBuffer[n] : _mixerBuffer[n]);

                            //if (numberOfStoredSamples <= i)
                            //    buffer[i] = (m == 1 ? -_mixerBuffer[n] : _mixerBuffer[n]);
                            //else
                                buffer[i] +=  _mixerBuffer[n] * _sampleVolumes[m];
                        }
                        if (read > numberOfStoredSamples)
                            numberOfStoredSamples = read;

                        if (read > 0)
                            numberOfReadSamples.Add(read);
                        else
                        {
                            //raise event here
                            RemoveSource(sampleSource); //remove the input to make sure that the event gets only raised once.
                        }
                    }

                    if (DivideResult)
                    {
                        numberOfReadSamples.Sort();
                        int currentOffset = offset;
                        int remainingSources = numberOfReadSamples.Count;

                        foreach (var readSamples in numberOfReadSamples)
                        {
                            if (remainingSources == 0)
                                break;

                            while (currentOffset < offset + readSamples)
                            {
                                buffer[currentOffset] /= remainingSources;
                                buffer[currentOffset] = Math.Max(-1, Math.Min(1, buffer[currentOffset]));
                                currentOffset++;
                            }
                            remainingSources--;
                        }
                    }
                }
            }

            if (FillWithZeros && numberOfStoredSamples != count)
            {
                Array.Clear(
                    buffer,
                    Math.Max(offset + numberOfStoredSamples - 1, 0),
                    count - numberOfStoredSamples);

                return count;
            }

            return numberOfStoredSamples;
        }

        public bool CanSeek { get { return false; } }

        public WaveFormat WaveFormat
        {
            get { return _waveFormat; }
        }

        public long Position
        {
            get { return 0; }
            set
            {
                throw new NotSupportedException();
            }
        }

        public long Length
        {
            get { return 0; }
        }

        public void Dispose()
        {
            lock (_lockObj)
            {
                foreach (var sampleSource in _sampleSources.ToArray())
                {
                    sampleSource.Dispose();
                    _sampleSources.Remove(sampleSource);
                }
                _sampleVolumes.Clear();
            }
        }
    }
}
