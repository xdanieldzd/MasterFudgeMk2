using System;
using System.IO;
using System.Xml.Serialization;

namespace MasterFudgeMk2.Common
{
    [Serializable]
    public abstract class XmlFile
    {
        [XmlIgnore]
        public abstract string Name { get; }
        [XmlIgnore]
        public abstract string Directory { get; }
        [XmlIgnore]
        public string FullName { get { return Path.Combine(Directory, Name); } }

        public XmlFile() { }

        public static T Load<T>() where T : XmlFile
        {
            XmlRootAttribute root = new XmlRootAttribute(typeof(T).Name) { IsNullable = true };
            XmlSerializer serializer = new XmlSerializer(typeof(T), root);
            T configInstance = (T)Activator.CreateInstance(typeof(T));

            if (File.Exists(configInstance.FullName))
            {
                using (FileStream stream = new FileStream(configInstance.FullName, FileMode.Open))
                {
                    configInstance = (T)serializer.Deserialize(stream);
                }
            }
            else
                Save(configInstance);

            return configInstance;
        }

        public static void Save<T>(T instance) where T : XmlFile
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            XmlSerializer serializer = new XmlSerializer(instance.GetType());

            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(instance.FullName));

            using (FileStream stream = new FileStream(instance.FullName, FileMode.Create))
            {
                serializer.Serialize(stream, instance, ns);
            }
        }
    }
}
