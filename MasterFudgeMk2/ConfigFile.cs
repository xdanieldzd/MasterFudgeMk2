using System.IO;

using Nini.Config;

namespace MasterFudgeMk2
{
    public abstract class ConfigFile
    {
        public abstract string Filename { get; }

        protected IConfigSource source;

        public ConfigFile()
        {
            string configFilePath = Path.Combine(Program.UserDataPath, Filename);

            Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));
            if (!File.Exists(configFilePath)) File.WriteAllText(configFilePath, "<Nini>\n</Nini>\n");

            source = new XmlConfigSource(configFilePath);
            source.AutoSave = true;
        }
    }
}
