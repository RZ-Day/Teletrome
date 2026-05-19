namespace Teletrome.API.Entities;

public class Event
{
    public long Id { get; set; }
    public int FunctionRegistryId { get; set; }
    public int InstallId { get; set; }
    public DateTimeOffset RecordedAt { get; set; }

    public FunctionRegistryEntry FunctionRegistryEntry { get; set; } = null!;
    public Install Install { get; set; } = null!;
}
