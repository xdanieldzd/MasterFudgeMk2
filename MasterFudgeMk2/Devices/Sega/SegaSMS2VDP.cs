using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.Enumerations;

namespace MasterFudgeMk2.Devices.Sega
{
    /* Sega 315-5246, commonly found on SMS2 systems */
    public class SegaSMS2VDP : TMS9918A
    {
        public const int NumVisibleLinesLow = NumActiveScanlines;
        public const int NumVisibleLinesMed = 224;
        public const int NumVisibleLinesHigh = 240;

        public const int NumSpritesMaxMode4 = 64;

        protected byte[] cram;

        int vCounter, hCounter, nametableHeight;
        int lineInterruptCounter;
        protected int screenHeight;

        bool isLineInterruptEnabled { get { return BitUtilities.IsBitSet(registers[0x00], 4); } }
        bool isLineInterruptPending;

        bool isColumn0MaskEnabled { get { return BitUtilities.IsBitSet(registers[0x00], 5); } }
        bool isVScrollPartiallyDisabled { get { return BitUtilities.IsBitSet(registers[0x00], 7); } }  /* Columns 24-31, i.e. pixels 192-255 */
        bool isHScrollPartiallyDisabled { get { return BitUtilities.IsBitSet(registers[0x00], 6); } }  /* Rows 0-1, i.e. pixels 0-15 */

        bool isBitM4Set { get { return BitUtilities.IsBitSet(registers[0x00], 2); } }

        protected override bool isModeGraphics1 { get { return !(isBitM1Set || isBitM2Set || isBitM3Set || isBitM4Set); } }
        protected override bool isModeText { get { return (isBitM1Set && !(isBitM2Set || isBitM3Set || isBitM4Set)); } }
        protected override bool isModeGraphics2 { get { return (isBitM2Set && !(isBitM1Set || isBitM3Set || isBitM4Set)); } }
        protected override bool isModeMulticolor { get { return (isBitM3Set && !(isBitM1Set || isBitM2Set || isBitM4Set)); } }

        bool isSMS240LineMode { get { return (isMasterSystemMode && isBitM3Set); } }
        bool isSMS224LineMode { get { return (isMasterSystemMode && isBitM1Set); } }

        bool isMasterSystemMode;

        bool isSpriteShiftLeft8 { get { return BitUtilities.IsBitSet(registers[0x00], 3); } }

        protected override ushort nametableBaseAddress
        {
            get
            {
                if (isMasterSystemMode) return (ushort)((registers[0x02] & 0x0E) << 10);
                else return (ushort)((registers[0x02] & 0x0F) << 10);
            }
        }
        protected override ushort spriteAttribTableBaseAddress
        {
            get
            {
                if (isMasterSystemMode) return (ushort)((registers[0x05] & 0x7E) << 7);
                else return (ushort)((registers[0x05] & 0x7F) << 7);
            }
        }
        protected override ushort spritePatternGenBaseAddress
        {
            get
            {
                if (isMasterSystemMode) return (ushort)((registers[0x06] & 0x04) << 11);
                else return (ushort)((registers[0x06] & 0x07) << 11);
            }
        }

        /* http://www.smspower.org/Development/Palette */
        // TODO: verify these, SMSPower has some mistakes (RGB approx correct, palette value wrong)
        // (not that we'll really use this, aside from for F-16 Fighting Falcon, as SG1000 games should always be loaded into the SG1000 core...)
        byte[] legacyColorMap = new byte[]
        {
            0x00,                                   /* Transparent */
            0x00,                                   /* Black */
            0x08,                                   /* Medium green */
            0x0C,                                   /* Light green */
            0x10,                                   /* Dark blue */
            0x30,                                   /* Light blue */
            0x01,                                   /* Dark red */
            0x3C,                                   /* Cyan */
            0x02,                                   /* Medium red */
            0x03,                                   /* Light red */
            0x05,                                   /* Dark yellow */
            0x0F,                                   /* Light yellow */
            0x04,                                   /* Dark green */
            0x33,                                   /* Magenta */
            0x15,                                   /* Gray */
            0x3F                                    /* White */
        };

