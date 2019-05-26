using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ObjectDetectionManager.Models
{
    public class TrainingFile
    {
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string MediaType { get; set; }
        public List<ObjectRegion> Regions { get; set; }
        public bool IsUploaded { get; set; }

        [NonSerialized]
        private byte[] fileData;
        public byte[] FileData
        {
            get { return fileData; }
            set { fileData = value; }
        }
    }
}
