using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDetectionManager.Models
{
    public class ODWorkspace : BaseModel
    {
        public string WorkspaceId { get; set; }
        public string ModelCotainerUri { get; set; }
        public string FilesCotainerUri { get; set; }
        public string ModelDefaultName { get; set; }
        public string CustomVisionProjectId { get; set; }
        public DateTime? LastTrainingDate { get; set; }
        public ModelPolicy Policy { get; set; }
        public List<TrainingFile> Files { get; set; }
    }
}
