using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CognitivePipeline.Functions.Helpers
{
    public class CognitiveServicesHelper
    {
        private static ComputerVisionClient csClient;
        private static CustomVisionPredictionClient cvPredictionClient;
        private static CustomVisionTrainingClient cvTrainingClient;

        //TODO: Needs to be managed by a training helper


        public static string DefaultPublishedModelName { get; set; } = "CustomObjectDetection";

        public static async Task<ComputerVisionClient> GetComputerVisionClientAsync()
        {
            if (csClient != null)
                return csClient;

            string subscriptionKey = await GlobalSettings.GetKeyValue("CSKey", false);
            string endpoint = await GlobalSettings.GetKeyValue("CSEndpoint", false);

            csClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey)) { Endpoint = endpoint };

            return csClient;
        }

        public static async Task<CustomVisionTrainingClient> GetCustomVisionTrainingClientAync()
        {
            if (cvTrainingClient != null)
                return cvTrainingClient;

            string subscriptionKey = await GlobalSettings.GetKeyValue("CVTrainingKey", false);
            string endpoint = await GlobalSettings.GetKeyValue("CVTrainingEndpoint", false);

            cvTrainingClient = new CustomVisionTrainingClient() { Endpoint = endpoint, ApiKey = subscriptionKey };
            
            return cvTrainingClient;
        }

        public static async Task<CustomVisionPredictionClient> GetCustomVisionPredictionClientAsync()
        {
            if(cvPredictionClient != null)
                return cvPredictionClient;

            string subscriptionKey = await GlobalSettings.GetKeyValue("CVPredectionKey", false);
            string endpoint = await GlobalSettings.GetKeyValue("CVPredictionEndpoint", false);

            cvPredictionClient = new CustomVisionPredictionClient() { Endpoint = endpoint, ApiKey = subscriptionKey };
            
            return cvPredictionClient;
        }

        public static Task<Guid> GetProjectIdForUserAsync(string userId)
        {
            return Task.FromResult<Guid>(Guid.Parse("8b3d0235-fc66-4b15-bb77-c4c170deebc0"));
        }

    }
}
