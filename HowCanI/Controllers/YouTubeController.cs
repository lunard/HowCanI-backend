using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HowCanI.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HowCanI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YouTubeController : ControllerBase
    {
        private readonly IYouTubeService _youtubeService;

        public YouTubeController(
            IYouTubeService youtubeService
            )
        {
            _youtubeService = youtubeService;
        }

        [HttpGet("test")]
        public IActionResult TestUpload()
        {

            return new JsonResult(new { result = true });
        }
    }
}