using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CognitivePipeline.Functions.Models;
using System.Net.Http;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using CognitivePipeline.Functions.Helpers;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;

namespace CognitivePipeline.Functions
{
    public static class GenericObjectDetector
    {
        private static HttpClient httpClient;
        private static ComputerVisionClient csClient;
        private static double thresholdLevel = 0.6;
        private static CustomVisionPredictionClient cvPredectionClient;
        private static string publishedModelName = CognitiveServicesHelper.DefaultPublishedModelName;

        [FunctionName("GenericObjectDetector")]
        public static async Task<IActionResult> Run(
            //HTTP Trigger (Functions allow only a single trigger)
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]NewRequest<SmartDoc> newRequest,

            // Inputs
            [Blob("smartdocs/{RequestItem.DocName}", FileAccess.Read, Connection = "SmartDocsStorageConnection")] byte[] smartDocImage,
            ILogger log)
        {
            var stream = new MemoryStream(smartDocImage);
            await GenericObjectDetection(log, newRequest, stream);
            await CustomObjectDetection(log, newRequest, stream);

            try
            {
                stream.Close();
            }
            catch
            {

            }

            return (ActionResult)new OkObjectResult(newRequest);
        }

        private static async Task GenericObjectDetection(ILogger log, NewRequest<SmartDoc> newRequest, Stream smartDocImage)
        {
            string stepName = InstructionFlag.GenericObjectDetection.ToString();

            log.LogInformation($"***New {stepName} Direct-HTTP Request triggered: {{InstructionFlag.FaceAuthentication.ToString()}}");

            if (csClient == null)
            {
                csClient = await CognitiveServicesHelper.GetComputerVisionClientAsync();
            }

            try
            {
                var result = await csClient.DetectObjectsInStreamAsync(smartDocImage);
                var resultJson = JsonConvert.SerializeObject(result);

                //Update the request information with the newly processed data
                newRequest.RequestItem.CognitivePipelineActions.Add(new ProcessingStep
                {
                    StepName = stepName,
                    LastUpdatedAt = DateTime.UtcNow,
                    Output = resultJson,
                    Status = SmartDocStatus.ProccessedSuccessfully.ToString(),
                    IsSuccessful = true
                });
            }
            catch (Exception ex)
            {
                newRequest.RequestItem.CognitivePipelineActions.Add(new ProcessingStep
                {
                    StepName = InstructionFlag.AnalyzeText.ToString(),
                    LastUpdatedAt = DateTime.UtcNow,
                    Output = ex.Message,
                    Status = SmartDocStatus.ProccessedSuccessfully.ToString(),
                    IsSuccessful = false
                });
            }
        }

        private static async Task CustomObjectDetection(ILogger log, NewRequest<SmartDoc> newRequest, Stream smartDocImage)
        {
            string stepName = InstructionFlag.CustomObjectDetection.ToString();

            log.LogInformation($"***New {stepName} Direct-HTTP Request triggered: {{InstructionFlag.FaceAuthentication.ToString()}}");

            if (cvPredectionClient == null)
            {
                cvPredectionClient = await CognitiveServicesHelper.GetCustomVisionPredictionClientAsync();
            }

            try
            {
                var projectId = await CognitiveServicesHelper.GetProjectIdForUserAsync("NA");

                var result = await cvPredectionClient.DetectImageAsync(projectId, publishedModelName, smartDocImage));
                var resultJson = JsonConvert.SerializeObject(result);

                //Update the request information with the newly processed data
                newRequest.RequestItem.CognitivePipelineActions.Add(new ProcessingStep
                {
                    StepName = stepName,
                    LastUpdatedAt = DateTime.UtcNow,
                    Output = resultJson,
                    Status = SmartDocStatus.ProccessedSuccessfully.ToString(),
                    IsSuccessful = true
                });
            }
            catch (Exception ex)
            {
                newRequest.RequestItem.CognitivePipelineActions.Add(new ProcessingStep
                {
                    StepName = InstructionFlag.AnalyzeText.ToString(),
                    LastUpdatedAt = DateTime.UtcNow,
                    Output = ex.Message,
                    Status = SmartDocStatus.ProccessedSuccessfully.ToString(),
                    IsSuccessful = false
                });
            }
        }
    }
}
