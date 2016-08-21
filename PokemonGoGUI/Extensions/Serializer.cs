using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.IO;
using System.Runtime.Serialization.Formatters;

namespace PokemonGoGUI.Extensions
{
    public static class Serializer
    {
        public static string ToJson<T>(T data)
        {
            return JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });
        }

        public static T FromJson<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });
        }

        public static byte[] ToBson<T>(T data)
        {
            MemoryStream ms = new MemoryStream();

            using (BsonWriter writer = new BsonWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.TypeNameHandling = TypeNameHandling.All;
                serializer.PreserveReferencesHandling = PreserveReferencesHandling.All;

                serializer.Serialize(writer, data);
            }

            return ms.ToArray();
        }

        public static T FromBson<T>(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            T responseObject = default(T);

            using (BsonReader reader = new BsonReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.TypeNameHandling = TypeNameHandling.All;
                serializer.PreserveReferencesHandling = PreserveReferencesHandling.All;

                responseObject = serializer.Deserialize<T>(reader);
            }

            return responseObject;
        }
    }
}
