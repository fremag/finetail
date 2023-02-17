using TextCopy;

namespace FineTail;

public class FineTailConsoleView : AbstractFineTailView
{
    public ConsoleColor Foreground { get; } = Console.ForegroundColor;
    public ConsoleColor Background { get; } = Console.BackgroundColor;
    
    public int TopLine { get; private set; }
    public int BottomLine { get; private set; }
    
    public int SelectedLine { get; set;}
    public bool SelectedLineVisible { get; set; }
    
    private string[] WhiteSpaces { get; }

    public int ViewHeight => Console.WindowHeight - 2;
    public int ViewWidth => Console.WindowWidth;

    public int CurrentWidth { get; set; }
    public int CurrentHeight { get; set; }
    
    public int NbLines => BottomLine - TopLine + 1;

    private readonly Timer timer;
    private readonly Clipboard clipboard = new();
    private DateTime LastUpdate { get; set; }
    public int Offset { get; set; }

    public enum Direction {None, Forward, Backward}
    public Direction SearchDirection { get; set; } = Direction.None;
    public string SearchExpr { get; set; }
    public int SearchExprCursor { get; set; }
    public string SearchPrefix { get; set; } = "Search: ";
    public ColorConfig SearchColorConfig { get; private set; }
        
    public FineTailConsoleView(IEnumerable<ColorConfig> colorConfigs) : base(colorConfigs)
    {
        timer = new Timer(OnTick, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        CurrentHeight = ViewHeight;
        CurrentWidth = ViewWidth;
        string s = string.Empty;
        WhiteSpaces = new string[512];
        for (int i = 0; i < WhiteSpaces.Length; i++)
        {
            WhiteSpaces[i] = s;
            s += ' ';
        }
    }

    private void OnTick(object state)
    {
        if (CurrentHeight == ViewHeight && CurrentWidth == ViewWidth)
        {
            return;
        }

        CurrentHeight = ViewHeight;
        CurrentWidth = ViewWidth;
        Init(Model);
    }

    public override void Init(TextFileModel model)
    {
        LastUpdate = DateTime.Now;
        base.Init(model);
        Model.Bottom();

        BottomLine = 0;
        TopLine = -(ViewHeight-1);
        while (Model.GetLine(TopLine) == null)
        {
            TopLine++;
        }

        SelectedLine = NbLines-1;
        Refresh();
    }

    public void Refresh()
    {
        PrintHeader();
        Console.BackgroundColor = Background;
        Console.ForegroundColor = Foreground;

        for (int i = 0; i < ViewHeight; i++)
        {
            int numLine = TopLine + i;
            var line = Model.GetLine(numLine);
            if (line == null)
            {
                line = string.Empty;
            }
            else
            {
                BottomLine = numLine;
            }

            //line = $"{i} / {numLine}: '"+line+"'";
            if (Offset > 0)
            {
                if (Offset > line.Length)
                {
                    line = string.Empty;
                }
                else
                {
                    line = line[Offset..];
                }
            }
            
            if (line.Length > ViewWidth)
            {
                line = line[..ViewWidth];
            }
            else
            {
                line += WhiteSpaces[ViewWidth-line.Length];
            }

            if (SearchColorConfig != null)
            {
                line = SearchColorConfig.Apply(line);
            }
            
            Console.SetCursorPosition(0, i+1);
            if (i == SelectedLine && SelectedLineVisible)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(line);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                line = Colorize(line);
                Console.Write(line);
            }
        }

        PrintStatusBar();
    }

    private void PrintInfo(string info, int yPosition, bool inverseVideo)
    {
        if( inverseVideo )
        {
            Console.BackgroundColor = Foreground;
            Console.ForegroundColor = Background;
        }
        else
        {
            Console.BackgroundColor = Background;
            Console.ForegroundColor = Foreground;
        }
        Console.SetCursorPosition(0, yPosition);
        while (info.Length < Console.WindowWidth-1)
        {
            info += '_';
        }
        Console.Write(info);
    }
    
