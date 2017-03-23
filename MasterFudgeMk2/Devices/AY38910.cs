using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.EventArguments;

namespace MasterFudgeMk2.Devices
{
    /* General Instruments AY-3-8910 PSG -- http://problemkaputt.de/portar.htm#soundgenerator */
    public class AY38910 : MustInitialize<EventHandler<AddSampleDataEventArgs>>, ISoundDevice
    {
        // TODO: everything that's actually related to sound, also double-check the IO ports (input/output enable stuff)

        byte[] registers;
        byte registerIndex;

        ushort channelAFrequency { get { return (ushort)(((registers[0x01] & 0x0F) << 8) | (registers[0x00])); } }
        ushort channelBFrequency { get { return (ushort)(((registers[0x03] & 0x0F) << 8) | (registers[0x02])); } }
        ushort channelCFrequency { get { return (ushort)(((registers[0x05] & 0x0F) << 8) | (registers[0x04])); } }

        byte noisePeriod { get { return (byte)(registers[0x06] & 0x1F); } }

        bool isChannelAToneEnabled { get { return BitUtilities.IsBitSet(registers[0x07], 0); } }
        bool isChannelBToneEnabled { get { return BitUtilities.IsBitSet(registers[0x07], 1); } }
        bool isChannelCToneEnabled { get { return BitUtilities.IsBitSet(registers[0x07], 2); } }
        bool isChannelANoiseEnabled { get { return BitUtilities.IsBitSet(registers[0x07], 3); } }
        bool isChannelBNoiseEnabled { get { return BitUtilities.IsBitSet(registers[0x07], 4); } }
        bool isChannelCNoiseEnabled { get { return BitUtilities.IsBitSet(registers[0x07], 5); } }
        bool isIoPortAOutput { get { return BitUtilities.IsBitSet(registers[0x07], 6); } }
        bool isIoPortBOutput { get { return BitUtilities.IsBitSet(registers[0x07], 7); } }

        byte channelAVolume { get { return (byte)(registers[0x08] & 0x0F); } }
        bool isChannelAEnvelope { get { return ((registers[0x08] & 0x10) == 0x10); } }
        byte channelBVolume { get { return (byte)(registers[0x09] & 0x0F); } }
        bool isChannelBEnvelope { get { return ((registers[0x09] & 0x10) == 0x10); } }
        byte channelCVolume { get { return (byte)(registers[0x0A] & 0x0F); } }
        bool isChannelCEnvelope { get { return ((registers[0x0A] & 0x10) == 0x10); } }

        ushort envelopeFrequency { get { return (ushort)((registers[0x0C] << 8) | (registers[0x0B])); } }

        byte envelopeShape { get { return (byte)(registers[0x0D] & 0x0F); } }

        public event EventHandler<AddSampleDataEventArgs> OnAddSampleData;
        double clockRate, refreshRate;
        double cycleCount;

        public AY38910(double clockRate, double refreshRate, EventHandler<AddSampleDataEventArgs> addSampleDataEvent) : base(addSampleDataEvent)
        {
            this.clockRate = clockRate;
            this.refreshRate = refreshRate;

            registers = new byte[16];

            //

            OnAddSampleData += addSampleDataEvent;

            //
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
            cycleCount = 0.0;
        }

        public void Step(int clockCyclesInStep)
        {
            //
        }

        public void WriteIndex(byte index)
        {
            registerIndex = (byte)(index & (registers.Length - 1));
        }

        public void WriteData(byte value)
        {
            registers[registerIndex] = value;
        }

        public byte ReadData()
        {
            return registers[registerIndex];
        }

        public void WriteDataDirect(byte register, byte value)
        {
            registers[register] = value;
        }
    }
}
