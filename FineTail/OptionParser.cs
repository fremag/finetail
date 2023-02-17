using FineTail;

public class OptionParser<T> where T : class, new()
{
    public List<Option<T>> Options { get; } = new();

    public void Add(string shortName, string longName, string description, Func<int, string[], T, int> parser)
    {
        Options.Add(new Option<T>
        {
            LongName = longName,
            ShortName = shortName,
            Description = description,
            ValueParser = parser
        });
    }

    public T Parse(string args) => Parse(args.Trim().Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)  );
    
    public T Parse(string[] args)
    {
        var options = new T();
        for (int i = 0; i < args.Length; i++)
        {
            var optionParsed = false;
            for (int j = 0; j < Options.Count; j++)
            {
                var option = Options[j];
                if (option.Parse(ref i, args, options))
                {
                    optionParsed = true;
                    break;
                }
            }

            if (!optionParsed)
            {
                Help(args[i]);
                return null;
            }
        }
        return options;
    }

    private void Help(string unknownArg)
    {
        Console.WriteLine($"Unknown argument: {unknownArg}");
        foreach (var option in Options)
        {
            string text = string.Empty;
            if (!string.IsNullOrEmpty(option.ShortName))
            {
                text += $"-{option.ShortName}, ";
            }
            if (!string.IsNullOrEmpty(option.LongName))
            {
                text += $"--{option.LongName}";
            }

            text += " : " + option.Description;
            Console.WriteLine(text);
        }
    }
}