    private void PrintHeader()
    {
        var header = $"{Model.FilePath}";
        
        // $"Top: {TopLine}/{BottomLine}={NbLines} - {ViewHeight} ({Console.WindowHeight} x {Console.WindowWidth}) - {Model.CacheInfo}";
        PrintInfo(header, 0, true);
    }

    private void PrintStatusBar()
    {
        var status = string.Empty;
        var searchActive = SearchDirection != Direction.None;
        if (searchActive)
        {
            status = $"{SearchPrefix}{SearchExpr}";
        }
        else if (Offset > 0)
        {
            status += $"[> {Offset}]";
        } 
        
        if (Pause)
        {
            status += " [Pause]";
        }
        else
        {
            status += $" Last: {LastUpdate:yyyy-MM-dd HH:mm:ss}";
        }
        
        PrintInfo(status, Console.WindowHeight-1, ! searchActive);

        Console.CursorVisible = searchActive;
        if (searchActive)
        {
            Console.SetCursorPosition(SearchPrefix.Length+SearchExprCursor, Console.WindowHeight-1);
        } 
    }

    public override void Update()
    {
        if (Pause)
        {
            return;
        }

        LastUpdate = DateTime.Now;
        while(Model.GetLine(BottomLine+1) != null) 
        {
            if (NbLines >= ViewHeight)
            {
                TopLine++;
            }
            BottomLine++;
        }

        Refresh();
    }

    public void ScrollOneLineUp()
    {
        Up(1);
    }

    private void Up(int nbLines)
    {
        int i = 1;
        while( i <= nbLines && Model.GetLine(TopLine - i) != null)
        {
            TopLine--;
            i++;
        }
    }

    public void ScrollOneLineDown()
    {
        Down(1);
    }

    private void Down(int nbLines)
    {
        int i = 1;
        while(i <= nbLines && Model.GetLine(BottomLine + i) != null)
        {
            TopLine++;
            i++ ;
        }
    }

    private void PageUp()
    {
        Up(ViewHeight);
    }

    private void PageDown()
    {
        Down(ViewHeight);
    }

    private void ScrollToTop()
    {
        Model.Top();
        TopLine = 0;
    }

    private void ScrollToBottom()
    {
        Init(Model);
    }

    public override void OnKey(ConsoleKeyInfo keyInfo)
    {
        var key = keyInfo.Key;
        if (SearchDirection != Direction.None && key != ConsoleKey.Escape)
        {
            InputSearchExpr(keyInfo);
            return;
        } 
        
        if (keyInfo.KeyChar == '/')
        {
            SearchDirection = Direction.Forward;
            SearchExpr = string.Empty;
            SearchExprCursor = 0;
            Refresh();
            return;
        }
        
        if (keyInfo.KeyChar == '?')
        {
            SearchDirection = Direction.Backward;
            SearchExpr = string.Empty;
            SearchExprCursor = 0;
            Refresh();
            return;
        }

        switch (key)
        {
            case ConsoleKey.N:
                var searchDirection = keyInfo.Modifiers == ConsoleModifiers.Shift ? Direction.Backward : Direction.Forward;
                Search(searchDirection);
                break;
            
            case ConsoleKey.J:
            case ConsoleKey.RightArrow:
                ScrollRight(keyInfo.Modifiers);
                break;
            
            case ConsoleKey.L:
            case ConsoleKey.LeftArrow:
                ScrollLeft(keyInfo.Modifiers);
                break;

            // UP
            case ConsoleKey.UpArrow:
                Pause = true;
                ScrollOneLineUp();
                break;
            
            case ConsoleKey.I:
                Pause = true;
                if (keyInfo.Modifiers == ConsoleModifiers.Shift)
                {
                    SelectedLineVisible = true;
                    MoveSelectedLine(-1);
                }
                else
                {
                    ScrollOneLineUp();
                }
                break;
            
            // Down
            case ConsoleKey.DownArrow:
                Pause = true;
                ScrollOneLineDown();
                break;
            
            case ConsoleKey.K:
                Pause = true;
                if (keyInfo.Modifiers == ConsoleModifiers.Shift)
                {
                    SelectedLineVisible = true;
                    MoveSelectedLine(+1);
                }
                else
                {
                    ScrollOneLineDown();
                }
                break;
            
            case ConsoleKey.PageUp:
                Pause = true;
                PageUp();
                break;
            
            case ConsoleKey.PageDown:
                Pause = true;
                PageDown();
                break;
            
            case ConsoleKey.Home:
                Pause = true;
                SelectedLine = 0;
                ScrollToTop();
                break;
            
            case ConsoleKey.End:
                Pause = true;
                SelectedLine = NbLines - 1;
                ScrollToBottom();
                Pause = false;
                break;
            
            case ConsoleKey.Escape:
                SearchColorConfig = null; 
                SearchDirection = Direction.None;
                Pause = false;
                SelectedLineVisible = false;
                break;
             
            case ConsoleKey.Enter:
                var selectedLine = Model.GetLine(TopLine+SelectedLine);
                clipboard.SetText(selectedLine);
                break;

            default: 
                base.OnKey(keyInfo);
                break;
        }

        Refresh();
    }

