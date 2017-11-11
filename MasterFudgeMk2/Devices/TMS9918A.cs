using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.Enumerations;

namespace MasterFudgeMk2.Devices
{
    public class TMS9918A
    {
        public const int NumTotalScanlinesPal = 313;
        public const int NumTotalScanlinesNtsc = 262;
        public const int NumTotalPixelsPerScanline = 342;

        public const int NumActiveScanlines = 192;
        public const int NumActivePixelsPerScanline = 256;

        // TODO: pixel-based renderer...? maybe not...
        protected const int pixelLeftBlanking1 = 0;
        protected const int pixelColorBurst = (pixelLeftBlanking1 + 2);
        protected const int pixelLeftBlanking2 = (pixelColorBurst + 14);
        protected const int pixelLeftBorder = (pixelLeftBlanking2 + 8);
        protected const int pixelActiveDisplay = (pixelLeftBorder + 13);
        protected const int pixelRightBorder = (pixelActiveDisplay + 256);
        protected const int pixelRightBlanking = (pixelRightBorder + 15);
        protected const int pixelHorizontalSync = (pixelRightBlanking + 8);

        public const int NumSpritesMax = 32;

        protected bool isPalChip;

        protected byte[] registers, vram;

        protected ushort vramMask16k { get { return 0x3FFF; } }
        protected ushort vramMask4k { get { return 0x0FFF; } }

        protected bool isSecondControlWrite;
        protected ushort controlWord;
        protected byte readBuffer;

        protected byte codeRegister { get { return (byte)((controlWord >> 14) & 0x03); } }
        protected ushort addressRegister
        {
            get { return (ushort)(controlWord & 0x3FFF); }
            set { controlWord = (ushort)((codeRegister << 14) | (value & 0x3FFF)); }
        }

        public virtual int NumTotalScanlines { get { return (isPalChip ? NumTotalScanlinesPal : NumTotalScanlinesNtsc); } }

        [Flags]
        protected enum StatusFlags : byte
        {
            None = 0,
            SpriteCollision = (1 << 5),
            SpriteOverflow = (1 << 6),
            FrameInterruptPending = (1 << 7)
        }
        protected StatusFlags statusFlags;
        protected bool isSpriteCollision
        {
            get { return ((statusFlags & StatusFlags.SpriteCollision) == StatusFlags.SpriteCollision); }
            set { statusFlags = ((statusFlags & ~StatusFlags.SpriteCollision) | (value ? StatusFlags.SpriteCollision : StatusFlags.None)); }
        }
        protected bool isSpriteOverflow
        {
            get { return ((statusFlags & StatusFlags.SpriteOverflow) == StatusFlags.SpriteOverflow); }
            set { statusFlags = ((statusFlags & ~StatusFlags.SpriteOverflow) | (value ? StatusFlags.SpriteOverflow : StatusFlags.None)); }
        }
        protected bool isFrameInterruptPending
        {
            get { return ((statusFlags & StatusFlags.FrameInterruptPending) == StatusFlags.FrameInterruptPending); }
            set { statusFlags = ((statusFlags & ~StatusFlags.FrameInterruptPending) | (value ? StatusFlags.FrameInterruptPending : StatusFlags.None)); }
        }
        protected bool isFrameInterruptEnabled { get { return BitUtilities.IsBitSet(registers[0x01], 5); } }

        public InterruptState InterruptLine { get; protected set; }

        protected int scanlineActiveDisplay, scanlineBottomBorder, scanlineBottomBlanking, scanlineVerticalBlanking, scanlineTopBlanking, scanlineTopBorder;
        protected int currentScanline;

        protected bool isDisplayBlanked { get { return !BitUtilities.IsBitSet(registers[0x01], 6); } }

        protected bool is16kVRAMEnabled { get { return BitUtilities.IsBitSet(registers[0x01], 7); } }

        protected bool isBitM1Set { get { return BitUtilities.IsBitSet(registers[0x01], 4); } }
        protected bool isBitM2Set { get { return BitUtilities.IsBitSet(registers[0x00], 1); } }
        protected bool isBitM3Set { get { return BitUtilities.IsBitSet(registers[0x01], 3); } }

        protected virtual bool isModeGraphics1 { get { return !(isBitM1Set || isBitM2Set || isBitM3Set); } }
        protected virtual bool isModeText { get { return (isBitM1Set && !(isBitM2Set || isBitM3Set)); } }
        protected virtual bool isModeGraphics2 { get { return (isBitM2Set && !(isBitM1Set || isBitM3Set)); } }
        protected virtual bool isModeMulticolor { get { return (isBitM3Set && !(isBitM1Set || isBitM2Set)); } }

