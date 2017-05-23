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
                media = (new RomOnlyCartridge() as IMedia);
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
