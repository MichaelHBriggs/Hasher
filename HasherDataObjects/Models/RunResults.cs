using System.Text.Json;

namespace HasherDataObjects.Models
{
    public class RunResults : AuditableThing
    {
        public int AddedFiles { get; set; }
        public int UpdatedFiles { get; set; }
        public int DeletedFiles { get; set; }
        public int UnchangedFiles { get; set; }
        public int TotalFiles { get; set; }
        public bool IsActive { get; set; } = false;

        override public string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

        public static RunResults operator +(RunResults? runResults, RunResults? other)
        {
            if (runResults == null || other == null)
            {
                throw new ArgumentNullException("RunResults cannot be null");
            }
            runResults.AddedFiles += other.AddedFiles;
            runResults.UpdatedFiles += other.UpdatedFiles;
            runResults.DeletedFiles += other.DeletedFiles;
            runResults.UnchangedFiles += other.UnchangedFiles;
            runResults.TotalFiles += other.TotalFiles;
            return runResults;
        }
    }
}
