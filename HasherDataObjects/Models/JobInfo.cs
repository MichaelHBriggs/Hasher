using HasherDataObjects.Configuration;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HasherDataObjects.Models
{
    public class JobInfo:AuditableThing
    {
        public string Name { get; set; } = string.Empty;
        public string RootFolder { get; set; } = string.Empty;
        public List<string> Extensions { get; set; } = [];
        public int FoundFilesCount { get; set; } = 0;
        public int ProcessedFilesCount { get; set; } = 0;
        public int FilesHashedCount { get; set; } = 0;

        public RunResults? MostRecentRun { get; set; } = null;

        [JsonIgnore]
        public List<HashableFile> Files { get; set; } = [];

        [NotMapped]
        public double PercentHashed
        {
            get
            {
                if (FoundFilesCount == 0)
                {
                    return 0;
                }
                return Math.Round( (double)FilesHashedCount / (double)FoundFilesCount * 100,2);
            }
        }

        [NotMapped]
        public double PercentProcessed
        {
            get
            {
                if (FoundFilesCount == 0)
                {
                    return 0;
                }
                return Math.Round((double)ProcessedFilesCount / (double)FoundFilesCount * 100, 2);
            }
        }


        public JobInfo() { }
        public JobInfo(Folder sourceFolder)
        {
            RootFolder = sourceFolder.RootFolder;
            Name = sourceFolder.Name;
            Extensions.AddRange(sourceFolder.Extensions);
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
