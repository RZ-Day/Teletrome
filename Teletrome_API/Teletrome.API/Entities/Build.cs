namespace Teletrome.API.Entities;

public class Build
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public Project Project { get; set; } = null!;
    public ICollection<FunctionRegistryEntry> Functions { get; set; } = new List<FunctionRegistryEntry>();
}
