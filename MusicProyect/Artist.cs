namespace MusicCatalog;

public class Artist
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Country { get; set; } = "";
    public List<Album> Albums { get; } = new();

    public override string ToString() => Name;
}
