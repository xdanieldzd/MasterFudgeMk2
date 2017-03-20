using System;
using System.Collections.Generic;
using System.ComponentModel;

using NAudio.Wave;

namespace MasterFudgeMk2.Common.AudioBackend
{
    [Description("NAudio (WaveOut)")]
    public class NAudioBackend : MustInitialize<int>, IAudioBackend
    {
        EmulatorWaveProvider waveProvider;
        WaveOut waveOutDevice;

        public float Volume
        {
            get { return waveOutDevice.Volume; }
            set { waveOutDevice.Volume = value; }
        }

        public NAudioBackend(int sampleRate, int numChannels) : base(sampleRate, numChannels)
        {
            waveProvider = new EmulatorWaveProvider(sampleRate, numChannels);

            waveOutDevice = new WaveOut();
            waveOutDevice.Init(waveProvider);
            waveOutDevice.Play();

            waveOutDevice.Volume = 1.0f;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (waveOutDevice != null)
                {
                    waveOutDevice.Stop();
                    waveOutDevice.Dispose();
                    waveOutDevice = null;
                }
            }
        }

        public void AddSampleData(short[] samples)
        {
            waveProvider?.AddSampleData(samples);
        }

        public void Play()
        {
            waveOutDevice?.Play();
        }

        public void Stop()
        {
            waveOutDevice?.Stop();
        }

        public void Reset()
        {
            waveProvider?.ClearSampleData();
        }
    }

    class EmulatorWaveProvider : WaveProvider16
    {
        Queue<short> samples;

        public EmulatorWaveProvider(int sampleRate, int numChannels)
        {
            SetWaveFormat(sampleRate, numChannels);

            samples = new Queue<short>();
        }

        public void AddSampleData(short[] samples)
        {
            foreach (short sample in samples)
                this.samples.Enqueue(sample);
        }

        public void ClearSampleData()
        {
            samples.Clear();
        }

        public override int Read(short[] buffer, int offset, int sampleCount)
        {
            for (int n = 0; n < sampleCount; n++)
            {
                if (samples.Count > 0)
                    buffer[n + offset] = samples.Dequeue();
            }
            return sampleCount;
        }
    }
}
