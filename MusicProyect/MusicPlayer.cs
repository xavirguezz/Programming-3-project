using System.Media;
using System.Timers;
using Timer = System.Timers.Timer;

namespace MusicCatalog;

public class MusicPlayer : IDisposable
{
    private readonly SoundPlayer player = new();
    private readonly PlayQueue queue = new();
    private readonly Timer timer = new() { AutoReset = false };
    private readonly object sync = new();
    private bool playingQueue;

    public Song? Current { get; private set; }

    // Eventos (antes estaban en PlayerBase)
    public event EventHandler<SongEventArgs>? SongStarted;
    public event EventHandler<SongEventArgs>? SongFinished;
    public event EventHandler<SongEventArgs>? SongEnqueued;
    public event EventHandler? QueueEmpty;

    public MusicPlayer()
    {
        // SoundPlayer no tiene evento de fin: lo simulamos con un Timer
        // que salta cuando pasa la duración de la canción.
        timer.Elapsed += OnTimerElapsed;
    }

    protected virtual void OnSongStarted(Song song) =>
        SongStarted?.Invoke(this, new SongEventArgs(song));

    protected virtual void OnSongFinished(Song song) =>
        SongFinished?.Invoke(this, new SongEventArgs(song));

    public void Enqueue(Song song)
    {
        queue.Enqueue(song);
        SongEnqueued?.Invoke(this, new SongEventArgs(song));
    }

    public List<Song> Pending => queue.ToList();

    public void Play(Song song)
    {
        lock (sync)
        {
            StopInternal();

            Current = song;
            player.SoundLocation = song.FilePath;
            player.Play(); // asíncrono: no bloquea el hilo

            if (song.Duration > TimeSpan.Zero)
            {
                timer.Interval = song.Duration.TotalMilliseconds;
                timer.Start();
            }
        }
        OnSongStarted(song);
    }

    /// <summary>Reproduce todas las canciones de la cola, una tras otra.</summary>
    public void PlayAll()
    {
        playingQueue = true;
        Next();
    }

    public void Next()
    {
        var next = queue.Dequeue();
        if (next is null)
        {
            playingQueue = false;
            lock (sync) { StopInternal(); Current = null; }
            QueueEmpty?.Invoke(this, EventArgs.Empty);
            return;
        }
        Play(next);
    }

    public void Stop()
    {
        playingQueue = false;
        lock (sync)
        {
            StopInternal();
            Current = null;
        }
    }

    private void StopInternal()
    {
        timer.Stop();
        player.Stop();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        Song? finished;
        lock (sync) { finished = Current; }
        if (finished is null) return;

        OnSongFinished(finished);

        if (playingQueue) Next();
        else lock (sync) { Current = null; }
    }

    public void Dispose()
    {
        timer.Dispose();
        player.Dispose();
    }
}