        /* For H-counter emulation */
        static byte[] hCounterTable = new byte[]
        {
            0x00, 0x01, 0x02, 0x02, 0x03, 0x04, 0x05, 0x05, 0x06, 0x07, 0x08, 0x08, 0x09, 0x0A, 0x0B, 0x0B,
            0x0C, 0x0D, 0x0E, 0x0E, 0x0F, 0x10, 0x11, 0x11, 0x12, 0x13, 0x14, 0x14, 0x15, 0x16, 0x17, 0x17,
            0x18, 0x19, 0x1A, 0x1A, 0x1B, 0x1C, 0x1D, 0x1D, 0x1E, 0x1F, 0x20, 0x20, 0x21, 0x22, 0x23, 0x23,
            0x24, 0x25, 0x26, 0x26, 0x27, 0x28, 0x29, 0x29, 0x2A, 0x2B, 0x2C, 0x2C, 0x2D, 0x2E, 0x2F, 0x2F,
            0x30, 0x31, 0x32, 0x32, 0x33, 0x34, 0x35, 0x35, 0x36, 0x37, 0x38, 0x38, 0x39, 0x3A, 0x3B, 0x3B,
            0x3C, 0x3D, 0x3E, 0x3E, 0x3F, 0x40, 0x41, 0x41, 0x42, 0x43, 0x44, 0x44, 0x45, 0x46, 0x47, 0x47,
            0x48, 0x49, 0x4A, 0x4A, 0x4B, 0x4C, 0x4D, 0x4D, 0x4E, 0x4F, 0x50, 0x50, 0x51, 0x52, 0x53, 0x53,
            0x54, 0x55, 0x56, 0x56, 0x57, 0x58, 0x59, 0x59, 0x5A, 0x5B, 0x5C, 0x5C, 0x5D, 0x5E, 0x5F, 0x5F,
            0x60, 0x61, 0x62, 0x62, 0x63, 0x64, 0x65, 0x65, 0x66, 0x67, 0x68, 0x68, 0x69, 0x6A, 0x6B, 0x6B,
            0x6C, 0x6D, 0x6E, 0x6E, 0x6F, 0x70, 0x71, 0x71, 0x72, 0x73, 0x74, 0x74, 0x75, 0x76, 0x77, 0x77,
            0x78, 0x79, 0x7A, 0x7A, 0x7B, 0x7C, 0x7D, 0x7D, 0x7E, 0x7F, 0x80, 0x80, 0x81, 0x82, 0x83, 0x83,
            0x84, 0x85, 0x86, 0x86, 0x87, 0x88, 0x89, 0x89, 0x8A, 0x8B, 0x8C, 0x8C, 0x8D, 0x8E, 0x8F, 0x8F,
            0x90, 0x91, 0x92, 0x92, 0x93,

            0xE9, 0xEA, 0xEA, 0xEB, 0xEC, 0xED, 0xED, 0xEE, 0xEF, 0xF0, 0xF0, 0xF1, 0xF2, 0xF3, 0xF3, 0xF4,
            0xF5, 0xF6, 0xF6, 0xF7, 0xF8, 0xF9, 0xF9, 0xFA, 0xFB, 0xFC, 0xFC, 0xFD, 0xFE, 0xFF, 0xFF
        };

        byte horizontalScrollLatched, verticalScrollLatched;

        const byte screenUsageBgLowPriority = screenUsageBg;
        const byte screenUsageBgHighPriority = (1 << 2);

        public int ScreenHeight { get { return screenHeight; } }

        // TODO: PAL mode is broken-ish (vcounter stuff)

