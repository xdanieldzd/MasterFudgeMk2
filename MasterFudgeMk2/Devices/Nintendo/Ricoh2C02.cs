using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.Enumerations;

namespace MasterFudgeMk2.Devices.Nintendo
{
    public enum PPUMirroring
    {
        Horizontal = 0,
        Vertical = 1,
        FourScreen = 2
    }

    // TODO: UGH, TERRIBLE STYLE, REWRITE, also fixes, etc etc etc <_<

    public class Ricoh2C02
    {
        public delegate byte ReadChrDelegate(uint address);
        public delegate void WriteChrDelegate(uint address, byte value);

        ReadChrDelegate readChrDelegate;
        WriteChrDelegate writeChrDelegate;

        // TODO: correct values?
        public const int NumTotalScanlinesPal = 313;
        public const int NumTotalScanlinesNtsc = 261;
        public const int NumTotalPixelsPerScanline = 342;

        public const int NumActiveScanlines = 240;
        public const int NumActivePixelsPerScanline = 256;

        public const int NumSpritesMax = 64;

        static byte[][] palette =
        {
            new byte[] { 0x52, 0x52, 0x52, 0xFF },
            new byte[] { 0x51, 0x1A, 0x01, 0xFF },
            new byte[] { 0x65, 0x0F, 0x0F, 0xFF },
            new byte[] { 0x63, 0x06, 0x23, 0xFF },
            new byte[] { 0x4B, 0x03, 0x36, 0xFF },
            new byte[] { 0x26, 0x04, 0x40, 0xFF },
            new byte[] { 0x04, 0x09, 0x3F, 0xFF },
            new byte[] { 0x00, 0x13, 0x32, 0xFF },
            new byte[] { 0x00, 0x20, 0x1F, 0xFF },
            new byte[] { 0x00, 0x2A, 0x0B, 0xFF },
            new byte[] { 0x00, 0x2F, 0x00, 0xFF },
            new byte[] { 0x0A, 0x2E, 0x00, 0xFF },
            new byte[] { 0x2D, 0x26, 0x00, 0xFF },
            new byte[] { 0x00, 0x00, 0x00, 0xFF },
            new byte[] { 0x00, 0x00, 0x00, 0xFF },
            new byte[] { 0x00, 0x00, 0x00, 0xFF },
            new byte[] { 0xA0, 0xA0, 0xA0, 0xFF },
            new byte[] { 0x9D, 0x4A, 0x1E, 0xFF },
            new byte[] { 0xBC, 0x37, 0x38, 0xFF },
            new byte[] { 0xB8, 0x28, 0x58, 0xFF },
            new byte[] { 0x94, 0x21, 0x75, 0xFF },
            new byte[] { 0x5C, 0x23, 0x84, 0xFF },
            new byte[] { 0x24, 0x2E, 0x82, 0xFF },
            new byte[] { 0x00, 0x3F, 0x6F, 0xFF },
            new byte[] { 0x00, 0x52, 0x51, 0xFF },
            new byte[] { 0x00, 0x63, 0x31, 0xFF },
            new byte[] { 0x05, 0x6B, 0x1A, 0xFF },
            new byte[] { 0x2E, 0x69, 0x0E, 0xFF },
            new byte[] { 0x68, 0x5C, 0x10, 0xFF },
            new byte[] { 0x00, 0x00, 0x00, 0xFF },
            new byte[] { 0x00, 0x00, 0x00, 0xFF },
            new byte[] { 0x00, 0x00, 0x00, 0xFF },
            new byte[] { 0xFF, 0xFF, 0xFE, 0xFF },
            new byte[] { 0xFC, 0x9E, 0x69, 0xFF },
            new byte[] { 0xFF, 0x87, 0x89, 0xFF },
            new byte[] { 0xFF, 0x76, 0xAE, 0xFF },
            new byte[] { 0xF1, 0x6D, 0xCE, 0xFF },
            new byte[] { 0xB2, 0x70, 0xE0, 0xFF },
            new byte[] { 0x70, 0x7C, 0xDE, 0xFF },
            new byte[] { 0x3E, 0x91, 0xC8, 0xFF },
            new byte[] { 0x25, 0xA7, 0xA6, 0xFF },
            new byte[] { 0x28, 0xBA, 0x81, 0xFF },
            new byte[] { 0x46, 0xC4, 0x63, 0xFF },
            new byte[] { 0x7D, 0xC1, 0x54, 0xFF },
            new byte[] { 0xC0, 0xB3, 0x56, 0xFF },
            new byte[] { 0x3C, 0x3C, 0x3C, 0xFF },
            new byte[] { 0x00, 0x00, 0x00, 0xFF },
            new byte[] { 0x00, 0x00, 0x00, 0xFF },
            new byte[] { 0xFF, 0xFF, 0xFE, 0xFF },
            new byte[] { 0xFD, 0xD6, 0xBE, 0xFF },
            new byte[] { 0xFF, 0xCC, 0xCC, 0xFF },
            new byte[] { 0xFF, 0xC4, 0xDD, 0xFF },
            new byte[] { 0xF9, 0xC0, 0xEA, 0xFF },
            new byte[] { 0xDF, 0xC1, 0xF2, 0xFF },
            new byte[] { 0xC2, 0xC7, 0xF1, 0xFF },
            new byte[] { 0xAA, 0xD0, 0xE8, 0xFF },
            new byte[] { 0x9D, 0xDA, 0xD9, 0xFF },
            new byte[] { 0x9E, 0xE2, 0xC9, 0xFF },
            new byte[] { 0xAE, 0xE6, 0xBC, 0xFF },
            new byte[] { 0xC7, 0xE5, 0xB4, 0xFF },
            new byte[] { 0xE4, 0xDF, 0xB5, 0xFF },
            new byte[] { 0xA9, 0xA9, 0xA9, 0xFF },
            new byte[] { 0x00, 0x00, 0x00, 0xFF },
            new byte[] { 0x00, 0x00, 0x00, 0xFF }
        };

