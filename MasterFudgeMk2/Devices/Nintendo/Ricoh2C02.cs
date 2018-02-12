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

        protected byte[] registers;

        byte[] nametables, spriteRam;
        byte vramReadBuffer;

        bool addressToggle;
        ushort ppuCurrentAddress, ppuNewAddress;

        bool isNmiOnVBlankEnabled { get { return ((registers[0] & 0x80) == 0x80); } }
        int spriteSize { get { return (((registers[0] & 0x20) == 0x20) ? 16 : 8); } }
        ushort bgPatternAddress { get { return (ushort)(((registers[0] & 0x10) == 0x10) ? 0x1000 : 0x0000); } }
        ushort spritePatternAddress { get { return (ushort)(((registers[0] & 0x08) == 0x08) ? 0x1000 : 0x0000); } }
        int vramAddressIncrement { get { return (((registers[0] & 0x04) == 0x04) ? 32 : 1); } }
        ushort nametableBaseAddress { get { return (ushort)(0x2000 | ((registers[0] & 0x3) << 10)); } }

        bool showSprites { get { return ((registers[1] & 0x10) == 0x10); } }
        bool showBackground { get { return ((registers[1] & 0x08) == 0x08); } }
        bool showSpritesLeftClip { get { return ((registers[1] & 0x04) == 0x04); } }
        bool showBackgroundLeftClip { get { return ((registers[1] & 0x02) == 0x02); } }
        byte palMask { get { return (byte)(((registers[1] & 0x01) == 0x01) ? 0x30 : 0x3F); } }

        byte spriteRamAddress, scrollX, scrollY;

        bool sprite0Hit;
        int spriteOverflow;

        PPUMirroring mirroringMode;
        bool isFrameInterruptPending;
        byte lastPPUWrite;

        public InterruptState InterruptLine { get; protected set; }
        int currentScanline;

        public virtual int NumTotalScanlines { get { return (isPalChip ? NumTotalScanlinesPal : NumTotalScanlinesNtsc); } }

        protected const byte screenUsageEmpty = 0;
        protected const byte screenUsageSprite0 = (1 << 0);
        protected const byte screenUsageBg = (1 << 1);
        protected const byte screenUsageSpriteN = (1 << 2);

        protected byte[] screenUsage;
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

            registers = new byte[0x08];

            nametables = new byte[0x2000];
            spriteRam = new byte[0x100];

            screenUsage = new byte[NumActivePixelsPerScanline];

            outputFramebuffer = new byte[(NumActivePixelsPerScanline * NumTotalScanlines) * 4];

            clockCyclesPerLine = (int)Math.Round((clockRate / refreshRate) / NumTotalScanlines);
        }

        public virtual void Startup()
        {
            Reset();
        }

        public virtual void Reset()
        {
            currentScanline = -1;
            addressToggle = false;
            vramReadBuffer = 0;
            scrollY = 0;
            scrollX = 0;
            sprite0Hit = false;

            SetMirroringMode(PPUMirroring.Horizontal);
            isFrameInterruptPending = false;

            cycleCount = 0;
        }

        public virtual bool Step(int clockCyclesInStep)
        {
            bool drawScreen = false;

            InterruptLine = InterruptState.Clear;

            cycleCount += clockCyclesInStep;

            if (cycleCount >= clockCyclesPerLine)
            {
                RenderLine(currentScanline);
                currentScanline++;

                if (currentScanline == (NumActiveScanlines + 1))
                {
                    isFrameInterruptPending = true;
                    if (isNmiOnVBlankEnabled)
                        InterruptLine = InterruptState.Assert;
                }

                if (currentScanline == NumTotalScanlines)
                {
                    isFrameInterruptPending = false;
                    sprite0Hit = false;

                    currentScanline = -1;
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
            if (!(showBackground || showSprites)) return;

            if (line >= 0 && line < NumActiveScanlines)
                BlankLine(line, (byte)(nametables[0x1F00] & palMask));

            spriteOverflow = 0;

            if (line >= 8 && line < 231)
            {
                for (int i = 0; i < NumActivePixelsPerScanline; i++)
                    screenUsage[i] = screenUsageEmpty;

                if (showSprites) RenderSprites(line, 0x20);
                if (showBackground) RenderBackground(line);
                if (showSprites) RenderSprites(line, 0x00);
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
                    if (sprite0Hit) retVal |= 0x40;
                    if (spriteOverflow > 8) retVal |= 0x20;

                    retVal |= (byte)(lastPPUWrite & 0x1F);

                    isFrameInterruptPending = false;
                    addressToggle = false;
                    break;

                case 0x04:
                    retVal = spriteRam[spriteRamAddress];
                    break;

                case 0x07:
                    ppuCurrentAddress &= 0x3FFF;

                    if (ppuCurrentAddress < 0x3F00)
                    {
                        retVal = vramReadBuffer;
                        if (ppuCurrentAddress >= 0x2000 && ppuCurrentAddress < 0x3000)
                            vramReadBuffer = nametables[ppuCurrentAddress - 0x2000];
                        else if (ppuCurrentAddress >= 0x3000 && ppuCurrentAddress < 0x3F00)
                            vramReadBuffer = nametables[ppuCurrentAddress - 0x3000];
                        else
                            vramReadBuffer = readChrDelegate(ppuCurrentAddress);
                    }
                    else
                    {
                        retVal = nametables[ppuCurrentAddress - 0x2000];
                    }

                    ppuCurrentAddress += (ushort)vramAddressIncrement;
                    break;
            }

            return retVal;
        }

        public void WriteRegister(byte register, byte value)
        {
            registers[register] = value;
            lastPPUWrite = value;

            switch (register)
            {
                case 0x03:
                    spriteRamAddress = value;
                    break;

                case 0x04:
                    spriteRam[spriteRamAddress] = value;
                    spriteRamAddress++;
                    break;

                case 0x05:
                    if (!addressToggle)
                        scrollY = value;
                    else
                    {
                        scrollX = value;
                        if (scrollX > 239) scrollX = 0;
                    }
                    addressToggle = !addressToggle;
                    break;

                case 0x06:
                    if (!addressToggle)
                    {
                        ppuNewAddress = (ushort)(((value << 8) & 0x3F00) | (ppuNewAddress & 0xFF));
                    }
                    else
                    {
                        ppuNewAddress = (ushort)(value | (ppuNewAddress & 0xFF00));
                        ppuCurrentAddress = ppuNewAddress;
                    }

                    addressToggle = !addressToggle;
                    break;

                case 0x07:
                    if (ppuCurrentAddress < 0x2000)
                    {
                        writeChrDelegate(ppuCurrentAddress, value);
                    }
                    else if (ppuCurrentAddress >= 0x2000 && ppuCurrentAddress < 0x3F00)
                    {
                        ushort writeAddress = (ushort)(ppuCurrentAddress & 0x0FFF);

                        if (mirroringMode == PPUMirroring.Horizontal)
                        {
                            if ((ppuCurrentAddress & 0x0400) == 0x0000)
                                nametables[writeAddress] = nametables[writeAddress + 0x0400] = value;
                            else
                                nametables[writeAddress] = nametables[writeAddress - 0x0400] = value;
                        }
                        else if (mirroringMode == PPUMirroring.Vertical)
                        {
                            if ((ppuCurrentAddress & 0x0800) == 0x0000)
                                nametables[writeAddress] = nametables[writeAddress + 0x0800] = value;
                            else
                                nametables[writeAddress] = nametables[writeAddress - 0x0800] = value;
                        }
                        else
                            throw new NotImplementedException($"Unimplemented PPU nametable mirroring mode {mirroringMode}");
                    }
                    else if ((ppuCurrentAddress >= 0x3F00) && (ppuCurrentAddress < 0x3F20))
                    {
                        nametables[ppuCurrentAddress - 0x2000] = value;
                        if ((ppuCurrentAddress & 0x07) == 0x00)
                            nametables[(ppuCurrentAddress - 0x2000) ^ 0x10] = value;
                    }

                    ppuCurrentAddress += (ushort)vramAddressIncrement;
                    break;
            }
        }

        public void RenderBackground(int line)
        {
            int currentTileColumn, tileNumber, tileDataOffset;
            byte tiledata1, tiledata2, paletteHighBits;
            int pixelColor, virtualScanline, nameTableBase, startColumn, endColumn, vScrollSide, startTilePixel, endTilePixel;

            for (vScrollSide = 0; vScrollSide < 2; vScrollSide++)
            {
                virtualScanline = line + scrollX;
                nameTableBase = nametableBaseAddress;
                if (vScrollSide == 0)
                {
                    if (virtualScanline >= 240)
                    {
                        switch (nametableBaseAddress)
                        {
                            case (0x2000): nameTableBase = 0x2800; break;
                            case (0x2400): nameTableBase = 0x2C00; break;
                            case (0x2800): nameTableBase = 0x2000; break;
                            case (0x2C00): nameTableBase = 0x2400; break;

                        }
                        virtualScanline = virtualScanline - 240;
                    }

                    startColumn = scrollY / 8;
                    endColumn = 32;
                }
                else
                {
                    if (virtualScanline >= 240)
                    {
                        switch (nametableBaseAddress)
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
                        switch (nametableBaseAddress)
                        {
                            case (0x2000): nameTableBase = 0x2400; break;
                            case (0x2400): nameTableBase = 0x2000; break;
                            case (0x2800): nameTableBase = 0x2C00; break;
                            case (0x2C00): nameTableBase = 0x2800; break;

                        }
                    }
                    startColumn = 0;
                    endColumn = (scrollY / 8) + 1;
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
                    throw new NotImplementedException($"Unimplemented PPU nametable mirroring mode {mirroringMode}");

                for (currentTileColumn = startColumn; currentTileColumn < endColumn; currentTileColumn++)
                {
                    //Starting tile row is currentScanline / 8
                    //The offset in the tile is currentScanline % 8

                    //Step #1, get the tile number
                    tileNumber = nametables[nameTableBase - 0x2000 + ((virtualScanline / 8) * 32) + currentTileColumn];

                    //Step #2, get the offset for the tile in the tile data
                    tileDataOffset = bgPatternAddress + (tileNumber * 16);

                    //Step #3, get the tile data from chr rom
                    tiledata1 = readChrDelegate((uint)(tileDataOffset + (virtualScanline % 8)));
                    tiledata2 = readChrDelegate((uint)(tileDataOffset + (virtualScanline % 8) + 8));

                    //Step #4, get the attribute byte for the block of tiles we're in
                    //this will put us in the correct section in the palette table
                    paletteHighBits = nametables[((nameTableBase - 0x2000 + 0x3c0 + (((virtualScanline / 8) / 4) * 8) + (currentTileColumn / 4)))];
                    paletteHighBits = (byte)(paletteHighBits >> ((4 * (((virtualScanline / 8) % 4) / 2)) + (2 * ((currentTileColumn % 4) / 2))));
                    paletteHighBits = (byte)((paletteHighBits & 0x3) << 2);

                    //Step #5, render the line inside the tile to the offscreen buffer
                    startTilePixel = 0;
                    endTilePixel = 8;
                    if (vScrollSide == 0)
                    {
                        if (currentTileColumn == startColumn)
                            startTilePixel = scrollY % 8;
                    }
                    else
                    {
                        if (currentTileColumn == endColumn)
                            endTilePixel = scrollY % 8;
                    }

                    for (int i = startTilePixel; i < endTilePixel; i++)
                    {
                        pixelColor = paletteHighBits + (((tiledata2 & (1 << (7 - i))) >> (7 - i)) << 1) + ((tiledata1 & (1 << (7 - i))) >> (7 - i));

                        if ((pixelColor % 4) != 0)
                        {
                            if (vScrollSide == 0)
                            {
                                int screenUsageOffset = ((8 * currentTileColumn) - scrollY + i);

                                WriteColorToFramebuffer((byte)(nametables[0x1F00 + pixelColor] & palMask), ((line * 256) + (8 * currentTileColumn) - scrollY + i) * 4);
                                screenUsage[screenUsageOffset] |= screenUsageBg;

                                if ((screenUsage[screenUsageOffset] & screenUsageSprite0) == screenUsageSprite0)
                                    sprite0Hit = true;
                            }
                            else if (((8 * currentTileColumn) + (256 - scrollY) + i) < 256)
                            {
                                int screenUsageOffset = ((8 * currentTileColumn) + (256 - scrollY) + i);

                                WriteColorToFramebuffer((byte)(nametables[0x1F00 + pixelColor] & palMask), ((line * 256) + (8 * currentTileColumn) + (256 - scrollY) + i) * 4);
                                screenUsage[screenUsageOffset] |= screenUsageBg;

                                if ((screenUsage[screenUsageOffset] & screenUsageSprite0) == screenUsageSprite0)
                                    sprite0Hit = true;
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
                actualY = (byte)(spriteRam[i] + 1);

                if (((spriteRam[i + 2] & 0x20) == behind) && (actualY <= line) && ((actualY + spriteSize) > line))
                {
                    spriteOverflow++;

                    spriteId = spriteRam[i + 1];

                    if (spriteSize == 8)
                    {
                        if ((spriteRam[i + 2] & 0x80) != 0x80)
                            spriteLineToDraw = line - actualY;
                        else
                            spriteLineToDraw = actualY + 7 - line;

                        offsetToSprite = spritePatternAddress + (spriteId * 16);
                    }
                    else
                    {
                        if ((spriteRam[i + 2] & 0x80) != 0x80)
                            spriteLineToDraw = line - actualY;
                        else
                            spriteLineToDraw = actualY + 15 - line;

                        if (spriteLineToDraw < 8)
                        {
                            if ((spriteId % 2) == 0)
                                offsetToSprite = 0x0000 + (spriteId * 16);
                            else
                                offsetToSprite = 0x1000 + ((spriteId - 1) * 16);
                        }
                        else
                        {
                            spriteLineToDraw = spriteLineToDraw - 8;

                            if ((spriteId % 2) == 0)
                                offsetToSprite = 0x0000 + ((spriteId + 1) * 16);
                            else
                                offsetToSprite = 0x1000 + (spriteId * 16);
                        }
                    }

                    tiledata1 = readChrDelegate((uint)(offsetToSprite + spriteLineToDraw));
                    tiledata2 = readChrDelegate((uint)(offsetToSprite + spriteLineToDraw + 8));

                    paletteHighBits = (byte)((spriteRam[i + 2] & 0x3) << 2);

                    for (int j = 0; j < 8; j++)
                    {
                        if ((spriteRam[i + 2] & 0x40) == 0x40)
                            pixelColor = paletteHighBits + (((tiledata2 & (1 << (j))) >> (j)) << 1) + ((tiledata1 & (1 << (j))) >> (j));
                        else
                            pixelColor = paletteHighBits + (((tiledata2 & (1 << (7 - j))) >> (7 - j)) << 1) + ((tiledata1 & (1 << (7 - j))) >> (7 - j));

                        if ((pixelColor % 4) != 0)
                        {
                            if ((spriteRam[i + 3] + j) < 256)
                            {
                                WriteColorToFramebuffer((byte)(nametables[0x1F10 + pixelColor] & palMask), ((line * 256) + (spriteRam[i + 3]) + j) * 4);

                                int screenUsageOffset = ((spriteRam[i + 3]) + j);
                                if (i == 0)
                                    screenUsage[screenUsageOffset] |= screenUsageSprite0;
                                else
                                    screenUsage[screenUsageOffset] |= screenUsageSpriteN;
                            }
                        }
                    }
                }
            }
        }
    }
}
