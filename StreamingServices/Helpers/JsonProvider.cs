using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;

namespace StreamingServices.Helpers
{
    public static class JsonProvider
    {
        public static object Deserialize(this Type type, string json)
        {
            object instance = null;

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                instance = new DataContractJsonSerializer(type).ReadObject(ms);
            }

            return instance;
        }

        public static object Deserialize(this Type type, byte[] data)
        {
            object instance = null;

            using (var ms = new MemoryStream(data))
            {
                instance = new DataContractJsonSerializer(type).ReadObject(ms);
            }

            return instance;
        }

        public static T Deserialize<T>(byte[] data)
            where T: class
        {
            T instance = null;
            
            using (var ms = new MemoryStream(data))
            {
                instance = (T) new DataContractJsonSerializer(typeof(T)).ReadObject(ms);
            }

            return instance;
        }

        public static byte[] Serialize<TCommandType>(this object obj, TCommandType cmdType)
            where TCommandType : struct, IConvertible
        {
            if (obj == null)
                return Encoding.UTF8.GetBytes("{}");

            var jsonBuilder = new StringBuilder();

            using (var ms = new MemoryStream())
            {
                new DataContractJsonSerializer(obj.GetType())
                    .WriteObject(ms, obj);

                var cmdName = cmdType.ToString();
                var reqName = cmdType
                    .GetType()
                    .GetField(cmdName)
                    ?.GetCustomAttribute<DescriptionAttribute>()
                    .Description;

                jsonBuilder
                    .Append(Encoding.UTF8.GetString(ms.ToArray()))
                    .Insert(1, $"\"type\":\"{reqName ?? cmdName}\",");
            }

            return Encoding.UTF8.GetBytes(jsonBuilder.ToString());
        }
    }
}
