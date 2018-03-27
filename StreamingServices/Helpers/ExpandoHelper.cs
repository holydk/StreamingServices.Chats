using System.Collections.Generic;
using System.Dynamic;

namespace StreamingServices.Helpers
{
    public static class ExpandoHelper
    {
        public static bool TryGetValue(this ExpandoObject expObj, string key, out dynamic value)
        {
            return (expObj as IDictionary<string, object>).TryGetValue(key, out value);
        }

        public static bool TryGetValue(dynamic expObj, string key, out dynamic value)
        {
            return (expObj as IDictionary<string, object>).TryGetValue(key, out value);
        }
    }
}
