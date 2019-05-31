using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;

namespace Datasave
{
    public class SaveableBindingList<T> : BindingList<T>
    {

        public void Save(Stream stream)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            });

            using (StreamWriter wrt = new StreamWriter(stream))
                wrt.WriteLine(json);
        }

        public void Load(Stream stream)
        {
            string json = "";
            using (StreamReader rdr = new StreamReader(stream))
                json = rdr.ReadToEnd();

            BindingList<T> deserializedObject = JsonConvert.DeserializeObject<BindingList<T>>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            this.Clear();
            foreach (T i in deserializedObject)
                this.Add(i);
        }

        public class TypeNameSerializationBinder : SerializationBinder
        {
            public string TypeFormat { get; private set; }

            public TypeNameSerializationBinder(string typeFormat)
            {
                TypeFormat = typeFormat;
            }

            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = null;
                typeName = serializedType.Name;
            }

            public override Type BindToType(string assemblyName, string typeName)
            {
                var resolvedTypeName = string.Format(TypeFormat, typeName);
                return Type.GetType(resolvedTypeName, true);
            }
        }
    }
}