        public SegaSMS2VDP(double clockRate, double refreshRate, bool isPalChip) : base(clockRate, refreshRate, isPalChip)
        {
            registers = new byte[0x0B];
            cram = new byte[0x20];

            screenUsage = new byte[NumActivePixelsPerScanline * NumTotalScanlines];

            outputFramebuffer = new byte[(NumActivePixelsPerScanline * NumTotalScanlines) * 4];
        }

        public override void Reset()
        {
            base.Reset();

            WriteRegister(0x00, 0x36);
            WriteRegister(0x01, 0x80);
            WriteRegister(0x02, 0xFF);
            WriteRegister(0x03, 0xFF);
            WriteRegister(0x04, 0xFF);
            WriteRegister(0x05, 0xFF);
            WriteRegister(0x06, 0xFB);
            WriteRegister(0x07, 0x00);
            WriteRegister(0x08, 0x00);
            WriteRegister(0x09, 0x00);
            WriteRegister(0x0A, 0xFF);

            for (int i = 0; i < cram.Length; i++) cram[i] = 0;

            vCounter = hCounter = 0;
            lineInterruptCounter = registers[0x0A];

            isLineInterruptPending = false;
            isMasterSystemMode = false;

            horizontalScrollLatched = verticalScrollLatched = 0;

            UpdateViewport();
        }

        public override void SetScanlineBoundaries()
        {
            if (!isPalChip)
            {
                /* NTSC */
                if (screenHeight == NumVisibleLinesHigh)
                {
                    /* 240 active lines, invalid on NTSC (dummy values) */
                    scanlineTopBlanking = 0;
                    scanlineTopBorder = 0;
                    scanlineActiveDisplay = 0;
                    scanlineBottomBorder = 0;
                    scanlineBottomBlanking = 0;
                    scanlineVerticalBlanking = 0;
                }
                else if (screenHeight == NumVisibleLinesMed)
                {
                    /* 224 active lines */
                    scanlineActiveDisplay = 0;
                    scanlineBottomBorder = (scanlineActiveDisplay + 224);
                    scanlineBottomBlanking = (scanlineBottomBorder + 8);
                    scanlineVerticalBlanking = (scanlineBottomBlanking + 3);
                    scanlineTopBlanking = (scanlineVerticalBlanking + 3);
                    scanlineTopBorder = (scanlineTopBlanking + 13);
                }
                else
                {
                    /* 192 active lines */
                    scanlineActiveDisplay = 0;
                    scanlineBottomBorder = (scanlineActiveDisplay + 192);
                    scanlineBottomBlanking = (scanlineBottomBorder + 24);
                    scanlineVerticalBlanking = (scanlineBottomBlanking + 3);
                    scanlineTopBlanking = (scanlineVerticalBlanking + 3);
                    scanlineTopBorder = (scanlineTopBlanking + 13);
                }
            }
            else
            {
                /* PAL */
                if (screenHeight == NumVisibleLinesHigh)
                {
                    /* 240 active lines */
                    scanlineActiveDisplay = 0;
                    scanlineBottomBorder = (scanlineActiveDisplay + 240);
                    scanlineBottomBlanking = (scanlineBottomBorder + 24);
                    scanlineVerticalBlanking = (scanlineBottomBlanking + 3);
                    scanlineTopBlanking = (scanlineVerticalBlanking + 3);
                    scanlineTopBorder = (scanlineTopBlanking + 13);
                }
                else if (screenHeight == NumVisibleLinesMed)
                {
                    /* 224 active lines */
                    scanlineActiveDisplay = 0;
                    scanlineBottomBorder = (scanlineActiveDisplay + 224);
                    scanlineBottomBlanking = (scanlineBottomBorder + 32);
                    scanlineVerticalBlanking = (scanlineBottomBlanking + 3);
                    scanlineTopBlanking = (scanlineVerticalBlanking + 3);
                    scanlineTopBorder = (scanlineTopBlanking + 13);
                }
                else
                {
                    /* 192 active lines */
                    scanlineActiveDisplay = 0;
                    scanlineBottomBorder = (scanlineActiveDisplay + 192);
                    scanlineBottomBlanking = (scanlineBottomBorder + 48);
                    scanlineVerticalBlanking = (scanlineBottomBlanking + 3);
                    scanlineTopBlanking = (scanlineVerticalBlanking + 3);
                    scanlineTopBorder = (scanlineTopBlanking + 13);
                }
            }
        }

