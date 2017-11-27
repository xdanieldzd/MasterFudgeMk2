using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;

using MasterFudgeMk2.Common;

namespace MasterFudgeMk2.Media
{
    // TODO: figure out how to best allow region overrides; ex. Pop Breaker (GG), can't press Start on export systems

    public class MediaOverrides : XmlFile
    {
        public override string Name { get { return Path.ChangeExtension(GetType().Name, ".xml"); } }
        public override string Directory { get { return Program.XmlDocumentPath; } }

        [XmlElement("Media")]
        public List<MediaOverrideEntry> MediaList { get; set; }
    }

    public class MediaOverrideEntry
    {
        [XmlElement("Crc")]
        public string CrcString { get; set; }
        [XmlIgnore]
        public uint Crc { get { return uint.Parse(CrcString, NumberStyles.HexNumber); } set { CrcString = string.Format($"{value:X8}"); } }

        [XmlElement("MediaType")]
        public string MediaTypeString { get; set; }
        [XmlIgnore]
        public Type MediaType { get { return Type.GetType(MediaTypeString); } set { MediaTypeString = value.FullName; } }
    }
}
