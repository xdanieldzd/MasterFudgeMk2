using System;

namespace MasterFudgeMk2.Media.Nintendo.NES
{
    public class Mapper1Cartridge : BaseCartridge
    {
        byte shiftRegister, shiftCount;
        byte[] prgRam;

        // Control reg
        byte mirroring, prgRomBankMode, prgRomBankSize, chrRomBankMode;

        // CHR bank 0
        byte chrBank0;

        // CHR bank 1
        byte chrBank1;

        // PRG bank
        byte prgBank;
        bool prgRamEnable;

        public Mapper1Cartridge()
        {
            prgRam = new byte[0x2000];
        }

        public override void Reset()
        {
            shiftRegister = 0x10;
            shiftCount = 0;

            mirroring = 0x00;
            prgRomBankMode = 0x01;
            prgRomBankSize = 0x01;
            chrRomBankMode = 0x00;

            chrBank0 = 0x00;

            chrBank1 = 0x00;

            prgBank = 0x00;
            prgRamEnable = true;
        }

        public override byte Read(uint address)
        {
            switch (address & 0xF000)
            {
                case 0x4000:
                case 0x5000:
                    return 0x00;

                case 0x6000:
                case 0x7000:
                    if (prgRamEnable)
                        return prgRam[address & (prgRam.Length - 1)];
                    else
                        return 0x00;

                case 0x8000:
                case 0x9000:
                case 0xA000:
                case 0xB000:
                    if (prgRomBankSize == 0x00)
                        return prgData[(prgBank & ((prgData.Length - 1) & 0x1E)) + 0][address & prgDataMask];
                    else
                        return prgData[prgRomBankMode == 0x00 ? 0 : prgBank & ((prgData.Length - 1) & 0x1F)][address & prgDataMask];

                case 0xC000:
                case 0xD000:
                case 0xE000:
                case 0xF000:
                    if (prgRomBankSize == 0x00)
                        return prgData[(prgBank & ((prgData.Length - 1) & 0x1E)) + 1][address & prgDataMask];
                    else
                        return prgData[prgRomBankMode != 0x00 ? (prgData.Length - 1) : prgBank & ((prgData.Length - 1) & 0x1F)][address & prgDataMask];

                default:
                    throw new Exception($"iNES Mapper 1: invalid read from 0x{address:X4}");
            }
        }

        public override void Write(uint address, byte value)
        {
            if (address >= 0x6000 && address <= 0x7FFF)
            {
                if (prgRamEnable)
                    prgRam[address & (prgRam.Length) - 1] = value;
            }
            else if ((address & 0x8000) == 0x8000)
            {
                if ((value & 0x80) == 0x80)
                {
                    prgRomBankMode = 0x01;
                    prgRomBankSize = 0x01;

                    shiftRegister = 0x10;
                }
                else
                {
                    shiftRegister = (byte)(((value & 0x01) << 4) | ((shiftRegister >> 1) & 0xF));
                    shiftCount++;
                    if (shiftCount == 5)
                    {
                        byte register = (byte)((address & 0x6000) >> 13);
                        switch (register)
                        {
                            case 0x00:
                                mirroring = (byte)((shiftRegister >> 0) & 0x03);
                                prgRomBankMode = (byte)((shiftRegister >> 2) & 0x01);
                                prgRomBankSize = (byte)((shiftRegister >> 3) & 0x01);
                                chrRomBankMode = (byte)((shiftRegister >> 4) & 0x01);
                                break;

                            case 0x01:
                                chrBank0 = (byte)(shiftRegister & 0x1F);
                                break;

                            case 0x02:
                                chrBank1 = (byte)(shiftRegister & 0x1F);
                                break;

                            case 0x03:
                                prgBank = (byte)(shiftRegister & 0x0F);
                                prgRamEnable = ((shiftRegister & 0x10) == 0x10);
                                break;

                            default:
                                throw new Exception($"iNES Mapper 1: invalid shift register target 0x{register:X2}, address 0x{address:X4}");
                        }

                        shiftRegister = 0x10;
                        shiftCount = 0;
                    }
                }
            }
            else
            {
                throw new Exception($"iNES Mapper 1: invalid write to 0x{address:X4}, value 0x{value:X2}");
            }
        }

        public override byte ReadPrg(uint address)
        {
            throw new NotImplementedException();
        }

        public override byte ReadChr(uint address)
        {
            switch (address & 0xF000)
            {
                case 0x0000:
                    if (chrRomBankMode == 0x00)
                        return chrData[(chrBank0 & ((chrData.Length - 1) & 0x1E)) + 0][address & chrDataMask];
                    else
                        return chrData[(chrBank0 & ((chrData.Length - 1) & 0x1F))][address & chrDataMask];

                case 0x1000:
                    if (chrRomBankMode == 0x00)
                        return chrData[(chrBank0 & ((chrData.Length - 1) & 0x1E)) + 1][address & chrDataMask];
                    else
                        return chrData[(chrBank1 & ((chrData.Length - 1) & 0x1F))][address & chrDataMask];
            }

            return 0;
        }

        public override void WriteChr(uint address, byte value)
        {
            switch (address & 0xF000)
            {
                case 0x0000:
                    if (chrRomBankMode == 0x00)
                        chrData[(chrBank0 & ((chrData.Length - 1) & 0x1E)) + 0][address & chrDataMask] = value;
                    else
                        chrData[(chrBank0 & ((chrData.Length - 1) & 0x1F))][address & chrDataMask] = value;
                    break;

                case 0x1000:
                    if (chrRomBankMode == 0x00)
                        chrData[(chrBank0 & ((chrData.Length - 1) & 0x1E)) + 1][address & chrDataMask] = value;
                    else
                        chrData[(chrBank1 & ((chrData.Length - 1) & 0x1F))][address & chrDataMask] = value;
                    break;
            }
        }
    }
}
