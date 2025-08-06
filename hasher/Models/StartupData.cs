using hasher.Messages;
using HasherDataObjects.Models;

namespace hasher.Models
{
    public class StartupData
    {
        public JobInfo? JobInfo { get; set; }
        public Guid RunResultsId { get; set; }
        
        /// <summary>
        /// Function to report updates to changed hashs.
        /// T1 is the FQN of the file
        /// T2 is the new hash
        /// T3 is the old hash
        /// t$ is the return of bool, which indicates if the update was successful
        /// </summary>
        public Func<string, string, string, bool>? OnChangedHashesUpdate; 
        public Func<IThreadMessage, bool>? SendRequestThingForFileMessage { get; set; }
        public Func<IThreadMessage, bool>? SendThingForUpdateMessage { get; set; }
    }
}
