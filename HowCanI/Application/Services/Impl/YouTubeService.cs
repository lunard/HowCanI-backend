using HowCanI.Application.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HowCanI.Application.Services.Impl
{
    public class YouTubeService : IYouTubeService
    {
        public Task UploadVideo(string title, byte[] content)
        {
            throw new NotImplementedException();
        }
    }
}
