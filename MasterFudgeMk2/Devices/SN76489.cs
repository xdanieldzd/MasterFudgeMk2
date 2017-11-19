using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.EventArguments;

namespace MasterFudgeMk2.Devices
{
    public class SN76489 : MustInitialize<EventHandler<AddSampleDataEventArgs>>, ISoundDevice
    {
        /* http://www.smspower.org/Development/SN76489 */
        /* Differences in various system's PSGs: http://forums.nesdev.com/viewtopic.php?p=190216#p190216 */

        // TODO: compare to Cydrak's code for higan! https://gitlab.com/higan/higan/tree/master/higan/ms/psg
        // TODO: eventually, when/if this gets less garbage, look into whatever the Game Gear has here in addition (stereo?)

        const int numChannels = 4, numToneChannels = 3, noiseChannelIndex = 3;

        /* Noise generation constants */
        protected virtual ushort noiseLfsrMask => 0x7FFF;
        protected virtual ushort noiseTappedBits => 0x0003;     /* Bits 0 and 1 */
        protected virtual int noiseBitShift => 14;

        /* Sample generation & event handling */
        public event EventHandler<AddSampleDataEventArgs> OnAddSampleData;
        List<short> sampleBuffer;

        /* Channel registers */
        ushort[] volumeRegisters;       /* Channels 0-3: 4 bits */
        ushort[] toneRegisters;         /* Channels 0-2 (tone): 10 bits; channel 3 (noise): 3 bits */

        /* Channel counters */
        ushort[] channelCounters;       /* 10-bit counters */
        bool[] channelOutput;

        /* Volume attenuation table */
        ushort[] volumeTable;           /* 2dB change per volume register step */

        /* Latched channel/type */
        byte latchedChannel, latchedType;

        /* Linear-feedback shift register, for noise generation */
        protected ushort noiseLfsr;     /* 15-bit */

        /* Clock & refresh rates */
        double clockRate, refreshRate;
        double cycleCount;
        int stepCount;

        public SN76489(double clockRate, double refreshRate, EventHandler<AddSampleDataEventArgs> addSampleDataEvent) : base(addSampleDataEvent)
        {
            this.clockRate = clockRate;
            this.refreshRate = refreshRate;

            OnAddSampleData += addSampleDataEvent;

            sampleBuffer = new List<short>(1024);

            volumeRegisters = new ushort[numChannels];
            toneRegisters = new ushort[numChannels];

            channelCounters = new ushort[numChannels];
            channelOutput = new bool[numChannels];

            /* https://gitlab.com/higan/higan/blob/master/higan/ms/psg/psg.cpp */
            volumeTable = new ushort[16];
            for (int i = 0; i < volumeTable.Length; i++)
                volumeTable[i] = (ushort)(0x2000 * Math.Pow(2.0, i * -2.0 / 6.0) + 0.5);
            volumeTable[15] = 0;
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

            latchedChannel = latchedType = 0x00;
            noiseLfsr = 0x4000;

            for (int i = 0; i < numChannels; i++)
            {
                volumeRegisters[i] = 0x000F;
                toneRegisters[i] = 0x0000;
            }

            cycleCount = 0.0;
            stepCount = 0;
        }

        public void Step(int clockCyclesInStep)
        {
            // TODO: very weird, probably terrible executing and all, but... okay-ish? maybe...?

            double limit = ((clockRate / refreshRate) / (44100.0 / refreshRate));

            for (int i = 0; i < clockCyclesInStep; i++)
            {
                stepCount++;
                if (stepCount == 16)
                {
                    for (int ch = 0; ch < numToneChannels; ch++)
                        StepToneChannel(ch);
                    StepNoiseChannel();

                    stepCount = 0;
                }

                cycleCount++;
                if (cycleCount >= limit)
                {
                    MixSamples();
                    cycleCount = 0;
                }
            }
        }

        private void StepToneChannel(int ch)
        {
            /* Check for counter underflow */
            if ((channelCounters[ch] & 0x03FF) > 0)
                channelCounters[ch]--;

            /* Counter underflowed, reload and flip output bit, then generate sample */
            if ((channelCounters[ch] & 0x03FF) == 0)
            {
                channelCounters[ch] = (ushort)(toneRegisters[ch] & 0x03FF);
                channelOutput[ch] = !channelOutput[ch];
            }
        }

