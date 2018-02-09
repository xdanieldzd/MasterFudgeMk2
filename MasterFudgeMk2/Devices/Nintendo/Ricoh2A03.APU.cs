using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.EventArguments;

namespace MasterFudgeMk2.Devices.Nintendo
{
    public partial class Ricoh2A03
    {
        public class APU : MustInitialize<EventHandler<AddSampleDataEventArgs>>, ISoundDevice
        {
            /* Sample generation & event handling */
            public event EventHandler<AddSampleDataEventArgs> OnAddSampleData;
            protected List<short> sampleBuffer;

            /* Audio output variables */
            protected int sampleRate, numOutputChannels;

            /* APU registers */
            byte[] registers = new byte[0x18];

            //

            /* Timing variables */
            double clockRate, refreshRate;
            int samplesPerFrame, cyclesPerFrame, cyclesPerSample;
            int sampleCycleCount, frameCycleCount, dividerCount;

            public APU(double clockRate, double refreshRate, int sampleRate, int numOutputChannels, EventHandler<AddSampleDataEventArgs> addSampleDataEvent) : base(addSampleDataEvent)
            {
                this.sampleRate = sampleRate;
                this.numOutputChannels = numOutputChannels;

                this.clockRate = clockRate;
                this.refreshRate = refreshRate;

                samplesPerFrame = (int)(this.sampleRate / refreshRate);
                cyclesPerFrame = (int)(this.clockRate / refreshRate);
                cyclesPerSample = (cyclesPerFrame / samplesPerFrame);

                OnAddSampleData += addSampleDataEvent;

                sampleBuffer = new List<short>();
            }

            public virtual void Startup()
            {
                Reset();
            }

            public virtual void Shutdown()
            {
                //
            }

            public virtual void Reset()
            {
                sampleBuffer.Clear();
                OnAddSampleData?.Invoke(this, new AddSampleDataEventArgs(sampleBuffer.ToArray()));

                //

                sampleCycleCount = frameCycleCount = dividerCount = 0;
            }

            public void Step(int clockCyclesInStep)
            {
                // TODO~~
            }

            public void WriteRegister(uint address, byte value)
            {
                byte register = (byte)(address & 0x17);
                registers[register] = value;
            }

            public byte ReadRegister(uint address)
            {
                byte register = (byte)(address & 0x17);
                return registers[register];
            }
        }
    }
}
