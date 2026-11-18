namespace MusicCatalog;

public class Song
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public TimeSpan Duration { get; set; }
    public int Year { get; set; }

    public string FilePath { get; set; } = "";

    public Artist Artist { get; set; } = null!;

   
    public Album? Album { get; set; }

    public Genre Genre { get; set; } = null!;

    public override string ToString() =>
        $"{Artist.Name} - {Title} ({Duration:mm\\:ss})";
}
