namespace MusicCatalog;


public static class SeedData
{
    private static readonly string MusicFolder =
        Path.Combine(AppContext.BaseDirectory, "Music");

    public static void Load(Catalog catalog)
    {
        
        Add(catalog, "My Way", "Frank Sinatra", "Pop", 1969, "My Way", "My_Way_-_Frank_Sinatra.wav");
        Add(catalog, "Gold", "Spandau Ballet", "New Wave", 1983, "True", "Spandau_Ballet_-_Gold.wav");
        
        
    }

    private static void Add(Catalog catalog, string title, string artistName,
        string genreName, int year, string? albumTitle, string fileName)
    {
        string path = Path.Combine(MusicFolder, fileName);
        if (!File.Exists(path))
        {
            Console.WriteLine($"Warning: '{fileName}' Was not found. It will be omited: \"{title}\".");
            return;
        }

        var artist = catalog.GetOrCreateArtist(artistName);
        var song = new Song
        {
            Title = title,
            Year = year,
            FilePath = path,
            Artist = artist,
            Genre = catalog.GetOrCreateGenre(genreName),
            Album = albumTitle is null ? null : catalog.GetOrCreateAlbum(albumTitle, year, artist)
        };

        try
        {
            catalog.Add(song); // la duración se calcula sola desde el .wav
        }
        catch (InvalidDataException ex)
        {
            Console.WriteLine($"Warning: '{fileName}' is not a valid .wav ({ex.Message}).");
        }
    }
}
