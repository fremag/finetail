namespace FineTail;

public class FineTailOptions
{
    public int NbLines { get; set; } = 20;
    public string FilePattern { get; set; } = "*.log";
    public List<string> Filters { get; } = new();
    public List<ColorConfig> Colors { get; } = new();
    public bool Follow { get; set; }
    public bool Interactive { get; set; }
}