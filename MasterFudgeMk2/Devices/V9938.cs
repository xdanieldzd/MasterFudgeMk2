using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MasterFudgeMk2.Common;

namespace MasterFudgeMk2.Devices
{
    /* Yamaha V9938, found in MSX2 systems */

    // TODO: EVERYTHING

    public class V9938 : TMS9918A
    {
        public const int NumActiveScanlinesLow = NumActiveScanlines;
        public const int NumActiveScanlinesHigh = 212;

        protected bool isBitM4Set { get { return BitUtilities.IsBitSet(registers[0x00], 2); } }
        protected bool isBitM5Set { get { return BitUtilities.IsBitSet(registers[0x00], 3); } }
        protected bool isHBlankInterruptEnabled { get { return BitUtilities.IsBitSet(registers[0x00], 4); } }

        protected bool isOBJSpritesDisabled { get { return BitUtilities.IsBitSet(registers[0x08], 1); } }
        protected int vramConfiguration { get { return (byte)((registers[0x08] >> 2) & 0x03); } }               /* 0: 1*16KB, 1: 4*16KB, 2: 1*64KB: 3=64KB/HighSpeed */
        protected bool isColorBusInput { get { return BitUtilities.IsBitSet(registers[0x08], 4); } }
        protected bool isPaletteColor0Solid { get { return BitUtilities.IsBitSet(registers[0x08], 5); } }

        //

        protected int screenHeight { get { return (BitUtilities.IsBitSet(registers[0x09], 7) ? NumActiveScanlinesLow : NumActiveScanlinesHigh); } }

        protected ushort colorTableBaseAddress { get { return (ushort)(((registers[0x0A] & 0x07) << 14) | (registers[0x03] << 6)); } }
        protected override ushort spriteAttribTableBaseAddress { get { return (ushort)(((registers[0x0B] & 0x03) << 15) | (registers[0x05] & 0xFF) << 7); } }

        public V9938(double clockRate, double refreshRate, bool isPalChip) : base(clockRate, refreshRate, isPalChip)
        {
            registers = new byte[0x2E];

            screenUsage = new byte[NumActivePixelsPerScanline * NumTotalScanlines];

            outputFramebuffer = new byte[(NumActivePixelsPerScanline * NumTotalScanlines) * 4];
        }

        public void WriteColorPaletteRegister(byte value)
        {
            //
        }

        public void WriteRegisterData(byte value)
        {
            byte registerIndex = (byte)(registers[0x11] & 0x3F);
            bool autoIncrement = BitUtilities.IsBitSet(registers[0x11], 7);

            if (registerIndex != 0x11)
                registers[registerIndex] = value;

            if (autoIncrement)
                registers[registerIndex] = (byte)((registers[registerIndex] & 0xC0) | ((registerIndex + 1) & 0x3F));
        }
    }
}
