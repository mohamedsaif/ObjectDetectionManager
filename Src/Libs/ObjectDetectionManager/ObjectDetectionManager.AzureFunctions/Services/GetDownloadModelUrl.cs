using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ObjectDetectionManager.AzureFunctions.Models;
using ObjectDetectionManager.Models;
using ObjectDetectionManager.AzureFunctions.Helpers;

namespace ObjectDetectionManager.AzureFunctions.Services
{
    public static class DownloadModel
    {
        [FunctionName("DownloadModel")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] DownloadModelRequest modelReq,
            ILogger log)
        {
            if(!modelReq.Platform.Contains(OfflineModelType.CoreML.ToString()) || !modelReq.Platform.Contains(OfflineModelType.TensorFlow.ToString()) || !modelReq.Platform.Contains(OfflineModelType.ONNX.ToString()))
            {
                return new BadRequestObjectResult("Invalid platform"); 
            }
            try
            {
                var workspaceManager = await ODWorkspaceManagerHelper.SetWorkspaceManager();
                var workspace = await workspaceManager.GetOrCreateWorkspaceAsync(modelReq.OwnerId);

                if (workspace.LastTrainingDate != null)
                    return new OkObjectResult(workspaceManager.GetModelDownloadUri(Enum.Parse<OfflineModelType>(modelReq.Platform)));

                return new BadRequestObjectResult("Workspace must be trained first before download");
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult("Failed to generate the download link");
            }

        }
    }
}
