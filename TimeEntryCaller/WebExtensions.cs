using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace TimeEntryCaller
{
    public static class WebExtensions
    {
        public static StringContent GetStringContent(this object obj)
        {
            var jsonContent = JsonConvert.SerializeObject(obj);

            var contentString = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return contentString;
        }
    }
}