using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ObjectDetectionManager.Helpers
{
    public class CognitiveServicesHelper
    {
        private static ComputerVisionClient csClient;
        private static CustomVisionPredictionClient cvPredictionClient;
        private static CustomVisionTrainingClient cvTrainingClient;

        private string computerVisionKey;
        private string computerVisionEndpoint;
        private string customVisionTrainingKey;
        private string customVisionTrainingEndpoint;
        private string customVisionPredictionKey;
        private string customVisionPredictionEndpoint;

        //TODO: Needs to be managed by a training helper


        public string DefaultPublishedModelName { get; set; } = "CustomObjectDetection";

        public CognitiveServicesHelper(string cvKey, string cvEndpoint, string cvTrainingKey, string cvTrainingEndpoint, string cvPredictionKey, string cvPredictionEndpoint)
        {
            computerVisionKey = cvKey;
            computerVisionEndpoint = cvEndpoint;
            customVisionTrainingKey = cvTrainingKey;
            customVisionTrainingEndpoint = cvTrainingEndpoint;
            customVisionPredictionKey = cvPredictionKey;
            customVisionPredictionEndpoint = cvPredictionEndpoint;
        }

        public ComputerVisionClient GetComputerVisionClient()
        {
            if (csClient != null)
                return csClient;

            string subscriptionKey = computerVisionKey;
            string endpoint = computerVisionEndpoint;

            csClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey)) { Endpoint = endpoint };

            return csClient;
        }

        public CustomVisionTrainingClient GetCustomVisionTrainingClient()
        {
            if (cvTrainingClient != null)
                return cvTrainingClient;

            string subscriptionKey = customVisionTrainingKey;
            string endpoint = customVisionTrainingEndpoint;

            cvTrainingClient = new CustomVisionTrainingClient() { Endpoint = endpoint, ApiKey = subscriptionKey };
            
            return cvTrainingClient;
        }

        public CustomVisionPredictionClient GetCustomVisionPredictionClient()
        {
            if(cvPredictionClient != null)
                return cvPredictionClient;

            string subscriptionKey = customVisionPredictionKey;
            string endpoint = customVisionPredictionEndpoint;

            cvPredictionClient = new CustomVisionPredictionClient() { Endpoint = endpoint, ApiKey = subscriptionKey };
            
            return cvPredictionClient;
        }

        public Task<Guid> GetProjectIdForUserAsync(string userId)
        {
            ///TODO: Update to retrieve related project id
            return Task.FromResult<Guid>(Guid.Parse("8b3d0235-fc66-4b15-bb77-c4c170deebc0"));
        }

    }
}
