using System.Text.Json;

namespace hasher.Models
{
    public class RequestThingForFileData
    {
        public string FolderPath { get; }
        public string CurrentHash { get; }
        public RequestThingForFileData(string folderPath, string currentHash)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentException("Folder path cannot be null or empty.", nameof(folderPath));
            }
            if (string.IsNullOrWhiteSpace(currentHash))
            {
                throw new ArgumentException("Current hash cannot be null or empty.", nameof(currentHash));
            }
            FolderPath = folderPath;
            CurrentHash = currentHash;
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
