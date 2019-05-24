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
using ObjectDetectionManager.Services;
using ObjectDetectionManager.Models;
using ObjectDetectionManager.AzureFunctions.Helpers;

namespace ObjectDetectionManager.AzureFunctions
{
    public static class GetOrCreateODWorkspace
    {
        //Here workspace manager can be shared accross all calls as it does not use user specific workspace
        private static ODMWorkspaceManager workspaceManager;

        [FunctionName("GetOrCreateODWorkspace")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] ODWorkspaceOwner owner,
            ILogger log)
        {
            log.LogInformation("HTTP triggered (GetOrCreateODWorkspace) function");

            if (workspaceManager == null)
                workspaceManager = await ODWorkspaceManagerHelper.SetWorkspaceManager();

            ODWorkspace workspace;

            if (owner != null)
                workspace = await workspaceManager.GetOrCreateWorkspaceAsync(owner.OwnerId);
            else
                return new BadRequestObjectResult ("Owner was not found. You should submit owner in the request body");

            if (workspace != null)
                return new OkObjectResult(workspace);
            else
                return new BadRequestObjectResult("Workspace could not be found or created");
        }
    }
}
