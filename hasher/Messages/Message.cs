using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace hasher.Messages
{
    public class Message(object data, int messageId, Func<IThreadMessage, bool>? sendResponseMessage) : 
        IThreadMessage
    {
        public int MessageId { get { return messageId; } }

        public object Data { get { return data; } }

        public Func<IThreadMessage, bool>? SendResponseMessage { get { return sendResponseMessage; } }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
