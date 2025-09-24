using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace HasherDataObjects.Models
{
    public abstract class AuditableThing
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = null;
        public DateTime? DeletedAt { get; set; } = null;
        public bool IsDeleted { get; set; } = false;

        [NotMapped]
        public double DurationInSeconds
        {
            get
            {
                if (UpdatedAt != null)
                {
                    return (UpdatedAt! - CreatedAt).Value.TotalSeconds;
                }
                return 0;
            }
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
