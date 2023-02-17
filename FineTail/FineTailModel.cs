using System.Text.RegularExpressions;

namespace FineTail;

public class TextFragmentInfo
{
    public long Begin { get; set; }
    public long End { get; set; }
    public string Line { get; init; }
    public int NumLine { get; init; }
    
    public override string ToString() => $"[{NumLine}]='{Line}'";
}

public class TextFileModel
{
    private static int MaxCacheSize => 10_000;
    public string FilePath { get;}

    private IEnumerable<Regex> Filters { get; }
    private TextFileReader Reader { get; }
    private SortedList<int, TextFragmentInfo> TextFragments { get; } = new();
    public string CacheInfo => $"{TextFragments.Keys.First()}/{TextFragments.Keys.Last()}: {TextFragments.Count}";

    public TextFileModel(string filePath) : this(filePath, Enumerable.Empty<string>())
    {
        
    }

    public TextFileModel(string filePath, IEnumerable<string> filters)
    {
        FilePath = filePath;
        Filters = filters?.Select(filter => new Regex(filter, RegexOptions.Compiled));
        Reader = new TextFileReader(filePath);
        Bottom();
    }

    public void Bottom()
    {
        TextFragments.Clear();
        Reader.Bottom();
        var end = Reader.Position;
        string line;
        do
        {
            line = Reader.ReadPreviousLine();
        } while (line != null && !Filter(line));
        
        var begin = Reader.Position;
        
        TextFragments[0] = new TextFragmentInfo
        {
            Begin =begin,
            End =  end,
            Line = line,
            NumLine = 0
        };
    }

    public void Top()
    {
        TextFragments.Clear();
        Reader.Top();
        var begin = Reader.Position;
        string line;
        do
        {
            line = Reader.ReadNextLine();
        } while (line != null && !Filter(line));
        
        var end = Reader.Position;
        
        TextFragments[0] = new TextFragmentInfo
        {
            Begin =begin,
            End =  end,
            Line = line,
            NumLine = 0
        };
    }

    public TextFragmentInfo GetLineInfo(int n)
    {
        if (TextFragments.TryGetValue(n, out var lineInfo))
        {
            return lineInfo;
        }

        int delta;
        var num = TextFragments.Keys.First();
        if (n < num)
        {
            delta = -1;
            lineInfo = TextFragments[num];
        }
        else
        {
            delta = 1;
            num = TextFragments.Keys.Last();
            lineInfo = TextFragments[num];
        }

        while (num != n)
        {
            num += delta;
            string line;
            long begin;
            long end;
            
            if (delta > 0)
            {
                begin = lineInfo.End;
                Reader.Position = begin;
                do
                {
                    line = Reader.ReadNextLine();
                } while (line != null && !Filter(line));
                end = Reader.Position;
                if (line == null)
                {
                    lineInfo.End = end;
                }
            }
            else
            {
                end = lineInfo.Begin;
                Reader.Position = end; 
                do
                {
                    line = Reader.ReadPreviousLine();
                } while (line != null && !Filter(line));
                
                begin = Reader.Position;
                if (line == null)
                {
                    lineInfo.Begin = begin;
                }
            }

            if (line == null)
            {
                return null;
            }

            lineInfo = new TextFragmentInfo
            {
                Begin = begin,
                End = end,
                Line = line,
                NumLine = num
            };
            TextFragments[num] = lineInfo;
        }

        while (TextFragments.Count > MaxCacheSize)
        {
            var key = delta > 0 ? TextFragments.Keys.First() : TextFragments.Keys.Last();

            TextFragments.Remove(key);
        }
        return lineInfo;
    }
    
    public string GetLine(int n)
    {
        var lineInfo = GetLineInfo(n);
        if (lineInfo == null)
        {
            return null;
        }

        return lineInfo.Line;
    }

    private bool Filter(string line)
    {
        if (Filters == null || !Filters.Any())
        {
            return true;
        }

        var b = Filters.Any(regex => regex.IsMatch(line));
        return b;
    }
}