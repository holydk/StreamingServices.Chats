using Microsoft.VisualStudio.TestTools.UnitTesting;
using StreamingServices.Chats.IRC;
using System.Collections.Generic;

namespace StreamingServices.Test.IRC
{
    [TestClass]
    public class IRCParserTest
    {
        private static string msg0 = "@test=super;single=0 :test!me@test.ing FOO :This is a test";
        private static string msg1 = "FOO";
        private static string msg2 = ":test!me@test.ing FOO";
        private static string msg3 = "@test=super;single=0 FOO";
        private static string msg4 = "FOO qwerty";
        private static string msg5 = "FOO :This is a test";
        private static string msg6 = "366 test1 test2 :This is a test";

        [TestMethod]
        public void Serialize_Test_msg0()
        {
            var parser = new IRCParser();
            var msg = new IRCMessage("FOO", new Dictionary<string, string>()
            {
                { "test", "super" }, { "single", "0" }
            }, "test!me@test.ing", null, "This is a test");

            var message = parser.Serialize(msg);

            Assert.AreEqual(msg0, message);
        }

        [TestMethod]
        public void Deserialize_TestSingleMessage_msg0()
        {
            var parser = new IRCParser();
            var message = parser.Deserialize(msg0)[0];

            Assert.IsNotNull(message);
            Assert.AreEqual("super", message.Tags["test"]);
            Assert.AreEqual("0", message.Tags["single"]);
            Assert.AreEqual("test!me@test.ing", message.Prefix);
            Assert.AreEqual("FOO", message.Command, "Command is not equal msg0.");
            Assert.IsNull(message.Middle, "Middle is not null");
            Assert.AreEqual("This is a test", message.Trailing);
        }

        [TestMethod]
        public void Serialize_Test_msg1()
        {
            var parser = new IRCParser();
            var msg = new IRCMessage(null, null, "FOO", null, null);

            var message = parser.Serialize(msg);

            Assert.AreEqual(msg1, message);
        }

        [TestMethod]
        public void Deserialize_TestSingleMessage_msg1()
        {
            var parser = new IRCParser();
            var message = parser.Deserialize(msg1)[0];

            Assert.IsNotNull(message, "IRC msg is null.");
            Assert.IsNull(message.Tags, "Tags is not null.");
            Assert.IsNull(message.Prefix, $"Prefix is not null. Prefix = '{message.Prefix}'");
            Assert.AreEqual(msg1, message.Command, "Command is not equal msg1.");
            Assert.IsNull(message.Middle, "Middle is not null");
            Assert.IsNull(message.Trailing, "Trailing is not null.");
        }

        [TestMethod]
        public void Serialize_Test_msg2()
        {
            var parser = new IRCParser();
            var msg = new IRCMessage("FOO", null, "test!me@test.ing", null, null);

            var message = parser.Serialize(msg);

            Assert.AreEqual(msg2, message);
        }

        [TestMethod]
        public void Deserialize_TestSingleMessage_msg2()
        {
            var parser = new IRCParser();
            var message = parser.Deserialize(msg2)[0];

            Assert.IsNotNull(message, "IRC msg is null.");
            Assert.IsNull(message.Tags, "Tags is not null.");
            Assert.AreEqual("test!me@test.ing", message.Prefix);
            Assert.AreEqual("FOO", message.Command, "Command is not equal msg2.");
            Assert.IsNull(message.Middle, "Middle is not null");
            Assert.IsNull(message.Trailing, "Trailing is not null.");
        }

        [TestMethod]
        public void Serialize_Test_msg3()
        {
            var parser = new IRCParser();
            var msg = new IRCMessage("FOO", new Dictionary<string, string>()
            {
                { "test", "super" }, { "single", "0" }
            }, null, null, null);

            var message = parser.Serialize(msg);

            Assert.AreEqual(msg3, message);
        }

        [TestMethod]
        public void Deserialize_TestSingleMessage_msg3()
        {
            var parser = new IRCParser();
            var message = parser.Deserialize(msg3)[0];

            Assert.IsNotNull(message, "IRC msg is null.");
            Assert.AreEqual("super", message.Tags["test"]);
            Assert.AreEqual("0", message.Tags["single"]);
            Assert.IsNull(message.Prefix, $"Prefix is not null. Prefix = '{message.Prefix}'");
            Assert.AreEqual("FOO", message.Command, "Command is not equal msg3.");
            Assert.IsNull(message.Middle, "Middle is not null");
            Assert.IsNull(message.Trailing, "Trailing is not null.");
        }

        [TestMethod]
        public void Serialize_Test_msg4()
        {
            var parser = new IRCParser();
            var msg = new IRCMessage(null, null, "FOO", new[] { "qwerty" }, null);

            var message = parser.Serialize(msg);

            Assert.AreEqual(msg4, message);
        }

        [TestMethod]
        public void Deserialize_TestSingleMessage_msg4()
        {
            var parser = new IRCParser();
            var message = parser.Deserialize(msg4)[0];

            Assert.IsNotNull(message, "IRC msg is null.");
            Assert.IsNull(message.Tags, "Tags is not null.");
            Assert.IsNull(message.Prefix, $"Prefix is not null. Prefix = '{message.Prefix}'");
            Assert.AreEqual("FOO", message.Command, "Command is not equal msg4.");
            Assert.AreEqual(1, message.Middle.Length);
            Assert.AreEqual("qwerty", message.Middle[0]);
            Assert.IsNull(message.Trailing, "Trailing is not null.");
        }

        [TestMethod]
        public void Serialize_Test_msg5()
        {
            var parser = new IRCParser();
            var msg = new IRCMessage(null, null, "FOO", null, "This is a test");

            var message = parser.Serialize(msg);

            Assert.AreEqual(msg5, message);
        }

        [TestMethod]
        public void Deserialize_TestSingleMessage_msg5()
        {
            var parser = new IRCParser();
            var message = parser.Deserialize(msg5)[0];

            Assert.IsNotNull(message, "IRC msg is null.");
            Assert.IsNull(message.Tags, "Tags is not null.");
            Assert.IsNull(message.Prefix, $"Prefix is not null. Prefix = '{message.Prefix}'");
            Assert.AreEqual("FOO", message.Command, "Command is not equal msg5.");
            Assert.IsNull(message.Middle, "Middle is not null");
            Assert.AreEqual("This is a test", message.Trailing);
        }

        [TestMethod]
        public void Serialize_Test_msg6()
        {
            var parser = new IRCParser();
            var msg = new IRCMessage(null, null, "366", new[] { "test1", "test2" }, "This is a test");

            var message = parser.Serialize(msg);

            Assert.AreEqual(msg6, message);
        }

        [TestMethod]
        public void Deserialize_TestSingleMessage_msg6()
        {
            var parser = new IRCParser();
            var message = parser.Deserialize(msg6)[0];

            Assert.IsNotNull(message, "IRC msg is null.");
            Assert.IsNull(message.Tags, "Tags is not null.");
            Assert.IsNull(message.Prefix, $"Prefix is not null. Prefix = '{message.Prefix}'");
            Assert.AreEqual("366", message.Command, "Command is not equal msg5.");
            Assert.AreEqual(2, message.Middle.Length);
            Assert.AreEqual("test1", message.Middle[0]);
            Assert.AreEqual("test2", message.Middle[1]);
            Assert.AreEqual("This is a test", message.Trailing);
        }
    }
}
