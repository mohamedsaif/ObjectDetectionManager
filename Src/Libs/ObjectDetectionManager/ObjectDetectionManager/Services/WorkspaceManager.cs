using ObjectDetectionManager.Data;
using ObjectDetectionManager.Helpers;
using ObjectDetectionManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ObjectDetectionManager.Services
{
    public class WorkspaceManager
    {
        private CognitiveServicesHelper cognitiveHelper;
        private AzureBlobStorageRepository blobRepo;
        private CosmosDBRepository<DetectionWorkspace> workspaceRepo;
        private DetectionWorkspace activeWorkspace;

        public WorkspaceManager(string storageName, string storageKey, string storageContainer, string dbEndpoint, string dbPrimaryKey, string dbName, string sourceSystem, string cvKey, string cvEndpoint, string cvTrainingKey, string cvTrainingEndpoint, string cvPredectionKey, string cvPredectionEndpoint)
        {
            cognitiveHelper = new CognitiveServicesHelper(cvKey, cvEndpoint, cvTrainingKey, cvTrainingEndpoint, cvPredectionKey, cvPredectionEndpoint);
            blobRepo = new AzureBlobStorageRepository(storageName, storageKey, storageContainer);
            workspaceRepo = new CosmosDBRepository<DetectionWorkspace>(dbEndpoint, dbPrimaryKey, dbName, sourceSystem);

        }

        public async Task<DetectionWorkspace> GetOrCreateWorkspaceAsync(string ownerId)
        {
            List<DetectionWorkspace> workspaceLockup = await workspaceRepo.GetItemsAsync(x => x.OwnerId == ownerId) as List<DetectionWorkspace>;
            
            //No workspace found for OwnerId
            if(workspaceLockup.Count == 0)
            {
                activeWorkspace = await CreateWorkspaceAsync(ownerId, new ModelPolicy());
            }
            else
            {
                //TODO: Add logic to handle multiple workspaces for same owner. for now only a single workspace for owner is supported.
                activeWorkspace = workspaceLockup[0];
            }

            return activeWorkspace;
        }

        public async Task<DetectionWorkspace> CreateWorkspaceAsync(string ownerId, ModelPolicy policy)
        {
            var newId = Guid.NewGuid().ToString();

            var result = new DetectionWorkspace()
            {
                Id = newId,
                WorkspaceId = newId,
                OwnerId = ownerId,
                Policy = new ModelPolicy(),
                CreationDate = DateTime.UtcNow,
                IsActive = true,
                ModelDefaultName = GlobalSettings.DefaultModelName,
                Files = new List<TrainingFile>(),
                PartitionId = GlobalSettings.DefaultPartitionId
            };

            result.FilesCotainerUri = $"{result.WorkspaceId}-files";
            result.ModelCotainerUri = $"{result.WorkspaceId}-models";

            await workspaceRepo.CreateItemAsync(result);

            return result;
        }

        private async Task UploadTrainingFiles(DetectionWorkspace workspace)
        {
            foreach (var file in workspace.Files)
            {
                if (!file.IsUploaded)
                {
                    if (file.FileData != null)
                    {
                        string fileAbsUri = await blobRepo.CreateFile(activeWorkspace.FilesCotainerUri, file.FileName, file.FileData);
                        file.IsUploaded = true;
                    }
                }
            }
        }

        public async Task SaveWorkspace(DetectionWorkspace workspace)
        {
            await UploadTrainingFiles(workspace);
            await workspaceRepo.UpdateItemAsync(workspace.Id, workspace);
        }

        public async Task DeleteWorkpaceAsync(bool isPhysicalDelete)
        {
            throw new NotImplementedException("Pending Implementation");
        }

        public async Task<Stream> DownloadModelAsync(DetectionWorkspace workspace, ModelType modelType)
        {
            throw new NotImplementedException("Pending Implementation");
        }

    }
}
