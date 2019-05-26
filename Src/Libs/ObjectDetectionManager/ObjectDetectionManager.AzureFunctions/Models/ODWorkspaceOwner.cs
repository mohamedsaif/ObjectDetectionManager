using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDetectionManager.AzureFunctions.Models
{
    public class ODWorkspaceOwner
    {
        public string OwnerId { get; set; }
        public bool CreateIfNotExists { get; set; }
    }
}
