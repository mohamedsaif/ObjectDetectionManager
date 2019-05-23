using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDetectionManager.Models
{
    public class ModelPolicy
    {
        public ModelPolicy()
        {
            PolicyName = "DefaultPolicy";
            MaxTags = 20;
            MaxFiles = 1000; //20 tags and 50 files per tag
            MinFilesPerTag = 15;
            AcceptableThreshold = 0.6;
        }
        public string PolicyName { get; set; }
        public int MaxTags { get; set; }
        public int MaxFiles { get; set; }
        public int MinFilesPerTag { get; set; }
        public double AcceptableThreshold { get; set; }
    }
}
