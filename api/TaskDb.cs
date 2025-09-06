//using Microsoft.EntityFrameworkCore;

//namespace Api.Models;

/*
public class TaskDb : DbContext
{
    public TaskDb(DbContextOptions<TaskDb> options) : base(options) { }

    // Point DbSet at your entity type in Api.Models
    public DbSet<Api.Models.Task> Tasks => Set<Api.Models.Task>();
}
*/

using Microsoft.EntityFrameworkCore;
namespace Api.Models;

using TaskEntity = Api.Models.Task;
using TagEntity = Api.Models.Tag;

public class TaskDb : DbContext
{
    public TaskDb(DbContextOptions<TaskDb> options) : base(options) { }

    public DbSet<Task> Tasks => Set<Task>();
    public DbSet<Tag> Tags => Set<Tag>();



//join table
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Task>()
                 .HasMany(t => t.Tags)
                 .WithMany(tg => tg.Tasks)
                 .UsingEntity(j => j.ToTable("TaskTags"));
    }

}