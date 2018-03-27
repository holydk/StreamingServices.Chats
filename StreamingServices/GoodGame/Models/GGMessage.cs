using System.Runtime.Serialization;

namespace StreamingServices.GoodGame.Models
{
    [DataContract]
    public class GGMessage<T>
        where T : class, new()
    {
        public GGMessage()
        {
            Data = new T();
        }

        [DataMember(Name = "data")]
        public T Data { get; set; }
    }
}