        protected bool isLargeSprites { get { return BitUtilities.IsBitSet(registers[0x01], 1); } }
        protected bool isZoomedSprites { get { return BitUtilities.IsBitSet(registers[0x01], 0); } }

        protected virtual ushort nametableBaseAddress { get { return (ushort)((registers[0x02] & 0x0F) << 10); } }
        protected virtual ushort spriteAttribTableBaseAddress { get { return (ushort)((registers[0x05] & 0x7F) << 7); } }
        protected virtual ushort spritePatternGenBaseAddress { get { return (ushort)((registers[0x06] & 0x07) << 11); } }

        protected int backgroundColor { get { return (registers[0x07] & 0x0F); } }
        protected int textColor { get { return ((registers[0x07] >> 4) & 0x0F); } }

        /* http://www.smspower.org/Development/Palette */
        byte[][] colorValuesBgra = new byte[][]
        {
            /*              B     G     R     A */
            new byte[] { 0x00, 0x00, 0x00, 0xFF },  /* Transparent */
            new byte[] { 0x00, 0x00, 0x00, 0xFF },  /* Black */
            new byte[] { 0x3B, 0xB7, 0x47, 0xFF },  /* Medium green */
            new byte[] { 0x6F, 0xCF, 0x7C, 0xFF },  /* Light green */
            new byte[] { 0xFF, 0x4E, 0x5D, 0xFF },  /* Dark blue */
            new byte[] { 0xFF, 0x72, 0x80, 0xFF },  /* Light blue */
            new byte[] { 0x47, 0x62, 0xB6, 0xFF },  /* Dark red */
            new byte[] { 0xED, 0xC8, 0x5D, 0xFF },  /* Cyan */
            new byte[] { 0x48, 0x6B, 0xD7, 0xFF },  /* Medium red */
            new byte[] { 0x6C, 0x8F, 0xFB, 0xFF },  /* Light red */
            new byte[] { 0x41, 0xCD, 0xC3, 0xFF },  /* Dark yellow */
            new byte[] { 0x76, 0xDA, 0xD3, 0xFF },  /* Light yellow */
            new byte[] { 0x2F, 0x9F, 0x3E, 0xFF },  /* Dark green */
            new byte[] { 0xC7, 0x64, 0xB6, 0xFF },  /* Magenta */
            new byte[] { 0xCC, 0xCC, 0xCC, 0xFF },  /* Gray */
            new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }   /* White */
        };

        protected const byte screenUsageEmpty = 0;
        protected const byte screenUsageSprite = (1 << 0);
        protected const byte screenUsageBg = (1 << 1);

        protected byte[] screenUsage;
        protected byte[] outputFramebuffer;

        public byte[] OutputFramebuffer { get { return outputFramebuffer; } }

        protected int cycleCount;
        double clockRate, refreshRate;

        protected int clockCyclesPerLine;

        public TMS9918A(double clockRate, double refreshRate, bool isPalChip)
        {
            this.clockRate = clockRate;
            this.refreshRate = refreshRate;
            this.isPalChip = isPalChip;

            registers = new byte[0x08];
            vram = new byte[0x4000];

            screenUsage = new byte[NumActivePixelsPerScanline * NumTotalScanlines];

            outputFramebuffer = new byte[(NumActivePixelsPerScanline * NumTotalScanlines) * 4];

            clockCyclesPerLine = (int)Math.Round((clockRate / refreshRate) / NumTotalScanlines);
        }

        public virtual void Startup()
        {
            Reset();
        }

        public virtual void Reset()
        {
            for (int i = 0; i < vram.Length; i++) vram[i] = 0;

            isSecondControlWrite = false;
            controlWord = 0x0000;
            readBuffer = 0;

            statusFlags = StatusFlags.None;

            SetScanlineBoundaries();
            currentScanline = 0;

            ClearScreen();

            cycleCount = 0;
        }

