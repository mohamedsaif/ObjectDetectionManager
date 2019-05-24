using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDetectionManager.AzureFunctions.Models
{
    public class DownloadModelRequest
    {
        public string OwnerId { get; set; }
        public string Platform { get; set; }
    }
}
