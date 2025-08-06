using hasher.Common;
using HasherDataObjects.Models;
using System.Text.Json;

namespace hasher.Models
{
    public class RequestThingForFileResultData
    {
        public AuditableThing? Thing { get; }
        public EnumResult Result { get; }
        public RequestThingForFileResultData(AuditableThing? thing, EnumResult result)
        {
            Thing = thing ;
            Result = result;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
