using HasherDataObjects.Models;

namespace hasher.Messages
{
    public class AuditableThingUpdateMessage(AuditableThing auditableThing, int messageId) : IThreadMessage<AuditableThing>
    {
        public int MessageId { get { return messageId; } }

        public AuditableThing Data { get { return auditableThing; } }

        public Func<IThreadMessage, bool>? SendResponseMessage { get; set; }

        object IThreadMessage.Data { get { return auditableThing; } }
    }
}
