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
    public class VideoIndexerController : ControllerBase
    {
        private readonly IVideoIndexer _azureVideoIndexer;
        private readonly IConfiguration _configuration;

        public VideoIndexerController(
            IVideoIndexer azureVideoIndexer,
            IConfiguration configuration
            )
        {
            _azureVideoIndexer = azureVideoIndexer;
            _configuration = configuration;
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