        protected bool isPalChip;

        protected byte[] Regs;

        byte[] Nametables;
        byte[] SpriteRAM;
        int[] Sprite0Buffer;
        byte VRAMReadBuffer;

        bool AddressToggle;
        ushort PPUCurrentAddress;
        ushort PPUNewAddress;

        public bool NMIonVBLANK { get { return ((Regs[0] & 0x80) == 0x80); } }    //public???
        int SpriteSize { get { return (((Regs[0] & 0x80) == 0x80) ? 16 : 8); } }
        ushort BGPatternAddress { get { return (ushort)(((Regs[0] & 0x10) == 0x10) ? 0x1000 : 0x0000); } }
        ushort SprPatternAddress { get { return (ushort)(((Regs[0] & 0x08) == 0x08) ? 0x1000 : 0x0000); } }
        int VRAMAddressIncrement { get { return (((Regs[0] & 0x04) == 0x04) ? 32 : 1); } }
        ushort NametableBaseAddress { get { return (ushort)(0x2000 | ((Regs[0] & 0x3) << 10)); } }

        bool ShowSprites { get { return ((Regs[1] & 0x10) == 0x10); } }
        bool ShowBackground { get { return ((Regs[1] & 0x08) == 0x08); } }
        bool ShowSpritesLeftClip { get { return ((Regs[1] & 0x04) == 0x04); } }
        bool ShowBackgroundLeftClip { get { return ((Regs[1] & 0x02) == 0x02); } }
        byte PalMask { get { return (byte)(((Regs[1] & 0x01) == 0x01) ? 0x30 : 0x3F); } }

        byte SpriteRAMAddress;

        byte ScrollX, ScrollY;

        public bool Sprite0Hit;     //public???
        int SpriteOverflow;

        PPUMirroring mirroringMode;
        bool isFrameInterruptPending;
        byte LastPPUWrite;

        public InterruptState InterruptLine { get; protected set; }
        public int CurrentScanline; //public???

        public virtual int NumTotalScanlines { get { return (isPalChip ? NumTotalScanlinesPal : NumTotalScanlinesNtsc); } }

        protected byte[] outputFramebuffer;

        public byte[] OutputFramebuffer { get { return outputFramebuffer; } }

        protected int cycleCount;
        double clockRate, refreshRate;

        protected int clockCyclesPerLine;

        public Ricoh2C02(double clockRate, double refreshRate, bool isPalChip, ReadChrDelegate readChr, WriteChrDelegate writeChr)
        {
            this.clockRate = clockRate;
            this.refreshRate = refreshRate;
            this.isPalChip = isPalChip;

            readChrDelegate = readChr;
            writeChrDelegate = writeChr;

            Regs = new byte[0x08];

            Nametables = new byte[0x2000];
            SpriteRAM = new byte[0x100];
            Sprite0Buffer = new int[256];

            outputFramebuffer = new byte[(NumActivePixelsPerScanline * NumTotalScanlines) * 4];

            clockCyclesPerLine = (int)Math.Round((clockRate / refreshRate) / NumTotalScanlines);
        }

