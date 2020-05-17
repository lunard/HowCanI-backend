using HowCanI.Application.Models.VideoIndexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HowCanI.Application.Services.Interfaces
{
    public interface IVideoStore
    {
        Task UploadVideo(string title, string filePath, string captions, List<Label> labels);
    }
}
