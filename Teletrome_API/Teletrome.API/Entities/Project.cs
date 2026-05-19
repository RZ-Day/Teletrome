namespace Teletrome.API.Entities;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Build> Builds { get; set; } = new List<Build>();
    public ICollection<Install> Installs { get; set; } = new List<Install>();
}
