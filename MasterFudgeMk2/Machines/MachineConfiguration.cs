using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MasterFudgeMk2.Machines
{
    public abstract class MachineConfiguration : ConfigFile
    {
        [XmlElement]
        public string LastDirectory { get; set; } = string.Empty;

        public MachineConfiguration() : base() { }
    }
}
