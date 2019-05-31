using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Datasave
{

    [Serializable]
    public class SettingsExtention
    {
        public event PropertyChangedEventHandler PropertyChanged;
        //private Dictionary<string, object> fields = new Dictionary<string, object>();

        private Dict fields = new Dict();
        private Serializer ser;



        public SettingsExtention()
        {
            ser = new Serializer_JSON();
        }
        public SettingsExtention(Serializer serializer)
        {
            ser = serializer;
        }

        public void Load<T>(string file)
        {
            if (Path.GetExtension(file) == "")
                file += ser.GetPreferedExtention();
            //fields.Clear();
            using (StreamReader stream = new StreamReader(file))
            {
                T cpy = ser.Deserialize<T>(stream.ReadToEnd());
                FieldInfo fi = typeof(T).BaseType.GetField(nameof(fields),BindingFlags.Instance | BindingFlags.NonPublic);

                fields = (Dict) fi.GetValue(cpy);
                return;

                //while (!stream.EndOfStream)
                //{
                //    DictEntry de = ser.Deserialize<DictEntry>(stream.ReadLine());
                //    fields[de.Key] = de.Value;
                //}
            }
        }

        public void Save(string file)
        {
            if (Path.GetExtension(file) == "")
                file += ser.GetPreferedExtention();

            using (StreamWriter stream = new StreamWriter(file))
                    stream.WriteLine(ser.Serialize(this));
            return;
            //using (StreamWriter stream = new StreamWriter(file))
            //    foreach (KeyValuePair<string, object> entry in fields)
            //        stream.WriteLine(ser.Serialize( new DictEntry(entry.Key, entry.Value)));
        }



        protected virtual void Verify(string propertyName)
        {
            //Override this if parameters are depending on eachother
            //use SetParSilent when adjusting...
        }


        protected bool SetPar<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(GetPar<T>(propertyName), value))
                return false;
            fields[propertyName] = value;
            Verify(propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }


        protected bool SetParSilent<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(GetPar<T>(propertyName), value))
                return false;
            fields[propertyName] = value;
            Verify(propertyName);
            return true;
        }


        protected T GetPar<T>([CallerMemberName] string propertyName = null)
        {
            object value = null;
            if (fields.TryGetValue(propertyName, out value))
            {
                if (value is T)
                {
                    return (T)value;
                }
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch (InvalidCastException)
                {
                    return default(T);
                }
            }
                
            return default(T);

        }


        [Serializable]
        private class Dict : Dictionary<string, object>
        {
        }

        [Serializable]
        private class DictEntry
        {
            public string Key { get; set; }
            public object Value { get; set; }

            public DictEntry()
            {

            }
            public DictEntry(string key, object value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
