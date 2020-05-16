using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using HowCanI.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xabe.FFmpeg;

namespace HowCanI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YouTubeController : ControllerBase
    {
        private readonly IVideoStore _youtubeService;
        private readonly IVideoIndexer _azureVideoIndexer;
        private readonly IConfiguration _configuration;

        public YouTubeController(
            IVideoStore youtubeService,
            IVideoIndexer azureVideoIndexer,
            IConfiguration configuration
            )
        {
            _youtubeService = youtubeService;
            _azureVideoIndexer = azureVideoIndexer;
            _configuration = configuration;
        }

        [HttpGet("upload")]
        public async Task<IActionResult> UploadVideoAsync()
        {

            var filePath = @"C:\Users\CodeTheCat\Pictures\Camera Roll\test.mp4";

            // 1) Extract the audio path via ffmpeg
            //IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(filePath);
            //Debug.WriteLine($"Video info (FFMPEG): duration {mediaInfo.Duration}, size:{(int)(mediaInfo.Size / 1024)}kByte, codec: {mediaInfo.VideoStreams.First().Codec}");
            //var audioPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".mp3");
            //var audioConversion = await FFmpeg.Conversions.FromSnippet.ExtractAudio(filePath, audioPath);
            //await audioConversion.Start();
            //Debug.WriteLine($"MP3 audio stream extracted in {audioPath}");

            // 2) Index video and extract info
            _azureVideoIndexer.AnalyzeVideo(filePath, Path.GetFileNameWithoutExtension(filePath), "Esempio di utilizzo dei servizi di indicizzazine di Azure", "it-IT");

            // 3) get captions from an external services
            await _youtubeService.UploadVideo(Path.GetFileNameWithoutExtension(filePath), filePath);

            return new JsonResult(new { result = true });
        }

        [HttpGet("analyze")]
        public async Task<IActionResult> AnalyzeVideoAsync()
        {

            var filePath = @"C:\Users\CodeTheCat\Pictures\Camera Roll\test.mp4";

            // 1) Extract the audio path via ffmpeg
            //IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(filePath);
            //Debug.WriteLine($"Video info (FFMPEG): duration {mediaInfo.Duration}, size:{(int)(mediaInfo.Size / 1024)}kByte, codec: {mediaInfo.VideoStreams.First().Codec}");
            //var audioPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".mp3");
            //var audioConversion = await FFmpeg.Conversions.FromSnippet.ExtractAudio(filePath, audioPath);
            //await audioConversion.Start();
            //Debug.WriteLine($"MP3 audio stream extracted in {audioPath}");

            // 2) Index video and extract info
            _azureVideoIndexer.AnalyzeVideo(filePath, Path.GetFileNameWithoutExtension(filePath), "Esempio di utilizzo dei servizi di indicizzazine di Azure", "it-IT");

            // 3) get captions from an external services
            await _youtubeService.UploadVideo(Path.GetFileNameWithoutExtension(filePath), filePath);

            return new JsonResult(new { result = true });
        }

        [HttpGet("caption/{videoId}")]
        public async Task<IActionResult> GetVideoCaptionsAsync(string videoId)
        {
            string captions = await _azureVideoIndexer.GetVideoCaptions(videoId, "it-IT");

            return File(Encoding.UTF8.GetBytes(captions), "text/srt");

            //return new JsonResult(captions);
        }

        [HttpGet("tags/{videoId}")]
        public async Task<IActionResult> GetVideoTagsAsync(string videoId)
        {
            var tags = await _azureVideoIndexer.GetVideoTags(videoId, "it-IT");

            return new JsonResult(tags);
        }
    }
}