using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace WPF_Minecraft_Launcher.Components
{
    public class LWebResponse
    {
        internal HttpResponseMessage HTTPResponseMessage { get; set; }
        internal HttpContent HTTPContent { get; set; }
        internal string ResponseText { get; set; }

        public LWebResponse(HttpResponseMessage HTTPResponseMessage)
        {
            this.HTTPResponseMessage = HTTPResponseMessage;
            HTTPContent = HTTPResponseMessage.Content;
            ResponseText = HTTPContent.ReadAsStringAsync().Result;
        }

        public HttpResponseMessage GetResponse()
        {
            return HTTPResponseMessage;
        }

        public T? ConvertResponse<T>()
        {
            if (HTTPResponseMessage != null)
                return JsonConvert.DeserializeObject<T>(ResponseText);

            return default(T);
        }
    }

    public class LWebRequest
    {
        private HttpMethod SendMethod = HttpMethod.Get;
        private string WebAddress = "";
        internal Dictionary<string, string> FormData = new Dictionary<string, string>();
        internal HttpClient HTTPClient = new HttpClient();
        internal HttpRequestMessage? HTTPRequestMessage = null;

        public LWebRequest() { }

        public LWebRequest(HttpMethod SendMethod) => this.SendMethod = SendMethod;

        public void SetAddress(string WebAddress) => this.WebAddress = WebAddress;

        public void AddValue(string key, string value) => FormData.Add(key, value);

        public LWebResponse Send(HttpMethod method)
        {
            HTTPRequestMessage = new HttpRequestMessage(SendMethod, WebAddress);
            HTTPRequestMessage.Method = method;
            HTTPRequestMessage.Content = new FormUrlEncodedContent(FormData);
            HTTPRequestMessage.Headers.Add("Accept", "application/json");
            HTTPRequestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0");

            HttpResponseMessage httpResponseMessage = HTTPClient.Send(HTTPRequestMessage);

            return new LWebResponse(httpResponseMessage);
        }
    }
}