        public override bool Step(int clockCyclesInStep)
        {
            bool drawScreen = false;

            cycleCount += clockCyclesInStep;

            hCounter = hCounterTable[(int)Math.Round(cycleCount / 3.0) % hCounterTable.Length];

            if (cycleCount >= clockCyclesPerLine)
            {
                if (currentScanline == 0)
                {
                    ClearScreen();

                    horizontalScrollLatched = registers[0x08];
                    verticalScrollLatched = registers[0x09];
                }

                RenderLine(currentScanline);

                vCounter = AdjustVCounter(currentScanline);
                currentScanline++;

                if (vCounter <= screenHeight)
                {
                    lineInterruptCounter--;
                    if (lineInterruptCounter < 0)
                    {
                        lineInterruptCounter = registers[0x0A];
                        isLineInterruptPending = true;
                    }
                }
                else
                    lineInterruptCounter = registers[0x0A];

                if (vCounter == (screenHeight + 1))
                    isFrameInterruptPending = true;

                if (currentScanline == NumTotalScanlines)
                {
                    RearrangeFramebuffer();

                    currentScanline = 0;
                    drawScreen = true;
                }

                cycleCount -= clockCyclesPerLine;
            }

            InterruptLine = (((isFrameInterruptEnabled && isFrameInterruptPending) || (isLineInterruptEnabled && isLineInterruptPending)) ? InterruptState.Assert : InterruptState.Clear);

            return drawScreen;
        }

        protected override byte ReadVram(ushort address)
        {
            return vram[address & vramMask16k];
        }

        protected override void WriteVram(ushort address, byte value)
        {
            vram[address & vramMask16k] = value;
        }

        protected override void ClearScreen()
        {
            for (int i = 0; i < screenUsage.Length; i++) screenUsage[i] = screenUsageEmpty;
        }

        protected override void RenderLine(int line)
        {
            if (line < scanlineBottomBorder)
            {
                /* Active display */
                if (isMasterSystemMode)
                {
                    RenderMode4Background(line);
                    RenderMode4Sprites(line);
                }
                else if (isModeGraphics1)
                {
                    RenderGraphics1Background(line);
                    RenderSprites(line);
                }
                else if (isModeGraphics2)
                {
                    RenderGraphics2Background(line);
                    RenderSprites(line);
                }
                else if (isModeMulticolor)
                {
                    RenderMulticolorBackground(line);
                    RenderSprites(line);
                }
                else if (isModeText)
                {
                    RenderTextBackground(line);
                }
                else
                {
                    /* Undocumented mode, not emulated */
                }
            }
            else if (line < scanlineBottomBlanking)
            {
                /* Bottom border */
                BlankLine(line, 1, backgroundColor);
            }
            else if (line < scanlineVerticalBlanking)
            {
                /* Bottom blanking */
                BlankLine(line, 0x08, 0x08, 0x08);
            }
            else if (line < scanlineTopBlanking)
            {
                /* Vertical blanking */
                BlankLine(line, 0x00, 0x00, 0x00);
            }
            else if (line < scanlineTopBorder)
            {
                /* Top blanking */
                BlankLine(line, 0x08, 0x08, 0x08);
            }
            else if (line < NumTotalScanlines)
            {
                /* Top border */
                BlankLine(line, 1, backgroundColor);
            }
        }

        protected void BlankLine(int line, int palette, int color)
        {
            int outputY = (line * NumActivePixelsPerScanline);
            for (int x = 0; x < NumActivePixelsPerScanline; x++)
                WriteColorToFramebuffer(cram[((palette * 16) + color)], (outputY + (x % NumActivePixelsPerScanline)) * 4);
        }

