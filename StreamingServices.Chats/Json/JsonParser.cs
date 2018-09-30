using Newtonsoft.Json;
using StreamingServices.Chats.Abstractions;

namespace StreamingServices.Chats.Json
{
    public class JsonParser : IJsonParser
    {
        #region Constructors

        public JsonParser()
        {

        }

        #endregion

        #region IJsonParser Interface Methods

        public string Serialize(object obj)
        {
            if (obj == null)
                return null;

            return JsonConvert.SerializeObject(
                obj,
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        }

        public T Deserialize<T>(string json)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonConvert.DeserializeObject<T>(json);
        } 

        #endregion
    }
}
