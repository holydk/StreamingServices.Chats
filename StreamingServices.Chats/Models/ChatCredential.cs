using System.Security;
using StreamingServices.Chats.Abstractions;

namespace StreamingServices.Chats.Models
{
    public class ChatCredential : IChatCredential
    {
        #region Public Properties

        /// <summary>
        /// User in chat
        /// </summary>
        public string User { get; }

        /// <summary>
        /// User token
        /// </summary>
        public SecureString Token { get; }

        #endregion

        #region Constructors

        public ChatCredential(string user, SecureString token)
        {
            if (!token.IsReadOnly())
                token.MakeReadOnly();

            User = user;
            Token = token;
        }

        #endregion
    }
}
