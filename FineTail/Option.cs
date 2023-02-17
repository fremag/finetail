namespace FineTail;

public class Option<T>
{
    public string LongName { get; set; }
    public string ShortName { get; set; }
    public string Description { get; set; }
    
    public Func<int, string[], T, int> ValueParser { get; set; }
    
    public bool Parse(ref int i, string[] args, T options)
    {
        var arg = args[i];
        if (string.IsNullOrEmpty(LongName) && string.IsNullOrEmpty(ShortName))
        {
            i += ValueParser(i, args, options);
            return true;
        }
        
        if (arg != $"--{LongName}" && arg != $"-{ShortName}")
        {
            return false;
        }

        i += ValueParser(i, args, options);
        return true;
    }

    public override string ToString() => $"{ShortName} / {LongName} : {Description}";
}