        public virtual void Startup()
        {
            Reset();
        }

        public virtual void Reset()
        {
            CurrentScanline = -1;
            AddressToggle = false;
            VRAMReadBuffer = 0;
            ScrollY = 0;
            ScrollX = 0;
            Sprite0Hit = false;

            SetMirroringMode(PPUMirroring.Horizontal);
            isFrameInterruptPending = false;

            cycleCount = 0;
        }

        public virtual bool Step(int clockCyclesInStep)
        {
            bool drawScreen = false;

            InterruptLine = ((NMIonVBLANK && isFrameInterruptPending) ? InterruptState.Assert : InterruptState.Clear);

            cycleCount += clockCyclesInStep;

            if (cycleCount >= clockCyclesPerLine)
            {
                RenderLine(CurrentScanline);
                CurrentScanline++;

                if (CurrentScanline == NumActiveScanlines)
                    isFrameInterruptPending = true;

                if (CurrentScanline == NumTotalScanlines)
                {
                    CurrentScanline = -1;
                    drawScreen = true;
                }

                cycleCount -= clockCyclesPerLine;
            }

            return drawScreen;
        }

        public void SetMirroringMode(PPUMirroring mirroring)
        {
            mirroringMode = mirroring;
        }

        protected virtual void RenderLine(int line)
        {
            if (line >= 0 && line <= 239)
            {
                BlankLine(line, (byte)(Nametables[0x1F00] & PalMask));
                for (int i = 0; i < NumActivePixelsPerScanline; i++)
                    Sprite0Buffer[i] = 0;
            }

            if (line >= 8 && line < 231)
            {
                if (ShowSprites) RenderSprites(line, 0x20);
                if (ShowBackground) RenderBackground(line);
                if (ShowSprites) RenderSprites(line, 0x00);
            }
            else
            {
                //
            }
        }

        protected void BlankLine(int line, byte b, byte g, byte r)
        {
            int outputY = (line * NumActivePixelsPerScanline);
            for (int x = 0; x < NumActivePixelsPerScanline; x++)
                WriteColorToFramebuffer(b, g, r, (outputY + (x % NumActivePixelsPerScanline)) * 4);
        }

        protected void BlankLine(int line, byte colorIndex)
        {
            int outputY = (line * NumActivePixelsPerScanline);
            for (int x = 0; x < NumActivePixelsPerScanline; x++)
                WriteColorToFramebuffer(colorIndex, (outputY + (x % NumActivePixelsPerScanline)) * 4);
        }

        protected void WriteColorToFramebuffer(byte b, byte g, byte r, int address)
        {
            outputFramebuffer[address] = b;
            outputFramebuffer[address + 1] = g;
            outputFramebuffer[address + 2] = r;
            outputFramebuffer[address + 3] = 0xFF;
        }

        protected virtual void WriteColorToFramebuffer(byte colorIndex, int address)
        {
            Buffer.BlockCopy(palette[(colorIndex & 0x3F)], 0, outputFramebuffer, address, 4);
        }

        public byte ReadRegister(byte register)
        {
            byte retVal = 0x00;

            switch (register)
            {
                case 0x02:
                    if (isFrameInterruptPending) retVal |= 0x80;
                    if (Sprite0Hit) retVal |= 0x40;
                    if (SpriteOverflow > 8) retVal |= 0x20;

                    retVal |= (byte)(LastPPUWrite & 0x1F);

                    isFrameInterruptPending = false;
                    AddressToggle = false;
                    break;

                case 0x04:
                    retVal = SpriteRAM[SpriteRAMAddress];
                    break;

                case 0x07:
                    PPUCurrentAddress &= 0x3FFF;

                    if (PPUCurrentAddress < 0x3F00)
                    {
                        retVal = VRAMReadBuffer;
                        if (PPUCurrentAddress >= 0x2000 && PPUCurrentAddress < 0x3000)
                            VRAMReadBuffer = Nametables[PPUCurrentAddress - 0x2000];
                        else if (PPUCurrentAddress >= 0x3000 && PPUCurrentAddress < 0x3F00)
                            VRAMReadBuffer = Nametables[PPUCurrentAddress - 0x3000];
                        else
                            VRAMReadBuffer = readChrDelegate(PPUCurrentAddress);
                    }
                    else
                    {
                        retVal = Nametables[PPUCurrentAddress - 0x2000];
                    }

                    PPUCurrentAddress += (ushort)VRAMAddressIncrement;
                    break;
            }

            return retVal;
        }

