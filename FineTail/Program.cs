namespace FineTail;

public static class Program
{
    public static void Main(string[] args)
    {
        var optionParser = new FineTailOptionParser();
        var options = optionParser.Parse(args);
        if (options != null)
        {
            Run(options);
        }
    }

    private static void Run(FineTailOptions fineTailOptions)
    {
        var cursorVisible = Console.CursorVisible;
        var background = Console.BackgroundColor;
        var foreground = Console.ForegroundColor;
        
        Console.CancelKeyPress += (_, _) =>
        {
            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;
            Console.CursorVisible = cursorVisible;
            Console.WriteLine("That's all folks !");
        };
    
        var colorConfigs = fineTailOptions.Colors;
        IFineTailView view;
        if( fineTailOptions.Interactive)
        {
            view = new FineTailConsoleView(colorConfigs);
        }
        else
        {
            view = new FineTailLogView(colorConfigs, fineTailOptions.NbLines);
        }
        var controller = new FineTailController(fineTailOptions.FilePattern, view, fineTailOptions.Filters);

        controller.Init();
        controller.Run(fineTailOptions.Follow, fineTailOptions.Interactive);
    }
}