using Newtonsoft.Json;

namespace PokemonGoGUI.Extensions
{
    public static class Serializer
    {
        public static string ToJson<T>(T data)
        {
            return JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });
        }

        public static T FromJson<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Error = HandleEventHandler
            });
        }
        private static void HandleEventHandler(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            errorArgs.ErrorContext.Handled = true;
        }

        /* [Obsolete("BSON reading and writing has been moved to its own package. See https://www.nuget.org/packages/Newtonsoft.Json.Bson for more details.")]
         * 
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
        */
    }
}
