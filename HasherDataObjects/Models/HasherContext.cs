using Microsoft.EntityFrameworkCore;

namespace HasherDataObjects.Models
{
    public class HasherContext : DbContext
    {
        public DbSet<Drive> Drives { get; set; }
        public DbSet<NestableFolder> Folders { get; set; }
        public DbSet<HashableFile> Files { get; set; }

        public DbSet<RunResults> RunResults { get; set; }

        public DbSet<JobInfo> Jobs { get; set; }

        public DbSet<LogEntry> Logging { get; set; }

        public HasherContext(DbContextOptions<HasherContext> options) : base(options)
        {
        }

        public void UpdateThing(AuditableThing thing)
        {
            if (thing == null)
            {
                throw new ArgumentNullException(nameof(thing), "AuditableThing cannot be null");
            }
            thing.UpdatedAt = DateTime.UtcNow;
            this.Entry(thing).State = EntityState.Modified;
        }

        public void SoftDeleteThing(AuditableThing thing)
        {
            if (thing == null)
            {
                throw new ArgumentNullException(nameof(thing), "AuditableThing cannot be null");
            }
            thing.DeletedAt = DateTime.UtcNow;
            thing.IsDeleted = true;
            this.Entry(thing).State = EntityState.Modified;
        }
    }
}
