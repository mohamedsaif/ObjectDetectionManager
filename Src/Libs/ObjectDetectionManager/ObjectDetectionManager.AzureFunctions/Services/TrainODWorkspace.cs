using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ObjectDetectionManager.AzureFunctions.Helpers;
using ObjectDetectionManager.AzureFunctions.Models;

namespace ObjectDetectionManager.AzureFunctions.Services
{
    public static class TrainODWorkspace
    {
        [FunctionName("TrainODWorkspace")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] ODWorkspaceOwner owner,
            ILogger log)
        {
            log.LogInformation("HTTP triggered (SaveODWorkspace) function");

            var workspaceManager = await ODWorkspaceManagerHelper.SetWorkspaceManager();
            var workspace = await workspaceManager.GetOrCreateWorkspaceAsync(owner.OwnerId);

            try
            {
                //Upload data to custom vision adhoc project
                await workspaceManager.PrepareWorkspaceForTraining();

                //create new training iteration, generate the offline models and upload them to storage
                await workspaceManager.TrainPreparedWorkspace();

                return new OkObjectResult("Training completed successfully");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Addition of the file failed");
            }
        }
    }
}
