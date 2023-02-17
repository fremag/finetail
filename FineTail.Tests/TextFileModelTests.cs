using FineTail;

namespace FineTailTests;

[TestFixture]
public class TextFileModelTests
{
    private string FileName { get; set; }
    
    [SetUp]
    public void SetUp()
    {
        FileName = Path.GetTempFileName();        
        var file = File.CreateText(FileName);
        for (int i = 1; i <= 1000; i++)
        {
            file.WriteLine($"Line {i}");
        }

        file.Dispose();
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(FileName))
        {
            File.Delete(FileName);
        }
    }
    
    [Test]
    public void BackwardTest()
    {
        TextFileModel model = new(FileName);
        var line = model.GetLine(-5);
        Check.That(line).IsEqualTo("Line 995");

        line = model.GetLine(-10);
        Check.That(line).IsEqualTo("Line 990");
        
        line = model.GetLine(-6);
        Check.That(line).IsEqualTo("Line 994");
    }

    [Test]
    public void ForwardTest()
    {
        TextFileModel model = new(FileName);
        var line = model.GetLine(-5);
        Check.That(line).IsEqualTo("Line 995");
        File.AppendAllLines(FileName, new [] {"AAAA", "BBBB"});

        line = model.GetLine(1);
        Check.That(line).IsEqualTo("AAAA");
        
        line = model.GetLine(2);
        Check.That(line).IsEqualTo("BBBB");
        
        line = model.GetLine(-999);
        Check.That(line).IsEqualTo("Line 1");
        
        File.AppendAllLines(FileName, new [] {"CCCC", "DDDD"});
        line = model.GetLine(4);
        Check.That(line).IsEqualTo("DDDD");
    }
    
    [Test]
    public void FilterTest() 
    {
        TextFileModel model = new(FileName, new [] {"23"});
        var line = model.GetLine(0);
        Check.That(line).IsEqualTo("Line 923");
        Check.That(model.GetLine(-1)).IsEqualTo("Line 823");
        Check.That(model.GetLine(-2)).IsEqualTo("Line 723");
        Check.That(model.GetLine(-3)).IsEqualTo("Line 623");
        Check.That(model.GetLine(-4)).IsEqualTo("Line 523");
        
        line = model.GetLine(1);
        Check.That(line).IsNull();

        File.AppendAllLines(FileName, new []{"Line xxx"});
        line = model.GetLine(1);
        Check.That(line).IsNull();

        File.AppendAllLines(FileName, new []{"Line 1023"});
        line = model.GetLine(1);
        Check.That(line).IsEqualTo("Line 1023");
    }
}