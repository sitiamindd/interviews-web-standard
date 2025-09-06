using Microsoft.EntityFrameworkCore;

public class TaskDb : DbContext
{
    public TaskDb(DbContextOptions<TaskDb> options) : base(options) { }

    // Point DbSet at your entity type in Api.Models
    public DbSet<Api.Models.Task> Tasks => Set<Api.Models.Task>();
}