        private void RenderMode4Background(int line)
        {
            /* Addresses (global) */
            ushort currentNametableBaseAddress = nametableBaseAddress;

            /* Horizontal scrolling (global) */
            int currentHorizontalScroll = ((isHScrollPartiallyDisabled && line < 16) ? 0 : horizontalScrollLatched);
            int horizontalOffset = (currentHorizontalScroll & 0x07);

            /* Vertical scrolling (global) */
            int scrolledLine = line;

            int numColumnsPerLine = (NumActivePixelsPerScanline / 8);
            for (int column = 0; column < numColumnsPerLine; column++)
            {
                /* Horizontal scroll (column) */
                int startingColumn = ((column - (currentHorizontalScroll / 8)) & 0x1F);

                /* Vertical scrolling (column) */
                if (!(isVScrollPartiallyDisabled && column >= 24))
                    scrolledLine = (line + verticalScrollLatched) % nametableHeight;

                /* Get tile data */
                ushort nametableAddress = (ushort)(currentNametableBaseAddress + ((scrolledLine / 8) * (numColumnsPerLine * 2)));
                ushort ntData = (ushort)((ReadVram((ushort)(nametableAddress + (startingColumn * 2) + 1)) << 8) | ReadVram((ushort)(nametableAddress + (startingColumn * 2))));

                int tileIndex = (ntData & 0x01FF);
                bool hFlip = ((ntData & 0x200) == 0x200);
                bool vFlip = ((ntData & 0x400) == 0x400);
                int palette = ((ntData & 0x800) >> 11);
                bool priority = ((ntData & 0x1000) == 0x1000);

                /* For vertical flip */
                int tileLine = (vFlip ? ((scrolledLine / 8) * 8) + (-(scrolledLine % 8) + 7) : scrolledLine);

                ushort tileAddress = (ushort)((tileIndex * 0x20) + ((tileLine % 8) * 4));
                for (int pixel = 0; pixel < 8; pixel++)
                {
                    /* Calculate output framebuffer location, determine column masking, write to framebuffer */
                    int outputX = ((horizontalOffset + (column * 8) + pixel) % NumActivePixelsPerScanline);
                    int outputY = ((line % NumTotalScanlines) * NumActivePixelsPerScanline);
                    int hShift = (hFlip ? pixel : (7 - pixel));

                    int c = (((ReadVram((ushort)(tileAddress + 0)) >> hShift) & 0x1) << 0);
                    c |= (((ReadVram((ushort)(tileAddress + 1)) >> hShift) & 0x1) << 1);
                    c |= (((ReadVram((ushort)(tileAddress + 2)) >> hShift) & 0x1) << 2);
                    c |= (((ReadVram((ushort)(tileAddress + 3)) >> hShift) & 0x1) << 3);

                    if (screenUsage[outputY + outputX] == screenUsageEmpty)
                    {
                        screenUsage[outputY + outputX] |= ((c != 0 && priority) ? screenUsageBgHighPriority : screenUsageBgLowPriority);

                        if ((isColumn0MaskEnabled && outputX < 8) || isDisplayBlanked)
                            WriteColorToFramebuffer(1, backgroundColor, ((outputY + outputX) * 4));
                        else
                            WriteColorToFramebuffer(palette, c, ((outputY + outputX) * 4));
                    }
                }
            }
        }

