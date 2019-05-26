using ObjectDetectionManager.Models;
using ObjectDetectionManager.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace ObjectDetectionManager.TestClient
{
    class Program
    {
        private static string cvPredectionEndpoint = "https://westeurope.api.cognitive.microsoft.com/customvision/v3.0/Prediction/";
        private static string cvPredectionKey = "da51de4ffb0f43aebcc90defd7c8eefb";
        private static string cvTrainingKey = "323eb8f1bd56442a99140d397ca38830";
        private static string cvTrainingEndpoint = "https://westeurope.api.cognitive.microsoft.com";
        private static string cvEndpoint = "https://westeurope.api.cognitive.microsoft.com/";
        private static string cvKey = "b7a6702052a545de84328589246437ab";
        private static string sourceSystem = "TestClient";
        private static string dbName = "seeingaivisiontrainer";
        private static string dbPrimaryKey = "GJL8abEUQ23W4okLmlw0W3E6q7zbICgRMkAxEKU7RdjaXA3ciMMbqi8TqPp8X34MAzMf7d4DU9jbBhx7kIIxTQ==";
        private static string dbEndpoint = "https://seeingaivisiontrainer.documents.azure.com:443/";
        private static string storageKey = "9N8EnjH3r36ZyQVRomoWwA1oa9eoRTsFD6+KToV8pX1dCBjTeDdNh2qYLrCaXAuyiEQVFUpbz1r1eGX/iiiXww==";
        private static string storageName = "seeingaihackcustomod";

        private static string ownerId = "11111222-b89b-4bb3-9140-14e3d244e0aa";

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Object Detection Manager Test");
            Console.WriteLine("This demo test include images for detection forks and scissors.");
            Console.WriteLine("The following steps will be performed to generate offline model to be used in mobile devices:");
            Console.WriteLine("1. Create or Update a new workspace for object detection");
            Console.WriteLine("2. Upload images along with their objects regions to Azure Storage");
            Console.WriteLine("3. Provision new custom vision project for object detection");
            Console.WriteLine("4. Upload the images to custom vision project along with tags");
            Console.WriteLine("5. Kick off project training in a new iteration");
            Console.WriteLine("6. Export the generated model into CoreML, TensorFlow and ONNX and upload them to storage");
            Console.WriteLine("7. Delete the custom vision project");

            ExecuteDemo();

            Console.ReadLine();
        }

        static async void ExecuteDemo()
        {
            //Create/get an object detection workspace for the provided ownerId
            ODMWorkspaceManager wm = ODMWorkspaceManager.Initialize(true, ownerId, storageName, storageKey, dbEndpoint, dbPrimaryKey, dbName, sourceSystem, cvKey, cvEndpoint, cvTrainingKey, cvTrainingEndpoint, cvPredectionKey, cvPredectionEndpoint);

            Console.WriteLine($"Creating or getting existent workspace for user: ({ownerId})");

            var workspace = await wm.GetWorkspaceAsync(ownerId, true);

            Console.WriteLine($"*** Workspace created/retrieved successfully with id: ({workspace.id})");

            Console.WriteLine("Staring to generate workspace files and regions locally and save them to the workspace");

            var sampleData = GetSampleDate();

            wm.AddTrainingFiles(sampleData);

            Console.WriteLine("Preparing the workspace for training by uploading the data to custom vision");

            //await wm.PrepareWorkspaceForTraining();

            Console.WriteLine($"*** Data upload to custom vision finished successfully and project ready for training. New custom vision project id is {workspace.CustomVisionProjectId}");

            Console.WriteLine("Starting the training process. This will end with a generated models uploaded to storage");

            var isTrainingCompleted = await wm.TrainPreparedWorkspace();

            Console.WriteLine("*** Training of model completed successfully. Model was also exported and saved to the storage");

            Console.WriteLine("Getting the download links for the offline models: ONNX, CoreML and TensorFlow:");

            string downloadModelUrl = wm.GetModelDownloadUri(OfflineModelType.CoreML);

            Console.WriteLine($"CoreML Download Link: {downloadModelUrl}");

            downloadModelUrl = wm.GetModelDownloadUri(OfflineModelType.TensorFlow);

            Console.WriteLine($"TensorFlow Download Link: {downloadModelUrl}");

            downloadModelUrl = wm.GetModelDownloadUri(OfflineModelType.ONNX);

            Console.WriteLine($"ONNX Download Link: {downloadModelUrl}");

            Console.WriteLine("****************************************");

            Console.WriteLine("*** Test simulation completed successfully! ***");
        }

        public static List<TrainingFile> GetSampleDate()
        {
            List<TrainingFile> result = new List<TrainingFile>();

            //Demo data is based on Azure Cognitive Services demo on GitHub: https://github.com/Azure-Samples/cognitive-services-dotnet-sdk-samples
            //Notice that bounding boxes are using normalized dimensions (x, y, width and height are between 0 and 1);
            Dictionary<string, double[]> fileToRegionMapScissors = new Dictionary<string, double[]>()
            {
                // FileName, Left, Top, Width, Height
                {"scissors_1", new double[] { 0.4007353, 0.194068655, 0.259803921, 0.6617647 } },
                {"scissors_2", new double[] { 0.426470578, 0.185898721, 0.172794119, 0.5539216 } },
                {"scissors_3", new double[] { 0.289215684, 0.259428144, 0.403186262, 0.421568632 } },
                {"scissors_4", new double[] { 0.343137264, 0.105833367, 0.332107842, 0.8055556 } },
                {"scissors_5", new double[] { 0.3125, 0.09766343, 0.435049027, 0.71405226 } },
                {"scissors_6", new double[] { 0.379901975, 0.24308826, 0.32107842, 0.5718954 } },
                {"scissors_7", new double[] { 0.341911763, 0.20714055, 0.3137255, 0.6356209 } },
                {"scissors_8", new double[] { 0.231617644, 0.08459154, 0.504901946, 0.8480392 } },
                {"scissors_9", new double[] { 0.170343131, 0.332957536, 0.767156839, 0.403594762 } },
                {"scissors_10", new double[] { 0.204656869, 0.120539248, 0.5245098, 0.743464053 } },
                {"scissors_11", new double[] { 0.05514706, 0.159754932, 0.799019635, 0.730392158 } },
                {"scissors_12", new double[] { 0.265931368, 0.169558853, 0.5061275, 0.606209159 } },
                {"scissors_13", new double[] { 0.241421565, 0.184264734, 0.448529422, 0.6830065 } },
                {"scissors_14", new double[] { 0.05759804, 0.05027781, 0.75, 0.882352948 } },
                {"scissors_15", new double[] { 0.191176474, 0.169558853, 0.6936275, 0.6748366 } },
                {"scissors_16", new double[] { 0.1004902, 0.279036, 0.6911765, 0.477124184 } },
                {"scissors_17", new double[] { 0.2720588, 0.131977156, 0.4987745, 0.6911765 } },
                {"scissors_18", new double[] { 0.180147052, 0.112369314, 0.6262255, 0.6666667 } },
                {"scissors_19", new double[] { 0.333333343, 0.0274019931, 0.443627447, 0.852941155 } },
                {"scissors_20", new double[] { 0.158088237, 0.04047389, 0.6691176, 0.843137264 } } };

            Dictionary<string, double[]> fileToRegionMapFork = new Dictionary<string, double[]>()
            {
                { "fork_1", new double[] { 0.145833328, 0.3509314, 0.5894608, 0.238562092 } },
                {"fork_2", new double[] { 0.294117659, 0.216944471, 0.534313738, 0.5980392 } },
                {"fork_3", new double[] { 0.09191177, 0.0682516545, 0.757352948, 0.6143791 } },
                {"fork_4", new double[] { 0.254901975, 0.185898721, 0.5232843, 0.594771266 } },
                {"fork_5", new double[] { 0.2365196, 0.128709182, 0.5845588, 0.71405226 } },
                {"fork_6", new double[] { 0.115196079, 0.133611143, 0.676470637, 0.6993464 } },
                {"fork_7", new double[] { 0.164215669, 0.31008172, 0.767156839, 0.410130739 } },
                {"fork_8", new double[] { 0.118872553, 0.318251669, 0.817401946, 0.225490168 } },
                {"fork_9", new double[] { 0.18259804, 0.2136765, 0.6335784, 0.643790841 } },
                {"fork_10", new double[] { 0.05269608, 0.282303959, 0.8088235, 0.452614367 } },
                {"fork_11", new double[] { 0.05759804, 0.0894935, 0.9007353, 0.3251634 } },
                {"fork_12", new double[] { 0.3345588, 0.07315363, 0.375, 0.9150327 } },
                {"fork_13", new double[] { 0.269607842, 0.194068655, 0.4093137, 0.6732026 } },
                {"fork_14", new double[] { 0.143382356, 0.218578458, 0.7977941, 0.295751631 } },
                {"fork_15", new double[] { 0.19240196, 0.0633497, 0.5710784, 0.8398692 } },
                {"fork_16", new double[] { 0.140931368, 0.480016381, 0.6838235, 0.240196079 } },
                {"fork_17", new double[] { 0.305147052, 0.2512582, 0.4791667, 0.5408496 } },
                {"fork_18", new double[] { 0.234068632, 0.445702642, 0.6127451, 0.344771236 } },
                {"fork_19", new double[] { 0.219362751, 0.141781077, 0.5919118, 0.6683006 } },
                {"fork_20", new double[] { 0.180147052, 0.239820287, 0.6887255, 0.235294119 } }
            };

            foreach(var entry in fileToRegionMapScissors)
            {
                var newTrainingFile = new TrainingFile { OriginalFileName = $"{entry.Key}.jpg", MediaType = "Image" };
                newTrainingFile.Regions = new List<ObjectRegion> {
                    new ObjectRegion {
                        Bounds = entry.Value,
                        TagName = "Scissors"
                    }
                };
                var imagesPath = Path.Combine("Images", "scissors");

                newTrainingFile.FileData = File.ReadAllBytes(Path.Combine(imagesPath, newTrainingFile.FileName));

                result.Add(newTrainingFile);
            }

            foreach (var entry in fileToRegionMapFork)
            {
                var newTrainingFile = new TrainingFile { OriginalFileName = $"{entry.Key}.jpg", MediaType = "Image" };
                newTrainingFile.Regions = new List<ObjectRegion> {
                    new ObjectRegion {
                        Bounds = entry.Value,
                        TagName = "Fork"
                    }
                };
                var imagesPath = Path.Combine("Images", "fork");

                newTrainingFile.FileData = File.ReadAllBytes(Path.Combine(imagesPath, newTrainingFile.FileName));

                result.Add(newTrainingFile);
            }

            return result;
        }
    }
}