        public virtual void SetScanlineBoundaries()
        {
            if (!isPalChip)
            {
                scanlineActiveDisplay = 0;
                scanlineBottomBorder = (scanlineActiveDisplay + 192);
                scanlineBottomBlanking = (scanlineBottomBorder + 24);
                scanlineVerticalBlanking = (scanlineBottomBlanking + 3);
                scanlineTopBlanking = (scanlineVerticalBlanking + 3);
                scanlineTopBorder = (scanlineTopBlanking + 13);
            }
            else
            {
                scanlineActiveDisplay = 0;
                scanlineBottomBorder = (scanlineActiveDisplay + 192);
                scanlineBottomBlanking = (scanlineBottomBorder + 48);
                scanlineVerticalBlanking = (scanlineBottomBlanking + 3);
                scanlineTopBlanking = (scanlineVerticalBlanking + 3);
                scanlineTopBorder = (scanlineTopBlanking + 13);
            }
        }

        public virtual bool Step(int clockCyclesInStep)
        {
            bool drawScreen = false;

            cycleCount += clockCyclesInStep;

            if (cycleCount >= clockCyclesPerLine)
            {
                if (currentScanline == 0) ClearScreen();

                RenderLine(currentScanline);
                currentScanline++;

                if (currentScanline == (NumActiveScanlines + 1))
                    isFrameInterruptPending = true;

                if (currentScanline == NumTotalScanlines)
                {
                    RearrangeFramebuffer();

                    currentScanline = 0;
                    drawScreen = true;
                }

                if (isFrameInterruptEnabled && isFrameInterruptPending)
                    InterruptLine = InterruptState.Assert;

                cycleCount -= clockCyclesPerLine;
            }

            return drawScreen;
        }

        protected virtual void RearrangeFramebuffer()
        {
            // TODO: a bit of a kludge, maybe rework somehow?
            int bytesUntilDisplayEnd = ((scanlineTopBlanking * NumActivePixelsPerScanline) * 4);
            byte[] pixelDataTop = new byte[((NumTotalScanlines - scanlineTopBlanking) * NumActivePixelsPerScanline) * 4];

            Buffer.BlockCopy(outputFramebuffer, bytesUntilDisplayEnd, pixelDataTop, 0, pixelDataTop.Length);
            Buffer.BlockCopy(outputFramebuffer, 0, outputFramebuffer, pixelDataTop.Length, bytesUntilDisplayEnd);
            Buffer.BlockCopy(pixelDataTop, 0, outputFramebuffer, 0, pixelDataTop.Length);
        }

        protected virtual byte ReadVram(ushort address)
        {
            if (is16kVRAMEnabled)
                return vram[address & vramMask16k];
            else
                return vram[address & vramMask4k];
        }

        protected virtual void WriteVram(ushort address, byte value)
        {
            if (is16kVRAMEnabled)
                vram[address & vramMask16k] = value;
            else
                vram[address & vramMask4k] = value;
        }

        protected virtual void ClearScreen()
        {
            for (int i = 0; i < screenUsage.Length; i++) screenUsage[i] = screenUsageEmpty;
        }

        protected virtual void RenderLine(int line)
        {
            if (line < scanlineBottomBorder)
            {
                /* Active display */
                if (isModeGraphics1)
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
                BlankLine(line, (byte)backgroundColor);
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
                BlankLine(line, (byte)backgroundColor);
            }
        }

        protected void BlankLine(int line, ushort colorValue)
        {
            int outputY = (line * NumActivePixelsPerScanline);
            for (int x = 0; x < NumActivePixelsPerScanline; x++)
                WriteColorToFramebuffer(colorValue, (outputY + (x % NumActivePixelsPerScanline)) * 4);
        }

        protected void BlankLine(int line, byte b, byte g, byte r)
        {
            int outputY = (line * NumActivePixelsPerScanline);
            for (int x = 0; x < NumActivePixelsPerScanline; x++)
                WriteColorToFramebuffer(b, g, r, (outputY + (x % NumActivePixelsPerScanline)) * 4);
        }

