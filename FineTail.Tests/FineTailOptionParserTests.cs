using FineTail;

namespace FineTailTests;

[TestFixture]
public class FineTailOptionParserTests
{
    [Test]
    public void BasicTest()
    {
        var parser = new FineTailOptionParser();
        var options = parser.Parse("");
        Check.That(options.FilePattern).IsEqualTo("*.log");
        Check.That(options.NbLines).IsEqualTo(20);
        Check.That(options.Colors).IsEmpty();
        Check.That(options.Filters).IsEmpty();
        Check.That(options.Interactive).IsFalse();
        Check.That(options.Follow).IsFalse();
        
        options = parser.Parse("MyApp*.log");
        Check.That(options.FilePattern).IsEqualTo("MyApp*.log");
        Check.That(options.NbLines).IsEqualTo(20);
        Check.That(options.Colors).IsEmpty();
        Check.That(options.Filters).IsEmpty();
        Check.That(options.Interactive).IsFalse();
        Check.That(options.Follow).IsFalse();
        
        options = parser.Parse(" -f    *.txt ");
        Check.That(options.FilePattern).IsEqualTo("*.txt");
        Check.That(options.NbLines).IsEqualTo(20);
        Check.That(options.Colors).IsEmpty();
        Check.That(options.Filters).IsEmpty();
        Check.That(options.Interactive).IsFalse();
        Check.That(options.Follow).IsTrue();

        options = parser.Parse(" *.txt -f ");
        Check.That(options.FilePattern).IsEqualTo("*.txt");
        Check.That(options.NbLines).IsEqualTo(20);
        Check.That(options.Colors).IsEmpty();
        Check.That(options.Filters).IsEmpty();
        Check.That(options.Interactive).IsFalse();
        Check.That(options.Follow).IsTrue();
    }
}