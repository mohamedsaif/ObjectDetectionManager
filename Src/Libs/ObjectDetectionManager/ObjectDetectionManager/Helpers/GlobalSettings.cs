using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDetectionManager.Helpers
{
    public class GlobalSettings
    {
        public const string SystemOrigin = "ODWorkspaceManagerV1.0.0";
        public const string DefaultModelName = "PersonalObjectDetection";
        public const string DefaultPartitionId = "Default";
        public const int MaxTrainingWaitingTimeInSeconds = 300;
        public const string AIAlgorithmType = "ObjectDetection";
        public const string AIAlgorithmSubtype = "General (compact)";
        public const string CustomVisionProjectPrefix = "ODM";
    }
}
