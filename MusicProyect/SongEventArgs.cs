namespace MusicCatalog;

public class SongEventArgs : EventArgs
{
    public Song Song { get; }
    public DateTime Timestamp { get; }

    public SongEventArgs(Song song)
    {
        Song = song;
        Timestamp = DateTime.Now;
    }
}
