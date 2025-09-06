namespace Api.Models;

public class Task
{
   public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }

    public List<Tag> Tags { get; } = [];
}
