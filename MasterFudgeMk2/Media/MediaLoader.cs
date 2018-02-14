using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Media.Sega;
using MasterFudgeMk2.Machines;

namespace MasterFudgeMk2.Media
{
    public static class MediaLoader
    {
        static MediaOverrides mediaOverrides = XmlFile.Load<MediaOverrides>();

        public static IMedia LoadMedia(IMachineManager machineManager, FileInfo fileInfo)
        {
            uint crc = Crc32.Calculate(fileInfo);

            IMedia media;

            /* Is media known to need special care? */
            MediaOverrideEntry mediaOverride = mediaOverrides.MediaList.FirstOrDefault(x => x.Crc == crc);
            if (mediaOverride != null)
            {
                media = (Activator.CreateInstance(mediaOverride.MediaType) as IMedia);
            }
            else if (machineManager is Machines.Sega.SG1000.Manager || machineManager is Machines.Sega.MasterSystem.Manager || machineManager is Machines.Sega.GameGear.Manager)
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
                media = (new Coleco.RomOnlyCartridge() as IMedia);
            }
            else if (machineManager is Machines.NEC.PCEngine.Manager)
            {
                // TODO: PC-Engine mappers if any
                media = (new NEC.RomOnlyCartridge() as IMedia);
            }
            else if (machineManager is Machines.Nintendo.NES.Manager)
            {
                using (FileStream file = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] header = new byte[0x10];
                    file.Read(header, 0, header.Length);

                    Nintendo.NES.INesHeader inesHeader = new Nintendo.NES.INesHeader(header);
                    switch (inesHeader.MapperNumber)
                    {
                        case 0: media = (new Nintendo.NES.Mapper0Cartridge() as IMedia); break;
                        case 2: media = (new Nintendo.NES.Mapper2Cartridge() as IMedia); break;
                        default: throw new NotImplementedException($"NES mapper number {inesHeader.MapperNumber} not implemented");
                    }
                }
            }
            else
            {
                throw new Exception("Could not identify cartridge");
            }

            media?.Load(fileInfo);

            return media;
        }
    }
}
