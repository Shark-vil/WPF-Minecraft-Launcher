using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Minecraft_Launcher.Components
{
    public class LWebResponse
    {
        internal HttpResponseMessage httpResponseMessage { get; set; }
        internal HttpContent httpContent { get; set; }
        internal string htmlTextMessage { get; set; }

        public LWebResponse(HttpResponseMessage httpResponseMessage)
        {
            this.httpResponseMessage = httpResponseMessage;
            httpContent = httpResponseMessage.Content;
            htmlTextMessage = httpContent.ReadAsStringAsync().Result;
        }

        public HttpResponseMessage GetResponse()
        {
            return httpResponseMessage;
        }

        public T? ConvertResponse<T>()
        {
            if (httpResponseMessage != null)
                return JsonConvert.DeserializeObject<T>(htmlTextMessage);

            return default(T);
        }
    }

    public class LWebRequest
    {
        private HttpMethod SendMethod = HttpMethod.Get;
        private string WebAddress = "";
        internal Dictionary<string, string> SendForm = new Dictionary<string, string>();
        internal HttpClient httpClient = new HttpClient();
        internal HttpRequestMessage? httpRequestMessage = null;

        public LWebRequest() { }

        public LWebRequest(HttpMethod SendMethod) => this.SendMethod = SendMethod;

        public void SetAddress(string WebAddress) => this.WebAddress = WebAddress;

        public void AddValue(string key, string value) => SendForm.Add(key, value);
        
        public async Task<LWebResponse> Send()
        {
            httpRequestMessage = new HttpRequestMessage(SendMethod, WebAddress);
#pragma warning disable CS8620 // Аргумент запрещено использовать для параметра из-за различий в отношении допустимости значений NULL для ссылочных типов.
            httpRequestMessage.Content = new FormUrlEncodedContent(SendForm);
#pragma warning restore CS8620 // Аргумент запрещено использовать для параметра из-за различий в отношении допустимости значений NULL для ссылочных типов.
            httpRequestMessage.Headers.Add("Accept", "application/json");
            httpRequestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0");

            HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            return new LWebResponse(httpResponseMessage);
        }
    }
}
