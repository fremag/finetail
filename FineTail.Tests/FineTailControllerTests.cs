using FineTail;

namespace FineTailTests;

[TestFixture]
public class FineTailControllerTests
{
    [Test]
    public void BasicTest()
    {
        var w1 = new FineTailController("e:\\tmp", new FineTailLogView(), null);
        Check.That(w1.Dir).IsEqualTo("e:\\tmp");
        Check.That(w1.Pattern).IsEqualTo("*.*");

        var w2 = new FineTailController("e:\\tmp\\myapp.log", new FineTailLogView(), null);
        Check.That(w2.Dir).IsEqualTo("e:\\tmp");
        Check.That(w2.Pattern).IsEqualTo("myapp.log");

        var w3 = new FineTailController("e:\\tmp\\myApp-*.log", new FineTailLogView(), null);
        Check.That(w3.Dir).IsEqualTo("e:\\tmp");
        Check.That(w3.Pattern).IsEqualTo("myApp-*.log");
    }

    [Test]
    public void Test()
    {
        var w2 = new FineTailController("e:\\tmp\\myapp.log", new FineTailLogView(), null);
        Check.That(w2.Dir).IsEqualTo("e:\\tmp");
        Check.That(w2.Pattern).IsEqualTo("myapp.log");

        w2.FsWatcher.WaitForChanged(WatcherChangeTypes.All, TimeSpan.FromSeconds(5));
    }
}