        public void WriteRegister(byte register, byte value)
        {
            Regs[register] = value;
            LastPPUWrite = value;

            switch (register)
            {
                case 0x03:
                    SpriteRAMAddress = value;
                    break;

                case 0x04:
                    SpriteRAM[SpriteRAMAddress] = value;
                    SpriteRAMAddress++;
                    break;

                case 0x05:
                    if (AddressToggle == false)
                        ScrollY = value;
                    else
                    {
                        ScrollX = value;
                        if (ScrollX > 239) ScrollX = 0;
                    }
                    AddressToggle = !AddressToggle;
                    break;

                case 0x06:
                    if (AddressToggle == false)
                    {
                        PPUNewAddress = (ushort)(((value << 8) & 0x3F00) | (PPUNewAddress & 0xFF));
                    }
                    else
                    {
                        PPUNewAddress = (ushort)(value | (PPUNewAddress & 0xFF00));
                        PPUCurrentAddress = PPUNewAddress;
                    }

                    AddressToggle = !AddressToggle;
                    break;

                case 0x07:
                    if (PPUCurrentAddress < 0x2000)
                    {
                        writeChrDelegate(PPUCurrentAddress, value);
                    }
                    else if (PPUCurrentAddress >= 0x2000 && PPUCurrentAddress < 0x3F00)
                    {
                        ushort WriteAddress = (ushort)(PPUCurrentAddress & 0xFFF);

                        if (mirroringMode == PPUMirroring.Horizontal)
                        {
                            switch (PPUCurrentAddress & 0xC00)
                            {
                                case 0x000: Nametables[WriteAddress] = value; break;
                                case 0x400: Nametables[WriteAddress - 0x400] = value; break;
                                case 0x800: Nametables[WriteAddress - 0x400] = value; break;
                                case 0xC00: Nametables[WriteAddress - 0x800] = value; break;
                            }
                        }
                        else if (mirroringMode == PPUMirroring.Vertical)
                        {
                            switch (PPUCurrentAddress & 0xC00)
                            {
                                case 0x000: Nametables[WriteAddress] = value; break;
                                case 0x400: Nametables[WriteAddress] = value; break;
                                case 0x800: Nametables[WriteAddress - 0x800] = value; break;
                                case 0xC00: Nametables[WriteAddress - 0x800] = value; break;
                            }
                        }
                        else
                        {
                            throw new NotImplementedException($"Unsupported PPU nametable mirroring mode {mirroringMode}");
                        }
                    }
                    else if ((PPUCurrentAddress >= 0x3F00) && (PPUCurrentAddress < 0x3F20))
                    {
                        Nametables[PPUCurrentAddress - 0x2000] = value;
                        if ((PPUCurrentAddress & 0x7) == 0)
                        {
                            Nametables[(PPUCurrentAddress - 0x2000) ^ 0x10] = value;
                        }
                    }

                    PPUCurrentAddress += (ushort)VRAMAddressIncrement;
                    break;
            }
        }