        private void StepNoiseChannel()
        {
            int chN = noiseChannelIndex;
            {
                /* Check for counter underflow */
                if ((channelCounters[chN] & 0x03FF) > 0)
                    channelCounters[chN]--;

                /* Counter underflowed, reload and flip output bit */
                if ((channelCounters[chN] & 0x03FF) == 0)
                {
                    switch (toneRegisters[chN] & 0x3)
                    {
                        case 0x0: channelCounters[chN] = 0x10; break;
                        case 0x1: channelCounters[chN] = 0x20; break;
                        case 0x2: channelCounters[chN] = 0x40; break;
                        case 0x3: channelCounters[chN] = (ushort)(toneRegisters[2] & 0x03FF); break;
                    }
                    channelOutput[chN] = !channelOutput[chN];

                    if (channelOutput[chN])
                    {
                        /* Check noise type, then generate sample */
                        bool isWhiteNoise = (((toneRegisters[chN] >> 2) & 0x1) == 0x1);

                        // TODO: SMS/GG == bits 0 and 3 (0009) into 15; SG-1000/CV == bits 0 and 1 (0003) into 14!
                        ushort newLfsrBit = (ushort)((isWhiteNoise ? CheckParity((ushort)(noiseLfsr & noiseTappedBits)) : (noiseLfsr & 0x01)) << noiseBitShift);

                        noiseLfsr = (ushort)((newLfsrBit | (noiseLfsr >> 1)) & noiseLfsrMask);
                    }
                }
            }
        }

        private void MixSamples()
        {
            /* Mix samples together */
            short mixed = 0;
            mixed += (short)(volumeTable[volumeRegisters[0]] * (channelOutput[0] ? 0.5 : -0.5));
            mixed += (short)(volumeTable[volumeRegisters[1]] * (channelOutput[1] ? 0.5 : -0.5));
            mixed += (short)(volumeTable[volumeRegisters[2]] * (channelOutput[2] ? 0.5 : -0.5));
            mixed += (short)(volumeTable[volumeRegisters[3]] * (noiseLfsr & 0x1));

            if (false)
            {
                // TEMP crappy sinewave test thingy
                sineCount = ((sineCount + 1) % sineWave.Length);
                short sineTemp = (short)(sineWave[sineCount] << 6);
                mixed = (short)(sineTemp - ((sineWave.Max() << 6) / 2));
            }

            /* Enqueue sample */
            sampleBuffer.Add(mixed);
            if (sampleBuffer.Count == sampleBuffer.Capacity)
            {
                OnAddSampleData?.Invoke(this, new AddSampleDataEventArgs(sampleBuffer.ToArray()));
                sampleBuffer.Clear();
            }
        }

        private ushort CheckParity(ushort val)
        {
            val ^= (ushort)(val >> 8);
            val ^= (ushort)(val >> 4);
            val ^= (ushort)(val >> 2);
            val ^= (ushort)(val >> 1);
            return (ushort)(val & 0x1);
        }

        public void WriteData(byte data)
        {
            if (BitUtilities.IsBitSet(data, 7))
            {
                /* LATCH/DATA byte; get channel (0-3) and type (0 is tone/noise, 1 is volume) */
                latchedChannel = (byte)((data >> 5) & 0x03);
                latchedType = (byte)((data >> 4) & 0x01);

                /* Mask off non-data bits */
                data &= 0x0F;

                /* If target is channel 3 noise (3 bits), mask off highest bit */
                if (latchedChannel == 3 && latchedType == 0)
                    data &= 0x07;

                /* Write to register */
                if (latchedType == 0)
                {
                    /* Data is tone/noise */
                    toneRegisters[latchedChannel] = (ushort)((toneRegisters[latchedChannel] & 0x03F0) | data);
                }
                else
                {
                    /* Data is volume */
                    volumeRegisters[latchedChannel] = data;
                }
            }
            else
            {
                /* DATA byte; mask off non-data bits */
                data &= 0x3F;

                /* Write to register */
                if (latchedType == 0)
                {
                    /* Data is tone/noise */
                    if (latchedChannel == 3)
                    {
                        /* Target is channel 3 noise, mask off excess bits and write to low bits of register */
                        toneRegisters[latchedChannel] = (ushort)(data & 0x07);
                    }
                    else
                    {
                        /* Target is not channel 3 noise, write to high bits of register */
                        toneRegisters[latchedChannel] = (ushort)((toneRegisters[latchedChannel] & 0x000F) | (data << 4));
                    }
                }
                else
                {
                    /* Data is volume; mask off excess bits and write to low bits of register */
                    volumeRegisters[latchedChannel] = (ushort)(data & 0x0F);
                }
            }
        }

