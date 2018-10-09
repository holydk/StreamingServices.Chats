namespace StreamingServices.Chats.Models
{
    public enum ChatError
    {
        InvalidConnection,

        NotJoinedToChannel,

        NotAuthorized,

        /// <summary>
        /// Connection to the same channel
        /// </summary>
        ConnectionToSameChannel,

        EmptyChannelsList,

        NotFoundChannel
    }
}
