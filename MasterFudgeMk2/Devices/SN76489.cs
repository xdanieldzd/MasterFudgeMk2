using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.AudioBackend;

namespace MasterFudgeMk2.Devices
{
    public class SN76489 : MustInitialize<EventHandler<AddSampleDataEventArgs>>, ISoundDevice
    {
        /* http://www.smspower.org/Development/SN76489 */
        /* Differences in various system's PSGs: http://forums.nesdev.com/viewtopic.php?p=190216#p190216 */

        // TODO: compare to Cydrak's code for higan! https://gitlab.com/higan/higan/tree/master/higan/ms/psg
        // TODO: better audio backend stuff! similar to https://github.com/fredericmeyer/nanoboy/tree/master/nanoboy/nanoboy/Core/Audio/Backend ??

        // (old TODOs)
        // TODO: sample generation and output is pretty garbage, probably timing and shit too
        // TODO: eventually, when/if this gets less garbage, look into whatever the Game Gear has here in addition

        const int numChannels = 4;

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

        /* Clock & refresh rates */
        int clockCyclesPerLine, cycleCount;
        double clockRate, refreshRate;

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

            clockCyclesPerLine = (int)(((clockRate / refreshRate) / TMS9918A.NumScanlinesNtsc) / 3.0);
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
            latchedChannel = latchedType = 0x00;

            for (int i = 0; i < numChannels; i++)
            {
                volumeRegisters[i] = 0x000F;
                toneRegisters[i] = 0x0000;
            }

            cycleCount = 0;
        }

        public void Step(int clockCyclesInStep)
        {
            // TODO TIMING  go over what byuu's said again, figure out how to tick this damn thing at its correct rate (i.e not at 3.58mhz but that /16), etc, etcccccccc...zzzzzzz

            /* Tick tone channels */
            for (int ch = 0; ch < 3; ch++)
            {
                /* Check for counter underflow */
                if ((channelCounters[ch] & 0x03FF) > 0) channelCounters[ch]--;
            }

            cycleCount += clockCyclesInStep;
            if (cycleCount >= clockCyclesPerLine)
            {
                /* Generate tone channel output */
                int totalOutput = 0;
                for (int ch = 0; ch < 3; ch++)
                {
                    /* Counter underflowed, reload and flip output bit */
                    if ((channelCounters[ch] & 0x03FF) == 0)
                    {
                        channelCounters[ch] = (ushort)(toneRegisters[ch] & 0x3FF);
                        channelOutput[ch] = !channelOutput[ch];

                        if (channelOutput[ch])
                            totalOutput += volumeTable[volumeRegisters[ch]];
                    }
                }

                /* Generate noise channel output */
                // TODO: noise channel here

                /* Enqueue output */
                short speakerOutput = (short)(totalOutput /*- 32768*/);

                if (false)
                {
                    // TEMP crappy sinewave test thingy
                    sineCount = ((sineCount + 1) % sineWave.Length);
                    speakerOutput = (short)(sineWave[sineCount] << 6);
                }

                sampleBuffer.Add(speakerOutput);

                if (sampleBuffer.Count == sampleBuffer.Capacity)
                {
                    OnAddSampleData?.Invoke(this, new AddSampleDataEventArgs(sampleBuffer.ToArray()));
                    sampleBuffer.Clear();
                }

                cycleCount -= clockCyclesPerLine;
            }
        }

        public void WriteData(byte data)
        {
            if (Utilities.IsBitSet(data, 7))
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
