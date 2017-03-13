﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MasterFudgeMk2.Media;
using MasterFudgeMk2.Media.Sega;
using MasterFudgeMk2.Media.Nintendo;

namespace MasterFudgeMk2.Machines
{
    public class MachineLoader
    {
        // TODO: maybe make more intelligent? i.e. have each known machine try and identify ROM, instead of having one function here?

        public static Type DetectMachine(FileInfo romFile)
        {
            byte[] romData = MediaLoader.ReadRomData(romFile.FullName);

            RomHeader romHeader = new RomHeader(romData);
            INESHeader inesHeader = new INESHeader(romData);

            if (romHeader.IsSEGAStringCorrect)
            {
                if (romHeader.IsGameGear)
                    return typeof(Sega.GameGear.Manager);
                else
                    return typeof(Sega.MasterSystem.Manager);
            }
            else if (romFile.Extension == ".sg")
            {
                // TODO: any other way of detecting SG1000 games?
                return typeof(Sega.SG1000.Manager);
            }
            else if ((romData[0x00] == 0xAA && romData[0x01] == 0x55) || (romData[0x00] == 0x55 && romData[0x01] == 0xAA))
            {
                // TODO: same as SG1000, a more reliable way?
                return typeof(Coleco.ColecoVision.Manager);
            }
            else if (inesHeader.IsSignatureCorrect)
            {
                return typeof(Nintendo.NES.Manager);
            }
            else
            {
                throw new Exception(string.Format("Could not identify machine from ROM '{0}'", romFile.Name));
            }
        }
    }
}
