using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDetectionManager.AzureFunctions.Models
{
    public class DTOTrainingFile
    {
        public string OwnerId { get; set; }
        public string FileName { get; set; }
        public List<DTOObjectRegion> Regions { get; set; }
    }
}
