# How Can I - backend

## Description
This project aims to be a POC for the backend operations of the project How Can I

The Web API is developed in ASP.Net Core 3.1.x.

The API uses both the Microsoft Indexer service (www.videoindexer.ai) and the YouTube Data API v3 (https://developers.google.com/youtube/v3)

There are two main controllers
- VideoIndexerController
- YouTubeController

The first on exposes three actions for *uploading* the video and for getting the *captions* and the *tags*.

The VideoIndexerController injects the interface **IVideoIndexer** via DI and uses the **AzureVideoIndexer** implementation of the service.

The VideoIndexerController injects the interface **IVideoStore** via DI and uses the **YouTubeVideoStore** implementation of the service. 

