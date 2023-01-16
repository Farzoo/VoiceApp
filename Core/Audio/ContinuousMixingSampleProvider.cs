using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Core.Audio;

public class ContinuousMixingSampleProvider : ISampleProvider
{
    private readonly List<ISampleProvider> _sources;
    private float[] _sourceBuffer;
    public ContinuousMixingSampleProvider(WaveFormat waveFormat)
    {
      if (waveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
        throw new ArgumentException("Mixer wave format must be IEEE float");
      this._sources = new List<ISampleProvider>();
      this.WaveFormat = waveFormat;
    }

    public ContinuousMixingSampleProvider(IEnumerable<ISampleProvider> sources)
    {
      this._sources = new List<ISampleProvider>();
      foreach (ISampleProvider source in sources)
        this.AddMixerInput(source);
      if (this._sources.Count == 0)
        throw new ArgumentException("Must provide at least one input in this constructor");
    }

    public IEnumerable<ISampleProvider> MixerInputs => this._sources;

    public bool ReadFully { get; set; }

    public void AddMixerInput(IWaveProvider mixerInput) => this.AddMixerInput(SampleProviderConverters.ConvertWaveProviderIntoSampleProvider(mixerInput));
    public void AddMixerInput(ISampleProvider mixerInput)
    {
      lock (this._sources)
      {
        if (this._sources.Count >= 1024)
          throw new InvalidOperationException("Too many mixer inputs");
        this._sources.Add(mixerInput);
      }
      if (this.WaveFormat == null)
        this.WaveFormat = mixerInput.WaveFormat;
      else if (this.WaveFormat.SampleRate != mixerInput.WaveFormat.SampleRate || this.WaveFormat.Channels != mixerInput.WaveFormat.Channels)
        throw new ArgumentException("All mixer inputs must have the same WaveFormat");
    }

    public event EventHandler<SampleProviderEventArgs> MixerInputEnded;

    public void RemoveMixerInput(ISampleProvider mixerInput)
    {
      lock (this._sources)
        this._sources.Remove(mixerInput);
    }

    public void RemoveAllMixerInputs()
    {
      lock (this._sources)
        this._sources.Clear();
    }

    public WaveFormat WaveFormat { get; private set; }

    public int Read(float[] buffer, int offset, int count)
    {
      int val2 = 0;
      this._sourceBuffer = BufferHelpers.Ensure(this._sourceBuffer, count);
      lock (this._sources)
      {
        for (int index1 = 0; index1 < this._sources.Count; index1++)
        {
          ISampleProvider source = this._sources[index1];
          int val1 = source.Read(this._sourceBuffer, 0, count);
          int num = offset;
          for (int index2 = 0; index2 < val1; ++index2)
          {
            if (index2 >= val2)
              buffer[num++] = this._sourceBuffer[index2];
            else
              buffer[num++] += this._sourceBuffer[index2];
          }
          val2 = Math.Max(val1, val2);
          if (val1 < count)
          {
            EventHandler<SampleProviderEventArgs> mixerInputEnded = this.MixerInputEnded;
            if (mixerInputEnded != null)
              mixerInputEnded((object) this, new SampleProviderEventArgs(source));
          }
        }
      }
      if (this.ReadFully && val2 < count)
      {
        int num = offset + val2;
        while (num < offset + count)
          buffer[num++] = 0.0f;
        val2 = count;
      }
      return val2;
    }
}