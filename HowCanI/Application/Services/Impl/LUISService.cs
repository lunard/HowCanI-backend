using HowCanI.Application.Models.LanguageUnderstanding;
using HowCanI.Application.Services.Interfaces;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HowCanI.Application.Services.Impl
{
    public class LUISService : ILanguageUnderstanding
    {

        private readonly IConfiguration _configuration;


        // Use Language Understanding (LUIS) prediction endpoint key
        // to create authentication credentials
        private readonly string predictionKey;

        // Endpoint URL example value = "https://YOUR-RESOURCE-NAME.api.cognitive.microsoft.com"
        private readonly string predictionEndpoint;

        // App Id example value = "df67dcdb-c37d-46af-88e1-8b97951ca1c2"
        private readonly string appId;

        private readonly LUISRuntimeClient _LUISClient;
        public LUISService(IConfiguration configuration)
        {
            _configuration = configuration;
            predictionKey = _configuration.GetValue<string>("LUIS_prediction_key");
            predictionEndpoint = _configuration.GetValue<string>("LUIS_prediction_endpoint");
            appId = _configuration.GetValue<string>("LUIS_app_id");

            var credentials = new ApiKeyServiceClientCredentials(predictionKey);
            _LUISClient = new LUISRuntimeClient(credentials, new System.Net.Http.DelegatingHandler[] { })
            {
                Endpoint = predictionEndpoint
            };
        }

        public async Task<Models.LanguageUnderstanding.Intent> GetIntent(string sentence)
        {
            Models.LanguageUnderstanding.Intent i = new Models.LanguageUnderstanding.Intent();

            var requestOptions = new PredictionRequestOptions
            {
                DatetimeReference = DateTime.Now,
                PreferExternalEntities = true
            };

            var predictionRequest = new PredictionRequest
            {
                Query = sentence,
                Options = requestOptions
            };

            // get prediction
            var response = await _LUISClient.Prediction.GetSlotPredictionAsync(
                Guid.Parse(appId),
                slotName: "production",
                predictionRequest,
                verbose: true,
                showAllIntents: true,
                log: true);

            i.Name = response.Prediction.TopIntent;
            i.Properties = response.Prediction.Entities.Select(e => e.Value.ToString()).ToList();

            return i;
        }
    }
}
