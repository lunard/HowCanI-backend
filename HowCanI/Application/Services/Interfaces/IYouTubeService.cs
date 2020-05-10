using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HowCanI.Application.Services.Interfaces
{
    public interface IYouTubeService
    {
        Task UploadVideo(string title, byte[] content);
    }
}
