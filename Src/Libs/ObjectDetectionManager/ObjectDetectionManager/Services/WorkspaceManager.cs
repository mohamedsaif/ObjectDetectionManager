using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using ObjectDetectionManager.Data;
using ObjectDetectionManager.Helpers;
using ObjectDetectionManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private async Task UploadTrainingFiles()
        {
            ValidateWorkspaceRereference();

            foreach (var file in activeWorkspace.Files)
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
                await workspaceRepo.UpdateItemAsync(activeWorkspace.Id, activeWorkspace);
            }
            else
                throw new InvalidOperationException("Workspace is invalid. " + validationMessage);
        }

        public async Task TrainWorkspace(DetectionWorkspace workspace)
        {
            await ValidateAndSaveWorkspace();

            var domains = await cognitiveHelper.CustomVisionTrainingClientInstance.GetDomainsAsync();
            var objDetectionDomain = domains.FirstOrDefault(d => d.Type == "ObjectDetection");

            //TODO: Add project existence validation
            var project = cognitiveHelper.CustomVisionTrainingClientInstance.CreateProject($"SeeingAI-{activeWorkspace.WorkspaceId}", null, objDetectionDomain.Id);

            //Tags Creation
            var tags = activeWorkspace.Files.SelectMany(r => r.Regions)
                                            .Select(t => t.TagName)
                                            .Distinct();
            var customVisionTags = new List<Tag>();
            foreach(var tag in tags)
            {
                var newTag = await cognitiveHelper.CustomVisionTrainingClientInstance.CreateTagAsync(project.Id, tag);
                customVisionTags.Add(newTag);
            }

            var imageFileEntries = new List<ImageFileCreateEntry>();
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

                imageFileEntries.Add(new ImageFileCreateEntry(file.FileName, file.FileData, null, regions);
            }
        }

        public async Task DeleteWorkpaceAsync(bool isPhysicalDelete)
        {
            throw new NotImplementedException("Pending Implementation");
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
