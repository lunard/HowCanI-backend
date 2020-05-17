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
using HowCanI.Application.Models.LanguageUnderstanding;
using HowCanI.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xabe.FFmpeg;

namespace HowCanI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguageUnderstandigController : ControllerBase
    {
        private readonly ILanguageUnderstanding _languageUnderstandingService;
        private readonly IConfiguration _configuration;

        public LanguageUnderstandigController(
            ILanguageUnderstanding languageUnderstandingService,
            IConfiguration configuration
            )
        {
            _languageUnderstandingService = languageUnderstandingService;
            _configuration = configuration;
        }

        [HttpPost("understand")]
        public async Task<IActionResult> IndexVideoAsync([FromBody] UnderstandRequest request)
        {
            Intent i = await _languageUnderstandingService.GetIntent(request.Text);

            return new JsonResult(i);
        }
    }
}