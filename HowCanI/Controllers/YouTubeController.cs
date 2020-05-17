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

        [HttpGet("upload/{videoId}")]
        public async Task<IActionResult> UploadVideoAsync(string videoId)
        {
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), videoId + ".mp4");

            string captions = await _azureVideoIndexer.GetVideoCaptions(videoId, "it-IT");
            var labels = await _azureVideoIndexer.GetVideoTags(videoId, "it-IT");

            // 3) get captions from an external services
            await _youtubeService.UploadVideo(Path.GetFileNameWithoutExtension(filePath), filePath, captions, labels);

            return new JsonResult(new { result = true });
        }

        [HttpPost("index")]
        public async Task<IActionResult> IndexVideoAsync(List<IFormFile> files)
        {
            if (files.Count == 0)
            {
                return BadRequest("No file uploaded");
            }

            long size = files.Sum(f => f.Length);

            var filePath = Path.GetTempFileName();

            using (var stream = System.IO.File.Create(filePath))
            {
                await files.First().CopyToAsync(stream);
            }

            var videoId = await _azureVideoIndexer.AnalyzeVideo(filePath, Path.GetFileNameWithoutExtension(filePath), "Esempio di utilizzo dei servizi di indicizzazine di Azure", "it-IT");

            System.IO.File.Copy(filePath, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), videoId + ".mp4"));

            return new JsonResult(new { uploaded = videoId != null, filePath, videoId });
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
            var labels = await _azureVideoIndexer.GetVideoTags(videoId, "it-IT");

            return new JsonResult(labels);
        }
    }
}