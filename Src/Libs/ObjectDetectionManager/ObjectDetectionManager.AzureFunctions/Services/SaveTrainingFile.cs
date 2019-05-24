using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ObjectDetectionManager.Models;
using ObjectDetectionManager.AzureFunctions.Helpers;
using ObjectDetectionManager.AzureFunctions.Models;
using System.Net.Http;
using AutoMapper;
using System.Collections.Generic;

namespace ObjectDetectionManager.AzureFunctions.Services
{
    public static class SaveTrainingFile
    {
        public static bool IsInitialized = false;

        [FunctionName("SaveTrainingFile")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            log.LogInformation("HTTP triggered (SaveODWorkspace) function");
            
            var workspaceManager = await ODWorkspaceManagerHelper.SetWorkspaceManager();

            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await req.Content.ReadAsMultipartAsync(provider);

                //Get training file attributes (name and regions)
                var trainingFileInfo = provider.Contents[0];
                var trainingFileJson = await trainingFileInfo.ReadAsStringAsync();
                var trainingFileDTO = JsonConvert.DeserializeObject<DTOTrainingFile>(trainingFileJson);

                //Get training file bytes
                var trainingFileData = provider.Contents[1];
                var trainingFileBytes = await trainingFileData.ReadAsByteArrayAsync();

                var workspace = await workspaceManager.GetWorkspaceAsync(trainingFileDTO.OwnerId);

                var newFileName = workspaceManager.AddTrainingFile(trainingFileDTO.FileName, trainingFileBytes, Mapper.Map<List<ObjectRegion>>(trainingFileDTO.Regions));

                await workspaceManager.ValidateAndSaveWorkspace();
                
                return new CreatedResult(newFileName, null);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Addition of the file failed");
            }
        }
    }
}
