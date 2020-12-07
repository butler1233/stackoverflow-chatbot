using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Web;


namespace StackoverflowChatbot.Services
{

    /// <summary>
    /// A little bit exploited but it should work :P
    /// only accepts plain text, but will render HTML
    /// </summary>
    public class htputFileService : IFileService
    {
        private readonly string _endpoint = "http://htput.com/";

        public string UploadFile(byte[] file)
        {
            return UploadFileAsync(file, "", "").GetAwaiter().GetResult();
        }
        public async Task<string> UploadFileAsync(byte[] file, string prefix, string suffix)
        {
            var client = new HttpClient();
            var guid = Guid.NewGuid();
            var address = HttpUtility.HtmlEncode(guid.ToString());

            var base64file = Convert.ToBase64String(file);
            var content = prefix + base64file + suffix;
            
            var data = new MultipartFormDataContent()
            {
                { new StringContent(address), "addr" },
                { new StringContent(content), "content"}
            };

            var response = await client.PostAsync(_endpoint, data);
            var rawHtml = await response.Content.ReadAsStringAsync();

            // TODO: validate sucess

            return _endpoint + address;
        }
    }
}