        protected void RenderGraphics1Background(int line)
        {
            /* Calculate/set some variables we'll need */
            int tileWidth = 8;
            int numTilesPerLine = 32;
            ushort patternGeneratorBaseAddress = (ushort)((registers[0x04] & 0x07) << 11);
            ushort colorTableBaseAddress = (ushort)(registers[0x03] << 6);

            for (int tile = 0; tile < numTilesPerLine; tile++)
            {
                /* Calculate nametable address, fetch character number */
                ushort nametableAddress = (ushort)(nametableBaseAddress + ((line / 8) * numTilesPerLine) + tile);
                byte characterNumber = ReadVram(nametableAddress);

                /* Fetch pixel and color data for current pixel line (1 byte, 8 pixels) */
                byte pixelLineData = ReadVram((ushort)(patternGeneratorBaseAddress + (characterNumber * 8) + (line % 8)));
                byte pixelLineColor = ReadVram((ushort)(colorTableBaseAddress + (characterNumber / 8)));

                /* Extract background and foreground color indices */
                byte[] colorIndicesBackgroundForeground = new byte[2];
                colorIndicesBackgroundForeground[0] = (byte)(pixelLineColor & 0x0F);
                colorIndicesBackgroundForeground[1] = (byte)(pixelLineColor >> 4);

                /* Draw pixels */
                for (int pixel = 0; pixel < tileWidth; pixel++)
                {
                    /* Fetch color index for current pixel (bit clear means background, bit set means foreground color) */
                    byte c = colorIndicesBackgroundForeground[((pixelLineData >> (7 - pixel)) & 0x01)];
                    /* Color index 0 is transparent, use background color */
                    if (c == 0 || isDisplayBlanked) c = (byte)backgroundColor;

                    /* Calculate output framebuffer location, get BGRA values from color table, write to framebuffer */
                    int outputY = ((line % NumTotalScanlines) * NumActivePixelsPerScanline);
                    int outputX = (((tile * tileWidth) + pixel) % NumActivePixelsPerScanline);

                    if (screenUsage[outputY + outputX] == screenUsageEmpty)
                    {
                        WriteColorToFramebuffer(c, ((outputY + outputX) * 4));
                        screenUsage[outputY + outputX] |= screenUsageBg;
                    }
                }
            }
        }

        protected void RenderGraphics2Background(int line)
        {
            int numTilesPerLine = (NumActivePixelsPerScanline / 8);

            /* Calculate some base addresses */
            ushort patternGeneratorBaseAddress = (ushort)((registers[0x04] & 0x04) << 11);
            ushort colorTableBaseAddress = (ushort)((registers[0x03] & 0x80) << 6);

            for (int tile = 0; tile < numTilesPerLine; tile++)
            {
                /* Calculate nametable address */
                ushort nametableAddress = (ushort)(nametableBaseAddress + ((line / 8) * numTilesPerLine) + tile);

                /* Calculate character number and masks */
                ushort characterNumber = (ushort)(((line / 64) << 8) | ReadVram(nametableAddress));
                ushort characterNumberDataMask = (ushort)(((registers[0x04] & 0x03) << 8) | 0xFF);
                ushort characterNumberColorMask = (ushort)(((registers[0x03] & 0x7F) << 3) | 0x07);

                /* Fetch pixel and color data for current pixel line (1 byte, 8 pixels) */
                byte pixelLineData = ReadVram((ushort)(patternGeneratorBaseAddress + ((characterNumber & characterNumberDataMask) * 8) + (line % 8)));
                byte pixelLineColor = ReadVram((ushort)(colorTableBaseAddress + ((characterNumber & characterNumberColorMask) * 8) + (line % 8)));

                /* Extract background and foreground color indices */
                byte[] colorIndicesBackgroundForeground = new byte[2];
                colorIndicesBackgroundForeground[0] = (byte)(pixelLineColor & 0x0F);
                colorIndicesBackgroundForeground[1] = (byte)(pixelLineColor >> 4);

                /* Draw pixels */
                for (int pixel = 0; pixel < 8; pixel++)
                {
                    /* Fetch color index for current pixel (bit clear means background, bit set means foreground color) */
                    byte c = colorIndicesBackgroundForeground[((pixelLineData >> (7 - pixel)) & 0x01)];
                    /* Color index 0 is transparent, use background color */
                    if (c == 0 || isDisplayBlanked) c = (byte)backgroundColor;

                    /* Calculate output framebuffer location, get BGRA values from color table, write to framebuffer */
                    int outputY = ((line % NumTotalScanlines) * NumActivePixelsPerScanline);
                    int outputX = (((tile * 8) + pixel) % NumActivePixelsPerScanline);

                    if (screenUsage[outputY + outputX] == screenUsageEmpty)
                    {
                        WriteColorToFramebuffer(c, ((outputY + outputX) * 4));
                        screenUsage[outputY + outputX] |= screenUsageBg;
                    }
                }
            }
        }

        protected void RenderMulticolorBackground(int line)
        {
            // TODO w/ Smurf Paint & Play (CV)
        }

