using Microsoft.VisualStudio.TestTools.UnitTesting;
using StreamingServices.Chats.WebSockets;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace StreamingServices.Test
{
    [TestClass]
    public class WebSocketClientTest
    {
        [TestMethod]
        public async Task Connecting_Test()
        {
            using (var client = new WebSocketClient())
            {               
                await client.ConnectAsync(new Uri(GGTest.GG_API_CHAT_URI));

                Assert.IsTrue(client.IsConnected);
            }
        }
        
        [TestMethod]
        public async Task OnMessage_Test()
        {
            using (var client = new WebSocketClient())
            {
                var isMsgReceived = false;

                EventHandler<WebSocketResponseEventArgs> handler = null;
                handler = (obj, e) => 
                {
                    isMsgReceived = true;

                    client.Message -= handler;
                    Assert.IsNotNull(e.Data);
                };
                client.Message += handler;

                await client.ConnectAsync(new Uri(GGTest.GG_API_CHAT_URI));
                await Task.Delay(500).ContinueWith(task =>
                {
                    Assert.IsTrue(isMsgReceived);
                });
            }
        }

        [TestMethod]
        public async Task Send_Test()
        {
            using (var client = new WebSocketClient())
            {
                var isMsgReceived = false;

                EventHandler<WebSocketResponseEventArgs> handler = null;
                handler = (obj, e) =>
                {
                    if (e.MessageType == WebSocketMessageType.Text)
                    {
                        var msg = Encoding.UTF8.GetString(e.Data);

                        if (msg.Contains("{\"type\":\"success_auth\",\"data\":{\"user_id\":0,\"user_name\":\"\"}}"))
                        {
                            client.Message -= handler;
                            isMsgReceived = true;
                        }
                    }
                };
                client.Message += handler;

                var data = Encoding.UTF8.GetBytes("{\"type\": \"auth\", \"data\": {\"user_id\": 0}}");

                await client.ConnectAsync(new Uri(GGTest.GG_API_CHAT_URI));
                await Task.Delay(500).ContinueWith(async task =>
                {
                    await client.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text);
                });

                await Task.Delay(1000).ContinueWith(task =>
                {                    
                    Assert.IsTrue(isMsgReceived, "Error on sending message. Client didn't recognize answer.");
                });
            }
        }
    }
}
