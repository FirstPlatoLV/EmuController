// Copyright 2020 Maris Melbardis
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
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
