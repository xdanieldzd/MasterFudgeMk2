using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.EventArguments;

namespace MasterFudgeMk2.AudioBackends
{
    // TODO: still not perfect, eventually becomes scratchy & crackles for a sec, then is okay again...?

    [Description("XAudio2 (SharpDX)")]
    public class XAudio2Backend : MustInitialize<int>, IAudioBackend
    {
        XAudio2 xaudio2;

        MasteringVoice masteringVoice;
        SourceVoice sourceVoice;

        short[] samples;
        DataStream dataStream;

        public XAudio2Backend(int sampleRate, int numChannels) : base(sampleRate, numChannels)
        {
            xaudio2 = new XAudio2();

            masteringVoice = new MasteringVoice(xaudio2);
            var waveFormat = new WaveFormat(sampleRate, numChannels);
            sourceVoice = new SourceVoice(xaudio2, waveFormat);

            samples = new short[sampleRate * numChannels];
            dataStream = DataStream.Create(samples, true, true);

            var audioBuffer = new AudioBuffer { Stream = dataStream, Flags = BufferFlags.EndOfStream, AudioBytes = samples.Length, LoopCount = AudioBuffer.LoopInfinite };
            sourceVoice.SubmitSourceBuffer(audioBuffer, null);
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
                if (sourceVoice != null)
                {
                    sourceVoice.Stop();
                    sourceVoice.Dispose();
                }

                if (dataStream != null) dataStream.Dispose();
                if (masteringVoice != null) masteringVoice.Dispose();
                if (xaudio2 != null) xaudio2.Dispose();
            }
        }

        public void OnAddSampleData(object sender, AddSampleDataEventArgs e)
        {
            foreach (short sample in e.Samples)
            {
                if (dataStream.Position >= samples.Length) dataStream.Position = 0;
                dataStream.Write(sample);
            }
        }

        public void Play()
        {
            sourceVoice.Start();
        }

        public void Stop()
        {
            sourceVoice.Stop();
        }

        public void Reset()
        {
            for (int i = 0; i < samples.Length; i++) samples[i] = 0;
            dataStream.Position = 0;
        }
    }
}
