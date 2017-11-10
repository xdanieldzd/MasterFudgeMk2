using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Machines;

namespace MasterFudgeMk2
{
    public static class DatHelper
    {
        static Dictionary<Type, DatFile> datFiles;

        static DatHelper()
        {
            datFiles = new Dictionary<Type, DatFile>();

            foreach (Type machineType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(IMachineManager).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract).OrderBy(x => x.Name))
            {
                IMachineManager machine = (Activator.CreateInstance(machineType) as IMachineManager);

                XmlRootAttribute root = new XmlRootAttribute("datafile") { IsNullable = true };
                XmlSerializer serializer = new XmlSerializer(typeof(DatFile), root);
                using (FileStream stream = new FileStream(Path.Combine("XML", machine.DatFileName), FileMode.Open))
                {
                    datFiles.Add(machineType, (DatFile)serializer.Deserialize(stream));
                }
            }
        }

        public static dynamic FindGameInDats(FileInfo mediaFile)
        {
            uint crc = Crc32.Calculate(mediaFile);

            string crcString = string.Format("{0:X8}", crc);
            string sizeString = string.Format("{0:D}", mediaFile.Length);

            foreach (KeyValuePair<Type, DatFile> datFile in datFiles)
            {
                var game = datFile.Value.Game.FirstOrDefault(x => x.Rom.Any(y => y.Crc == crcString && y.Size == sizeString));
                if (game != null)
                    return new { Type = datFile.Key, Game = game };
            }

            return null;
        }
    }
}
