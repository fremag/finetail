using System.Text;

namespace FineTail;

public class TextFileReader
{
    private const int LineFeedLf = 10;
    private const int LineFeedCr = 13;
    
    private readonly List<byte> line = new();
    private Stream Stream { get; }
    private Encoding Encoding { get; }

    public long Position { get; set; }
    
    public TextFileReader(string filePath, int bufferSize = 2 << 8) : this(File.Open(filePath, new FileStreamOptions
    {
        Mode = FileMode.Open, 
        Access = FileAccess.Read, 
        Share = FileShare.Read | FileShare.Write | FileShare.Delete,
        BufferSize = bufferSize 
    }))
    {
        
    }

    public TextFileReader(Stream stream) : this(stream, Encoding.Default)
    {
        
    }
    
    public TextFileReader(Stream stream, Encoding encoding)
    {
        Stream = stream;
        Encoding = encoding;
        Bottom();
    }

    public string ReadNextLine()
    {
        if (Position >= Stream.Length-1) return null;
        
        var endOfLine = false;
        line.Clear();
        
        while (!endOfLine)
        {
            var i = ReadNextByte();

            switch (i)
            {
                case -1:
                    endOfLine = true;
                    break;
                case LineFeedLf:
                    endOfLine = true;
                    break;
                case LineFeedCr:
                    // do nothing
                    break;
                default:
                    var b = Convert.ToByte(i);
                    line.Add(b);
                    break;
            }
        }

        return Encoding.GetString(line.ToArray());
    }
    
    public string ReadPreviousLine()
    {
        if (Position <= 0) return null;
        var endOfLine = false;
        line.Clear();
        
        while (!endOfLine)
        {
            var i = ReadPreviousByte();

            if (i is -1 or LineFeedLf)
            {
                endOfLine = true;
            }
            else if (i != LineFeedCr)
            {
                var b = Convert.ToByte(i);
                line.Add(Convert.ToByte(b));
            }
        }

        line.Reverse();
        return Encoding.GetString(line.ToArray());
    }

    private int ReadPreviousByte()
    {
        if (Position <= 0) return -1;

        Stream.Position = Position - 1;
        var value = Stream.ReadByte();
        Position = Stream.Position - 1;
        return value;
    }

    private int ReadNextByte()
    {
        if (Position >= Stream.Length-1) return -1;

        Stream.Position = Position + 1;
        var value = Stream.ReadByte();
        Position = Stream.Position - 1;
        return value;
    }

    public void Top()
    {
        Position = -1;
    }
    
    public void Bottom()
    {
        Position = Stream.Length-1;
    }
}