namespace FineTail;

public class FineTailOptionParser: OptionParser<FineTailOptions>
{
    public FineTailOptionParser()
    {
        Add("n", "NbLines", "Set output number of lines to read.", (i, args, options) =>
        {
            options.NbLines = int.Parse(args[i+1]);
            return 1;
        });
        
        Add("e", "expr", "One or more regexes to filter lines", (i, args, options) =>
        {
            options.Filters.Add(args[i+1]);
            return 1;
        });
        
        Add("c", "color", "Color and regex to highlight text. Example: red ERROR", (i, args, options) =>
        {
            options.Colors.Add(new ColorConfig(args[i+1], args[i + 2]));
            return 2;
        });
        
        Add("f", "Follow", "Follow file changes", (_, _, options) =>
        {
            options.Follow = true;
            return 0;
        });
        
        Add("i", "interactive", "Interactive mode", (_, _, options) =>
        {
            options.Interactive = true;
            return 0;
        });
        
        Add(null, null, "File Pattern", (i, args, options) =>
        {
            options.FilePattern = args[i];
            return 0;
        }
        );
    }
}