        private void RenderMode4Sprites(int line)
        {
            /* Determine sprite size */
            int spriteWidth = 8;
            int spriteHeight = (isLargeSprites ? 16 : 8);

            /* Check and adjust for zoomed sprites */
            if (isZoomedSprites)
            {
                spriteWidth *= 2;
                spriteHeight *= 2;
            }

            /* Addresses (global) */
            ushort currentSpriteAttribTableBaseAddress = spriteAttribTableBaseAddress;
            ushort currentSpritePatternGenBaseAddress = spritePatternGenBaseAddress;

            int numSprites = 0;
            for (int sprite = 0; sprite < NumSpritesMaxMode4; sprite++)
            {
                int yCoordinate = ReadVram((ushort)(currentSpriteAttribTableBaseAddress + sprite));

                /* Ignore following if Y coord is 208 in 192-line mode */
                if (yCoordinate == 208 && screenHeight == NumVisibleLinesLow)
                {
                    /* Store first "illegal sprite" number in status register */
                    statusFlags |= (StatusFlags)sprite;
                    return;
                }

                /* Modify Y coord as needed */
                yCoordinate++;
                if (yCoordinate > screenHeight)
                    yCoordinate -= 256;

                /* Ignore this sprite if on incorrect lines */
                if (line < yCoordinate || line >= (yCoordinate + spriteHeight)) continue;

                /* Check for sprite overflow */
                numSprites++;
                if (numSprites > 8)
                {
                    isSpriteOverflow = true;
                    /* Store sprite number in status register */
                    statusFlags |= (StatusFlags)sprite;
                    return;
                }

                /* If display isn't blanked, draw line */
                if (!isDisplayBlanked)
                {
                    int xCoordinate = ReadVram((ushort)(currentSpriteAttribTableBaseAddress + 0x80 + (sprite * 2)));
                    int tileIndex = ReadVram((ushort)(currentSpriteAttribTableBaseAddress + 0x80 + (sprite * 2) + 1));
                    int zoomShift = (isZoomedSprites ? 1 : 0);

                    /* Adjust according to registers */
                    if (isSpriteShiftLeft8) xCoordinate -= 8;
                    if (isLargeSprites) tileIndex &= ~0x01;

                    ushort tileAddress = (ushort)(currentSpritePatternGenBaseAddress + (tileIndex * 0x20) + ((((line - yCoordinate) >> zoomShift) % spriteHeight) * 4));

                    /* Draw sprite line */
                    for (int pixel = 0; pixel < spriteWidth; pixel++)
                    {
                        /* Get output X position & check column 0 masking */
                        int outputX = ((xCoordinate + pixel) % NumActivePixelsPerScanline);
                        if (isColumn0MaskEnabled && outputX < 8) continue;

                        /* Get color & check transparency and position */
                        int c = (((ReadVram((ushort)(tileAddress + 0)) >> (7 - (pixel >> zoomShift))) & 0x1) << 0);
                        c |= (((ReadVram((ushort)(tileAddress + 1)) >> (7 - (pixel >> zoomShift))) & 0x1) << 1);
                        c |= (((ReadVram((ushort)(tileAddress + 2)) >> (7 - (pixel >> zoomShift))) & 0x1) << 2);
                        c |= (((ReadVram((ushort)(tileAddress + 3)) >> (7 - (pixel >> zoomShift))) & 0x1) << 3);
                        if (c == 0 || xCoordinate + pixel >= NumActivePixelsPerScanline) continue;

                        int outputY = ((line % screenHeight) * NumActivePixelsPerScanline);
                        if ((screenUsage[outputY + outputX] & screenUsageSprite) == screenUsageSprite)
                        {
                            /* Set sprite collision flag */
                            isSpriteCollision = true;
                        }
                        else if ((screenUsage[outputY + outputX] & screenUsageBgHighPriority) != screenUsageBgHighPriority)
                        {
                            /* Draw if pixel isn't occupied by high-priority BG */
                            WriteColorToFramebuffer(1, c, ((outputY + outputX) * 4));
                        }

                        /* Note that there is a sprite here regardless */
                        screenUsage[outputY + outputX] |= screenUsageSprite;
                    }
                }
            }

            /* Because we didn't bow out before already, store total number of sprites in status register */
            statusFlags |= (StatusFlags)31;
        }

