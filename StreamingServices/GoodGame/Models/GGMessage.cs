using Newtonsoft.Json;

namespace StreamingServices.GoodGame.Models
{
    public class GGMessage<T> 
        where T : class, new()
    {
        [JsonProperty(PropertyName = "data")]
        public T Data { get; set; }

        public GGMessage()
        {
            Data = new T();
        }
    }
}
