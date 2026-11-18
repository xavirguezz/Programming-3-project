namespace MusicCatalog;

public class PlayQueue
{
    private readonly Queue<Song> queue = new();

    public int Count => queue.Count;

    public void Enqueue(Song song) => queue.Enqueue(song);

    public Song? Dequeue() =>
        queue.Count > 0 ? queue.Dequeue() : null;

    public void Clear() => queue.Clear();

    /// <summary>Copia de las canciones pendientes.</summary>
    public List<Song> ToList() => queue.ToList();
}
