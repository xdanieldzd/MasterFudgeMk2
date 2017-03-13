using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Media.Sega;
using MasterFudgeMk2.Media.Nintendo;
using MasterFudgeMk2.Machines;

namespace MasterFudgeMk2.Media
{
    public static class MediaLoader
    {
        // TODO: de-Sega-ify, make everything more generic, better NES mapper identification, etc, etc!

        static Dictionary<uint, MediaIdentity> mediaIdents = new Dictionary<uint, MediaIdentity>()
        {
            { 0x092F29D6, new MediaIdentity() { Name = "The Castle (SG-1000)", MediaType = typeof(RomRam32kCartridge) } },  // TODO: actually wrong type, needs special mapping
            { 0xAF4F14BC, new MediaIdentity() { Name = "Othello (SG-1000)", MediaType = typeof(RomRam32kCartridge) } },
        };

        public static IMedia LoadMedia(IMachineManager machineManager, FileInfo fileInfo)
        {
            byte[] romData = ReadRomData(fileInfo.FullName);
            uint crc = Utilities.CalculateCrc32(romData);

            IMedia media = null;

            /* Is media known to need special care? */
            MediaIdentity cartIdent = (mediaIdents.ContainsKey(crc) ? mediaIdents[crc] : null);
            if (cartIdent != null)
            {
                media = (Activator.CreateInstance(cartIdent.MediaType) as IMedia);
                media?.Load(romData);
            }
            else if ((machineManager is Machines.Sega.SG1000.Manager || machineManager is Machines.Sega.MasterSystem.Manager || machineManager is Machines.Sega.GameGear.Manager))
            {
                if (romData.Length <= 0xC000)
                {
                    /* Size is 48k max, assume ROM only mapper */
                    media = (new RomOnlyCartridge() as IMedia);
                }
                else
                {
                    /* No special treatment and bigger than 48k, assume default Sega mapper */
                    media = (new StandardMapperCartridge() as IMedia);
                }
                media?.Load(romData);
            }
            else if (machineManager is Machines.Coleco.ColecoVision.Manager)
            {
                media = (new RomOnlyCartridge() as IMedia);
                media?.Load(romData);
            }
            else if (machineManager is Machines.Nintendo.NES.Manager)
            {
                media = LoadMediaNES(machineManager, romData);
            }

            if (media == null)
                throw new Exception("Could not identify media");

            return media;
        }

        private static INESMedia LoadMediaNES(IMachineManager machineManager, byte[] romData)
        {
            RomHeaderINES inesHeader = new RomHeaderINES(romData);

            byte[] headerlessRom = new byte[romData.Length - 0x10];
            Buffer.BlockCopy(romData, 0x10, headerlessRom, 0, headerlessRom.Length);

            INESMedia media;

            switch (inesHeader.MapperNumber)
            {
                case 0x00: media = (new NROMCartridge() as INESMedia); break;
                default: throw new Exception("Unsupported mapper");
            }

            media.RomHeader = inesHeader;
            media?.Load(headerlessRom);

            return media;
        }

        public static byte[] ReadRomData(string filename)
        {
            using (FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] data;
                if ((file.Length % 0x4000) == 0x200)
                {
                    /* Sega ROM w/ copier header */
                    data = new byte[file.Length - (file.Length % 0x4000)];
                    file.Seek(file.Length % 0x4000, SeekOrigin.Begin);
                }
                else if ((file.Length % 0x2000) == 0x10)
                {
                    /* NES ROM */
                    data = new byte[file.Length];
                }
                else
                {
                    /* Raw ROM data */
                    data = new byte[file.Length];
                }
                file.Read(data, 0, data.Length);
                return data;
            }
        }
    }
}
