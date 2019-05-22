using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDetectionManager.Models
{
    public class TrainingFile
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public List<ObjectRegion> Regions { get; set; }
    }
}
