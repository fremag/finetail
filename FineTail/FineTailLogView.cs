namespace FineTail;

public class FineTailLogView : AbstractFineTailView
{
    public int NbLines { get; }
    public int CurrentLine { get; set; }
    
    public FineTailLogView(IEnumerable<ColorConfig> colorConfigs, int nbLines) : base(colorConfigs)
    {
        NbLines = nbLines;
    }

    public FineTailLogView() : this(null, 20)
    {
        
    }

    public override void Init(TextFileModel model)
    {
        base.Init(model);

        var firstRowFound = false;

        for (int i = NbLines - 1; i >= 0; i--)
        {
            var line = Model.GetLine(-i);
            if (line == null)
            {
                if (firstRowFound)
                {
                    return;
                }

                continue;
            }

            firstRowFound = true;
            PrintLine(line);
            CurrentLine = i;
        }
    }

    private void PrintLine(string line)
    {
        var coloredLine = Colorize(line);
        Console.WriteLine(coloredLine);
    }

    public override void Update()
    {
        while (Model.GetLine(CurrentLine+1) is string line)
        {
            CurrentLine++;
            PrintLine(line);
        }
    }
}