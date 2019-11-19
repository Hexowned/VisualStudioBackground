#region USING_DIRECTIVES
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
#endregion

namespace VisualStudioBackground.Settings
{
    public static class JsonSerializer<TType> where TType : class
    {
        public static string Serialize(TType instance)
        {
            var serializer = new DataContractJsonSerializer(typeof(TType));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, instance);

                return Encoding.UTF8.GetString(stream.ToArray(), 0, stream.ToArray().Count());
            }
        }

        public static TType Deserialize(string json)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(typeof(TType));

                return serializer.ReadObject(stream) as TType;
            }
        }
    }
}
