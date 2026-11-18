namespace MusicCatalog;

public class Album
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int Year { get; set; }
    public Artist Artist { get; set; } = null!;
    public List<Song> Songs { get; } = new();

    public override string ToString() => $"{Title} ({Year})";
}
