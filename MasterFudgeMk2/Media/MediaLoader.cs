using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Media.Sega;
using MasterFudgeMk2.Media.MSX;
using MasterFudgeMk2.Machines;

namespace MasterFudgeMk2.Media
{
    public static class MediaLoader
    {
        static Dictionary<uint, MediaIdentity> mediaIdents = new Dictionary<uint, MediaIdentity>()
        {
            /* SG-1000 */
            { 0x092F29D6, new MediaIdentity() { Name = "The Castle (SG-1000)", MediaType = typeof(RomRam32kCartridge) } },
            { 0xAF4F14BC, new MediaIdentity() { Name = "Othello (SG-1000)", MediaType = typeof(RomRam32kCartridge) } },

            // TODO: region forcing, ex. Pop Breaker (GG)? not sure yet
        };

        public static IMedia LoadMedia(IMachineManager machineManager, FileInfo fileInfo)
        {
            uint crc = Crc32.Calculate(fileInfo);

            IMedia media;

            /* Is media known to need special care? */
            MediaIdentity cartIdent = (mediaIdents.ContainsKey(crc) ? mediaIdents[crc] : null);
            if (cartIdent != null)
            {
                media = (Activator.CreateInstance(cartIdent.MediaType) as IMedia);
            }
            else if (machineManager is Machines.Sega.SG1000.Manager || machineManager is Machines.Sega.MasterSystem.Manager || machineManager is Machines.Sega.GameGear.Manager || machineManager is Machines.Sega.SC3000.Manager)
            {
                if (fileInfo.Length <= 0xC000)
                {
                    /* Size is 48k max, assume ROM only mapper */
                    media = (new RomOnlyCartridge() as IMedia);
                }
                else
                {
                    /* No special treatment and bigger than 48k, assume default Sega mapper */
                    media = (new StandardMapperCartridge() as IMedia);
                }
            }
            else if (machineManager is Machines.Coleco.ColecoVision.Manager)
            {
                // TODO: whatever mapper(s) there's on Coleco
                media = (new RomOnlyCartridge() as IMedia);
            }
            else if (machineManager is Machines.Various.MSX.Manager || machineManager is Machines.Various.MSX2.Manager)
            {
                if (fileInfo.Length <= 0x8000)
                    media = (new RawRomCartridge() as IMedia);
                else
                    media = (Activator.CreateInstance(IdentifyMsxMapper(fileInfo)) as IMedia);
            }
            else
            {
                throw new Exception("Could not identify cartridge");
            }

            media?.Load(fileInfo);

            return media;
        }

        private static Type IdentifyMsxMapper(FileInfo fileInfo)
        {
            /* Check for writes to cartridge
             *  http://problemkaputt.de/portar.htm#cartridgememorymappers
             *  http://bifi.msxnet.org/msxnet/tech/megaroms
             */

            /* Opcode to look for; LD (nnnn), A */
            const byte opcodeCheckLd = 0x32;

            /* Initialize type dictionary */
            var mapperTypeProbabilities = new Dictionary<Type, int>()
            {
                { typeof(Konami8kCartridge), 0 },
                { typeof(Konami8kSccCartridge), 0 },
                { typeof(Ascii8kCartridge), 1 },    /* Give ASCII mappers more weight, seem less reliable to detect...? */
                { typeof(Ascii16kCartridge), 2 }
            };

            /* Load & check the first ~16k of the given file */
            byte[] romData = new byte[Math.Min(0x4000, fileInfo.Length)];
            using (FileStream file = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { file.Read(romData, 0, romData.Length); }

            for (int i = 0x10; i < Math.Min(0x4000, romData.Length) - 3; i++)
            {
                /* Check for opcode */
                byte opcode = romData[i];
                if (opcode == opcodeCheckLd)
                {
                    /* Check for bankswitching addresses, as per Portar */
                    ushort address = (ushort)(romData[i + 2] << 8 | (romData[i + 1]));

                    if (address >= 0x4000 && address <= 0xBFFF)
                    {
                        if (address >= 0x5000 && address <= 0x57FF)
                        {
                            mapperTypeProbabilities[typeof(Konami8kSccCartridge)]++;
                        }
                        else if (address >= 0x6000 && address <= 0x67FF)
                        {
                            if (address == 0x6000)
                                mapperTypeProbabilities[typeof(Konami8kCartridge)]++;

                            mapperTypeProbabilities[typeof(Ascii8kCartridge)]++;
                            mapperTypeProbabilities[typeof(Ascii16kCartridge)]++;
                        }
                        else if (address >= 0x6800 && address <= 0x6FFF)
                        {
                            mapperTypeProbabilities[typeof(Ascii8kCartridge)]++;
                        }
                        else if (address >= 0x7000 && address <= 0x77FF)
                        {
                            mapperTypeProbabilities[typeof(Konami8kSccCartridge)]++;
                            mapperTypeProbabilities[typeof(Ascii8kCartridge)]++;
                            mapperTypeProbabilities[typeof(Ascii16kCartridge)]++;
                        }
                        else if (address >= 0x7800 && address <= 0x7FFF)
                        {
                            mapperTypeProbabilities[typeof(Ascii8kCartridge)]++;
                        }
                        else if (address == 0x8000)
                        {
                            mapperTypeProbabilities[typeof(Konami8kCartridge)]++;
                        }
                        else if (address >= 0x9000 && address <= 0x97FF)
                        {
                            mapperTypeProbabilities[typeof(Konami8kSccCartridge)]++;
                        }
                        else if (address == 0xA000)
                        {
                            mapperTypeProbabilities[typeof(Konami8kCartridge)]++;
                        }
                        else if (address >= 0xB000 && address <= 0xB7FF)
                        {
                            mapperTypeProbabilities[typeof(Konami8kSccCartridge)]++;
                        }
                    }
                }
            }

            /* Get mapper type with highest probability */
            return mapperTypeProbabilities.OrderByDescending(x => x.Value).FirstOrDefault().Key;
        }
    }
}
