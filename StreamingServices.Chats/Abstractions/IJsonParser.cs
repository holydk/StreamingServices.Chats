namespace StreamingServices.Chats.Abstractions
{
    public interface IJsonParser
    {
        string Serialize(object obj);

        T Deserialize<T>(string json) where T : class;
    }
}
