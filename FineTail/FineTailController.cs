//#define DEBUG_CONTROLLER 

namespace FineTail;

public class FineTailController
{
    private string FilePattern { get; }
    private IFineTailView FineTailView { get; }
    private IEnumerable<string> Filters { get; }
    public string Dir { get; }
    public string Pattern { get; }
    public FileSystemWatcher FsWatcher { get; }
    private bool StopRequested { get; set; }
    private string FileName { get; set; }
    private Thread t1;
    private Thread t2;

    public FineTailController(string filePattern, IFineTailView fineTailView, IEnumerable<string> filters)
    {
        FilePattern = filePattern;
        FineTailView = fineTailView;
        Filters = filters;
        // Directory Mode
        if (Directory.Exists(FilePattern))
        {
            Dir = FilePattern;
            Pattern = "*.*";
        }
        // Pattern mode
        else
        {
            Dir = Path.GetDirectoryName(FilePattern);
            Pattern = Path.GetFileName(FilePattern);
        }

        if (string.IsNullOrEmpty(Dir))
        {
            Dir = ".";
        }

        if (string.IsNullOrEmpty(Pattern))
        {
            Pattern = "*.*";
        }

        FsWatcher = new FileSystemWatcher();
        FsWatcher.Path = Dir;
        FsWatcher.Filter = Pattern;
    }

    public void Init()
    {
        var fileName = GetFileName();
        Init(fileName);
    }

    private void Init(string fileName)
    {
        FileName = fileName;
        if (FileName == null)
        {
            return;
        }

#if DEBUG_CONTROLLER
        Console.WriteLine($">>> Init: {FileName}");
#endif
        var filePath = Path.Combine(Dir, fileName);
        var model = new TextFileModel(filePath, Filters);
        FineTailView.Init(model);
    }

    private string GetFileName()
    {
        if (!Directory.Exists(Dir))
        {
            return null;
        }

        var files = Directory.GetFiles(Dir, Pattern);
        if (!files.Any())
        {
            return null;
        }

        var mostRecentFile = files.Select(file => new FileInfo(file)).OrderBy(fileInfo => fileInfo.LastWriteTime).Last();
        return mostRecentFile.Name;
    }

    private void Follow()
    {
        try
        {
            while (!StopRequested)
            {
                var change = FsWatcher.WaitForChanged(WatcherChangeTypes.All);
                lock (FineTailView)
                {

                    if (FineTailView.Pause)
                    {
                        continue;
                    }
#if DEBUG_CONTROLLER
            Console.WriteLine($">>> {Enum.GetName(change.ChangeType)}: {change.Name}");
#endif
                    switch (change.ChangeType)
                    {
                        case WatcherChangeTypes.Created:
                            Init();
                            break;
                        case WatcherChangeTypes.Deleted:
                            break;
                        case WatcherChangeTypes.Changed:
                            if (change.Name == FileName)
                            {
                                FineTailView.Update();
                            }

                            break;
                        case WatcherChangeTypes.Renamed:
                            break;
                        case WatcherChangeTypes.All:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
        catch (ThreadInterruptedException)
        {
            // Nothing to do
        }
    }

    public void Run(bool follow, bool interactive)
    {
        if (follow)
        {
            t1 = new Thread(Follow);
            t1.Start();
        }

        if (interactive)
        {
            t2 = new Thread(Interactive);
            t2.Start();
        }

        t1?.Join();
        t2?.Join();
    }

    private void Interactive()
    {
        while (true)
        {
            var keyInfo = Console.ReadKey(true);
            lock (FineTailView)
            {
                FineTailView.OnKey(keyInfo);
            }
        }
    }
}