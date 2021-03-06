﻿using System;
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
using HowCanI.Application.Models.VideoIndexer;
using HowCanI.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xabe.FFmpeg;

namespace HowCanI.Application.Services.Impl
{
    public class YouTubeVideoStore : IVideoStore
    {
        private readonly IConfiguration _configuration;

        public YouTubeVideoStore(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task UploadVideo(string title, string filePath, string captions, List<Label> labels)
        {

            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                      new ClientSecrets()
                      {
                          ClientId = _configuration.GetValue<string>("youtube_client_id"),
                          ClientSecret = _configuration.GetValue<string>("youtube_client_secret")
                      },
                      // This OAuth 2.0 access scope allows an application to upload files to the
                      // authenticated user's YouTube channel, but doesn't allow other types of access.
                      new[] { YouTubeService.Scope.YoutubeUpload },
                      "user",
                      CancellationToken.None
                  );


            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
            });

            var video = new Video();
            video.Snippet = new VideoSnippet();
            video.Snippet.Title = title;
            video.Snippet.Description = "Automatic caption test";
            video.Snippet.Tags = labels.Select(l=>l.Name).ToList();
            video.Snippet.CategoryId = "29"; // See https://gist.github.com/dgp/1b24bf2961521bd75d6c
            video.Status = new VideoStatus();
            video.Status.PrivacyStatus = "unlisted"; // or "private" or "public"
            
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
                videosInsertRequest.ProgressChanged += videosInsertRequest_ProgressChanged;
                videosInsertRequest.ResponseReceived += async (video)=> {

                    // insert captions
                    var videoId = video.Id;

                    //create a CaptionSnippet object...
                    CaptionSnippet capSnippet = new CaptionSnippet();
                    capSnippet.Language = "it";
                    capSnippet.Name = videoId + "_Caption";
                    capSnippet.VideoId = videoId;
                    capSnippet.IsDraft = false;

                    //create new caption object
                    Caption caption = new Caption();
           
                    //set the completed snippet to the object now...
                    caption.Snippet = capSnippet;

                    // convert string to stream
                    byte[] byteArray = Encoding.UTF8.GetBytes(captions);
                    //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
                    using (MemoryStream captionStream = new MemoryStream(byteArray))
                    {

                        //create the request now and insert our params...
                        var captionRequest = youtubeService.Captions.Insert(caption, "snippet", captionStream, "application/atom+xml");

                        //finally upload the request... and wait.
                        await captionRequest.UploadAsync();
                    }
                };

                await videosInsertRequest.UploadAsync();
            }
        }


        private void videosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case UploadStatus.Uploading:
                    Debug.WriteLine("{0} bytes sent.", progress.BytesSent);
                    break;

                case UploadStatus.Failed:
                    Debug.WriteLine("An error prevented the upload from completing.\n{0}", progress.Exception);
                    break;
            }
        }
    }
}
