using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDetectionManager.Models
{
    public class WorkspaceStatus
    {
        public bool IsModelPolicyValid { get; set; }
        public string ValidationErrorDescription { get; set; }
    }
}
