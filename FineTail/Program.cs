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

        Console.CursorVisible = false;
        controller.Run(fineTailOptions.Follow, fineTailOptions.Interactive);
        Console.CursorVisible = true;
    }
}