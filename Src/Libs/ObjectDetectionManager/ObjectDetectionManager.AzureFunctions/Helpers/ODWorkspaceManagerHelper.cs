using ObjectDetectionManager.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ObjectDetectionManager.AzureFunctions.Helpers
{
    public static class ODWorkspaceManagerHelper
    {
        public static async Task<ODMWorkspaceManager> SetWorkspaceManager()
        {
            var storageName = await GlobalSettings.GetKeyValue("storageName", false);
            var storageKey = await GlobalSettings.GetKeyValue("storageKey", false);
            var dbEndpoint = await GlobalSettings.GetKeyValue("dbEndpoint", false);
            var dbPrimaryKey = await GlobalSettings.GetKeyValue("dbPrimaryKey", false);
            var dbName = await GlobalSettings.GetKeyValue("dbName", false);
            var sourceSystem = await GlobalSettings.GetKeyValue("sourceSystem", false);
            var cvKey = await GlobalSettings.GetKeyValue("cvKey", false);
            var cvEndpoint = await GlobalSettings.GetKeyValue("cvEndpoint", false);
            var cvTrainingKey = await GlobalSettings.GetKeyValue("cvTrainingKey", false);
            var cvTrainingEndpoint = await GlobalSettings.GetKeyValue("cvTrainingEndpoint", false);
            var cvPredectionKey = await GlobalSettings.GetKeyValue("cvPredectionKey", false);
            var cvPredectionEndpoint = await GlobalSettings.GetKeyValue("cvPredectionEndpoint", false);

            return new ODMWorkspaceManager(storageName, storageKey, dbEndpoint, dbPrimaryKey, dbName, sourceSystem, cvKey, cvEndpoint, cvTrainingKey, cvTrainingEndpoint, cvPredectionKey, cvPredectionEndpoint);
        }
    }
}
