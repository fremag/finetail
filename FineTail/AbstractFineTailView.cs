namespace FineTail;

public abstract class AbstractFineTailView : IFineTailView
{
    protected TextFileModel Model { get; set; }
    public IEnumerable<ColorConfig> ColorConfigs { get; }
    public bool Pause { get; set; }

    public AbstractFineTailView(IEnumerable<ColorConfig> colorConfigs)
    {
        ColorConfigs = colorConfigs;
    }

    public virtual void Init(TextFileModel model)
    {
        Model = model;
        Console.Title = $"{Path.GetFileName(model.FilePath)} ({Path.GetDirectoryName(model.FilePath)})";
    }

    public abstract void Update();

    public virtual void OnKey(ConsoleKeyInfo key)
    {
        if (key.Key != ConsoleKey.Spacebar)
        {
            return;
        }
        
        if (Pause)
        {
            Pause = false;
            Update();
        }
        else
        {
            Pause = true;
        }
    }

    public virtual void Stop()
    { }

    public string Colorize(string line)
    {
        var coloredLine = line;
        
        if (ColorConfigs == null)
        {
            return coloredLine;
        }

        foreach (var colorConfig in ColorConfigs)
        {
            coloredLine = colorConfig.Apply(coloredLine);
        }

        return coloredLine;
    }
}