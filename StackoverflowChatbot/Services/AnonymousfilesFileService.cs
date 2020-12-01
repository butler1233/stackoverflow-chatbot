using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace StackoverflowChatbot.Services
{
    /// <summary>
    /// almost the same as gofile, but more ads
    /// </summary>
    public class AnonymousfilesFileService : IFileService
    {
        private string _endpoint = "https://api.anonymousfiles.io/";

        public string UploadFile(byte[] file)
        {
            return UploadFileAsync(file).GetAwaiter().GetResult();
        }

        public async Task<string> UploadFileAsync(byte[] file)
        {
            var client = new HttpClient();

            var data = new ByteArrayContent(file);
            var content = new MultipartFormDataContent() {
                { data, "file", "tts.wav" }
            };

            var request = await client.PostAsync(_endpoint, content);
            var rawJson = await request.Content.ReadAsStringAsync();
            var uploadResponse = JObject.Parse(rawJson);

            return uploadResponse["url"].ToString();
        }
    }
}