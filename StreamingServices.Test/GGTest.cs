using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StreamingServices.GoodGame.Chat;

namespace StreamingServices.Test
{
    public enum GGChannel
    {
        WELDER = 26421,
        JUICE = 21793,
        PHOMBIE = 1168,
        KITTYKLAWTV = 34490,
        HAPPY = 6968,
        EVILMICKIE = 68946,
        SCATMAN = 3893,
        MIKER = 5,
        AloneWave = 6515,
        TauTau = 17912,
        Bu_Bu = 142276,
        Pomi = 1644,
        Abver = 1053
    }

    [TestClass]
    public class GGTest
    {        
        private static string GG_API_BASE_URI = "https://api2.goodgame.ru";
        private static string GG_API_CHAT_URI = "wss://chat-2.goodgame.ru/chat/websocket";
        private static string GG_REDIRECT_URI = "http://localhost:8888/GoodGame/oauth2/authorize";

        [TestMethod]
        public async Task GGConnectionTest()
        {
            using (var ggChat = new GoodGameChat(GG_API_CHAT_URI, 0, null))
            {
                await ggChat.ConnectAsync();

                Assert.IsTrue(ggChat.IsConnected);
            }
        }
    }
}
