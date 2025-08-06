using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace HasherDataObjects.Models
{
    public class Drive : AuditableThing
    {
        [StringLength(1)]
        public string Letter { get; set; } = string.Empty;
        public List<NestableFolder> Folders { get; set; } = [];
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
