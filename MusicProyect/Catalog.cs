namespace MusicCatalog;

public class Catalog
{
    private readonly List<Song> songs = new();
    private readonly List<Artist> artists = new();
    private readonly List<Genre> genres = new();
    private readonly List<Album> albums = new();
    private int nextSongId = 1, nextArtistId = 1, nextGenreId = 1, nextAlbumId = 1;

    // ---------- Altas, bajas y ediciones de canciones ----------

    public void Add(Song song)
    {
        if (song.Duration == TimeSpan.Zero && File.Exists(song.FilePath))
            song.Duration = WavHelper.GetDuration(song.FilePath);

        song.Id = nextSongId++;
        songs.Add(song);
        song.Album?.Songs.Add(song);
    }

    public bool Remove(int id)
    {
        var song = songs.FirstOrDefault(s => s.Id == id);
        if (song is null) return false;

        songs.Remove(song);
        song.Album?.Songs.Remove(song);
        return true;
    }

    public bool Edit(int id, Song updated)
    {
        var song = songs.FirstOrDefault(s => s.Id == id);
        if (song is null) return false;

        if (updated.Duration == TimeSpan.Zero && File.Exists(updated.FilePath))
            updated.Duration = WavHelper.GetDuration(updated.FilePath);

        // Si cambia de álbum, actualizar ambas colecciones
        if (song.Album != updated.Album)
        {
            song.Album?.Songs.Remove(song);
            updated.Album?.Songs.Add(song);
        }

        song.Title = updated.Title;
        song.Duration = updated.Duration != TimeSpan.Zero ? updated.Duration : song.Duration;
        song.Year = updated.Year;
        song.FilePath = updated.FilePath;
        song.Artist = updated.Artist;
        song.Album = updated.Album;
        song.Genre = updated.Genre;
        return true;
    }

    public Song? GetById(int id) =>
        songs.FirstOrDefault(s => s.Id == id);

    // ---------- Búsquedas con LINQ ----------

    public List<Song> SearchByTitle(string title) =>
        songs.Where(s => s.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
             .ToList();

    public List<Song> SearchByArtist(string name) =>
        songs.Where(s => s.Artist.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
             .ToList();

    public List<Song> SearchByGenre(string name) =>
        songs.Where(s => s.Genre.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
             .ToList();

    public List<Song> SearchByYear(int year) =>
        songs.Where(s => s.Year == year).ToList();

    public List<Song> GetAll() =>
        songs.OrderBy(s => s.Artist.Name).ThenBy(s => s.Title).ToList();

    // ---------- Artistas, géneros y álbumes (se reutilizan si ya existen) ----------

    public Artist GetOrCreateArtist(string name, string country = "")
    {
        var artist = artists.FirstOrDefault(a =>
            a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (artist is not null) return artist;

        artist = new Artist { Id = nextArtistId++, Name = name, Country = country };
        artists.Add(artist);
        return artist;
    }

    public Genre GetOrCreateGenre(string name)
    {
        var genre = genres.FirstOrDefault(g =>
            g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (genre is not null) return genre;

        genre = new Genre { Id = nextGenreId++, Name = name };
        genres.Add(genre);
        return genre;
    }

    public Album GetOrCreateAlbum(string title, int year, Artist artist)
    {
        var album = albums.FirstOrDefault(a =>
            a.Artist == artist &&
            a.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        if (album is not null) return album;

        album = new Album { Id = nextAlbumId++, Title = title, Year = year, Artist = artist };
        albums.Add(album);
        artist.Albums.Add(album);
        return album;
    }
}