namespace Teletrome.API.Entities;

public class FunctionRegistryEntry
{
    public int Id { get; set; }
    public int BuildId { get; set; }
    public string FunctionName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTimeOffset FirstSeenAt { get; set; }

    public Build Build { get; set; } = null!;
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