        protected virtual int AdjustVCounter(int scanline)
        {
            int counter = scanline;

            if (!isPalChip)
            {
                if (screenHeight == NumVisibleLinesHigh)
                {
                    /* Invalid on NTSC */
                    if (scanline > 0xFF)
                        counter = (scanline - 0x100);
                }
                else if (screenHeight == NumVisibleLinesMed)
                {
                    if (scanline > 0xEA)
                        counter = (scanline - 0x06);
                }
                else
                {
                    if (scanline > 0xDA)
                        counter = (scanline - 0x06);
                }
            }
            else
            {
                if (screenHeight == NumVisibleLinesHigh)
                {
                    if (scanline > 0xFF && scanline < 0xFF + 0x0A)
                        counter = (scanline - 0x100);
                    else
                        counter = (scanline - 0x38);
                }
                else if (screenHeight == NumVisibleLinesMed)
                {
                    if (scanline > 0xFF && scanline < 0xFF + 0x02)
                        counter = (scanline - 0x100);
                    else
                        counter = (scanline - 0x38);
                }
                else
                {
                    if (scanline > 0xF2)
                        counter = (scanline - 0x39);
                }
            }
            return counter;
        }

        public byte ReadVCounter()
        {
            return (byte)vCounter;
        }

        public byte ReadHCounter()
        {
            return (byte)hCounter;
        }

        private void UpdateViewport()
        {
            if (isSMS240LineMode)
            {
                screenHeight = NumVisibleLinesHigh;
                nametableHeight = 256;
            }
            else if (isSMS224LineMode)
            {
                screenHeight = NumVisibleLinesMed;
                nametableHeight = 256;
            }
            else
            {
                screenHeight = NumVisibleLinesLow;
                nametableHeight = 224;
            }

            SetScanlineBoundaries();
        }

        protected virtual void WriteColorToFramebuffer(int palette, int color, int address)
        {
            WriteColorToFramebuffer(cram[((palette * 16) + color)], address);
        }

        protected override void WriteColorToFramebuffer(ushort colorValue, int address)
        {
            /* If not in Master System video mode, color value is index into legacy colormap */
            if (!isMasterSystemMode)
                colorValue = (legacyColorMap[colorValue & 0x000F]);

            byte r = (byte)((colorValue >> 0) & 0x3), g = (byte)((colorValue >> 2) & 0x3), b = (byte)((colorValue >> 4) & 0x3);
            outputFramebuffer[address] = (byte)((b << 6) | (b << 4) | (b << 2) | b);
            outputFramebuffer[address + 1] = (byte)((g << 6) | (g << 4) | (g << 2) | g);
            outputFramebuffer[address + 2] = (byte)((r << 6) | (r << 4) | (r << 2) | r);
            outputFramebuffer[address + 3] = 0xFF;
        }

        public override void WriteDataPort(byte value)
        {
            isSecondControlWrite = false;

            readBuffer = value;

            switch (codeRegister)
            {
                case 0x00:
                case 0x01:
                case 0x02:
                    WriteVram(addressRegister, value);
                    break;
                case 0x03:
                    cram[(addressRegister & 0x001F)] = value;
                    break;
            }

            addressRegister++;
        }

        public override byte ReadControlPort()
        {
            byte statusCurrent = (byte)statusFlags;

            statusFlags = StatusFlags.None;
            isSecondControlWrite = false;

            isLineInterruptPending = false;

            InterruptLine = InterruptState.Clear;

            return statusCurrent;
        }

        protected override void WriteRegister(byte register, byte value)
        {
            // TODO: verify what's the correct behavior here? VDPTEST writes to invalid registers...?
            if (register < registers.Length)
                registers[register] = value;

            /* Some value caching for (minor) performance reasons */
            if (register == 0x00)
                isMasterSystemMode = isBitM4Set;
            else if (register == 0x08)
                horizontalScrollLatched = registers[0x08];

            UpdateViewport();
        }
    }
}
