using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MasterFudgeMk2.Machines
{
    public class MachineLoader
    {
        public static Type DetectMachine(FileInfo mediaFile)
        {
            foreach (Type machineType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(IMachineManager).IsAssignableFrom(p) && !p.IsInterface).OrderBy(x => x.Name))
            {
                IMachineManager machine = (Activator.CreateInstance(machineType) as IMachineManager);
                if (machine.CanLoadMedia(mediaFile)) return machineType;
            }

            throw new Exception(string.Format("Could not identify machine from media file '{0}'", mediaFile.Name));
        }
    }
}