        public void RenderBackground(int line)
        {
            int currentTileColumn;
            int tileNumber;
            int tileDataOffset;
            byte tiledata1, tiledata2;
            byte paletteHighBits;
            int pixelColor;
            int virtualScanline;
            int nameTableBase;
            int startColumn, endColumn;
            int vScrollSide;
            int startTilePixel, endTilePixel;

            for (vScrollSide = 0; vScrollSide < 2; vScrollSide++)
            {
                virtualScanline = line + ScrollX;
                nameTableBase = NametableBaseAddress;
                if (vScrollSide == 0)
                {
                    if (virtualScanline >= 240)
                    {
                        switch (NametableBaseAddress)
                        {
                            case (0x2000): nameTableBase = 0x2800; break;
                            case (0x2400): nameTableBase = 0x2C00; break;
                            case (0x2800): nameTableBase = 0x2000; break;
                            case (0x2C00): nameTableBase = 0x2400; break;

                        }
                        virtualScanline = virtualScanline - 240;
                    }

                    startColumn = ScrollY / 8;
                    endColumn = 32;
                }
                else
                {
                    if (virtualScanline >= 240)
                    {
                        switch (NametableBaseAddress)
                        {
                            case (0x2000): nameTableBase = 0x2C00; break;
                            case (0x2400): nameTableBase = 0x2800; break;
                            case (0x2800): nameTableBase = 0x2400; break;
                            case (0x2C00): nameTableBase = 0x2000; break;

                        }
                        virtualScanline = virtualScanline - 240;
                    }
                    else
                    {
                        switch (NametableBaseAddress)
                        {
                            case (0x2000): nameTableBase = 0x2400; break;
                            case (0x2400): nameTableBase = 0x2000; break;
                            case (0x2800): nameTableBase = 0x2C00; break;
                            case (0x2C00): nameTableBase = 0x2800; break;

                        }
                    }
                    startColumn = 0;
                    endColumn = (ScrollY / 8) + 1;
                }

                if (mirroringMode == PPUMirroring.Horizontal)
                {
                    switch (nameTableBase)
                    {
                        case (0x2400): nameTableBase = 0x2000; break;
                        case (0x2800): nameTableBase = 0x2400; break;
                        case (0x2C00): nameTableBase = 0x2400; break;
                    }
                }
                else if (mirroringMode == PPUMirroring.Vertical)
                {
                    switch (nameTableBase)
                    {
                        case (0x2800): nameTableBase = 0x2000; break;
                        case (0x2C00): nameTableBase = 0x2400; break;
                    }
                }
                else
                {
                    // TODO
                }

                for (currentTileColumn = startColumn; currentTileColumn < endColumn; currentTileColumn++)
                {
                    //Starting tile row is currentScanline / 8
                    //The offset in the tile is currentScanline % 8

                    //Step #1, get the tile number
                    tileNumber = Nametables[nameTableBase - 0x2000 + ((virtualScanline / 8) * 32) + currentTileColumn];

                    //Step #2, get the offset for the tile in the tile data
                    tileDataOffset = BGPatternAddress + (tileNumber * 16);

                    //Step #3, get the tile data from chr rom
                    tiledata1 = readChrDelegate((uint)(tileDataOffset + (virtualScanline % 8)));
                    tiledata2 = readChrDelegate((uint)(tileDataOffset + (virtualScanline % 8) + 8));

                    //Step #4, get the attribute byte for the block of tiles we're in
                    //this will put us in the correct section in the palette table
                    paletteHighBits = Nametables[((nameTableBase - 0x2000 + 0x3c0 + (((virtualScanline / 8) / 4) * 8) + (currentTileColumn / 4)))];
                    paletteHighBits = (byte)(paletteHighBits >> ((4 * (((virtualScanline / 8) % 4) / 2)) + (2 * ((currentTileColumn % 4) / 2))));
                    paletteHighBits = (byte)((paletteHighBits & 0x3) << 2);

                    //Step #5, render the line inside the tile to the offscreen buffer
                    startTilePixel = 0;
                    endTilePixel = 8;
                    if (vScrollSide == 0)
                    {
                        if (currentTileColumn == startColumn)
                            startTilePixel = ScrollY % 8;
                    }
                    else
                    {
                        if (currentTileColumn == endColumn)
                            endTilePixel = ScrollY % 8;
                    }

                    for (int i = startTilePixel; i < endTilePixel; i++)
                    {
                        pixelColor = paletteHighBits + (((tiledata2 & (1 << (7 - i))) >> (7 - i)) << 1) + ((tiledata1 & (1 << (7 - i))) >> (7 - i));

                        if ((pixelColor % 4) != 0)
                        {
                            if (vScrollSide == 0)
                            {
                                WriteColorToFramebuffer((byte)(Nametables[0x1F00 + pixelColor] & PalMask), ((line * 256) + (8 * currentTileColumn) - ScrollY + i));
                                Sprite0Buffer[(8 * currentTileColumn) - ScrollY + i] = 1;
                            }
                            else if (((8 * currentTileColumn) + (256 - ScrollY) + i) < 256)
                            {
                                WriteColorToFramebuffer((byte)(Nametables[0x1F00 + pixelColor] & PalMask), ((line * 256) + (8 * currentTileColumn) + (256 - ScrollY) + i));
                                Sprite0Buffer[(8 * currentTileColumn) + (256 - ScrollY) + i] = 1;
                            }
                        }
                    }
                }
            }
        }

