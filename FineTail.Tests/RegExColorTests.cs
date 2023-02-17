using System.Text.RegularExpressions;

namespace FineTailTests;

[TestFixture]
public class RegExColorTests
{
    [Test]
    public void Test()
    {
        Regex regex = new Regex("(\\[.*?\\])");

        var line = regex.Replace("test ABC[123] CDE[abc]", "_$1_");
        Check.That(line).IsEqualTo("test ABC_[123]_ CDE_[abc]_");
    }
}