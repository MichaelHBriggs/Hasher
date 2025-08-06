using hasher.Models;
using hasher.Threads;
using System.Collections.Concurrent;

namespace hasher.Messages
{
    public class StartupMessage(StartupData data, Func<IThreadMessage, bool>? SendResponseMessage ) : 
            Message(data, MainWorkerThread.MSG_START, SendResponseMessage)
    {
        
    }
}