        static int sineCount = 0;
        /* https://gist.github.com/funkfinger/965900 */
        static short[] sineWave = new short[] {
            0x80, 0x83, 0x86, 0x89, 0x8C, 0x90, 0x93, 0x96,
            0x99, 0x9C, 0x9F, 0xA2, 0xA5, 0xA8, 0xAB, 0xAE,
            0xB1, 0xB3, 0xB6, 0xB9, 0xBC, 0xBF, 0xC1, 0xC4,
            0xC7, 0xC9, 0xCC, 0xCE, 0xD1, 0xD3, 0xD5, 0xD8,
            0xDA, 0xDC, 0xDE, 0xE0, 0xE2, 0xE4, 0xE6, 0xE8,
            0xEA, 0xEB, 0xED, 0xEF, 0xF0, 0xF1, 0xF3, 0xF4,
            0xF5, 0xF6, 0xF8, 0xF9, 0xFA, 0xFA, 0xFB, 0xFC,
            0xFD, 0xFD, 0xFE, 0xFE, 0xFE, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFE, 0xFE, 0xFE, 0xFD,
            0xFD, 0xFC, 0xFB, 0xFA, 0xFA, 0xF9, 0xF8, 0xF6,
            0xF5, 0xF4, 0xF3, 0xF1, 0xF0, 0xEF, 0xED, 0xEB,
            0xEA, 0xE8, 0xE6, 0xE4, 0xE2, 0xE0, 0xDE, 0xDC,
            0xDA, 0xD8, 0xD5, 0xD3, 0xD1, 0xCE, 0xCC, 0xC9,
            0xC7, 0xC4, 0xC1, 0xBF, 0xBC, 0xB9, 0xB6, 0xB3,
            0xB1, 0xAE, 0xAB, 0xA8, 0xA5, 0xA2, 0x9F, 0x9C,
            0x99, 0x96, 0x93, 0x90, 0x8C, 0x89, 0x86, 0x83,
            0x80, 0x7D, 0x7A, 0x77, 0x74, 0x70, 0x6D, 0x6A,
            0x67, 0x64, 0x61, 0x5E, 0x5B, 0x58, 0x55, 0x52,
            0x4F, 0x4D, 0x4A, 0x47, 0x44, 0x41, 0x3F, 0x3C,
            0x39, 0x37, 0x34, 0x32, 0x2F, 0x2D, 0x2B, 0x28,
            0x26, 0x24, 0x22, 0x20, 0x1E, 0x1C, 0x1A, 0x18,
            0x16, 0x15, 0x13, 0x11, 0x10, 0x0F, 0x0D, 0x0C,
            0x0B, 0x0A, 0x08, 0x07, 0x06, 0x06, 0x05, 0x04,
            0x03, 0x03, 0x02, 0x02, 0x02, 0x01, 0x01, 0x01,
            0x01, 0x01, 0x01, 0x01, 0x02, 0x02, 0x02, 0x03,
            0x03, 0x04, 0x05, 0x06, 0x06, 0x07, 0x08, 0x0A,
            0x0B, 0x0C, 0x0D, 0x0F, 0x10, 0x11, 0x13, 0x15,
            0x16, 0x18, 0x1A, 0x1C, 0x1E, 0x20, 0x22, 0x24,
            0x26, 0x28, 0x2B, 0x2D, 0x2F, 0x32, 0x34, 0x37,
            0x39, 0x3C, 0x3F, 0x41, 0x44, 0x47, 0x4A, 0x4D,
            0x4F, 0x52, 0x55, 0x58, 0x5B, 0x5E, 0x61, 0x64,
            0x67, 0x6A, 0x6D, 0x70, 0x74, 0x77, 0x7A, 0x7D
        };
    }
}
