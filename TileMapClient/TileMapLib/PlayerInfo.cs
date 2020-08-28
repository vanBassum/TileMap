using STDLib.Misc;
using System;
using STDLib.Serializers;
using System.Reflection;
using System.Linq;

namespace TileMapLib
{
    public class PlayerInfo : PropertySensitive
    {
        public UInt16 ID { get { return GetPar<UInt16>(0); } set { SetPar(value); } }
        public string Username { get { return GetPar("Noname"); } set { SetPar(value); } }
        public double LocXPos { get { return GetPar<double>(0.0); } set { SetPar(value); } }
        public double LocYPos { get { return GetPar<double>(0.0); } set { SetPar(value); } }
        public double DirXPos { get { return GetPar<double>(0.0); } set { SetPar(value); } }
        public double DirYPos { get { return GetPar<double>(0.0); } set { SetPar(value); } }
        public DateTime LastUpdate { get { return GetPar(DateTime.MinValue); } set { SetPar(value); } }

        JSON serializer = new JSON();



        public byte[] Serialize()
        {
            return serializer.Serialize(this);
        }


        public static PlayerInfo FromBytes(byte[] data)
        {
            JSON serializer = new JSON();
            PlayerInfo des = serializer.Deserialize<PlayerInfo>(data);
            return des;
        }

        public void Populate(PlayerInfo pInfo)
        {
            foreach (PropertyInfo property in typeof(PlayerInfo).GetProperties().Where(p => p.CanWrite))
            {
                property.SetValue(this, property.GetValue(pInfo));
            }
        }


        public override string ToString()
        {
            return $"{ID}, {Username}";
        }
    }

}
