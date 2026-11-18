namespace MusicCatalog;

public abstract class PlayerBase
{
    public event EventHandler<SongEventArgs>? SongStarted;
    public event EventHandler<SongEventArgs>? SongFinished;

    protected virtual void OnSongStarted(Song song) =>
        SongStarted?.Invoke(this, new SongEventArgs(song));

    protected virtual void OnSongFinished(Song song) =>
        SongFinished?.Invoke(this, new SongEventArgs(song));

    public abstract void Play(Song song);
    public abstract void Stop();
}
