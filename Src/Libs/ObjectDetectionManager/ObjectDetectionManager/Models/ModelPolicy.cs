using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDetectionManager.Models
{
    public class ModelPolicy
    {
        public string PolicyName { get; set; }
        public int MaxTags { get; set; }
        public int MaxFiles { get; set; }
        public int MinFilesPerTag { get; set; }
        public double AcceptableThreshold { get; set; }
    }
}
