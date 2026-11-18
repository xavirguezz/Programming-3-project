namespace MusicCatalog;

public static class WavHelper
{
    public static TimeSpan GetDuration(string path)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);

        if (new string(br.ReadChars(4)) != "RIFF")
            throw new InvalidDataException("No es un archivo WAV válido.");
        br.ReadInt32();
        if (new string(br.ReadChars(4)) != "WAVE")
            throw new InvalidDataException("No es un archivo WAV válido.");

        int byteRate = 0;

        while (fs.Position + 8 <= fs.Length)
        {
            string id = new string(br.ReadChars(4));
            int size = br.ReadInt32();

            if (id == "fmt ")
            {
                br.ReadInt16();            // formato
                br.ReadInt16();            // canales
                br.ReadInt32();            // frecuencia de muestreo
                byteRate = br.ReadInt32(); // bytes por segundo
                fs.Position += size - 12 + (size & 1);
            }
            else if (id == "data")
            {
                if (byteRate == 0)
                    throw new InvalidDataException("Cabecera WAV incompleta.");
                return TimeSpan.FromSeconds((double)size / byteRate);
            }
            else
            {
                fs.Position += size + (size & 1); // saltar otros bloques
            }
        }

        throw new InvalidDataException("No se encontró el bloque de datos.");
    }
}