        protected void RenderTextBackground(int line)
        {
            /* Draw left and right borders */
            for (int i = 0; i < 8; i++)
            {
                int outputY = ((line % NumTotalScanlines) * NumActivePixelsPerScanline);
                int outputX = (i % NumActivePixelsPerScanline);

                WriteColorToFramebuffer((byte)backgroundColor, ((outputY + outputX) * 4));
                WriteColorToFramebuffer((byte)backgroundColor, ((outputY + (outputX + (NumActivePixelsPerScanline - 8))) * 4));
            }

            /* Calculate/set some variables we'll need */
            int tileWidth = 6;
            int numTilesPerLine = 40;
            ushort patternGeneratorBaseAddress = (ushort)((registers[0x04] & 0x07) << 11);

            /* Get background and text color indices */
            byte[] colorIndicesBackgroundText = new byte[2];
            colorIndicesBackgroundText[0] = (byte)backgroundColor;
            colorIndicesBackgroundText[1] = (byte)textColor;

            for (int tile = 0; tile < numTilesPerLine; tile++)
            {
                /* Calculate nametable address, fetch character number */
                ushort nametableAddress = (ushort)(nametableBaseAddress + ((line / 8) * numTilesPerLine) + tile);
                byte characterNumber = ReadVram(nametableAddress);

                /* Fetch pixel data for current pixel line (1 byte, 8 pixels) */
                byte pixelLineData = ReadVram((ushort)(patternGeneratorBaseAddress + (characterNumber * 8) + (line % 8)));

                /* Draw pixels */
                for (int pixel = 0; pixel < tileWidth; pixel++)
                {
                    /* Fetch color index for current pixel (bit clear means background, bit set means text color) */
                    byte c = (isDisplayBlanked ? (byte)backgroundColor : colorIndicesBackgroundText[((pixelLineData >> (7 - pixel)) & 0x01)]);

                    /* Calculate output framebuffer location, get BGRA values from color table, write to framebuffer */
                    int outputY = ((line % NumTotalScanlines) * NumActivePixelsPerScanline);
                    int outputX = (8 + ((tile * tileWidth) + pixel) % NumActivePixelsPerScanline);

                    if (screenUsage[outputY + outputX] == screenUsageEmpty)
                    {
                        WriteColorToFramebuffer(c, ((outputY + outputX) * 4));
                        screenUsage[outputY + outputX] |= screenUsageBg;
                    }
                }
            }
        }

        protected void RenderSprites(int line)
        {
            /* Clear last illegal sprite number (if any) and overflow flag from status register */
            statusFlags &= (StatusFlags.FrameInterruptPending | StatusFlags.SpriteCollision);

            /* Determine sprite size */
            int spriteSize = (isLargeSprites ? 16 : 8);

            /* Check and adjust for zoomed sprites */
            int zoomShift = (isZoomedSprites ? 1 : 0);
            int zoomOffset = (8 << zoomShift);

            int numSprites = 0;
            for (int sprite = 0; sprite < NumSpritesMax; sprite++)
            {
                int yCoordinate = ReadVram((ushort)(spriteAttribTableBaseAddress + (sprite * 4)));

                /* Ignore following if Y coord is 208 */
                if (yCoordinate == 208)
                {
                    /* Store first "illegal sprite" number in status register */
                    statusFlags |= (StatusFlags)sprite;
                    return;
                }

                /* Modify Y coord as needed */
                yCoordinate++;
                if (yCoordinate > NumActiveScanlines)
                    yCoordinate -= 256;

                /* Ignore this sprite if on incorrect lines */
                if (line < yCoordinate || line >= (yCoordinate + zoomOffset)) continue;

                /* Check for sprite overflow */
                numSprites++;
                if (numSprites > 4)
                {
                    isSpriteOverflow = true;
                    /* Store sprite number in status register */
                    statusFlags |= (StatusFlags)sprite;
                    return;
                }

                if (!isDisplayBlanked)
                {
                    int xCoordinate = ReadVram((ushort)(spriteAttribTableBaseAddress + (sprite * 4) + 1));
                    int characterNumber = ReadVram((ushort)(spriteAttribTableBaseAddress + (sprite * 4) + 2));
                    int attributes = ReadVram((ushort)(spriteAttribTableBaseAddress + (sprite * 4) + 3));

                    /* Extract attributes */
                    bool earlyClock = ((attributes & 0x80) == 0x80);
                    int spriteColor = (attributes & 0x0F);

                    /* Adjust according to registers/attributes */
                    if (earlyClock) xCoordinate -= 32;
                    if (isLargeSprites) characterNumber &= 0xFC;

                    ushort spritePatternAddress = (ushort)(spritePatternGenBaseAddress + (characterNumber * 8) + (((line - yCoordinate) >> zoomShift) % spriteSize));
                    if (isLargeSprites)
                    {
                        RenderSpriteTile(spritePatternAddress, xCoordinate, line, spriteColor, zoomShift);
                        RenderSpriteTile((ushort)(spritePatternAddress + 16), (xCoordinate + zoomOffset), line, spriteColor, zoomShift);

                        RenderSpriteTile((ushort)(spritePatternAddress + 8), xCoordinate, (line + zoomOffset), spriteColor, zoomShift);
                        RenderSpriteTile((ushort)(spritePatternAddress + 24), (xCoordinate + zoomOffset), (line + zoomOffset), spriteColor, zoomShift);
                    }
                    else
                    {
                        RenderSpriteTile(spritePatternAddress, xCoordinate, line, spriteColor, zoomShift);
                    }
                }
            }

            /* Because we didn't bow out before already, store total number of sprites in status register */
            statusFlags |= (StatusFlags)31;
        }

