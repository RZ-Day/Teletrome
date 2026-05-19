namespace Teletrome.API.Entities;

public class Install
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string InstallId { get; set; } = string.Empty;
    public DateTimeOffset FirstSeenAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }

    public Project Project { get; set; } = null!;
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
