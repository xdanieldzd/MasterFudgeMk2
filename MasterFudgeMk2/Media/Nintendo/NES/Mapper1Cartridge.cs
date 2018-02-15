using System;

namespace MasterFudgeMk2.Media.Nintendo.NES
{
    public class Mapper1Cartridge : BaseCartridge
    {
        byte shiftRegister, shiftCount;
        byte[] prgRam;

        // Control reg
        byte mirroring, prgRomBankMode, prgRomBankSize, chrRomBankMode;

        // CHR banks 0 & 1
        byte[] chrBanks;

        // PRG bank
        byte prgBank;
        bool prgRamEnable;      // TODO: currently ignored; different MMC1s and/or board variants work differently

        public Mapper1Cartridge()
        {
            prgRam = new byte[0x2000];
            chrBanks = new byte[2];
        }

        public override void Reset()
        {
            shiftRegister = 0x10;
            shiftCount = 0;

            mirroring = 0x00;
            prgRomBankMode = 0x01;
            prgRomBankSize = 0x01;
            chrRomBankMode = 0x00;

            chrBanks[0] = 0x00;
            chrBanks[1] = 0x00;

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
                    if (true || prgRamEnable)
                        return prgRam[address & (prgRam.Length - 1)];
                    else
                        return 0x00;

                case 0x8000:
                case 0x9000:
                case 0xA000:
                case 0xB000:
                    if (prgRomBankSize == 0x00)
                        return prgData[(uint)(((prgBank + 0) & 0x1E) << 13) | (address & 0x3FFF)];
                    else
                        return prgData[(prgRomBankMode == 0x00 ? 0 : (uint)((prgBank & 0x1F) << 14)) | (address & 0x3FFF)];

                case 0xC000:
                case 0xD000:
                case 0xE000:
                case 0xF000:
                    if (prgRomBankSize == 0x00)
                        return prgData[(uint)(((prgBank + 1) & 0x1E) << 13) | (address & 0x3FFF)];
                    else
                        return prgData[(prgRomBankMode != 0x00 ? (uint)(prgData.Length - 0x4000) : (uint)((prgBank & 0x1F) << 14)) | (address & 0x3FFF)];

                default:
                    throw new Exception($"iNES Mapper 1: invalid read from 0x{address:X4}");
            }
        }

        public override void Write(uint address, byte value)
        {
            if (address >= 0x6000 && address <= 0x7FFF)
            {
                if (true || prgRamEnable)
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
                                chrBanks[0] = (byte)(shiftRegister & 0x1F);
                                break;

                            case 0x02:
                                chrBanks[1] = (byte)(shiftRegister & 0x1F);
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

        private uint GetChrAddress(uint address)
        {
            byte chunk4k = (byte)((address >> 12) & 0x01);
            if (chrRomBankMode == 0x00)
                return ((uint)(((chrBanks[0] + chunk4k) & 0x1E) << 13) | (address & 0x1FFF));
            else
                return ((uint)((chrBanks[chunk4k] & 0x1F) << 12) | (address & 0x0FFF));
        }

        public override byte ReadChr(uint address)
        {
            return chrData[GetChrAddress(address & 0x3FFF)];
        }

        public override void WriteChr(uint address, byte value)
        {
            chrData[GetChrAddress(address & 0x3FFF)] = value;
        }

        public override uint NametableMirror(uint address)
        {
            switch (mirroring)
            {
                case 0x00: return (0x0000 | (address & 0x03FF));
                case 0x01: return (0x0400 | (address & 0x03FF));
                case 0x02: return (((address & 0x0400) >> 0) | (address & 0x03FF));
                case 0x03: return (((address & 0x0800) >> 1) | (address & 0x03FF));
                default: throw new Exception($"iNES Mapper 1: unsupported mirroring mode 0x{mirroring:X2}");
            }
        }
    }
}
