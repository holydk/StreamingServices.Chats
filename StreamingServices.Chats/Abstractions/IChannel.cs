namespace StreamingServices.Chats.Abstractions
{
    public interface IChannel
    {
        int? Id { get; }

        string Name { get; }
    }
}
