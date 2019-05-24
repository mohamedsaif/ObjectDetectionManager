using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDetectionManager.AzureFunctions.Models
{
    public class DTOObjectRegion
    {
        public string TagName { get; set; }
        public string TagId { get; set; }
        public double[] Bounds { get; set; }
    }
}
