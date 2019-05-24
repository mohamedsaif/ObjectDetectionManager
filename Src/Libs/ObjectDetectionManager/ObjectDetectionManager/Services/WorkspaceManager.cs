using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using ObjectDetectionManager.Data;
using ObjectDetectionManager.Helpers;
using ObjectDetectionManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectDetectionManager.Services
{
    public class WorkspaceManager
    {
        private CognitiveServicesHelper cognitiveHelper;
        private AzureBlobStorageRepository filesBlobContainer;
        private AzureBlobStorageRepository modelsBlobContainer;
        private CosmosDBRepository<DetectionWorkspace> workspaceRepo;
        private string blobStorageName;
        private string blobStorageKey;
        private DetectionWorkspace activeWorkspace;

        public WorkspaceManager(string storageName, string storageKey, string dbEndpoint, string dbPrimaryKey, string dbName, string sourceSystem, string cvKey, string cvEndpoint, string cvTrainingKey, string cvTrainingEndpoint, string cvPredectionKey, string cvPredectionEndpoint)
        {
            cognitiveHelper = new CognitiveServicesHelper(cvKey, cvEndpoint, cvTrainingKey, cvTrainingEndpoint, cvPredectionKey, cvPredectionEndpoint);
            workspaceRepo = new CosmosDBRepository<DetectionWorkspace>(dbEndpoint, dbPrimaryKey, dbName, sourceSystem);
            blobStorageName = storageName;
            blobStorageKey = storageKey;

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

            filesBlobContainer = new AzureBlobStorageRepository(blobStorageName, blobStorageKey, activeWorkspace.FilesCotainerUri);
            modelsBlobContainer = new AzureBlobStorageRepository(blobStorageName, blobStorageKey, activeWorkspace.ModelCotainerUri);

            return activeWorkspace;
        }

        private async Task<DetectionWorkspace> CreateWorkspaceAsync(string ownerId, ModelPolicy policy)
        {
            var newId = Guid.NewGuid().ToString();

            var result = new DetectionWorkspace()
            {
                id = newId,
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

        private async Task UploadTrainingFiles()
        {
            ValidateWorkspaceRereference();

            foreach (var file in activeWorkspace.Files)
            {
                if (!file.IsUploaded)
                {
                    if (file.FileData != null)
                    {
                        string fileAbsUri = await filesBlobContainer.CreateFile(file.FileName, file.FileData);
                        file.IsUploaded = true;
                    }
                }
            }
        }

        public void ValidateWorkspaceRereference()
        {
            if (activeWorkspace == null)
                throw new InvalidOperationException("You must call GetOrCreateWorkspaceAsync(ownerId) first before any operations");
        }

        public string ValidateWorkspacePolicy()
        {
            ValidateWorkspaceRereference();

            string result = "";
            //This will be used to validate the model policy enforcement and any other related validations.
            if (activeWorkspace.Files.Count > activeWorkspace.Policy.MaxFiles)
            {
                result += "\nTraining Files exceed the maximum limit of the policy";
            }

            foreach(var file in activeWorkspace.Files)
            {
                //TODO: Update validation to cover maximum tags, min files per tag
            }

            return result;
        }

        public async Task ValidateAndSaveWorkspace()
        {
            ValidateWorkspaceRereference();
            var validationMessage = ValidateWorkspacePolicy();
            if (string.IsNullOrEmpty(validationMessage))
            {
                await UploadTrainingFiles();
                await workspaceRepo.UpdateItemAsync(activeWorkspace.id, activeWorkspace);
            }
            else
                throw new InvalidOperationException("Workspace is invalid. " + validationMessage);
        }

        public async Task PrepareWorkspaceForTraining()
        {
            await ValidateAndSaveWorkspace();

            var domains = await cognitiveHelper.CustomVisionTrainingClientInstance.GetDomainsAsync();
            var objDetectionDomain = domains.FirstOrDefault(d => d.Type == "ObjectDetection" && d.Name == "General (compact)");

            //TODO: Add project existence validation
            var project = cognitiveHelper.CustomVisionTrainingClientInstance.CreateProject($"SeeingAI-{activeWorkspace.WorkspaceId}", null, objDetectionDomain.Id);

            activeWorkspace.CustomVisionProjectId = project.Id.ToString();

            //Tags Creation
            var tags = activeWorkspace.Files.SelectMany(r => r.Regions)
                                            .Select(t => t.TagName)
                                            .Distinct();
            var customVisionTags = new List<Tag>();
            var imageFileEntries = new List<ImageFileCreateEntry>();
            foreach (var tag in tags)
            {
                var newTag = await cognitiveHelper.CustomVisionTrainingClientInstance.CreateTagAsync(project.Id, tag);
                customVisionTags.Add(newTag);
            }

            
            foreach (var file in activeWorkspace.Files)
            {
                List<Region> regions = new List<Region>();
                foreach(var region in file.Regions)
                {
                    //get the TagId
                    var customVisionTag = customVisionTags.Where(t => t.Name == region.TagName).FirstOrDefault();
                    region.TagId = customVisionTag.Id.ToString();
                    regions.Add(new Region(customVisionTag.Id, region.Bounds[0], region.Bounds[1], region.Bounds[2], region.Bounds[3]));
                }

                imageFileEntries.Add(new ImageFileCreateEntry(file.FileName, file.FileData, null, regions));
            }

            //Finalize data upload with regions to customer vision project
            await cognitiveHelper.CustomVisionTrainingClientInstance.CreateImagesFromFilesAsync(project.Id, new ImageFileCreateBatch(imageFileEntries));

            activeWorkspace.LastUpdatedDate = DateTime.UtcNow;

            await ValidateAndSaveWorkspace();
        }

        public async Task<bool> TrainPreparedWorkspace()
        {
            if (string.IsNullOrEmpty(activeWorkspace.CustomVisionProjectId))
                throw new InvalidOperationException("You need to execute PrepareWorkspaceForTraining first");
            Guid projectId = Guid.Parse(activeWorkspace.CustomVisionProjectId);
            var iteration = cognitiveHelper.CustomVisionTrainingClientInstance.TrainProject(projectId);
            int totalWaitingInSeconds = 0;
            while (iteration.Status == "Training")
            {
                if (totalWaitingInSeconds > GlobalSettings.MaxTrainingWaitingTimeInSeconds)
                    throw new TimeoutException($"Training timeout as it took more than ({GlobalSettings.MaxTrainingWaitingTimeInSeconds}) seconds.");

                Thread.Sleep(1000);
                totalWaitingInSeconds++;
                // Re-query the iteration to get its updated status
                iteration = cognitiveHelper.CustomVisionTrainingClientInstance.GetIteration(projectId, iteration.Id);
            }

            var changeDate = DateTime.UtcNow;
            activeWorkspace.LastTrainingDate = changeDate;
            activeWorkspace.LastUpdatedDate = changeDate;

            totalWaitingInSeconds = 0;
            foreach (var platform in iteration.ExportableTo)
            {
                if(platform == ExportPlatform.CoreML.ToString() || platform == ExportPlatform.TensorFlow.ToString() || platform == ExportPlatform.ONNX.ToString())
                {
                    var currentExport = await cognitiveHelper.CustomVisionTrainingClientInstance.ExportIterationAsync(projectId, iteration.Id, platform);
                    bool waitForExport = true;
                    while (waitForExport)
                    {
                        if (totalWaitingInSeconds > GlobalSettings.MaxTrainingWaitingTimeInSeconds)
                            throw new TimeoutException($"Exporting timeout as it took more than ({GlobalSettings.MaxTrainingWaitingTimeInSeconds}) seconds.");

                        Thread.Sleep(1000);
                        totalWaitingInSeconds++;
                        var updatedExports = await cognitiveHelper.CustomVisionTrainingClientInstance.GetExportsAsync(projectId, iteration.Id);
                        currentExport = updatedExports.Where(e => e.Platform == platform).FirstOrDefault();
                        if (currentExport != null)
                        {
                            if (currentExport.Status == "Done")
                                waitForExport = false;
                        }
                    }

                    //modelsBlobContainer.CreateFile()
                    WebClient wc = new WebClient();
                    var modelData = wc.DownloadData(currentExport.DownloadUri);
                    await modelsBlobContainer.CreateFile($"{platform}.zip", modelData);
                }
            }

            //Delete the Custom Vision project
            await DeleteCustomVisionProject(projectId);

            return true;
        }

        public async Task DeleteCustomVisionProject(Guid projectId)
        {
            await cognitiveHelper.CustomVisionTrainingClientInstance.DeleteProjectAsync(projectId);
            return;
        }
        public async Task DeleteWorkpaceAsync(bool isPhysicalDelete)
        {
        }

        public async Task DeleteTrainingFile(DetectionWorkspace workspace, TrainingFile file)
        {
            throw new NotImplementedException("Pending Implementation");
        }

        public async Task<Stream> DownloadModelAsync(DetectionWorkspace workspace, ModelType modelType)
        {
            throw new NotImplementedException("Pending Implementation");
        }

    }
}
