using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ObjectDetectionManager.Models
{
    public class TrainingFile
    {
        public string FileName { get; set; }
        public string MediaType { get; set; }
        public List<ObjectRegion> Regions { get; set; }
        public bool IsUploaded { get; set; }

        [NonSerialized]
        private Stream fileData;
        public Stream FileData
        {
            get { return fileData; }
            set { fileData = value; }
        }
    }
}
