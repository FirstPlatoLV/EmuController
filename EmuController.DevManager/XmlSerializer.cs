using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace EmuController.DevManager
{
    public class XmlSerializer<T>
    {
        private readonly Type type;

        public XmlSerializer()
        {
            type = typeof(T);
        }

        public void Save(string path, object obj)
        {
            using TextWriter textWriter = new StreamWriter(path);
            XmlSerializer serializer = new XmlSerializer(type);
            serializer.Serialize(textWriter, obj);

        }

        public T Read(string path)
        {
            T result;
            using (TextReader textReader = new StreamReader(Path.GetFullPath(path)))
            {
                XmlSerializer deserializer = new XmlSerializer(type);
                result = (T)deserializer.Deserialize(textReader);
            }
            return result;
        }
    }
}
