using System.Text.RegularExpressions;

namespace FineTail;

public enum Color
{
    Black = 30,
    Red = 31,
    Green = 32,
    Yellow = 33,
    Blue = 34,
    Magenta = 35,
    Cyan = 36,
    White = 37
}

public static class ColorUtils
{
    public static string Color(this string s, Color color) => s.Color((int)color);
    public static string Color(this string s, int color) => $"\u001b[{color}m{s}\u001b[0m";
}

public class ColorConfig
{
    public Regex RegEx { get; }
    public string ReplaceString { get; }

    public ColorConfig(string colorName, string regExpr) : this((int)Enum.Parse<Color>(colorName, true), regExpr)
    { }
    
    // inverse video
    public ColorConfig(string regExpr) : this(7, regExpr)
    { }

    public ColorConfig(int color, string regExpr)
    {
        var regEx = new Regex(regExpr, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var nums = regEx.GetGroupNumbers();

        RegEx = regEx;
        ReplaceString = $"${nums.Last()}".Color(color);
    }

    public string Apply(string line) => RegEx.Replace(line, ReplaceString);
}