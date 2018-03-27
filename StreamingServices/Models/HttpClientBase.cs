using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace StreamingServices.Models
{
    public abstract class HttpClientBase<TCommandType> : ClientWebService<TCommandType>
        where TCommandType : struct, IConvertible
    {
        public HttpClientBase(string url)
            : base(url)
        { }

        protected async Task<byte[]> PostAsync(string uriString, Dictionary<string, string> parameters)
        {
            byte[] data = null;

            using (var client = new HttpClient()
            {
                BaseAddress = new Uri(uriString)
            })
            {
                var absolutePath = client.BaseAddress.AbsolutePath;
                var content = new FormUrlEncodedContent(parameters);

                using (var response = await client.PostAsync(absolutePath, content))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var responseContent = response.Content)
                        {
                            data = await responseContent.ReadAsByteArrayAsync();
                        }
                    }
                }
            }

            return data;
        }
    }
}