    private void Search(Direction searchDirection)
    {
        if (string.IsNullOrEmpty(SearchExpr) || searchDirection == Direction.None )
        {
            return;
        }

        int delta = searchDirection == Direction.Forward ? +1 : -1;
        var initialLineInfo = Model.GetLineInfo(TopLine);
        TextFragmentInfo lineInfo;
        do
        {
            TopLine += delta;
            lineInfo = Model.GetLineInfo(TopLine);
            if (lineInfo == null)
            {
                if (delta > 0)
                {
                    ScrollToTop();
                }
                else
                {
                    ScrollToBottom();
                }

                TopLine = 0;
                lineInfo = Model.GetLineInfo(TopLine);
            }
            
            if (lineInfo.Line.Contains(SearchExpr))
            {
                break;
            }
            
        } while (lineInfo.Begin != initialLineInfo.Begin);
    }

    private void InputSearchExpr(ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.Backspace:
                if (!string.IsNullOrEmpty(SearchExpr))
                {
                    SearchExpr = SearchExpr[.. ^1];
                    SearchExprCursor--;
                }
                break;
            case  ConsoleKey.Delete:
            {
                if( SearchExprCursor < SearchExpr.Length)
                {
                    SearchExpr = SearchExpr.Remove(SearchExprCursor, 1);
                }

                break;
            }

            case ConsoleKey.Home:
                SearchExprCursor = 0;
                break;
            
            case ConsoleKey.End:
                SearchExprCursor = SearchExpr.Length;
                break;
            
            case ConsoleKey.RightArrow:
                SearchExprCursor = Math.Min(SearchExprCursor + 1, SearchExpr.Length);
                break;
            
            case ConsoleKey.LeftArrow:
                SearchExprCursor = Math.Max(SearchExprCursor - 1, 0);
                break;
            
            case ConsoleKey.Enter:
                SearchColorConfig = new ColorConfig(SearchExpr); 
                Search(SearchDirection);
                SearchDirection = Direction.None;
                break;             
            
            default:
                SearchExpr += keyInfo.KeyChar;
                SearchExprCursor++;
                break;
        }
        
        Refresh();
    }

    private void ScrollLeft(ConsoleModifiers modifiers)
    {
        if (modifiers == ConsoleModifiers.Control)
        {
            Offset -= 10;
        }
        else
        {
            Offset--;
        }
        Offset = Math.Max(Offset - 1, 0);

    }

    private void ScrollRight(ConsoleModifiers modifiers)
    {
        if (modifiers == ConsoleModifiers.Control)
        {
            Offset += 10;
        }
        else
        {
            Offset += 1;
        }

    }

    private void MoveSelectedLine(int n)
    {
        SelectedLine +=  n;
        switch (n)
        {
            case > 0 when SelectedLine > NbLines - 1:
                SelectedLine = NbLines - 1;
                ScrollOneLineDown();
                break;
            case <= 0 when SelectedLine < 0:
                SelectedLine = 0;
                ScrollOneLineUp();
                break;
        }
    }

    public override void Stop()
    {
        timer.Dispose();
    }
}
