using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDetectionManager.Models
{
    public class ObjectRegion
    {
        public string TagName { get; set; }
        public string TagId { get; set; }
        public double[] Bounds { get; set; }
    }
}
