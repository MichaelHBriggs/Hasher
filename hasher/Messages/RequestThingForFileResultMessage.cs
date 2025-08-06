using hasher.Models;
using hasher.Threads;

namespace hasher.Messages
{
    public class RequestThingForFileResultMessage(RequestThingForFileResultData? data, Func<IThreadMessage,bool>? sendResponseMessage) : IThreadMessage
    {
        public int MessageId { get;  } = MainWorkerThread.MSG_RESPONSE_THING_FOR_FILE;

        public Func<IThreadMessage, bool>? SendResponseMessage { get { return sendResponseMessage; } }

#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        public object? Data => data;
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    }
}
