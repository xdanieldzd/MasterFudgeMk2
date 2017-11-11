using System;
using System.IO;
using System.Xml.Serialization;

namespace MasterFudgeMk2
{
    [Serializable]
    public abstract class ConfigFile
    {
        [XmlIgnore]
        public abstract string Name { get; }
        [XmlIgnore]
        public string FullName { get { return Path.Combine(Program.UserDataPath, Name); } }

        public ConfigFile() { }

        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(GetType());

            Directory.CreateDirectory(Path.GetDirectoryName(FullName));

            using (FileStream stream = new FileStream(FullName, FileMode.OpenOrCreate))
            {
                serializer.Serialize(stream, this);
            }
        }

        public static T Load<T>() where T : ConfigFile
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T configInstance = (T)Activator.CreateInstance(typeof(T));

            if (File.Exists(configInstance.FullName))
            {
                using (FileStream stream = new FileStream(configInstance.FullName, FileMode.Open))
                {
                    configInstance = (T)serializer.Deserialize(stream);
                }
            }
            else
                configInstance.Save();

            return configInstance;
        }
    }
}
