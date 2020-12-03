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
    /// Sadly gofile doesnt allow direct download so this is just here because i implemented it already :'(
    /// </summary>
    public class GofileFileService : IFileService
    {
        private readonly string _serviceEndpoint = "https://{server}.gofile.io/";

        public string UploadFile(byte[] file)
        {
            return UploadFileAsync(file).GetAwaiter().GetResult();
        }

        public async Task<string> UploadFileAsync(byte[] file)
        {
            var client = new HttpClient();
            var optimalServer = await GetServer(client);
            var endpoint = _serviceEndpoint.Replace("{server}", optimalServer);

            var data = new ByteArrayContent(file);
            var content = new MultipartFormDataContent() {
                { data, "file", "tts.wav" }
            };

            var request = await client.PostAsync(endpoint + "uploadFile", content);
            var rawJson = await request.Content.ReadAsStringAsync();
            var uploadResponse = JsonSerializer.Deserialize<ApiResponse>(rawJson);

            if (!uploadResponse.Status.Equals("ok"))
            {
                throw new NotSupportedException($"FileUpload status returned a {uploadResponse.Status} value.");
            }

            var code = uploadResponse.Data["code"].ToString();
            var fileInfo = JObject.Parse(uploadResponse.Data["file"].ToString());
            var fileName = fileInfo["name"].ToString();
            
            return endpoint + $"d/{code}";
        }

        private async Task<string> GetServer(HttpClient client)
        {
            var request = await client.GetAsync(_serviceEndpoint.Replace("{server}", "apiv2") + "getServer");
            var rawJson = await request.Content.ReadAsStringAsync();
            var serverStatus = JsonSerializer.Deserialize<ApiResponse>(rawJson);

            if (!serverStatus.Status.Equals("ok"))
            {
                throw new NotSupportedException($"FileService status returned a {serverStatus.Status} value.");
            }

            return serverStatus.Data.First().Value?.ToString();
        }
    }

    /// <summary>
    /// Data class for the gofile.io api v2
    /// example: {"status":"ok","data":{"server":"srv-file6"}}
    /// example: {"status":"ok","data":{"code":"123Abc","adminCode":"3ZcBq12nTgb4cbSwJVYY","file":{"name":"file.txt", [...]}}}
    /// </summary>
    internal class ApiResponse
    {
		[System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; }
		[System.Text.Json.Serialization.JsonPropertyName("data")]
        public Dictionary<string, object> Data { get; set; } 
    }
}