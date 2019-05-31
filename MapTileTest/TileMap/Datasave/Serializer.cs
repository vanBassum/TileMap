using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Datasave
{
    public interface Serializer
    {
        string GetPreferedExtention();
        void Serialize<T>(T obj, Stream stream);
        T Deserialize<T>(Stream stream);
    }


    public class Serializer_JSON : Serializer
    {
        public T Deserialize<T>(Stream data)
        {
            using (StreamReader sr = new StreamReader(data))
                return JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
        }

        public string GetPreferedExtention()
        {
            return ".json";
        }

        public void Serialize<T>(T obj, Stream stream)
        {
            using (StreamWriter sr = new StreamWriter(stream))
                sr.WriteLine(JsonConvert.SerializeObject(obj, Formatting.Indented));
        }
    }
}