        protected void RenderSpriteTile(ushort spritePatternAddress, int xCoordinate, int yCoordinate, int spriteColor, int zoomShift)
        {
            /* Fetch pixel data */
            byte pixelLineData = ReadVram(spritePatternAddress);

            /* Draw pixels */
            for (int pixel = 0; pixel < (8 * (zoomShift + 1)); pixel++)
            {
                /* Check if a pixel needs to be drawn, and if we're outside the screen */
                if (spriteColor == 0 || ((pixelLineData >> (7 - (pixel >> zoomShift))) & 0x01) == 0x00 || (xCoordinate + pixel) >= NumActivePixelsPerScanline || (xCoordinate + pixel) < 0) continue;

                /* Calculate output framebuffer coordinates */
                int outputY = ((yCoordinate + scanlineActiveDisplay) * NumActivePixelsPerScanline);
                int outputX = ((xCoordinate + pixel) % NumActivePixelsPerScanline);

                if ((screenUsage[outputY + outputX] & screenUsageSprite) == screenUsageSprite)
                {
                    /* Set sprite collision flag */
                    isSpriteCollision = true;
                }
                else
                {
                    /* Get BGRA values from color table, write to framebuffer */
                    WriteColorToFramebuffer((ushort)spriteColor, ((outputY + outputX) * 4));
                }

                /* Note that there is a sprite here regardless */
                screenUsage[outputY + outputX] |= screenUsageSprite;
            }
        }

        protected void WriteColorToFramebuffer(byte b, byte g, byte r, int address)
        {
            outputFramebuffer[address] = b;
            outputFramebuffer[address + 1] = g;
            outputFramebuffer[address + 2] = r;
            outputFramebuffer[address + 3] = 0xFF;
        }

        protected virtual void WriteColorToFramebuffer(ushort colorValue, int address)
        {
            Buffer.BlockCopy(colorValuesBgra[(colorValue & 0x0F)], 0, outputFramebuffer, address, 4);
        }

        public virtual byte ReadDataPort()
        {
            isSecondControlWrite = false;
            statusFlags = StatusFlags.None;

            byte data = readBuffer;
            readBuffer = ReadVram(addressRegister);
            addressRegister++;

            return data;
        }

        public virtual void WriteDataPort(byte value)
        {
            isSecondControlWrite = false;

            readBuffer = value;
            WriteVram(addressRegister, value);
            addressRegister++;
        }

        public virtual byte ReadControlPort()
        {
            byte statusCurrent = (byte)statusFlags;

            statusFlags = StatusFlags.None;
            isSecondControlWrite = false;

            InterruptLine = InterruptState.Clear;

            return statusCurrent;
        }

        public virtual void WriteControlPort(byte value)
        {
            if (!isSecondControlWrite)
                controlWord = (ushort)((controlWord & 0xFF00) | (value << 0));
            else
            {
                controlWord = (ushort)((controlWord & 0x00FF) | (value << 8));

                switch (codeRegister)
                {
                    case 0x00: readBuffer = ReadVram(addressRegister); addressRegister++; break;
                    case 0x01: break;
                    case 0x02: WriteRegister((byte)((controlWord >> 8) & 0x0F), (byte)(controlWord & 0xFF)); break;
                    case 0x03: break;
                }
            }

            isSecondControlWrite = !isSecondControlWrite;
        }

        protected virtual void WriteRegister(byte register, byte value)
        {
            // TODO: same as with SMS2 VDP, correct behavior on invalid registers?
            if (register < registers.Length)
                registers[register] = value;
        }
    }
}
