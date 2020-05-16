using HowCanI.Application.Models.VideoIndexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HowCanI.Application.Services.Interfaces
{
    public interface IVideoIndexer
    {
        Task<string> AnalyzeVideo(string videoPath, string name, string description, string language);
        Task<string> GetVideoCaptions(string videoId, string language);

        Task<List<string>> GetVideoTags(string videoId, string language);
    }
}