        private void RenderSprites(int line, int behind)
        {
            int spriteLineToDraw;
            byte tiledata1, tiledata2;
            int offsetToSprite;
            byte paletteHighBits;
            int pixelColor;
            byte actualY;

            byte spriteId;

            for (int i = 252; i >= 0; i = i - 4)
            {
                actualY = (byte)(SpriteRAM[i] + 1);

                if (((SpriteRAM[i + 2] & 0x20) == behind) && (actualY <= line) && ((actualY + SpriteSize) > line))
                {
                    SpriteOverflow++;

                    if (SpriteSize == 8)
                    {
                        if ((SpriteRAM[i + 2] & 0x80) != 0x80)
                            spriteLineToDraw = line - actualY;
                        else
                            spriteLineToDraw = actualY + 7 - line;

                        offsetToSprite = SprPatternAddress + SpriteRAM[i + 1] * 16;

                        tiledata1 = readChrDelegate((uint)(offsetToSprite + spriteLineToDraw));
                        tiledata2 = readChrDelegate((uint)(offsetToSprite + spriteLineToDraw + 8));

                        paletteHighBits = (byte)((SpriteRAM[i + 2] & 0x3) << 2);

                        for (int j = 0; j < 8; j++)
                        {
                            if ((SpriteRAM[i + 2] & 0x40) == 0x40)
                                pixelColor = paletteHighBits + (((tiledata2 & (1 << (j))) >> (j)) << 1) + ((tiledata1 & (1 << (j))) >> (j));
                            else
                                pixelColor = paletteHighBits + (((tiledata2 & (1 << (7 - j))) >> (7 - j)) << 1) + ((tiledata1 & (1 << (7 - j))) >> (7 - j));

                            if ((pixelColor % 4) != 0)
                            {
                                if ((SpriteRAM[i + 3] + j) < 256)
                                {
                                    WriteColorToFramebuffer((byte)(Nametables[0x1F10 + pixelColor] & PalMask), ((line * 256) + (SpriteRAM[i + 3]) + j));

                                    if (i == 0 && Sprite0Buffer[(SpriteRAM[i + 3]) + j] == 1)
                                        Sprite0Hit = true;

                                    Sprite0Buffer[(SpriteRAM[i + 3]) + j] = 2;
                                }
                            }
                        }
                    }
                    else
                    {
                        spriteId = SpriteRAM[i + 1];
                        if ((SpriteRAM[i + 2] & 0x80) != 0x80)
                            spriteLineToDraw = line - actualY;
                        else
                            spriteLineToDraw = actualY + 15 - line;

                        if (spriteLineToDraw < 8)
                        {
                            if ((spriteId % 2) == 0)
                                offsetToSprite = 0x0000 + (spriteId) * 16;
                            else
                                offsetToSprite = 0x1000 + (spriteId - 1) * 16;
                        }
                        else
                        {
                            spriteLineToDraw = spriteLineToDraw - 8;

                            if ((spriteId % 2) == 0)
                                offsetToSprite = 0x0000 + (spriteId + 1) * 16;
                            else
                                offsetToSprite = 0x1000 + (spriteId) * 16;
                        }

                        tiledata1 = readChrDelegate((uint)(offsetToSprite + spriteLineToDraw));
                        tiledata2 = readChrDelegate((uint)(offsetToSprite + spriteLineToDraw + 8));

                        paletteHighBits = (byte)((SpriteRAM[i + 2] & 0x3) << 2);

                        for (int j = 0; j < 8; j++)
                        {
                            if ((SpriteRAM[i + 2] & 0x40) == 0x40)
                                pixelColor = paletteHighBits + (((tiledata2 & (1 << (j))) >> (j)) << 1) + ((tiledata1 & (1 << (j))) >> (j));
                            else
                                pixelColor = paletteHighBits + (((tiledata2 & (1 << (7 - j))) >> (7 - j)) << 1) + ((tiledata1 & (1 << (7 - j))) >> (7 - j));

                            if ((pixelColor % 4) != 0)
                            {
                                if ((SpriteRAM[i + 3] + j) < 256)
                                {
                                    WriteColorToFramebuffer((byte)(Nametables[0x1F10 + pixelColor] & PalMask), ((line * 256) + (SpriteRAM[i + 3]) + j));

                                    if (i == 0 && Sprite0Buffer[(SpriteRAM[i + 3]) + j] == 1)
                                        Sprite0Hit = true;

                                    Sprite0Buffer[(SpriteRAM[i + 3]) + j] = 2;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
