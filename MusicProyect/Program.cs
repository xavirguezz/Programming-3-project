
using MusicCatalog;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var catalog = new Catalog();
SeedData.Load(catalog);
using var player = new MusicPlayer();

// Notificaciones: se suscriben a los eventos del reproductor
player.SongStarted += (s, e) => Notify($"▶ Playing: {e.Song} [{e.Timestamp:HH:mm:ss}]");
player.SongFinished += (s, e) => Notify($"⏹ Finished: {e.Song} [{e.Timestamp:HH:mm:ss}]");
player.SongEnqueued += (s, e) => Notify($"➕ On queue: {e.Song}");
player.QueueEmpty += (s, e) => Notify("The queue has finished.");

bool exit = false;
while (!exit)
{
    ShowMenu();
    switch (Console.ReadLine()?.Trim().ToUpper())
    {
        case "1": AddSong(); break;
        case "2": PrintSongs(catalog.GetAll()); break;
        case "3": Search(); break;
        case "4": EditSong(); break;
        case "5": DeleteSong(); break;
        case "6": EnqueueSong(); break;
        case "7": EnqueueAll(); break;
        case "8": ShowQueue(); break;
        case "9": PlayQueue(); break;
        case "P": PlayNow(); break;
        case "N": player.Next(); break;
        case "S": player.Stop(); Console.WriteLine("Playback stopped."); break;
        case "0": exit = true; break;
        default: Console.WriteLine("Invalid option."); break;
    }
}
player.Stop();

// ====================== Menú ======================

void ShowMenu()
{
    Console.WriteLine();
    Console.WriteLine("===== MUSIC CATALOG AND PLAYER =====");
    Console.WriteLine(" 1. Add song");
    Console.WriteLine(" 2. View catalog");
    Console.WriteLine(" 3. Search");
    Console.WriteLine(" 4. Edit song");
    Console.WriteLine(" 5. Delete song");
    Console.WriteLine(" 6. Add song to queue");
    Console.WriteLine(" 7. Add entire catalog to queue");
    Console.WriteLine(" 8. View queue");
    Console.WriteLine(" 9. Play queue");
    Console.WriteLine(" P. Play a song now");
    Console.WriteLine(" N. Next      S. Stop");
    Console.WriteLine(" 0. Exit");
    if (player.Current is not null)
        Console.WriteLine($" Now playing: {player.Current}");
    Console.Write("> ");
}

// ====================== Operaciones del catálogo ======================

void AddSong()
{
    string path = Ask("WAV file path").Trim('"');
    if (!IsValidWav(path)) return;

    string title = Ask("Title");
    string artistName = Ask("Artist");
    string genreName = Ask("Genre");
    int year = AskInt("Year");
    string albumTitle = Ask("Album (press Enter if none)", allowEmpty: true);

    var artist = catalog.GetOrCreateArtist(artistName);
    var song = new Song
    {
        Title = title,
        Year = year,
        FilePath = path,
        Artist = artist,
        Genre = catalog.GetOrCreateGenre(genreName),
        Album = albumTitle == "" ? null : catalog.GetOrCreateAlbum(albumTitle, year, artist)
    };

    try
    {
        catalog.Add(song);
        Console.WriteLine($"Added with id {song.Id}: {song}");
    }
    catch (InvalidDataException ex)
    {
        Console.WriteLine($"Could not add song: {ex.Message}");
    }
}

void Search()
{
    Console.WriteLine("Search by: 1. Title  2. Artist  3. Genre  4. Year");
    Console.Write("> ");
    switch (Console.ReadLine()?.Trim())
    {
        case "1": PrintSongs(catalog.SearchByTitle(Ask("Title"))); break;
        case "2": PrintSongs(catalog.SearchByArtist(Ask("Artist"))); break;
        case "3": PrintSongs(catalog.SearchByGenre(Ask("Genre"))); break;
        case "4": PrintSongs(catalog.SearchByYear(AskInt("Year"))); break;
        default: Console.WriteLine("Invalid option."); break;
    }
}

