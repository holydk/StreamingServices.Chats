using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StreamingServices.Test
{
    [TestClass]
    public class TwitchTest
    {
        private static string TWITCH_API_BASE_URI = "https://id.twitch.tv";
        private static string TWITCH_API_CHAT_URI = "wss://irc-ws.chat.twitch.tv/";
        private static string TWITCH_REDIRECT_URI = "http://localhost:8888/Twitch/oauth2/authorize";
    }
}
