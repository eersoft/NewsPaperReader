using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Policy;

namespace NewsPaperReader
{
    internal class NetFunction
    {
        static readonly HttpClient client = new HttpClient();
        public NetFunction() { }
        /*public async string GetHtml(string url)
        {
            try
            {
                HttpResponseMessage responseMessage = await client.GetAsync(url);
                responseMessage.EnsureSuccessStatusCode();
                string responseBody = await responseMessage.Content.ReadAsStringAsync();
                return responseBody;
            }catch (HttpRequestException e)
            {
                return "HttpError:" + e.Message;
            }
        }*/

    }
}