void EditSong()
{
    var song = catalog.GetById(AskInt("Song ID to edit"));
    if (song is null) { Console.WriteLine("Song not found."); return; }

    Console.WriteLine($"Editing: {song}  (Press Enter to keep the current value)");

    string title = AskDefault("Title", song.Title);
    string artistName = AskDefault("Artist", song.Artist.Name);
    string genreName = AskDefault("Genre", song.Genre.Name);
    int year = AskIntDefault("Year", song.Year);
    string albumTitle = AskDefault("Album (- to delete)", song.Album?.Title ?? "-");
    string path = AskDefault("WAV file path", song.FilePath).Trim('"');

    if (path != song.FilePath && !IsValidWav(path)) return;

    var artist = catalog.GetOrCreateArtist(artistName);
    var updated = new Song
    {
        Title = title,
        Year = year,
        FilePath = path,
        Artist = artist,
        Genre = catalog.GetOrCreateGenre(genreName),
        Album = albumTitle == "-" ? null : catalog.GetOrCreateAlbum(albumTitle, year, artist)
    };

    try
    {
        catalog.Edit(song.Id, updated);
        Console.WriteLine($"Edited: {song}");
    }
    catch (InvalidDataException ex)
    {
        Console.WriteLine($"Could not edit song: {ex.Message}");
    }
}

void DeleteSong()
{
    int id = AskInt("Song ID to delete");
    Console.WriteLine(catalog.Remove(id) ? "Song deleted." : "Song not found.");
}

// ====================== Reproductor ======================

void EnqueueSong()
{
    var song = catalog.GetById(AskInt("Song ID"));
    if (song is null) { Console.WriteLine("Song not found."); return; }
    player.Enqueue(song);
}

void EnqueueAll()
{
    foreach (var song in catalog.GetAll())
        player.Enqueue(song);
}

void ShowQueue()
{
    var pending = player.Pending;
    if (player.Current is not null) Console.WriteLine($"Now playing: {player.Current}");
    if (pending.Count == 0) { Console.WriteLine("The queue is empty."); return; }

    Console.WriteLine("Upcoming songs:");
    for (int i = 0; i < pending.Count; i++)
        Console.WriteLine($"  {i + 1}. {pending[i]}");
}

void PlayQueue()
{
    if (player.Pending.Count == 0) { Console.WriteLine("The queue is empty."); return; }
    player.PlayAll();
}

void PlayNow()
{
    var song = catalog.GetById(AskInt("Song ID"));
    if (song is null) { Console.WriteLine("Song not found."); return; }
    player.Play(song);
}

// ====================== Utilidades ======================

void Notify(string message)
{
    // Los eventos llegan desde otro hilo, mientras el menú espera una tecla
    Console.WriteLine($"\n{message}");
}

void PrintSongs(List<Song> list)
{
    if (list.Count == 0) { Console.WriteLine("No results found."); return; }
    foreach (var s in list)
        Console.WriteLine($"  [{s.Id}] {s} | {s.Genre} | {s.Year} | {(s.Album?.Title ?? "No album")}");
}

bool IsValidWav(string path)
{
    if (!File.Exists(path)) { Console.WriteLine("File not found."); return false; }
    if (!Path.GetExtension(path).Equals(".wav", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Only .wav files are supported (SoundPlayer limitation).");
        return false;
    }
    return true;
}

string Ask(string prompt, bool allowEmpty = false)
{
    while (true)
    {
        Console.Write($"{prompt}: ");
        string text = Console.ReadLine()?.Trim() ?? "";
        if (text != "" || allowEmpty) return text;
        Console.WriteLine("This field cannot be empty.");
    }
}

string AskDefault(string prompt, string current)
{
    Console.Write($"{prompt} [{current}]: ");
    string text = Console.ReadLine()?.Trim() ?? "";
    return text == "" ? current : text;
}

int AskInt(string prompt)
{
    while (true)
    {
        Console.Write($"{prompt}: ");
        if (int.TryParse(Console.ReadLine(), out int value)) return value;
        Console.WriteLine("Please enter an integer.");
    }
}

int AskIntDefault(string prompt, int current)
{
    while (true)
    {
        Console.Write($"{prompt} [{current}]: ");
        string text = Console.ReadLine()?.Trim() ?? "";
        if (text == "") return current;
        if (int.TryParse(text, out int value)) return value;
        Console.WriteLine("Please enter an integer.");
    }
}
