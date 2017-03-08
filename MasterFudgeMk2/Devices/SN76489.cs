using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

using MasterFudgeMk2.Common;

namespace MasterFudgeMk2.Devices
{
    public class SN76489 : WaveProvider16
    {
        /* http://www.smspower.org/Development/SN76489 */
        /* Differences in various system's PSGs: http://forums.nesdev.com/viewtopic.php?p=190216#p190216 */

        // TODO: compare to Cydrak's code for higan! https://gitlab.com/higan/higan/tree/master/higan/ms/psg
        // TODO: better audio backend stuff! similar to https://github.com/fredericmeyer/nanoboy/tree/master/nanoboy/nanoboy/Core/Audio/Backend ??

        // (old TODOs)
        // TODO: sample generation and output is pretty garbage, probably timing and shit too
        // TODO: eventually, when/if this gets less garbage, look into whatever the Game Gear has here in addition

        const int numChannels = 4;

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

        /* Variables for output */
        int currentSamplePosition;

        /* Generated samples */
        short[] samples;

        // TEMP: wavewriter
        Common.AudioBackend.WavWriter wavWriter;

        public SN76489(double clockRate, double refreshRate, int sampleRate)
        {
            this.clockRate = clockRate;
            this.refreshRate = refreshRate;

            volumeRegisters = new ushort[numChannels];
            toneRegisters = new ushort[numChannels];

            channelCounters = new ushort[numChannels];
            channelOutput = new bool[numChannels];

            /* https://gitlab.com/higan/higan/blob/master/higan/ms/psg/psg.cpp */
            volumeTable = new ushort[16];
            for (int i = 0; i < volumeTable.Length; i++)
                volumeTable[i] = (ushort)(0x2000 * Math.Pow(2.0, i * -2.0 / 6.0) + 0.5);
            volumeTable[15] = 0;

            /* For NAudio WaveProvider16 */
            SetWaveFormat(sampleRate, 1);

            clockCyclesPerLine = (int)(((clockRate / refreshRate) / TMS9918A.NumScanlinesNtsc) / 16.0);

            samples = new short[65535];
        }

        public virtual void Startup()
        {
            // TEMP
            wavWriter = new Common.AudioBackend.WavWriter();

            Reset();
        }

        public virtual void Shutdown()
        {
            // TEMP
            wavWriter?.Save(@"E:\temp\sms\new\test.wav");
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
            currentSamplePosition = 0;
        }

        public void Step(int clockCyclesInStep)
        {


            // TODO TIMING  go over what byuu's said again, figure out how to tick this damn thing at its correct rate (i.e not at 3.58mhz but that /16), etc, etcccccccc...zzzzzzz


            int totalOutput = 0;

            bool endofline = ((cycleCount + clockCyclesInStep) >= clockCyclesPerLine);
            cycleCount = ((cycleCount + clockCyclesInStep) % clockCyclesPerLine);

            if (endofline)
            {
                /* Process tone channels */
                for (int ch = 0; ch < 3; ch++)
                {
                    /* Check for counter underflow */
                    if ((channelCounters[ch] & 0x03FF) > 0)
                        channelCounters[ch]--;
                }
            }

            /* Process tone channels */
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

            /* Process tone channel */
            // TODO: noise channel here

            /* Prepare output */
            int speakerOutput = (totalOutput - 32768);

            //if (totalOutput != 0)
            {
                // TEMP
                wavWriter.AddSampleData(new short[] { (short)totalOutput });


                if (currentSamplePosition < samples.Length)
                    samples[currentSamplePosition] = (short)totalOutput;
                currentSamplePosition++;
            }
        }

        /* For NAudio WaveProvider16 */
        public override int Read(short[] buffer, int offset, int sampleCount)
        {
            Buffer.BlockCopy(samples, 0, buffer, 0, sampleCount * 2);
            currentSamplePosition = 0;
            return sampleCount;
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
    }
}
