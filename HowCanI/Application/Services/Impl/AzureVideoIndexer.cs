using HowCanI.Application.Models.VideoIndexer;
using HowCanI.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Label = HowCanI.Application.Models.VideoIndexer.Label;

namespace HowCanI.Application.Services.Impl
{
    public class AzureVideoIndexer : IVideoIndexer
    {
        private readonly IConfiguration _configuration;

        private string apiUrl = "https://api.videoindexer.ai";
        private string accountId;
        private string location = "trial"; // https://docs.microsoft.com/en-us/azure/media-services/video-indexer/regions
        private string apiKey;

        private string accountAccessToken;

        public AzureVideoIndexer(
            IConfiguration configuration)
        {
            this._configuration = configuration;
            accountId = _configuration.GetValue<string>("azure_video_indexer_account_id");
            apiKey = _configuration.GetValue<string>("azure_video_indexer_api_key");

            System.Net.ServicePointManager.SecurityProtocol =
              System.Net.ServicePointManager.SecurityProtocol | System.Net.SecurityProtocolType.Tls12;

            // create the http client
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

            // obtain account access token
            var accountAccessTokenRequestResult = client.GetAsync($"{apiUrl}/auth/{location}/Accounts/{accountId}/AccessToken?allowEdit=true").Result;
            accountAccessToken = accountAccessTokenRequestResult.Content.ReadAsStringAsync().Result.Replace("\"", "");
        }

        public async Task<string> AnalyzeVideo(string videoPath, string name, string description, string language)
        {
            var content = new MultipartFormDataContent();
            Debug.WriteLine("Uploading...");

            FileStream video = File.OpenRead(videoPath);
            byte[] buffer = new byte[video.Length];
            video.Read(buffer, 0, buffer.Length);
            content.Add(new ByteArrayContent(buffer));

            // create the http client
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;
            var client = new HttpClient(handler);

            var uploadRequestResult = client.PostAsync($"{apiUrl}/{location}/Accounts/{accountId}/Videos?accessToken={accountAccessToken}&fileName={name}&name={name}&description={description}&privacy=private&language={language}", content).Result;
            var uploadContentResult = await uploadRequestResult.Content.ReadAsStringAsync();

            // get the video id from the upload result
            var uploadResult = (JObject)JsonConvert.DeserializeObject<dynamic>(uploadContentResult);

            if (uploadResult["ErrorType"] != null)
            {
                Debug.WriteLine($"Azure Indexer error: {uploadResult["ErrorType"]}");
                return null;
            }

            var videoId = uploadResult["id"].Value<string>();
            Debug.WriteLine("Uploaded");
            Debug.WriteLine("Video ID: " + videoId);

            //// Obtain video access token
            //client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            //var videoTokenRequestResult = await client.GetAsync($"{apiUrl}/auth/{location}/Accounts/{accountId}/Videos/{videoId}/AccessToken?allowEdit=true");
            //var videoAccessToken = videoTokenRequestResult.Content.ReadAsStringAsync().Result.Replace("\"", "");

            //while (true)
            //{
            //    Thread.Sleep(10000);

            //    var videoGetIndexRequestResult = await client.GetAsync($"{apiUrl}/{location}/Accounts/{accountId}/Videos/{videoId}/Index?accessToken={videoAccessToken}&language={language}");
            //    var videoGetIndexResult = videoGetIndexRequestResult.Content.ReadAsStringAsync().Result;

            //    var processingState = JsonConvert.DeserializeObject<dynamic>(videoGetIndexResult)["state"];

            //    Debug.WriteLine("");
            //    Debug.WriteLine("State:");
            //    Debug.WriteLine((string)processingState);

            //    // job is finished
            //    if (processingState != "Uploaded" && processingState != "Processing")
            //    {
            //        Debug.WriteLine("");
            //        Debug.WriteLine("Full JSON:");
            //        Debug.WriteLine(videoGetIndexResult);
            //        break;
            //    }
            //}

            return videoId;
        }

        public async Task<string> GetVideoCaptions(string videoId, string language)
        {
            // create the http client
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;
            var client = new HttpClient(handler);

            // Obtain video access token
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            var videoTokenRequestResult = client.GetAsync($"{apiUrl}/auth/{location}/Accounts/{accountId}/Videos/{videoId}/AccessToken?allowEdit=true").Result;
            var videoAccessToken = videoTokenRequestResult.Content.ReadAsStringAsync().Result.Replace("\"", "");

            var videoGetSubtitleResult =  await client.GetAsync($"{apiUrl}/{location}/Accounts/{accountId}/Videos/{videoId}/Captions?accessToken={videoAccessToken}&format=Srt&language={language}");
            return videoGetSubtitleResult.Content.ReadAsStringAsync().Result;
        }

        public async Task<List<Label>> GetVideoTags(string videoId, string language)
        {
            // create the http client
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;
            var client = new HttpClient(handler);

            // Obtain video access token
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            var videoTokenRequestResult = client.GetAsync($"{apiUrl}/auth/{location}/Accounts/{accountId}/Videos/{videoId}/AccessToken?allowEdit=true").Result;
            var videoAccessToken = videoTokenRequestResult.Content.ReadAsStringAsync().Result.Replace("\"", "");

            var videoGetIndexRequestResult = await client.GetAsync($"{apiUrl}/{location}/Accounts/{accountId}/Videos/{videoId}/Index?accessToken={videoAccessToken}&language={language}");
            JObject videoGetIndexResult = (JObject)JsonConvert.DeserializeObject( await videoGetIndexRequestResult.Content.ReadAsStringAsync());

            return JsonConvert.DeserializeObject<List<Label>>( videoGetIndexResult["summarizedInsights"]["labels"].ToString());
        }
    }
}
