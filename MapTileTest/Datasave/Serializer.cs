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
        string Serialize<T>(T obj);
        T Deserialize<T>(string data);
    }


    public class Serializer_JSON : Serializer
    {
        public T Deserialize<T>(string data)
        {
            
            return JsonConvert.DeserializeObject<T>(data);
        }

        public string GetPreferedExtention()
        {
            return ".json";
        }

        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj,Formatting.Indented);
        }
    }
}
