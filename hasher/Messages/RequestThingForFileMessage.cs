using hasher.Models;
using hasher.Threads;

namespace hasher.Messages
{

    public class RequestThingForFileMessage(RequestThingForFileData data, Func<IThreadMessage, bool>? sendResponseMessage) : IThreadMessage<RequestThingForFileData>
    {
        public int MessageId { get { return AuditableThingRequestThread.MSG_REQUEST_THING_FOR_FILE; } }

        public RequestThingForFileData Data { get { return data; ; } } 

        public Func<IThreadMessage, bool>? SendResponseMessage { get { return sendResponseMessage; } }

        object IThreadMessage.Data { get { return data; ; } }
    }
}
