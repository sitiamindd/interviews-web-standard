namespace Api.Models;

using System.Text.Json.Serialization;

public class Tag
{
    public int Id { get; set; }
    public string? Name { get; set; }

    [JsonIgnore]                      // <-- prevents cycles
    public List<Task> Tasks { get; } = [];
}